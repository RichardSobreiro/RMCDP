﻿using System;

namespace Contracts.Entities
{
    public class DeliveryOrderTrip
    {
        public int DeliveryOrderTripId { get; set; }
        public int InstanceNumber { get; set; }
        public int DeliveryOrderId { get; set; }
        public int ClientId { get; set; }
        public int ConstructionId { get; set; }
        public int ReadyMixedConcreteId { get; set; }
        public DateTime RequestedTime { get; set; }
        public int Interval { get; set; }
        public int DischargeDuration { get; set; }
        public double Volume { get; set; }
    }
}