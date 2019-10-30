using System;
using System.Collections.Generic;

namespace Contracts.Entities.Results
{
    public class Route
    {
        public int RouteId { get; set; }
        public Queue<RouteNode> RouteNodes { get; set; }

        public int StartLoadPlaceId { get; set; }
        public int EndLoadPlaceId { get; set; }

        public int VehicleId { get; set; }
        public int VehicleType { get; set; }
        public double VehicleTypeVolume { get; set; }

        public double RemainingVolume { get; set; }

        public DateTime? InitialLoadTime { get; set; }
        public DateTime? FinalLoadTime { get; set; }
        public DateTime? DepartureTimeFromLoadPlace { get; set; }
        public DateTime? ArrivalTimeAtLoadPlace { get; set; }

        public TimeSpan? WaitTimeAtLoadPlace { get; set; }
    }
}
