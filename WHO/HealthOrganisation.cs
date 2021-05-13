using System;
using System.Threading.Tasks;
using Serilog;

namespace WHO
{
    class HealthOrganisation : IAsyncDisposable
    {
        private readonly Interface.Client _client;

        public HealthOrganisation(string uri)
        {
            this._client = new(uri);
        }

        public async ValueTask DisposeAsync()
        {
            await this._client.DisposeAsync();
        }
    }
}
