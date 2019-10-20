using System;
using System.Collections.Generic;

namespace Contracts.Entities.Results
{
    public class VehiclePath
    {
        public int VehicleId { get; set; }
        public decimal MaintenanceCostPerKm { get; set; }
        public decimal FuelConsumptionPerKm { get; set; }
        public List<Place> PlacesSequence { get; set; }
        public List<DateTime> EndOfServiceTimes { get; set; }
    }
}
