using Contracts.Entities.Instances;
using Contracts.Interfaces.Repository.Instances;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Text;

namespace Repository.Instances
{
    public class LoadPlacesRepository : ILoadPlacesRepository
    {
        public Dictionary<int, Location> GetLoadPlacesWithVehicles(int instanceNumber)
        {
            StringBuilder query = new StringBuilder("");
            query.Append("SELECT ");
            query.Append("lc.\"LocationId\", ");
            query.Append("lc.\"InstanceNumber\", ");
            query.Append("lc.\"Latitude\", ");
            query.Append("lc.\"Longitude\", ");
            query.Append("lc.\"ReferenceNumber\", ");
            query.Append("v.\"VehicleId\", ");
            query.Append("v.\"MaintenanceCostPerKm\", ");
            query.Append("v.\"FuelConsumptionPerKm\", ");
            query.Append("FROM ");
            query.Append("public.\"Location\" AS lc");
            query.Append("INNER JOIN public.\"Vehicle\" AS v ON v.\"LocationId\" = lc.\"LocationId\"");
            query.Append("WHERE lp.\"InstanceNumber\" = @InstanceNumber AND lc.\"Kind\" = 1");

            Dictionary<int, Location> deliveryOrdersDictionary = new Dictionary<int, Location>();
            using (NpgsqlConnection connection = new NpgsqlConnection(
                _configuration.GetValue<string>("ConnectionStrings:READYMIXEDCONCRETEDELIVERYPROBLEMDB")))
            {
                connection.Query<Location, Vehicle, Location>(
                    query.ToString(),
                    (lc, v) =>
                    {
                        Location loadPlace;
                        if (!deliveryOrdersDictionary.TryGetValue(lc.LocationId, out loadPlace))
                        {
                            loadPlace = lc;
                            loadPlace.Vehicles = new List<Vehicle>();
                        }

                        loadPlace.Vehicles.Add(v);

                        return lc;
                    },
                    param: new { InstanceNumber = instanceNumber },
                    splitOn: "VehicleId");
                return deliveryOrdersDictionary;
            }
        }

        public LoadPlacesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConfiguration _configuration;
    }
}
