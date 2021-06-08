using System;
using System.Threading.Tasks;

namespace WHO
{
    public interface IHealthOrganisation : IAsyncDisposable
    {
        Task Run();
    }
}
