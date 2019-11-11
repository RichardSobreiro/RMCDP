using System;
using System.Collections.Generic;

namespace Contracts.Entities.EvolutionaryAlgorithms.MaghrebiWallerSammut
{
    public class VehicleGene
    {
        public List<DateTime> BeginLastTrip { get; set; }
        public List<DateTime> EndLastTrip { get; set; }
        public int VehicleId { get; set; }
        public int LoadPlaceId { get; set; }
        public VehicleGene()
        {
            BeginLastTrip = new List<DateTime>();
            EndLastTrip = new List<DateTime>();
        }
    }
}
