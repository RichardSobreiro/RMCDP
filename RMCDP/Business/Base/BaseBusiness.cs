using Business.Extensions.ValueTypes;
using Contracts.Entities.Instances;
using Contracts.Interfaces.Repository.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Base
{
    public abstract class BaseBusiness
    {
        public List<DeliveryOrderTrip>  GetDeliveriesOrdersWithDeliveryOrderTrips(int instanceNumber, 
            DateTime begin, DateTime end)
        {
            return deliveryOrderRepository.GetDeliveriesOrdersWithDeliveryOrderTrips(
                instanceNumber,
                begin,
                end).
                OrderBy(d => d.RequestedTime).ToList();
        }

        public Dictionary<int, Location> GetLoadPlacesWithVehicles(int instanceNumber)
        {
            return loadPlacesRepository.GetLoadPlacesWithVehicles(instanceNumber);
        }

        public Dictionary<string, double> ComputeDistances(StringBuilder log, int instanceNumber, 
            DateTime begin, DateTime end,
            List<DeliveryOrderTrip> deliveryOrdersTrips, Dictionary<int, Location> loadPlaces)
        {
            Dictionary<string, double> distances = new Dictionary<string, double>();
            ComputeDistancesForLoadPlaces(deliveryOrdersTrips, loadPlaces, distances);
            ComputeDistancesForConstructions(deliveryOrdersTrips, loadPlaces, distances);
            return distances;
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
                    if (!distances.TryGetValue(loadPlace.Value.LocationId.Format(deliveryOrdersTrip.Construction.LocationId),
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

        public BaseBusiness(IDeliveryOrderRepository _deliveryOrderRepository, ILoadPlacesRepository _loadPlacesRepository)
        {
            deliveryOrderRepository = _deliveryOrderRepository;
            loadPlacesRepository = _loadPlacesRepository;
        }

        private IDeliveryOrderRepository deliveryOrderRepository;
        private ILoadPlacesRepository loadPlacesRepository;
    }
}
