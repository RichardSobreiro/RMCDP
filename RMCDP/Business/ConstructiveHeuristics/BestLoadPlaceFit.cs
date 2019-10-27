using Business.Extensions;
using Business.Extensions.ValueTypes;
using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.ConstructiveHeuristics
{
    public class BestLoadPlaceFit : IBestLoadPlaceFit
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            StringBuilder log = new StringBuilder();

            List<DeliveryOrderTrip> deliveryOrdersTrips =
                deliveryOrderRepository.GetDeliveriesOrdersWithDeliveryOrderTrips(
                    instanceNumber,
                    begin,
                    end).
                    OrderBy(d => d.RequestedTime).ToList();

            Dictionary<int, Location> loadPlaces = loadPlacesRepository.GetLoadPlacesWithVehicles(instanceNumber);

            Dictionary<string, double> distances = new Dictionary<string, double>();

            ComputeDistancesForLoadPlaces(deliveryOrdersTrips, loadPlaces, distances);
            ComputeDistancesForConstructions(deliveryOrdersTrips, loadPlaces, distances);

            decimal totalIncome = 0;
            List<Trip> trips = BestFitConstructionHeuristic(log, deliveryOrdersTrips, 
                loadPlaces, distances, instanceNumber, out totalIncome);

            Console.WriteLine($"Total Income: {totalIncome}");
            
            File.WriteAllText(Directory.GetCurrentDirectory() + $"/Logs-{instanceNumber}.txt", "");
            File.AppendAllText(Directory.GetCurrentDirectory() + $"/Logs-{instanceNumber}.txt", log.ToString());
            log.Clear();
        }

        public void ComputeDistancesForLoadPlaces(List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, Dictionary<string, double> distances)
        {
            foreach (KeyValuePair<int, Location> loadPlace in loadPlaces)
            {
                foreach (KeyValuePair<int, Location> loadPlaceDestiny in loadPlaces)
                {
                    distances.Add(
                        loadPlace.Value.LocationId.Format(loadPlaceDestiny.Value.LocationId),
                        (loadPlace.Value.GeoCordinates.GetDistanceTo(
                            loadPlaceDestiny.Value.GeoCordinates)
                        ) / 1000
                        );
                }

                foreach (DeliveryOrderTrip deliveryOrdersTrip in deliveryOrdersTrips)
                {
                    double distance;
                    if(!distances.TryGetValue(loadPlace.Value.LocationId.Format(deliveryOrdersTrip.Construction.LocationId),
                        out distance))
                    {
                        distances.Add(
                            loadPlace.Value.LocationId.Format(deliveryOrdersTrip.Construction.LocationId),
                            (loadPlace.Value.GeoCordinates.GetDistanceTo(
                                deliveryOrdersTrip.Construction.GeoCordinates)
                            ) / 1000
                            );
                    }
                }
            }
        }

        public void ComputeDistancesForConstructions(List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, Dictionary<string, double> distances)
        {
            foreach (DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
            {
                foreach (KeyValuePair<int, Location> loadPlaceDestiny in loadPlaces)
                {
                    double distance;
                    if (!distances.TryGetValue(
                        deliveryOrderTrip.Construction.LocationId.Format(loadPlaceDestiny.Value.LocationId),
                        out distance))
                    {
                        distances.Add(
                            deliveryOrderTrip.Construction.LocationId.Format(loadPlaceDestiny.Value.LocationId),
                            (deliveryOrderTrip.Construction.GeoCordinates.GetDistanceTo(
                                loadPlaceDestiny.Value.GeoCordinates)
                            ) / 1000);
                    }
                }

                foreach (DeliveryOrderTrip deliveryOrderTripDestiny in deliveryOrdersTrips)
                {
                    double distance;
                    if (!distances.TryGetValue(
                        deliveryOrderTrip.Construction.LocationId.Format(deliveryOrderTripDestiny.Construction.LocationId),
                        out distance))
                    {
                        distances.Add(
                            deliveryOrderTrip.Construction.LocationId.Format(deliveryOrderTripDestiny.Construction.LocationId),
                            (deliveryOrderTrip.Construction.GeoCordinates.GetDistanceTo(
                                deliveryOrderTripDestiny.Construction.GeoCordinates)
                            ) / 1000);
                    }
                }
            }
        }

        public List<Trip> BestFitConstructionHeuristic(StringBuilder log,
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, Dictionary<string, double> distances,
            int instanceNumber, out decimal TotalIncome)
        {
            List<Trip> trips = new List<Trip>();
            decimal returnTotalIncome = 0;

            foreach (DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
            {
                Trip resultTrip = new Trip();

                double minDistance = double.MaxValue;
                Location loadPlaceMinDistance = null;
                double currentDistance;
                foreach (KeyValuePair<int , Location> loadPlace in loadPlaces)
                {
                    if (distances.TryGetValue(
                        deliveryOrderTrip.Construction.LocationId.Format(loadPlace.Value.LocationId), 
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
                    TimeSpan.FromMinutes(minDistance + 10));
                int? vehicleId = loadPlaceMinDistance.GetFirstVehicleAvailebleBeforeTime(
                    bestInitialLoadTime);

                resultTrip.Cost =
                        (decimal)(minDistance * loadPlaceMinDistance.Vehicles[vehicleId.Value].MaintenanceCostPerKm) +
                        (decimal)(2 * (minDistance / loadPlaceMinDistance.Vehicles[vehicleId.Value].FuelConsumptionKmPerLiter) *
                            loadPlaceMinDistance.FuelCost);

                resultTrip.Income = deliveryOrderTrip.Income - resultTrip.Cost - deliveryOrderTrip.RMCCost;

                if (vehicleId.HasValue && resultTrip.Income > 0)
                {
                    returnTotalIncome += resultTrip.Income;

                    resultTrip.InstanceNumber = instanceNumber;
                    resultTrip.LocationIdLoadPlace = loadPlaceMinDistance.LocationId;
                    resultTrip.LocationIdConstruction = deliveryOrderTrip.Construction.LocationId;
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

                    resultTrip.WaitTimeAtLoadPlace = TimeSpan.MinValue;
                    resultTrip.WaitTimeAfterArrivalAtConstruction = TimeSpan.MinValue;
                    resultTrip.WaitTimeAfterUnloadAtConstruction = TimeSpan.MinValue;

                    trips.Add(resultTrip);
                }
                else
                {
                    log.AppendLine($"DeliveryOrderTripId: {deliveryOrderTrip.DeliveryOrderTripId} - " +
                        $"RequestedTime: {deliveryOrderTrip.RequestedTime} - " +
                        $"LoadPlaceMinDistance: {loadPlaceMinDistance} - " +
                        $"MinDistance: {minDistance} - " +
                        $"BestInitialLoadTime: {bestInitialLoadTime} - " +
                        $"Income: {resultTrip.Income}");
                }
            }

            TotalIncome = returnTotalIncome;
            log.AppendLine($"Total Income: {TotalIncome}");

            return trips;
        }

        public BestLoadPlaceFit(IDeliveryOrderRepository _deliveryOrderRepository, ILoadPlacesRepository _loadPlacesRepository)
        {
            deliveryOrderRepository = _deliveryOrderRepository;
            loadPlacesRepository = _loadPlacesRepository;
        }

        private IDeliveryOrderRepository deliveryOrderRepository;
        private ILoadPlacesRepository loadPlacesRepository;
    }
}
