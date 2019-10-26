using Contracts.Entities.Instances;
using System;
using System.Collections.Generic;

namespace Business.Extensions
{
    public static class LoadPlaceExtensions
    {
        public static int? GetFirstVehicleAvailebleBeforeTime(this Location loadPlace,
            DateTime requestedInitialLoadTime)
        {
            int? vehicleId = null;
            TimeSpan? waitTime = TimeSpan.MaxValue;
            int index = 0;
            foreach (KeyValuePair<int, Vehicle> vehicle in loadPlace.Vehicles)
            {
                DateTime? currentVehicleAvailableTime = vehicle.Value.GetEndOfLastTrip();
                if ((!currentVehicleAvailableTime.HasValue && !vehicleId.HasValue) ||
                    (currentVehicleAvailableTime.HasValue &&
                    currentVehicleAvailableTime <= requestedInitialLoadTime &&
                    (currentVehicleAvailableTime - requestedInitialLoadTime) < waitTime))
                {
                    vehicleId = vehicle.Value.VehicleId;
                    waitTime = currentVehicleAvailableTime.HasValue ? 
                        currentVehicleAvailableTime.Value - requestedInitialLoadTime : TimeSpan.MinValue;
                }
                index++;
            }
            return vehicleId;
        }
    }
}
