using System;

namespace Contracts.Entities.Results
{
    public class Trip
    {
        public int TripId { get; set; }
        public int InstanceNumber { get; set; }
        public int LoadPlaceId { get; set; }
        public int VehicleId { get; set; }
        public DateTime DesiredRequestedTime { get; set; }
        public decimal Income { get; set; }

        public decimal Cost { get; set; }
        public DateTime InitialLoadTime { get; set; }
        public DateTime FinalLoadTime { get; set; }
        public DateTime WaitTimeAtLoadPlace { get; set; }
        public DateTime DepartureTimeFromLoadPlace { get; set; }
        public DateTime ArrivalTimeAtConstruction { get; set; }
        public DateTime WaitTimeAfterArrivalAtConstruction { get; set; }
        public DateTime InitialUnloadTimeAtConstruction { get; set; }
        public DateTime FinalUnloadTimeAtConstruction { get; set; }
        public DateTime WaitTimeAfterUnloadAtConstruction { get; set; }
        public DateTime DepartureTimeFromConstruction { get; set; }
        public DateTime ArrivalTimeAtLoadPlace { get; set; }
    }
}
