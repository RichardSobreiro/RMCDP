namespace Contracts.Entities.EvolutionaryAlgorithms.MaghrebiWallerSammut
{
    public class Individual
    {
        public decimal Fitness { get; set; }
        public Chromosome Chromosome { get; set; }
        public int FleetSize { get; set; }
    }
}
