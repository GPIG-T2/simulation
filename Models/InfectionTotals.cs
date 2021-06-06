using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Models
{

    /// <summary>
    /// Provides the number of people in each infection state in a given location.
    /// </summary>
    public class InfectionTotals : IEquatable<InfectionTotals>
    {
        public static InfectionTotals Empty() => new(new(), 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// Number of people who have never come into contact with the virus.
        /// </summary>
        /// <value>Number of people who have never come into contact with the virus.</value>
        public long Uninfected { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, but are not showing symptoms, nor are capable of spreading it.
        /// </summary>
        /// <value>Number of people who have contracted the virus, but are not showing symptoms, nor are capable of spreading it.</value>
        public long AsymptomaticInfectedNotInfectious { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, are not showing symptoms, but are able to spread it to others.
        /// </summary>
        /// <value>Number of people who have contracted the virus, are not showing symptoms, but are able to spread it to others.</value>
        public long AsymptomaticInfectedInfectious { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus and are showing symptoms of it. They can also spread it to others.
        /// </summary>
        /// <value>Number of people who have contracted the virus and are showing symptoms of it. They can also spread it to others.</value>
        public long Symptomatic { get; set; }

        /// <summary>
        /// Number of people who have been hospitalised due to the virus.
        /// </summary>
        /// <value>Number of people who have been hospitalised due to the virus.</value>
        public long SeriousInfection { get; set; }

        /// <summary>
        /// Number of people who have died due to the virus.
        /// </summary>
        /// <value>Number of people who have died due to the virus.</value>
        public long Dead { get; set; }

        /// <summary>
        /// Number of people who have contracted the virus, but have fully recovered and are not able to spread it or re-contract it.
        /// </summary>
        /// <value>Number of people who have contracted the virus, but have fully recovered and are not able to spread it or re-contract it.</value>
        public long RecoveredImmune { get; set; }

        [JsonIgnore]
        public bool IsInfected => this.AsymptomaticInfectedNotInfectious > 0
            || this.AsymptomaticInfectedInfectious > 0
            || this.Symptomatic > 0
            || this.SeriousInfection > 0;

        public InfectionTotals(
            List<string> location,
            long uninfected,
            long asymptomaticInfectedNotInfectious,
            long asymptomaticInfectedInfectious,
            long symptomatic,
            long seriousInfection,
            long dead,
            long recoveredImmune)
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

        public long GetTotalPeople()
        {
            return this.AsymptomaticInfectedInfectious +
                this.AsymptomaticInfectedNotInfectious +
                this.Dead +
                this.RecoveredImmune +
                this.SeriousInfection +
                this.Symptomatic +
                this.Uninfected;
        }

        public InfectionTotals Add(InfectionTotals other)
        {
            this.Uninfected += other.Uninfected;
            this.AsymptomaticInfectedInfectious += other.AsymptomaticInfectedInfectious;
            this.AsymptomaticInfectedNotInfectious += other.AsymptomaticInfectedNotInfectious;
            this.Symptomatic += other.Symptomatic;
            this.SeriousInfection += other.SeriousInfection;
            this.Dead += other.Dead;
            this.RecoveredImmune += other.RecoveredImmune;

            return this;
        }

        public bool Equals(InfectionTotals? other)
        {
            if (other == null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(this.Location, other.Location) &&
                this.SeriousInfection == other.SeriousInfection &&
                this.Symptomatic == other.Symptomatic &&
                this.Uninfected == other.Uninfected &&
                this.Dead == other.Dead &&
                this.AsymptomaticInfectedNotInfectious == other.AsymptomaticInfectedNotInfectious &&
                this.AsymptomaticInfectedInfectious == other.AsymptomaticInfectedInfectious;

        }

        public InfectionTotals Clone() =>
            new(this.Location, this.Uninfected, this.AsymptomaticInfectedNotInfectious,
                this.AsymptomaticInfectedInfectious, this.Symptomatic,
                this.SeriousInfection, this.Dead, this.RecoveredImmune);

        public static InfectionTotals operator +(InfectionTotals left, InfectionTotals right)
            => left.Clone().Add(right);

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
