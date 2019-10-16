using Contracts.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class DeliveryOrderRepository
    {
        public List<DeliveryOrder> GetDeliveriesOrders(int instanceNumber)
        {
            StringBuilder query = new StringBuilder("");
            query.Append("SELECT ");
            query.Append("dlo.\"InstanceNumber\", ");
            query.Append("dlo.\"RequestedInitialDischargeTime\", ");
            query.Append("dlo.\"VolumeTotal\", ");
            query.Append("dlo.\"Interval\", ");
            query.Append("dlo.\"DeliveryOrderId\", ");
            query.Append("dlot.\"DeliveryOrderTripId\" ");
            query.Append("FROM ");
            query.Append("public.\"DeliveryOrder\" AS dlo");
            query.Append("INNER JOIN public.\"DeliveryOrderTrip\" AS dlot ON dlot.\"DeliveryOrderId\" = dlo.\"DeliveryOrderId\"");
            query.Append("INNER JOIN public.\"Client\" AS client ON client.\"ClientId\" = dlot.\"ClientId\"");
            query.Append("INNER JOIN public.\"Construction\" AS const ON const.\"ConstructionId\" = dlot.\"ConstructionId\"");
            query.Append("INNER JOIN public.\"ReadyMixedConcrete\" AS rmc ON rmc.\"ReadyMixedConcreteId\" = dlot.\"ReadyMixedConcreteId\"");
            query.Append("WHERE dlo.\"InstanceNumber\" = @InstanceNumber");

            using (NpgsqlConnection connection = new NpgsqlConnection(
                _configuration.GetValue<string>("ConnectionStrings:READYMIXEDCONCRETEDELIVERYPROBLEMDB")))
            {
                return connection.Query<DeliveryOrder>(query.ToString(), new { InstanceNumber = instanceNumber }).AsList();
            }
        }

        public DeliveryOrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConfiguration _configuration;
    }
}
