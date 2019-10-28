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
        public double Volume { get; set; } = 8;

        List<DateTime?> EndOfLastTrip { get; set; }
        List<DateTime?> BeginOfLastTrip { get; set; }
        List<int?> LoadPlaceIdOfLastTrip { get; set; }
        public DateTime? GetEndOfLastTrip()
        {
            return EndOfLastTrip.Any() ? EndOfLastTrip?.Last() : null;
        }
        public void AddEndOfLastTrip(DateTime value)
        {
            EndOfLastTrip?.Add(value);
        }
        public DateTime? GetBeginOfLastTrip()
        {
            return BeginOfLastTrip.Any() ? BeginOfLastTrip?.Last() : null;
        }
        public void AddBeginOfLastTrip(DateTime value)
        {
            BeginOfLastTrip?.Add(value);
        }
        public int? GetLoadPlaceIdOfLastTrip()
        {
            return LoadPlaceIdOfLastTrip.Any() ? LoadPlaceIdOfLastTrip?.Last() : null;
        }
        public void AddLoadPlaceIdOfLastTrip(int value)
        {
            LoadPlaceIdOfLastTrip?.Add(value);
        }

        public Vehicle()
        {
            EndOfLastTrip = new List<DateTime?>();
            BeginOfLastTrip = new List<DateTime?>();
            LoadPlaceIdOfLastTrip = new List<int?>();
        }
    }
}
