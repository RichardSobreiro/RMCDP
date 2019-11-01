using Contracts.Entities.Instances;
using System;

namespace Business.Extensions
{
    public static class DeliveryOrderTripExtension
    {
        public static DateTime GetBestInitialLoadTime(this DeliveryOrderTrip deliveryOrderTrip,
            Location loadPlace, TimeSpan travelTime)
        {
            double loadDuration = loadPlace.RateRMCProduction * deliveryOrderTrip.Volume;
            return deliveryOrderTrip.RequestedTime.
                Subtract(travelTime).
                Subtract(TimeSpan.FromMinutes(5)).
                Subtract(TimeSpan.FromMinutes(loadDuration));
        }
    }
}
