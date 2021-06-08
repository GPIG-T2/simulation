using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Serilog;
using Virus.Serialization;
using Virus.Test.Utils;
using Xunit;

namespace Virus.Test
{
    public class TestWorld : IDisposable
    {
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

        public static IEnumerable<object[]> Worlds()
        {
            yield return new[] { new OutputPaths(Uk) };
            yield return new[] { new OutputPaths(Europe) };
            yield return new[] { new OutputPaths(Earth) };
        }

        [Theory]
        [MemberData(nameof(Worlds))]
        public async Task GenerateWorldData(OutputPaths paths)
        {
            var data = Program.LoadWorld(new[] { paths.World });
            var world = (World)data!;

            world.StartInfection();
            foreach (var _ in Enumerable.Range(0, DaysCount))
            {
                world.Update();
            }

            Directory.CreateDirectory(paths.Dir);

            await Task.WhenAll(
                File.WriteAllTextAsync(paths.Dump, Json.Serialize(world.Tracking.Snapshots)),
                File.WriteAllLinesAsync(paths.Csv, world.Tracking.AggregateCsv),
                Task.WhenAll(world.Tracking.NodeCsvs.Select((csv, i) => File.WriteAllLinesAsync(paths.CsvNode(i), csv))),
                File.WriteAllTextAsync(paths.DumpNode, Json.Serialize(world.Tracking.Nodes))
            );

            Console.WriteLine($"Generated all data for {paths.World}");
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
