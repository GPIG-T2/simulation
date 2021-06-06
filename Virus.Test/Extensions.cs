using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Virus.Test
{
    public static class Extensions
    {
        public static List<InfectionTotals> Snapshot(this World world)
            => world.Nodes.Select(n => n.Totals.Clone()).ToList();
    }
}
