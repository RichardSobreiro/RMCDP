using System;

namespace Contracts.Interfaces.Business
{
    public interface IBestLoadPlaceFit
    {
        void Execute(int instanceNumber, DateTime begin, DateTime end);
    }
}
