using System;

namespace Contracts.Entities.Results
{
    public class RouteNode
    {
        public int RouteNodeId { get; set; }
        public int InstanceNumber { get; set; }
        public int DeliveryOrderTripId { get; set; }

        public decimal? Income { get; set; }
        public double Volume { get; set; }

        public int VehicleType { get; set; }
        public double VehicleTypeVolume { get; set; }

        public decimal? Cost { get; set; }
        
        public DateTime? ArrivalTimeAtConstruction { get; set; }
        public DateTime? InitialUnloadTimeAtConstruction { get; set; }
        public DateTime? FinalUnloadTimeAtConstruction { get; set; }
        public DateTime? DepartureTimeFromConstruction { get; set; }
                       
        public TimeSpan? WaitTimeAtLoadPlace { get; set; }
        public TimeSpan? WaitTimeAfterArrivalAtConstruction { get; set; }
        public TimeSpan? WaitTimeAfterUnloadAtConstruction { get; set; }
    }
}