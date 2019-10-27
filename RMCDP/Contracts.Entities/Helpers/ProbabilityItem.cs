namespace Contracts.Entities.Helpers
{
    public class ProbabilityItem<T>
    {
        public double Probability { get; set; }
        public T Item { get; set; }
    }
}
