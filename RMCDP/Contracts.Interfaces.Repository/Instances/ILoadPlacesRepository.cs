using Contracts.Entities.Instances;
using System.Collections.Generic;

namespace Contracts.Interfaces.Repository.Instances
{
    public interface ILoadPlacesRepository
    {
        Dictionary<int, Location> GetLoadPlacesWithVehicles(int instanceNumber);
    }
}
