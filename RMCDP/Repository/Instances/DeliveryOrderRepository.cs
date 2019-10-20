using Contracts.Entities.Instances;
using Contracts.Interfaces.Repository.Instances;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Instances
{
    public class DeliveryOrderRepository : IDeliveryOrderRepository
    {
        public List<DeliveryOrderTrip> GetDeliveriesOrdersWithDeliveryOrderTrips(int instanceNumber, DateTime begin, DateTime end)
        {
            StringBuilder query = new StringBuilder("");
            query.Append("SELECT ");
            query.Append("dlot.\"DeliveryOrderId\", ");
            query.Append("dlot.\"DeliveryOrderTripId\", ");
            query.Append("dlot.\"ReadyMixedConcreteId\", ");
            query.Append("dlot.\"RequestedTime\", ");
            query.Append("dlot.\"Interval\", ");
            query.Append("dlot.\"DischargeDuration\", ");
            query.Append("dlot.\"Volume\", ");
            query.Append("dlot.\"ClientId\", ");
            query.Append("dlot.\"Income\", ");
            query.Append("dlot.\"Cost\", ");
            query.Append("const.\"LocationId\", ");
            query.Append("const.\"InstanceNumber\", ");
            query.Append("const.\"Latitude\", ");
            query.Append("const.\"Longitude\" ");
            query.Append("FROM ");
            query.Append("public.\"DeliveryOrderTrip\" AS dlot");
            query.Append("INNER JOIN public.\"Location\" AS const ON const.\"LocationId\" = dlot.\"LocationId\" AND const.Kind = 2");
            query.Append("WHERE dlo.\"InstanceNumber\" = @InstanceNumber");
            query.Append("AND dlo.\"RequestedTime\" >= @Begin");
            query.Append("AND dlo.\"RequestedTime\" <= @End");

            Dictionary<int, DeliveryOrderTrip> deliveryOrdersDictionary = new Dictionary<int, DeliveryOrderTrip>();
            using (NpgsqlConnection connection = new NpgsqlConnection(
                _configuration.GetValue<string>("ConnectionStrings:READYMIXEDCONCRETEDELIVERYPROBLEMDB")))
            {
                connection.Query<DeliveryOrderTrip, Location, DeliveryOrderTrip>(
                    query.ToString(),
                    (dlot, l) =>
                    {
                        DeliveryOrderTrip deliveryOrderTrip;
                        if (!deliveryOrdersDictionary.TryGetValue(dlot.DeliveryOrderTripId, out deliveryOrderTrip))
                        {
                            deliveryOrderTrip = dlot;
                            deliveryOrderTrip.Construction = l;
                            deliveryOrdersDictionary.Add(deliveryOrderTrip.DeliveryOrderTripId, deliveryOrderTrip);
                        }

                        return dlot;
                    },
                    param: new { InstanceNumber = instanceNumber, Begin = begin, End = end },
                    splitOn: "LocationId").AsList();

                return deliveryOrdersDictionary.Values.AsList();
            }
        }

        public DeliveryOrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConfiguration _configuration;
    }
}
