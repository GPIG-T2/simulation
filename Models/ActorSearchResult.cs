using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The object containing the result from an actor search. Provides both the location the search was done against, and all of the IDs of the actors that match the search.
    /// </summary>
    public class ActorSearchResult
    {
        /// <summary>
        /// Gets or Sets Actors
        /// </summary>
        public List<int> Actors { get; set; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public ActorSearchResult(List<int> actors, List<string> location)
        {
            this.Actors = actors;
            this.Location = location;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ActorSearchResult {\n");
            sb.Append("  Actors: ").Append(this.Actors).Append("\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
