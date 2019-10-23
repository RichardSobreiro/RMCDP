using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts.Entities.Instances
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int LoadPlaceId { get; set; }
        public decimal MaintenanceCostPerKm { get; set; }
        public decimal FuelConsumptionPerKm { get; set; }
        public bool Available { get; set; }

        public DateTime? GetEndOfLastTrip()
        {
            return EndOfLastTrip.Last();
        }
        public void AddEndOfLastTrip(DateTime value)
        {
            EndOfLastTrip.Add(value);
        }
        List<DateTime> EndOfLastTrip { get; set; }

        public DateTime? GetBeginOfLastTrip()
        {
            return BeginOfLastTrip.Last();
        }
        public void AddBeginOfLastTrip(DateTime value)
        {
            BeginOfLastTrip.Add(value);
        }
        List<DateTime> BeginOfLastTrip { get; set; }

        public int? GetLoadPlaceIdOfLastTrip()
        {
            return LoadPlaceIdOfLastTrip.Last();
        }
        public void AddLoadPlaceIdOfLastTrip(int value)
        {
            LoadPlaceIdOfLastTrip.Add(value);
        }
        List<int> LoadPlaceIdOfLastTrip { get; set; }
    }
}
