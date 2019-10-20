using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Repository.Instances;
using CrossCutting.DataExtructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    public class Kinable
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            List<DeliveryOrderTrip> deliveryOrders = deliveryOrderRepository.GetDeliveriesOrdersWithDeliveryOrderTrips(
                instanceNumber,
                begin,
                end).
                OrderBy(d => d.RequestedTime).ToList();

            List<Location> loadPlaces = loadPlacesRepository.GetLoadPlacesWithVehicles(instanceNumber);

            MultiKeyDictionary<int, int, double> distances = new MultiKeyDictionary<int, int, double>();




            List<Trip> trips = BestFitConstructionHeuristic(deliveryOrders, loadPlaces);

        }

        public void ComputeDistancesForLoadPlaces(List<DeliveryOrderTrip> deliveryOrdersTrips,
            List<Location> loadPlaces, MultiKeyDictionary<int, int, double> distances)
        {
            foreach(Location loadPlace in loadPlaces)
            {
                foreach (Location loadPlaceDestiny in loadPlaces)
                {
                    if(distances.TryGetValue())
                    distances.Add(loadPlace.LocationId,
                        loadPlaceDestiny.LocationId,
                        loadPlace.GeoCordinates.GetDistanceTo(loadPlaceDestiny.GeoCordinates));
                }

                foreach (DeliveryOrderTrip deliveryOrdersTrip in deliveryOrdersTrips)
                {
                    distances.Add(loadPlace.LocationId,
                        deliveryOrdersTrip.Construction.LocationId,
                        loadPlace.GeoCordinates.GetDistanceTo(deliveryOrdersTrip.Construction.GeoCordinates));
                }
            }
        }

        public void ComputeDistancesForConstructions(List<DeliveryOrderTrip> deliveryOrdersTrips,
            List<Location> loadPlaces, MultiKeyDictionary<int, int, double> distances)
        {
            foreach (Location loadPlace in loadPlaces)
            {
                foreach (Location loadPlaceDestiny in loadPlaces)
                {
                    distances.Add(loadPlace.LocationId,
                        loadPlaceDestiny.LocationId,
                        loadPlace.GeoCordinates.GetDistanceTo(loadPlaceDestiny.GeoCordinates));
                }

                foreach (Location construction in loadPlaces)
                {
                    distances.Add(loadPlace.LocationId,
                        construction.LocationId,
                        loadPlace.GeoCordinates.GetDistanceTo(construction.GeoCordinates));
                }
            }
        }

        public List<Trip> BestFitConstructionHeuristic(List<DeliveryOrderTrip> deliveryOrdersTrips, 
            List<Location> loadPlaces)
        {
            List<Trip> trips = new List<Trip>();

            foreach(DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
            {

            }

            return trips;
        }

        public Kinable(IDeliveryOrderRepository _deliveryOrderRepository, ILoadPlacesRepository _loadPlacesRepository)
        {
            deliveryOrderRepository = _deliveryOrderRepository;
            loadPlacesRepository = _loadPlacesRepository;
        }

        private IDeliveryOrderRepository deliveryOrderRepository;
        private ILoadPlacesRepository loadPlacesRepository;
    }
}
