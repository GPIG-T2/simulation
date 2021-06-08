using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace Virus
{
    public class Tracking
    {
        /// <summary>
        /// An enumerable of snapshots that have been taken.
        /// </summary>
        /// <remarks>
        /// History[Nodes[InfectionTotals]]
        /// </remarks>
        public IEnumerable<IEnumerable<InfectionTotals>> Snapshots => this._snapshots;
        public IEnumerable<int> NodesInfected => this.Snapshots.Select(s => s.Where(t => t.IsInfected).Count());
        /// <summary>
        /// An enumerable of per-node infection total histories.
        /// </summary>
        /// <remarks>
        /// Nodes[History[InfectionTotals]]
        /// </remarks>
        public IEnumerable<IEnumerable<InfectionTotals>> Nodes => this._nodes;

        /// <summary>
        /// When a new snapshot is created, this event is raised with the snapshot
        /// as the argument.
        /// </summary>
        public event Action<IEnumerable<InfectionTotals>>? OnSnapshot;

        // Data output properties.
        public IEnumerable<string> AggregateCsv => this.NodesInfected
                .Zip(this.Snapshots.Select(ts =>
                    ts.Aggregate(InfectionTotals.Empty(), (p, c) => p.Add(c))))
                .Select((z) => $"{z.First},{z.Second.ToCsvLine()}");
        public IEnumerable<IEnumerable<string>> NodeCsvs => this.Nodes.Select(ts => ts.Select(t => t.ToCsvLine()));

        private readonly World _world;
        private readonly List<List<InfectionTotals>> _snapshots = new();
        private readonly List<List<InfectionTotals>> _nodes;

        public Tracking(World world)
        {
            this._world = world;
            this._nodes = world.Nodes.Select(_ => new List<InfectionTotals>()).ToList();
        }

        public void Snapshot()
        {
            var snapshot = this._world.Nodes.Select(n => n.Totals.Clone()).ToList();
            this._snapshots.Add(snapshot);

            // We do this tracking here since converting from the list of
            // snapshots into the list of node histories is weird and annoying.
            foreach (var (ns, i) in snapshot.Select((ns, i) => (ns, i)))
            {
                this._nodes[i].Add(ns);
            }

            this.OnSnapshot?.Invoke(snapshot);
        }

        public async Task Dump(DataPaths paths)
        {
            Directory.CreateDirectory(paths.Dir);

            await Task.WhenAll(
                File.WriteAllTextAsync(paths.Dump, Json.Serialize(this.Snapshots)),
                File.WriteAllLinesAsync(paths.Csv, this.AggregateCsv),
                Task.WhenAll(this.NodeCsvs.Select((csv, i) => File.WriteAllLinesAsync(paths.CsvNode(i), csv))),
                File.WriteAllTextAsync(paths.DumpNode, Json.Serialize(this.Nodes))
            );
        }
    }
}
