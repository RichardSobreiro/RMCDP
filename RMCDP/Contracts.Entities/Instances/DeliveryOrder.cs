using System;
using System.Collections.Generic;

namespace Contracts.Entities.Instances
{
    public class DeliveryOrder
    {
        public int DeliveryOrderId { get; set; }
        public int InstanceNumber { get; set; }
        public DateTime RequestedInitialDischargeTime { get; set; }
        public double VolumeTotal { get; set; }
        public int Interval { get; set; }
        public Location Construction { get; set; }
        public List<DeliveryOrderTrip> Trips { get; set; }
    }
}
