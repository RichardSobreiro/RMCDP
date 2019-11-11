using System.Collections.Generic;

namespace Contracts.Entities.EvolutionaryAlgorithms.MaghrebiWallerSammut
{
    public class Chromosome
    {
        public List<LoadPlaceGene> LoadPlaceGenes { get; set; }
        public List<VehicleGene> VehicleGenes { get; set; }
    }
}
