using Business.Base;
using Contracts.Entities.Helpers;
using Contracts.Entities.Instances;
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

        private void AssignimentOfVehicleTypes()
        {

        }

        public BehrouzAlireza(IDeliveryOrderRepository _deliveryOrderRepository, 
            ILoadPlacesRepository _loadPlacesRepository) : base(_deliveryOrderRepository, _loadPlacesRepository)
        {
            deliveryOrderRepository = _deliveryOrderRepository;
            loadPlacesRepository = _loadPlacesRepository;
        }

        private IDeliveryOrderRepository deliveryOrderRepository;
        private ILoadPlacesRepository loadPlacesRepository;
    }
}
