﻿using System.Threading.Tasks;
using Serilog;

namespace WHO
{
    class Program
    {

        // TODO:
        //  Create unit testing for the functions using Moq to simulate the server.
        //  Create more functions for creating the various actions
        //  Have the ability to track which actions have been taken, currently there is no way of knowing if a location is in lockdown
        // it would be good to have it track that in the LocationTracker too.
        //  The Location needs to be updated to keep track of the List<string> implementation of them, so that location A1->B3->D1 is
        // used in the function calls as { "A1", "B3", "D1" } instead of "D1" as it is current implemented.

        private const string _uri = "ws://127.0.0.1";

        async static Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                await using HealthOrganisation org = new(_uri);
                await org.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
