using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{

    /// <summary>
    /// 
    /// </summary>
    public class Actor
    {
        /// <summary>
        /// The current infection state of the actor.
        /// </summary>
        /// <value>The current infection state of the actor.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// Gets or Sets Home
        /// </summary>
        public List<string> Home { get; set; }

        /// <summary>
        /// The age of the actor in years.
        /// </summary>
        /// <value>The age of the actor in years.</value>
        public int Age { get; set; }

        /// <summary>
        /// A flag for whether the actor is considered clinically vulnerable.
        /// </summary>
        /// <value>A flag for whether the actor is considered clinically vulnerable.</value>
        public bool Vulnerable { get; set; }

        /// <summary>
        /// The ID of the actor. This is unique for each actor.
        /// </summary>
        /// <value>The ID of the actor. This is unique for each actor.</value>
        public int Id { get; set; }

        public Actor(
            string state,
            List<string> location,
            List<string> home,
            int age,
            bool vulnerable,
            int id)
        {
            this.State = state;
            this.Location = location;
            this.Home = home;
            this.Age = age;
            this.Vulnerable = vulnerable;
            this.Id = id;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Actor {\n");
            sb.Append("  State: ").Append(this.State).Append("\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  Home: ").Append(this.Home).Append("\n");
            sb.Append("  Age: ").Append(this.Age).Append("\n");
            sb.Append("  Vulnerable: ").Append(this.Vulnerable).Append("\n");
            sb.Append("  Id: ").Append(this.Id).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
