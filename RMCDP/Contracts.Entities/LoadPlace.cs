namespace Contracts.Entities
{
    public class LoadPlace
    {
        public int LoadPlaceId { get; set; }
        public int InstanceNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int ReferenceNumber { get; set; }
    }
}
