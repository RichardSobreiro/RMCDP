using Contracts.Entities.Instances;
using System;
using System.Linq;

namespace Business.Extensions
{
    public static class LoadPlaceExtensions
    {
        public static int? GetFirstVehicleAvailebleBeforeTime(this Location loadPlace,
            DateTime requestedInitialLoadTime)
        {
            int? vehicleId = null;
            TimeSpan? waitTime = TimeSpan.MaxValue;
            int? selectedVehicleIndex = 0;
            int index = 0;
            foreach (Vehicle vehicle in loadPlace.Vehicles)
            {
                DateTime? currentVehicleAvailableTime = vehicle.GetEndOfLastTrip();
                if (!(currentVehicleAvailableTime.HasValue) ||
                    (currentVehicleAvailableTime.HasValue &&
                    currentVehicleAvailableTime <= requestedInitialLoadTime &&
                    (currentVehicleAvailableTime - requestedInitialLoadTime) < waitTime))
                {
                    vehicleId = vehicle.VehicleId;
                    waitTime = currentVehicleAvailableTime.HasValue ? 
                        currentVehicleAvailableTime.Value - requestedInitialLoadTime : TimeSpan.MinValue;
                    selectedVehicleIndex = index;
                }
                index++;
            }
            return vehicleId;
        }
    }
}
