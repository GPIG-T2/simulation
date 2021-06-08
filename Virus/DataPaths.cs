using System;
using System.IO;

namespace Virus
{
    public class DataPaths
    {
        private const string _outputDump = "snapshots.json";
        private const string _outputDumpNode = "per_node_snapshots.json";
        private const string _outputCsv = "agg.csv";
        private const string _outputCsvNode = "node_{0}.csv";

        public string Dir { get; }
        public string Dump => Path.Join(this.Dir, _outputDump);
        public string DumpNode => Path.Join(this.Dir, _outputDumpNode);
        public string Csv => Path.Join(this.Dir, _outputCsv);

        public DataPaths(string dir)
        {
            this.Dir = dir;
        }

        public string CsvNode(int i) => Path.Join(this.Dir, string.Format(_outputCsvNode, i));
    }
}
