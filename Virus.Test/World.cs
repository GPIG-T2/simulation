﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Serilog;
using Virus.Serialization;
using Xunit;

namespace Virus.Test
{
    public class TestWorld : IDisposable
    {
        public const string BaseDir = "../../../..";
        public const string DataDir = BaseDir + "/tmp/{0}";
        public const string WorldJson = BaseDir + "/WorldFiles/{0}.json";

        public const string Uk = "UK";
        public const string Europe = "Europe";
        public const string Earth = "Earth";

        public const int DaysCount = 400;

        private readonly World _world;

        public TestWorld()
        {
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

            this._world = (World)new WorldData(
                new()
                {
                    new(100000, new Demographics(10, 10, 10, 10, 10, 10, 10, 10, 10), "left", new Coordinate(1, 2), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), 0, 0),
                    new(100000, new Demographics(10, 10, 10, 10, 10, 10, 10, 10, 10), "right", new Coordinate(1, 2), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), 0, 0)
                },
                new()
                {
                    new("test", 0, 1, 50000, 10, 5)
                },
                new(new Demographics(1, 1, 1, 1, 1, 1, 1, 1, 1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1)),
                SimulationSettings.SelectedMap.Country
            );
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
        }

        [Theory]
        [InlineData(Uk)]
        [InlineData(Europe)]
        [InlineData(Earth)]
        public async Task GenerateWorldData(string worldKind)
        {
            var paths = new DataPaths(string.Format(DataDir, worldKind));
            var worldJson = string.Format(WorldJson, worldKind);

            var data = Program.LoadWorld(new[] { worldJson });
            var world = (World)data!;

            world.StartInfection();
            foreach (var _ in Enumerable.Range(0, DaysCount))
            {
                world.Update();
            }

            await world.Tracking.Dump(paths);

            Console.WriteLine($"Generated all data for {worldJson}");
        }

        [Theory]
        [InlineData("N0")] // Test that nothing leaves
        [InlineData("N1")] // Test that nothing enters
        public void TestCloseBorders(string node)
        {
            this._world.Nodes[0].Infect(10);
            this._world.CloseBorders(new(new() { node }));

            Assert.Equal(10, this._world.Nodes[0].Totals.AsymptomaticInfectedNotInfectious);
            Assert.Equal(0, this._world.Nodes[1].Totals.AsymptomaticInfectedNotInfectious);

            // Skip forward a fortnight
            for (int i = 0; i < 14; i++)
            {
                this._world.Update();
            }

            Assert.Equal(0, this._world.Nodes[1].Totals.AsymptomaticInfectedNotInfectious);
            Assert.Equal(0, this._world.Nodes[1].Totals.AsymptomaticInfectedInfectious);
            Assert.Equal(0, this._world.Nodes[1].Totals.Symptomatic);
            Assert.Equal(0, this._world.Nodes[1].Totals.SeriousInfection);
            Assert.Equal(0, this._world.Nodes[1].Totals.Dead);
            Assert.Equal(0, this._world.Nodes[1].Totals.RecoveredImmune);
        }
    }
}
