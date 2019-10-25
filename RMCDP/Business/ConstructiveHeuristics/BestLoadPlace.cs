using Business.Extensions;
using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Repository.Instances;
using CrossCutting.DataExtructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.ConstructiveHeuristics
{
    public class BestLoadPlace
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            List<DeliveryOrderTrip> deliveryOrdersTrips =
                deliveryOrderRepository.GetDeliveriesOrdersWithDeliveryOrderTrips(
                    instanceNumber,
                    begin,
                    end).
                    OrderBy(d => d.RequestedTime).ToList();

            Dictionary<int, Location> loadPlaces = loadPlacesRepository.GetLoadPlacesWithVehicles(instanceNumber);

            MultiKeyDictionary<int, int, double> distances = new MultiKeyDictionary<int, int, double>();

            ComputeDistancesForLoadPlaces(deliveryOrdersTrips, loadPlaces, distances);
            ComputeDistancesForConstructions(deliveryOrdersTrips, loadPlaces, distances);

            List<Trip> trips = BestFitConstructionHeuristic(deliveryOrdersTrips, 
                loadPlaces, distances, instanceNumber);


        }

        public void ComputeDistancesForLoadPlaces(List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, MultiKeyDictionary<int, int, double> distances)
        {
            foreach (KeyValuePair<int, Location> loadPlace in loadPlaces)
            {
                foreach (KeyValuePair<int, Location> loadPlaceDestiny in loadPlaces)
                {
                    distances.Add(
                        loadPlace.Value.LocationId,
                        loadPlaceDestiny.Value.LocationId,
                        loadPlace.Value.GeoCordinates.GetDistanceTo(
                            loadPlaceDestiny.Value.GeoCordinates));
                }

                foreach (DeliveryOrderTrip deliveryOrdersTrip in deliveryOrdersTrips)
                {
                    distances.Add(
                        loadPlace.Value.LocationId,
                        deliveryOrdersTrip.Construction.LocationId,
                        loadPlace.Value.GeoCordinates.GetDistanceTo(
                            deliveryOrdersTrip.Construction.GeoCordinates));
                }
            }
        }

        public void ComputeDistancesForConstructions(List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, MultiKeyDictionary<int, int, double> distances)
        {
            foreach (DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
            {
                foreach (KeyValuePair<int, Location> loadPlaceDestiny in loadPlaces)
                {
                    distances.Add(
                        deliveryOrderTrip.Construction.LocationId,
                        loadPlaceDestiny.Value.LocationId,
                        deliveryOrderTrip.Construction.GeoCordinates.GetDistanceTo(
                            loadPlaceDestiny.Value.GeoCordinates));
                }

                foreach (DeliveryOrderTrip deliveryOrderTripDestiny in deliveryOrdersTrips)
                {
                    distances.Add(
                        deliveryOrderTrip.Construction.LocationId,
                        deliveryOrderTripDestiny.Construction.LocationId,
                        deliveryOrderTrip.Construction.GeoCordinates.GetDistanceTo(
                            deliveryOrderTripDestiny.Construction.GeoCordinates));
                }
            }
        }

        public List<Trip> BestFitConstructionHeuristic(List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, MultiKeyDictionary<int, int, double> distances,
            int instanceNumber)
        {
            List<Trip> trips = new List<Trip>();

            foreach (DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
            {
                Trip resultTrip = new Trip();

                double minDistance = double.MaxValue;
                Location loadPlaceMinDistance = null;
                double currentDistance;
                foreach (KeyValuePair<int , Location> loadPlace in loadPlaces)
                {
                    if(distances.TryGetValue(
                        deliveryOrderTrip.Construction.LocationId, 
                        loadPlace.Value.LocationId, 
                        out currentDistance))
                    {
                        if (currentDistance < minDistance)
                        {
                            minDistance = currentDistance;
                            loadPlaceMinDistance = loadPlace.Value;
                        }
                    }
                }

                resultTrip.InstanceNumber = instanceNumber;
                resultTrip.LocationIdLoadPlace = loadPlaceMinDistance.LocationId;
                DateTime bestInitialLoadTime = deliveryOrderTrip.GetBestInitialLoadTime(loadPlaceMinDistance,
                    TimeSpan.FromMinutes(minDistance));
                int? vehicleId = loadPlaceMinDistance.GetFirstVehicleAvailebleBeforeTime(
                    bestInitialLoadTime);

                if (vehicleId.HasValue)
                {
                    resultTrip.InstanceNumber = instanceNumber;
                    resultTrip.LocationIdLoadPlace = loadPlaceMinDistance.LocationId;
                    resultTrip.LocationIdConstruction = deliveryOrderTrip.ConstructionId;
                    resultTrip.VehicleId = vehicleId.Value;
                    resultTrip.DesiredRequestedTime = deliveryOrderTrip.RequestedTime;
                    resultTrip.Income = deliveryOrderTrip.Income;
                    resultTrip.Volume = deliveryOrderTrip.Volume;

                    resultTrip.InitialLoadTime = bestInitialLoadTime;
                    resultTrip.FinalLoadTime = resultTrip.InitialLoadTime.Add(
                        TimeSpan.FromMinutes(resultTrip.Volume * loadPlaceMinDistance.RateRMCProduction));
                    resultTrip.DepartureTimeFromLoadPlace = 
                        resultTrip.FinalLoadTime.Add(TimeSpan.FromMinutes(5));
                    resultTrip.ArrivalTimeAtConstruction = 
                        resultTrip.DepartureTimeFromLoadPlace.Add(TimeSpan.FromMinutes(minDistance));
                    resultTrip.InitialUnloadTimeAtConstruction = 
                        resultTrip.ArrivalTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    resultTrip.FinalUnloadTimeAtConstruction =
                        resultTrip.InitialUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(20));
                    resultTrip.DepartureTimeFromConstruction =
                        resultTrip.FinalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    resultTrip.ArrivalTimeAtLoadPlace =
                        resultTrip.FinalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(minDistance));

                    loadPlaceMinDistance.Vehicles[vehicleId.Value].AddBeginOfLastTrip(resultTrip.InitialLoadTime);
                    loadPlaceMinDistance.Vehicles[vehicleId.Value].AddEndOfLastTrip(resultTrip.ArrivalTimeAtLoadPlace);
                    loadPlaceMinDistance.Vehicles[vehicleId.Value].AddLoadPlaceIdOfLastTrip(deliveryOrderTrip.DeliveryOrderTripId);

                    resultTrip.Cost =
                        (decimal)(minDistance * loadPlaceMinDistance.Vehicles[vehicleId.Value].MaintenanceCostPerKm) +
                        (decimal)(2 * (minDistance / loadPlaceMinDistance.Vehicles[vehicleId.Value].FuelConsumptionKmPerLiter) * 
                            loadPlaceMinDistance.FuelCost);

                    resultTrip.Income = deliveryOrderTrip.Income - resultTrip.Cost - deliveryOrderTrip.RMCCost;

                    resultTrip.WaitTimeAtLoadPlace = TimeSpan.MinValue;
                    resultTrip.WaitTimeAfterArrivalAtConstruction = TimeSpan.MinValue;
                    resultTrip.WaitTimeAfterUnloadAtConstruction = TimeSpan.MinValue;

                    trips.Add(resultTrip);
                }

            }

            return trips;
        }

        public BestLoadPlace(IDeliveryOrderRepository _deliveryOrderRepository, ILoadPlacesRepository _loadPlacesRepository)
        {
            deliveryOrderRepository = _deliveryOrderRepository;
            loadPlacesRepository = _loadPlacesRepository;
        }

        private IDeliveryOrderRepository deliveryOrderRepository;
        private ILoadPlacesRepository loadPlacesRepository;
    }
}
