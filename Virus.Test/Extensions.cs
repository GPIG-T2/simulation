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

        public static string ToCsvLine(this InfectionTotals t)
            => string.Format("{0},{1},{2},{3},{4},{5},{6}", t.Uninfected,
                    t.AsymptomaticInfectedNotInfectious, t.AsymptomaticInfectedInfectious,
                    t.Symptomatic, t.SeriousInfection, t.Dead, t.RecoveredImmune
                );
    }
}
