using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Serilog;
using Xunit;

namespace Virus.Test
{
    public class TestWorld : IClassFixture<Runner>, IDisposable
    {
        public const string Uk = "UK";
        public const string Europe = "Europe";
        public const string Earth = "Earth";

        public const int DaysCount = 400;

        public TestWorld()
        {
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
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
        public async Task TestUncontrolledWorld(OutputPaths paths)
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
    }
}
