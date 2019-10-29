using Business.Base;
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

            List<RouteNode> routeNodes = AssignimentOfVehicleTypes(instanceNumber, loadPlaces, sequenceOfCustomers);

            List<Route> routes = ConstructionOfRoutes(instanceNumber, routeNodes, sequenceOfCustomers);

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
            Dictionary<int, Location> loadPlaces, 
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
                        VehicleType = 1,
                        Volume = deliveryOrderTrip.Volume
                    }
                );
            }

            return new List<RouteNode>();
        }

        private List<Route> ConstructionOfRoutes(int instanceNumber,
            List<RouteNode> routeNodes,
            Queue<DeliveryOrderTrip> sequenceOfCustomers)
        {
            List<Route> routes = new List<Route>();
            List<int> deliveryOrderTripsInRoutes = new List<int>();

            foreach (DeliveryOrderTrip deliveryOrderTrip in sequenceOfCustomers)
            {
                if(!deliveryOrderTripsInRoutes.Any(d => d == deliveryOrderTrip.DeliveryOrderTripId))
                {
                    RouteNode routeNode = routeNodes.FirstOrDefault(rn => 
                        rn.DeliveryOrderTripId == deliveryOrderTrip.DeliveryOrderTripId);


                }
            }

            return routes;
        }

        public BehrouzAlireza(IDeliveryOrderRepository _deliveryOrderRepository, 
            ILoadPlacesRepository _loadPlacesRepository) : base(_deliveryOrderRepository, _loadPlacesRepository)
        {

        }
    }
}
