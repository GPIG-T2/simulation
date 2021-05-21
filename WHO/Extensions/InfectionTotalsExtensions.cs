using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WHO.Tracking;

namespace WHO.Extensions
{
    public static class InfectionTotalsExtensions
    {

        public static int GetTotalPeople(this InfectionTotals totals)
        {
            return totals.AsymptomaticInfectedInfectious +
                totals.AsymptomaticInfectedNotInfectious +
                totals.Dead +
                totals.RecoveredImmune +
                totals.SeriousInfection +
                totals.Symptomatic +
                totals.Uninfected;
        }
        public static int GetParameterTotals(this InfectionTotals totals, TrackingValue value)
        {
            Type myType = typeof(InfectionTotals);
            PropertyInfo myPropInfo = myType.GetProperty(value.ToString());
            return (int)myPropInfo.GetValue(totals, null);
        }

        public static InfectionTotals Clone(this InfectionTotals self)
        {
            return new(self.Location, self.Uninfected, self.AsymptomaticInfectedNotInfectious, self.AsymptomaticInfectedInfectious, self.Symptomatic, self.SeriousInfection, self.Dead, self.RecoveredImmune);
        }

        public static void Add(this InfectionTotals self, InfectionTotals other)
        {
            self.Uninfected += other.Uninfected;
            self.AsymptomaticInfectedInfectious += other.AsymptomaticInfectedInfectious;
            self.AsymptomaticInfectedNotInfectious += other.AsymptomaticInfectedNotInfectious;
            self.Symptomatic += other.Symptomatic;
            self.SeriousInfection += other.SeriousInfection;
            self.Dead += other.Dead;
            self.RecoveredImmune += self.RecoveredImmune;
        }

    }
}
