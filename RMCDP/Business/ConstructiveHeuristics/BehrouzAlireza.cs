using Business.Base;
using Business.Extensions.ValueTypes;
using Contracts.Entities.Helpers;
using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.ConstructiveHeuristics
{
    public class BehrouzAlireza : BaseBusiness, IBehrouzAlireza
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            StringBuilder log = new StringBuilder();
            List<DeliveryOrderTrip> deliveryOrdersTrips = GetDeliveriesOrdersWithDeliveryOrderTrips(instanceNumber, begin, end);
            Dictionary<int, Location> loadPlaces = GetLoadPlacesWithVehicles(instanceNumber);
            Dictionary<string, double> distances = ComputeDistances(log, instanceNumber, begin, end, deliveryOrdersTrips, loadPlaces);

            Queue<DeliveryOrderTrip> sequenceOfCustomers = ConstructSequenceOfCustomers(deliveryOrdersTrips);

            List<RouteNode> routeNodes = AssignimentOfVehicleTypes(instanceNumber, sequenceOfCustomers);

            List<Route> routes = ConstructionOfRoutes(instanceNumber, routeNodes, sequenceOfCustomers, distances);

        }

        private Queue<DeliveryOrderTrip> ConstructSequenceOfCustomers(List<DeliveryOrderTrip> deliveryOrdersTrips)
        {
            Queue<DeliveryOrderTrip> sequenceOfCustomers = new Queue<DeliveryOrderTrip>();
            List<DeliveryOrderTrip> deliveryOrdersTripsCopy = new List<DeliveryOrderTrip>(deliveryOrdersTrips);
            var rnd = new Random();
            var probabilityItems = new List<ProbabilityItem<DeliveryOrderTrip>>();

            while (deliveryOrdersTripsCopy.Any())
            {
                DeliveryOrderTrip deliveryOrderTrip = deliveryOrdersTripsCopy.First();

                IOrderedEnumerable<DeliveryOrderTrip> restrictedCustomerList = deliveryOrdersTripsCopy.Where(d =>
                        (d.RequestedTime >= deliveryOrderTrip.RequestedTime &&
                            d.RequestedTime <= deliveryOrderTrip.RequestedTimeEndWindow) ||
                        (d.RequestedTimeEndWindow >= deliveryOrderTrip.RequestedTime &&
                            d.RequestedTimeEndWindow <= deliveryOrderTrip.RequestedTimeEndWindow)
                    ).OrderBy(d => d.RequestedTime);


                double rclLenght = (double)restrictedCustomerList.Count();
                int j = 1;
                probabilityItems.Clear();
                foreach (DeliveryOrderTrip deliveryRCL in restrictedCustomerList)
                {
                    probabilityItems.Add(new ProbabilityItem<DeliveryOrderTrip>
                    {
                        Probability = (j / (rclLenght * ((rclLenght + 1) / 2))),
                        Item = deliveryRCL
                    });
                    j++;
                }

                ProbabilityItem<DeliveryOrderTrip> selected = null;
                while (selected == null)
                {
                    var probability = rnd.NextDouble();
                    IEnumerable<ProbabilityItem<DeliveryOrderTrip>> selectedItems =
                        probabilityItems.SkipWhile(i => i.Probability < probability);
                    selected = selectedItems.Any() ? selectedItems.First() : null;
                }

                sequenceOfCustomers.Enqueue(selected.Item);

                deliveryOrdersTripsCopy.RemoveAll(d => d.DeliveryOrderTripId == selected.Item.DeliveryOrderTripId);
            }

            return sequenceOfCustomers;
        }

        private List<RouteNode> AssignimentOfVehicleTypes(int instanceNumber,
            Queue<DeliveryOrderTrip> sequenceOfCustomers)
        {
            List<RouteNode> routeNodes = new List<RouteNode>();

            foreach (DeliveryOrderTrip deliveryOrderTrip in sequenceOfCustomers)
            {
                routeNodes.Add(
                    new RouteNode() 
                    {
                        InstanceNumber = instanceNumber,
                        DeliveryOrderTripId = deliveryOrderTrip.DeliveryOrderTripId,
                        Volume = deliveryOrderTrip.Volume,
                        VehicleType = 1,
                        VehicleTypeVolume = 8,
                        ArrivalTimeAtConstruction = deliveryOrderTrip.RequestedTime,
                        InitialUnloadTimeAtConstruction = deliveryOrderTrip.RequestedTime.Add(TimeSpan.FromMinutes(5)),
                        FinalUnloadTimeAtConstruction = deliveryOrderTrip.RequestedTime.Add(TimeSpan.FromMinutes(25)),
                        DepartureTimeFromConstruction = deliveryOrderTrip.RequestedTime.Add(TimeSpan.FromMinutes(30))
                    }
                );
            }

            return new List<RouteNode>();
        }

        private List<Route> ConstructionOfRoutes(int instanceNumber,
            List<RouteNode> routeNodes,
            Queue<DeliveryOrderTrip> sequenceOfCustomers,
            Dictionary<string, double> distances)
        {
            
            List<Route> routes = new List<Route>();
            List<int> deliveryOrderTripsInRoutes = new List<int>();

            foreach (DeliveryOrderTrip deliveryOrderTrip in sequenceOfCustomers)
            {
                if(!deliveryOrderTripsInRoutes.Any(d => d == deliveryOrderTrip.DeliveryOrderTripId))
                {
                    RouteNode routeNode = routeNodes.FirstOrDefault(rn => 
                        rn.DeliveryOrderTripId == deliveryOrderTrip.DeliveryOrderTripId);

                    List<Route> routeSameVehicleTypeAndAvailableVolume = routes.Where(r => 
                        r.VehicleType == routeNode.VehicleType &&
                        r.RemainingVolume >= routeNode.Volume).ToList();

                    Route routeSelected = null;
                    foreach(Route route in routeSameVehicleTypeAndAvailableVolume)
                    {
                        RouteNode nextRouteNodeCurrentRoute;
                        double minDistance = double.MaxValue;
                        if (route.RouteNodes.TryPeek(out nextRouteNodeCurrentRoute))
                        {
                            double distance;
                            if (distances.TryGetValue(
                                routeNode.DeliveryOrderTripId.Format(nextRouteNodeCurrentRoute.DeliveryOrderTripId),
                                out distance))
                            {
                                if(distance < minDistance)
                                {
                                    TimeSpan? availableTimeWindowBetweenNodes =
                                        nextRouteNodeCurrentRoute.ArrivalTimeAtConstruction -
                                        routeNode.DepartureTimeFromConstruction;
                                    if(availableTimeWindowBetweenNodes >= TimeSpan.FromMinutes(distance))
                                    {
                                        routeSelected = route;
                                    }
                                }
                            } 
                        }
                        else
                        {
                            break;
                        }
                    }

                    if(routeSelected == null)
                    {
                        routeSelected = new Route();
                        routeSelected.RemainingVolume -= (routeNode.VehicleTypeVolume - routeNode.Volume);
                        routeSelected.TotalVolume += (routeNode.VehicleTypeVolume - routeNode.Volume);
                        routeSelected.VehicleType = routeNode.VehicleType;
                        routeSelected.VehicleTypeVolume = routeNode.VehicleTypeVolume;
                        routeSelected.RouteNodes.Enqueue(routeNode);
                        
                        routes.Add(routeSelected);
                    }
                    else
                    {
                        routeSelected.RemainingVolume -= routeNode.Volume;
                        routeSelected.TotalVolume += routeNode.Volume;
                        routeSelected.RouteNodes.Enqueue(routeNode);
                    }
                    deliveryOrderTripsInRoutes.Add(deliveryOrderTrip.DeliveryOrderTripId);
                }
            }

            return routes;
        }

        private void DeterminationOfStartAndEndDepots(
            List<Route> routes, 
            Dictionary<string, double> distances,
            Dictionary<int, Location> loadPlaces)
        {
            foreach(Route route in routes)
            {
                RouteNode firstNode = route.RouteNodes.FirstOrDefault();
                RouteNode lastNode = route.RouteNodes.LastOrDefault();

                double distance;
                double minDistanceStartLoadPlace = double.MaxValue;
                double minDistanceEndLoadPlace = double.MaxValue;
                Location startLoadPlace = null;
                Location endLoadPlace = null;
                foreach (KeyValuePair<int, Location> loadPlace in loadPlaces)
                {
                    if((!distances.TryGetValue(firstNode.DeliveryOrderTripId.Format(loadPlace.Key), 
                        out distance) ||
                        !distances.TryGetValue(loadPlace.Key.Format(firstNode.DeliveryOrderTripId),
                        out distance)))
                    {
                        DateTime initialLoadTime =
                            firstNode.ArrivalTimeAtConstruction.Value.
                            Subtract(TimeSpan.FromMinutes(5)).
                            Subtract(TimeSpan.FromMinutes(route.TotalVolume * startLoadPlace.RateRMCProduction)).
                            Subtract(TimeSpan.FromMinutes(distance)).
                            Subtract(TimeSpan.FromMinutes(5));

                        if (distance < minDistanceStartLoadPlace &&
                            loadPlace.Value.Vehicles.Any(v => v.Value.GetEndOfLastTrip() <= initialLoadTime))
                        {
                            minDistanceStartLoadPlace = distance;
                            startLoadPlace = loadPlace.Value;
                        }
                    }

                    if (!distances.TryGetValue(lastNode.DeliveryOrderTripId.Format(loadPlace.Key),
                        out distance) ||
                        !distances.TryGetValue(loadPlace.Key.Format(lastNode.DeliveryOrderTripId),
                        out distance))
                    {
                        if (distance < minDistanceEndLoadPlace)
                        {
                            minDistanceEndLoadPlace = distance;
                            endLoadPlace = loadPlace.Value;
                        }
                    }
                }

                if(startLoadPlace != null && endLoadPlace != null)
                {
                    route.StartLoadPlaceId = startLoadPlace.LocationId;

                    route.InitialLoadTime = firstNode.ArrivalTimeAtConstruction.Value.
                        Subtract(TimeSpan.FromMinutes(5)).
                        Subtract(TimeSpan.FromMinutes(minDistanceStartLoadPlace)).
                        Subtract(TimeSpan.FromMinutes(5)).
                        Subtract(TimeSpan.FromMinutes(route.TotalVolume * startLoadPlace.RateRMCProduction));
                    route.FinalLoadTime = route.InitialLoadTime.Value.
                        Add(TimeSpan.FromMinutes(route.TotalVolume * startLoadPlace.RateRMCProduction));

                    route.DepartureTimeFromLoadPlace = route.FinalLoadTime.Value.
                        Add(TimeSpan.FromMinutes(5));

                    route.ArrivalTimeAtLoadPlace = lastNode.FinalUnloadTimeAtConstruction.Value.
                        Add(TimeSpan.FromMinutes(5)).
                        Add(TimeSpan.FromMinutes(minDistanceEndLoadPlace));
                }
            }
        }

        public BehrouzAlireza(IDeliveryOrderRepository _deliveryOrderRepository, 
            ILoadPlacesRepository _loadPlacesRepository) : 
            base(_deliveryOrderRepository, _loadPlacesRepository)
        {

        }
    }
}
