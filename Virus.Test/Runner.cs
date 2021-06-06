using System;
using System.Diagnostics;

namespace Virus.Test
{
    public class Runner : IDisposable
    {
        public void Dispose()
        {
            GenerateGraphs();
            Console.WriteLine("Finished graph generation script");
        }

        private static void GenerateGraphs()
        {
            using var python = new Process();
            python.StartInfo.FileName = "python3";
            python.StartInfo.Arguments = "generate_graphs.py";
            python.StartInfo.WorkingDirectory = OutputPaths.Base;
            python.Start();

            python.WaitForExit();
        }
    }
}
