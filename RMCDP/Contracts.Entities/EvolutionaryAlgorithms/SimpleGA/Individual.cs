namespace Contracts.Entities.EvolutionaryAlgorithms.SimpleGA
{
    public class Individual
    {
        public decimal Fitness { get; set; }
        public Chromosome Chromosome { get; set; }
    }
}
