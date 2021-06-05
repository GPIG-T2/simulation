using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{

    /// <summary>
    /// Provides the number of people in each infection state in a given location.
    /// </summary>
    public class InfectionTotals
    {
        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// Number of people who have never come into contact with the virus.
        /// </summary>
        /// <value>Number of people who have never come into contact with the virus.</value>
        public int Uninfected { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, but are not showing symptoms, nor are capable of spreading it.
        /// </summary>
        /// <value>Number of people who have contracted the virus, but are not showing symptoms, nor are capable of spreading it.</value>
        public int AsymptomaticInfectedNotInfectious { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, are not showing symptoms, but are able to spread it to others.
        /// </summary>
        /// <value>Number of people who have contracted the virus, are not showing symptoms, but are able to spread it to others.</value>
        public int AsymptomaticInfectedInfectious { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus and are showing symptoms of it. They can also spread it to others.
        /// </summary>
        /// <value>Number of people who have contracted the virus and are showing symptoms of it. They can also spread it to others.</value>
        public int Symptomatic { get; set; }

        /// <summary>
        /// Number of people who have been hospitalised due to the virus.
        /// </summary>
        /// <value>Number of people who have been hospitalised due to the virus.</value>
        public int SeriousInfection { get; set; }

        /// <summary>
        /// Number of people who have died due to the virus.
        /// </summary>
        /// <value>Number of people who have died due to the virus.</value>
        public int Dead { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, but have fully recovered and are not able to spread it or re-contract it.
        /// </summary>
        /// <value>Number of people who have contracted the virus, but have fully recovered and are not able to spread it or re-contract it.</value>
        public int RecoveredImmune { get; set; }

        public InfectionTotals(
            List<string> location,
            int uninfected,
            int asymptomaticInfectedNotInfectious,
            int asymptomaticInfectedInfectious,
            int symptomatic,
            int seriousInfection,
            int dead,
            int recoveredImmune)
        {
            this.Location = location;
            this.Uninfected = uninfected;
            this.AsymptomaticInfectedNotInfectious = asymptomaticInfectedNotInfectious;
            this.AsymptomaticInfectedInfectious = asymptomaticInfectedInfectious;
            this.Symptomatic = symptomatic;
            this.SeriousInfection = seriousInfection;
            this.Dead = dead;
            this.RecoveredImmune = recoveredImmune;
        }

        public InfectionTotals Clone() =>
            new(this.Location, this.Uninfected, this.AsymptomaticInfectedNotInfectious,
                this.AsymptomaticInfectedInfectious, this.Symptomatic,
                this.SeriousInfection, this.Dead, this.RecoveredImmune);

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InfectionTotals {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  Uninfected: ").Append(this.Uninfected).Append("\n");
            sb.Append("  AsymptomaticInfectedNotInfectious: ").Append(this.AsymptomaticInfectedNotInfectious).Append("\n");
            sb.Append("  AsymptomaticInfectedInfectious: ").Append(this.AsymptomaticInfectedInfectious).Append("\n");
            sb.Append("  Symptomatic: ").Append(this.Symptomatic).Append("\n");
            sb.Append("  SeriousInfection: ").Append(this.SeriousInfection).Append("\n");
            sb.Append("  Dead: ").Append(this.Dead).Append("\n");
            sb.Append("  RecoveredImmune: ").Append(this.RecoveredImmune).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
