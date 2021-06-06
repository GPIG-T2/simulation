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

            List<List<InfectionTotals>> totals = new(DaysCount);
            List<int> nodesInfected = new(DaysCount);

            totals.Add(world.Snapshot());
            nodesInfected.Add(0);

            world.StartInfection();

            foreach (var _ in Enumerable.Range(0, DaysCount))
            {
                world.Update();

                var snapshot = world.Snapshot();
                totals.Add(snapshot);
                nodesInfected.Add(snapshot.Where(t => t.IsInfected).Count());
            }

            Directory.CreateDirectory(paths.Dir);
            await File.WriteAllTextAsync(paths.Dump, Json.Serialize(totals));

            var lines = nodesInfected
                .Zip(totals.Select(ts =>
                    ts.Aggregate(InfectionTotals.Empty(), (p, c) => p.Add(c))))
                .Select((z) => $"{z.First},{z.Second.ToCsvLine()}");
            await File.WriteAllLinesAsync(paths.Csv, lines);

            List<List<InfectionTotals>> nodes = totals[0].Select(_ => new List<InfectionTotals>()).ToList();
            foreach (var ts in totals)
            {
                foreach (var (t, i) in ts.Select((t, i) => (t, i)))
                {
                    nodes[i].Add(t);
                }
            }

            await Task.WhenAll(nodes.Select((ts, i) => File.WriteAllLinesAsync(paths.CsvNode(i), ts.Select(t => t.ToCsvLine()))));
            await File.WriteAllTextAsync(paths.DumpNode, Json.Serialize(nodes));

            Console.WriteLine($"Generated all data for {paths.World}");
        }
    }
}
