using System;
using System.Threading.Tasks;
using Interface.Client;
using Serilog;

namespace WHO
{
    public class EmptyHealthOrganisation : IHealthOrganisation
    {
        private const int _statusPingDelayInMs = 100;

        private readonly IClient _client;
        private bool _running = true;

        public EmptyHealthOrganisation(string uri) : this(new WebSocket(uri))
        {
        }

        public EmptyHealthOrganisation(IClient client)
        {
            this._client = client;
            this._client.Closed += this.Stop;
        }

        public async Task Run()
        {
            try
            {
                while (this._running)
                {
                    Log.Information("Waiting for our turn...");
                    await this.WaitForOurTurn();
                    await this._client.EndTurn();
                }
            }
            catch (TaskCanceledException) { }
        }

        public async ValueTask DisposeAsync()
        {
            await this._client.DisposeAsync();
        }

        private async Task WaitForOurTurn()
        {
            // Request the status every _statusPingDelayInMs milliseconds until it is the who turn and return the budget
            do
            {
                await Task.Delay(_statusPingDelayInMs);

                if (!this._running)
                {
                    throw new TaskCanceledException();
                }

                var status = await this._client.GetStatus();

                if (status.Budget == -1)
                {
                    if (status.IsWhoTurn)
                    {
                        await this._client.EndTurn();
                    }

                    this.Stop();
                    throw new TaskCanceledException();
                }

                if (status.IsWhoTurn)
                {
                    return;
                }
            }
            while (true);
        }

        private void Stop()
        {
            this._running = false;
        }
    }
}
