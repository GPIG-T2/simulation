using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// An object providing details for a search request.
    /// </summary>
    public class SearchRequest
    {
        /// <summary>
        /// The array of coordinates to search.
        /// </summary>
        /// <value>The array of coordinates to search.</value>
        public List<List<string>> Locations { get; set; }

        public SearchRequest(List<List<string>> locations)
        {
            this.Locations = locations;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SearchRequest {\n");
            sb.Append("  Locations: ").Append(this.Locations).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
