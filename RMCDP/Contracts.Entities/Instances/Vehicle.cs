namespace Contracts.Entities.Instances
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int LoadPlaceId { get; set; }
        public decimal MaintenanceCostPerKm { get; set; }
        public decimal FuelConsumptionPerKm { get; set; }
        public bool Available { get; set; }
    }
}
