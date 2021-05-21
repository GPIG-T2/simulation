using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WHO.Tracking
{ 
    /// <summary>
    /// These refer to the Parameters of the InfectionTotals so they can be accessed easier
    /// </summary>
    public enum TrackingValue
    {
        Uninfected,
        AsymptomaticInfectedNotInfectious,
        AsymptomaticInfectedInfectious,
        Symptomatic,
        SeriousInfection,
        Dead,
        RecoveredImmune,
    }
}
