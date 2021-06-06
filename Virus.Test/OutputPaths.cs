using System;
namespace Virus.Test
{
    public class OutputPaths
    {
        public const string Base = "../../../..";
        private const string _worldPath = Base + "/WorldFiles/{0}.json";
        private const string _outputDir = Base + "/tmp/{0}";
        private const string _outputDump = _outputDir + "/snapshots.json";
        private const string _outputDumpNode = _outputDir + "/per_node_snapshots.json";
        private const string _outputCsv = _outputDir + "/agg.csv";
        private const string _outputCsvNode = _outputDir + "/node_{1}.csv";

        private string _csvNodeRaw;

        public string World { get; }
        public string Dir { get; }
        public string Dump { get; }
        public string DumpNode { get; }
        public string Csv { get; }

        public OutputPaths(string world)
        {
            this.World = _worldPath.Format(world);
            this.Dir = _outputDir.Format(world);
            this.Dump = _outputDump.Format(world);
            this.DumpNode = _outputDumpNode.Format(world);
            this.Csv = _outputCsv.Format(world);
            this._csvNodeRaw = _outputCsvNode.Format(world, "{0}");
        }

        public string CsvNode(int i) => _csvNodeRaw.Format(i);
    }
}
