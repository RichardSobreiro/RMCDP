using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Interfaces.Business
{
    public interface IBestLoadPlaceFit
    {
        void Execute(int instanceNumber, DateTime begin, DateTime end);

        List<Trip> BestFitConstructionHeuristic(StringBuilder log,
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, Dictionary<string, double> distances,
            int instanceNumber, out decimal TotalIncome);
    }
}
