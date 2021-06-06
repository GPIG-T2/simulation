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
    public class TestWorld : IDisposable
    {
        public const string Base = "../../../..";
        public const string WorldUkPath = Base + "/WorldFiles/UK.json";
        public const string OutputDir = Base + "/tmp";
        public const string OutputDump = OutputDir + "/uk.json";
        public const string OutputDumpNode = OutputDir + "/uk_nodes.json";
        public const string OutputCsv = OutputDir + "/uk.csv";
        public const string OutputCsvNode = OutputDir + "/uk_node_{0}.csv";
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

        [Fact]
        public async Task TestUncontrolledWorld()
        {
            var data = Program.LoadWorld(new[] { WorldUkPath });
            var world = (World)data;

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

            Directory.CreateDirectory(OutputDir);
            await File.WriteAllTextAsync(OutputDump, Json.Serialize(totals));

            var lines = nodesInfected
                .Zip(totals.Select(ts =>
                    ts.Aggregate(InfectionTotals.Empty(), (p, c) => p.Add(c))))
                .Select((z) => $"{z.First},{z.Second.ToCsvLine()}");
            await File.WriteAllLinesAsync(OutputCsv, lines);

            List<List<InfectionTotals>> nodes = totals[0].Select(_ => new List<InfectionTotals>()).ToList();
            foreach (var ts in totals)
            {
                foreach (var (t, i) in ts.Select((t, i) => (t, i)))
                {
                    nodes[i].Add(t);
                }
            }

            await Task.WhenAll(nodes.Select((ts, i) => File.WriteAllLinesAsync(string.Format(OutputCsvNode, i), ts.Select(t => t.ToCsvLine()))));
            await File.WriteAllTextAsync(OutputDumpNode, Json.Serialize(nodes));
        }
    }
}
