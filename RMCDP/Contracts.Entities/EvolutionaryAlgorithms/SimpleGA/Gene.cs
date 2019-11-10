using System;

namespace Contracts.Entities.EvolutionaryAlgorithms.SimpleGA
{
    public class Gene
    {
        public int DeliveryOrderTripId { get; set; }
        public int Income { get; set; }
        public int RMCCost { get; set; }
        public int LocationId { get; set; }
        public DateTime RequestedTime { get; set; }
        public double Volume { get; set; }

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public int VehicleId { get; set; }
        public int LoadPlaceId { get; set; }
    }
}
