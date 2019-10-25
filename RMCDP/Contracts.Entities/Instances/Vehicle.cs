using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts.Entities.Instances
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int LoadPlaceId { get; set; }
        public double MaintenanceCostPerKm { get; set; } = 0.1d;
        public double FuelConsumptionKmPerLiter { get; set; } = 4d;
        public bool Available { get; set; }

        List<DateTime?> EndOfLastTrip { get; set; }
        List<DateTime?> BeginOfLastTrip { get; set; }
        List<int?> LoadPlaceIdOfLastTrip { get; set; }
        public DateTime? GetEndOfLastTrip()
        {
            return EndOfLastTrip.Last();
        }
        public void AddEndOfLastTrip(DateTime value)
        {
            EndOfLastTrip.Add(value);
        }
        public DateTime? GetBeginOfLastTrip()
        {
            return BeginOfLastTrip.Last();
        }
        public void AddBeginOfLastTrip(DateTime value)
        {
            BeginOfLastTrip.Add(value);
        }
        public int? GetLoadPlaceIdOfLastTrip()
        {
            return LoadPlaceIdOfLastTrip.Last();
        }
        public void AddLoadPlaceIdOfLastTrip(int value)
        {
            LoadPlaceIdOfLastTrip.Add(value);
        }
    }
}
