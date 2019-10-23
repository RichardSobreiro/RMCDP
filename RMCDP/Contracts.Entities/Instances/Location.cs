using GeoCoordinatePortable;
using System.Collections.Generic;

namespace Contracts.Entities.Instances
{
    public class Location
    {
        public int LocationId { get; set; }
        public short Kind { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? ReferenceNumber { get; set; }
        public int InstanceNumber { get; set; }
        public int RateRMCProduction { get; set; } = 1;

        public List<Vehicle> Vehicles { get; set; }
        public GeoCoordinate GeoCordinates { get { return new GeoCoordinate(Latitude, Longitude); } }
    }
}
