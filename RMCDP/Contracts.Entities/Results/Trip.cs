using System;

namespace Contracts.Entities.Results
{
    public class Trip
    {
        public int TripId { get; set; }
        public int InstanceNumber { get; set; }
        public int LocationIdLoadPlace { get; set; }
        public int LocationIdConstruction { get; set; }
        public int VehicleId { get; set; }
        public DateTime DesiredRequestedTime { get; set; }
        public decimal Income { get; set; }
        public double Volume { get; set; }

        public decimal Cost { get; set; }
        public DateTime InitialLoadTime { get; set; }
        public DateTime FinalLoadTime { get; set; }
        public DateTime DepartureTimeFromLoadPlace { get; set; }
        public DateTime ArrivalTimeAtConstruction { get; set; }
        public DateTime InitialUnloadTimeAtConstruction { get; set; }
        public DateTime FinalUnloadTimeAtConstruction { get; set; }
        public DateTime DepartureTimeFromConstruction { get; set; }
        public DateTime ArrivalTimeAtLoadPlace { get; set; }

        public TimeSpan WaitTimeAtLoadPlace { get; set; }
        public TimeSpan WaitTimeAfterArrivalAtConstruction { get; set; }
        public TimeSpan WaitTimeAfterUnloadAtConstruction { get; set; }
    }
}
