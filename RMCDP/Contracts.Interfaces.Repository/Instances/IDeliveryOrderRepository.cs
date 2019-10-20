using Contracts.Entities.Instances;
using System;
using System.Collections.Generic;

namespace Contracts.Interfaces.Repository.Instances
{
    public interface IDeliveryOrderRepository
    {
        List<DeliveryOrderTrip> GetDeliveriesOrdersWithDeliveryOrderTrips(int instanceNumber, DateTime begin, DateTime end);
    }
}
