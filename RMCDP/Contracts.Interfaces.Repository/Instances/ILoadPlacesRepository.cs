using Contracts.Entities.Instances;
using System.Collections.Generic;

namespace Contracts.Interfaces.Repository.Instances
{
    public interface ILoadPlacesRepository
    {
        List<Location> GetLoadPlacesWithVehicles(int instanceNumber);
    }
}
