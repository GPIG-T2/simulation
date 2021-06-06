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
        public const string OutputDump = OutputDir + "/uk.csv";
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
                    ts.Aggregate(new InfectionTotals(new(), 0, 0, 0, 0, 0, 0, 0), (p, c) => p.Add(c))))
                .Select((z) =>
                {
                    var (n, t) = z;
                    return string.Format(
                        "{0},{1},{2},{3},{4},{5},{6},{7}", n, t.Uninfected,
                        t.AsymptomaticInfectedNotInfectious, t.AsymptomaticInfectedInfectious,
                        t.Symptomatic, t.SeriousInfection, t.Dead, t.RecoveredImmune
                    );
                });
            await File.WriteAllLinesAsync(OutputDump, lines);
        }
    }
}
