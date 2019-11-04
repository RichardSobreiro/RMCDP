using Business.Base;
using Business.Extensions.ValueTypes;
using Contracts.Entities.Helpers;
using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Business.ConstructiveHeuristics
{
    public class BehrouzAlireza : BaseBusiness, IBehrouzAlireza
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            StringBuilder log = new StringBuilder();

            List<DeliveryOrderTrip> deliveryOrdersTrips = GetDeliveriesOrdersWithDeliveryOrderTrips(
                instanceNumber, 
                begin, 
                end);
            Dictionary<int, Location> loadPlaces = GetLoadPlacesWithVehicles(instanceNumber);
            Dictionary<string, double> distances = ComputeDistances(
                log, 
                instanceNumber, 
                begin, 
                end, 
                deliveryOrdersTrips, 
                loadPlaces);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Queue<DeliveryOrderTrip> sequenceOfCustomers = 
                ConstructSequenceOfCustomers(deliveryOrdersTrips);

            List<RouteNode> routeNodes = AssignimentOfVehicleTypes(
                instanceNumber, 
                sequenceOfCustomers);

            List<Route> routes = ConstructionOfRoutes(
                instanceNumber, 
                routeNodes, 
                sequenceOfCustomers, 
                distances);

            DeterminationOfStartAndEndDepots(
                routes, 
                distances, 
                loadPlaces);

            FindBestVehicleAvailable(routes, loadPlaces);

            stopwatch.Stop();

            PrintResults(
                routes,
                distances,
                loadPlaces,
                deliveryOrdersTrips,
                stopwatch);

        }

        private Queue<DeliveryOrderTrip> ConstructSequenceOfCustomers(
            List<DeliveryOrderTrip> deliveryOrdersTrips)
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

        private List<RouteNode> AssignimentOfVehicleTypes(
            int instanceNumber,
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
                        LocationId = deliveryOrderTrip.Construction.LocationId,
                        Income = deliveryOrderTrip.Income,
                        RMCCost = deliveryOrderTrip.RMCCost,
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

            return routeNodes;
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
                                routeNode.LocationId.Format(nextRouteNodeCurrentRoute.LocationId),
                                out distance) ||
                                distances.TryGetValue(
                                nextRouteNodeCurrentRoute.LocationId.Format(routeNode.LocationId),
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
                                        minDistance = distance;
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
                        routeSelected.RouteNodes = new Queue<RouteNode>();
                        routeSelected.RemainingVolume = (routeNode.VehicleTypeVolume - routeNode.Volume);
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
                    if((distances.TryGetValue(firstNode.LocationId.Format(loadPlace.Key), 
                        out distance) ||
                        distances.TryGetValue(loadPlace.Key.Format(firstNode.LocationId),
                        out distance)))
                    {
                        DateTime initialLoadTime =
                            firstNode.ArrivalTimeAtConstruction.Value.
                            Subtract(TimeSpan.FromMinutes(5)).
                            Subtract(TimeSpan.FromMinutes(route.TotalVolume * loadPlace.Value.RateRMCProduction)).
                            Subtract(TimeSpan.FromMinutes(distance)).
                            Subtract(TimeSpan.FromMinutes(5));

                        if (distance < minDistanceStartLoadPlace &&
                            loadPlace.Value.Vehicles.Any(v => !v.Value.GetEndOfLastTrip().HasValue  || 
                                v.Value.GetEndOfLastTrip() <= initialLoadTime))
                        {
                            minDistanceStartLoadPlace = distance;
                            startLoadPlace = loadPlace.Value;
                        }
                    }

                    if (distances.TryGetValue(lastNode.LocationId.Format(loadPlace.Key),
                        out distance) ||
                        distances.TryGetValue(loadPlace.Key.Format(lastNode.LocationId),
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
                    route.EndLoadPlaceId = endLoadPlace.LocationId;

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

        private void FindBestVehicleAvailable(
            List<Route> routes, 
            Dictionary<int, Location> loadPlaces)
        {
            foreach(Route route in routes)
            {
                KeyValuePair<int, Location> loadPlace = 
                    loadPlaces.FirstOrDefault(lp => lp.Key == route.StartLoadPlaceId);

                TimeSpan timeWindowBetweenLastAndNextTrip = TimeSpan.MaxValue;  
                foreach(KeyValuePair<int, Vehicle> vehicle in loadPlace.Value.Vehicles)
                {
                    DateTime? endOfLastTrip = vehicle.Value.GetEndOfLastTrip();
                    if (endOfLastTrip.HasValue &&
                        endOfLastTrip.Value <= route.InitialLoadTime &&
                        vehicle.Value.GetLoadPlaceIdOfLastTrip() == loadPlace.Key &&
                        ((endOfLastTrip.Value - route.InitialLoadTime) < timeWindowBetweenLastAndNextTrip))
                    {
                        route.VehicleId = vehicle.Key;
                        timeWindowBetweenLastAndNextTrip = (endOfLastTrip.Value - route.InitialLoadTime).Value;
                        vehicle.Value.AddBeginOfLastTrip(route.InitialLoadTime.Value);
                        vehicle.Value.AddEndOfLastTrip(route.ArrivalTimeAtLoadPlace.Value);
                        vehicle.Value.AddLoadPlaceIdOfLastTrip(route.EndLoadPlaceId);
                        break;
                    }
                    else if(!endOfLastTrip.HasValue &&
                        timeWindowBetweenLastAndNextTrip == TimeSpan.MaxValue)
                    {
                        route.VehicleId = vehicle.Key;
                        vehicle.Value.AddBeginOfLastTrip(route.InitialLoadTime.Value);
                        vehicle.Value.AddEndOfLastTrip(route.ArrivalTimeAtLoadPlace.Value);
                        vehicle.Value.AddLoadPlaceIdOfLastTrip(route.EndLoadPlaceId);
                        break;
                    }
                }
            }
        }

        private void PrintResults(
            List<Route> routes,
            Dictionary<string, double> distances,
            Dictionary<int, Location> loadPlaces,
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Stopwatch stopwatch)
        {
            decimal totalIncome = 0;
            int deliveriesSatisfiedCount = 0;

            foreach(Route route in routes)
            {
                Location startLoadPlace = loadPlaces.FirstOrDefault(lp => lp.Key == route.StartLoadPlaceId).Value;
                Location endLoadPlace = loadPlaces.FirstOrDefault(lp => lp.Key == route.EndLoadPlaceId).Value;
                Vehicle vehicle = startLoadPlace.Vehicles.FirstOrDefault(v => v.Key == route.VehicleId).Value;

                if(vehicle != null && endLoadPlace != null && startLoadPlace != null)
                {
                    int i = 0;
                    int? locationIdLastRouteNode = null;
                    foreach(RouteNode routeNode in route.RouteNodes)
                    {
                        totalIncome += (routeNode.Income.Value - routeNode.RMCCost.Value);
                        deliveriesSatisfiedCount++;
                        if (i == 0)
                        {
                            decimal startTravelCost = 0;
                            double distance = 0;
                            if(distances.TryGetValue(startLoadPlace.LocationId.Format(routeNode.LocationId), out distance)
                                || distances.TryGetValue(routeNode.LocationId.Format(startLoadPlace.LocationId), out distance))
                            {
                                startTravelCost = (decimal)(vehicle.MaintenanceCostPerKm * distance) +
                                    (decimal)((distance / vehicle.FuelConsumptionKmPerLiter) * startLoadPlace.FuelCost);

                                totalIncome -= startTravelCost;
                            }

                        }
                    
                        if(i == (route.RouteNodes.Count - 1))
                        {
                            decimal endTravelCost = 0;
                            double distance = 0;
                            if(distances.TryGetValue(routeNode.LocationId.Format(endLoadPlace.LocationId), out distance)
                                || distances.TryGetValue(endLoadPlace.LocationId.Format(routeNode.LocationId), out distance))
                            {
                                endTravelCost = (decimal)(vehicle.MaintenanceCostPerKm * distance) +
                                    (decimal)((distance / vehicle.FuelConsumptionKmPerLiter) * startLoadPlace.FuelCost);

                                totalIncome -= endTravelCost;
                            }

                        }
                        else
                        {
                            decimal travelCostBetweenNodes = 0;
                            double distance = 0;
                            distances.TryGetValue(locationIdLastRouteNode.Value.Format(routeNode.LocationId), out distance);

                            travelCostBetweenNodes = (decimal)(vehicle.MaintenanceCostPerKm * distance) +
                                (decimal)((distance / vehicle.FuelConsumptionKmPerLiter) * startLoadPlace.FuelCost);

                            totalIncome -= travelCostBetweenNodes;
                        }
                        locationIdLastRouteNode = routeNode.LocationId;
                        i++;
                    }
                }
            }

            Console.WriteLine("BEHROUZ-ALIREZA HEURISTIC");
            Console.WriteLine("{0,15} {1,15} {2,15} {3,15} {4,15} {5,15}",
                 "Total V.",
                 "Qtd. V. At.",
                 "Betoneiras",
                 "V. Nao Atendidas",
                 "Tempo",
                 "Lucro");
            Console.WriteLine("{0,15} {1,15} {2,15} {3,15} {4,15} {5,15}",
                deliveryOrdersTrips.Count,
                routes.Count, 
                routes.GroupBy(r => r.VehicleId).Count().ToString(),
                deliveryOrdersTrips.Count - deliveriesSatisfiedCount,
                stopwatch.Elapsed.TotalSeconds,
                totalIncome);
        }

        public BehrouzAlireza(IDeliveryOrderRepository _deliveryOrderRepository, 
            ILoadPlacesRepository _loadPlacesRepository) : 
            base(_deliveryOrderRepository, _loadPlacesRepository)
        {

        }
    }
}
