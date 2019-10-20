using System;

namespace Contracts.Entities.Results
{
    public class Place
    {
        public int PlaceId { get; set; }
        public int VehicleId { get; set; }
        public int? ConstructionId { get; set; }
        public int? LoadPlaceId { get; set; }
        public DateTime InitialServiceTime { get; set; }
        public DateTime FinalServiceTime { get; set; }
    }
}
