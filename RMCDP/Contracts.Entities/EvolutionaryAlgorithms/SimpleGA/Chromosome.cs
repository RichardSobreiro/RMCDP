using System.Collections.Generic;

namespace Contracts.Entities.EvolutionaryAlgorithms.SimpleGA
{
    public class Chromosome
    {
        public List<Gene> Genes { get; set; }

        public Chromosome()
        {
            Genes = new List<Gene>();
        }
    }
}
