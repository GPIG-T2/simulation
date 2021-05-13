using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Models.Parameters;

namespace Models
{
    /// <summary>
    /// The request object for creating an action.  The &#x60;action&#x60; parameter has to match the corresonding &#x60;parameters&#x60; object, not doing so is an error.
    /// </summary>
    [DataContract]
    public class WhoAction
    {
        public static readonly ISet<string> ActionNames = new HashSet<string> {
            CloseBorders.ActionName,
            CloseRecreationalLocations.ActionName,
            CloseSchools.ActionName,
            Curfew.ActionName,
            Furlough.ActionName,
            HealthDrive.ActionName,
            InformationPressRelease.ActionName,
            InvestInHealthServices.ActionName,
            InvestInVaccine.ActionName,
            Loan.ActionName,
            MaskMandate.ActionName,
            MovementRestrictions.ActionName,
            ShieldingProgram.ActionName,
            SocialDistancingMandate.ActionName,
            StayAtHome.ActionName,
            TestAndIsolation.ActionName,
        };

        /// <summary>
        /// The ID of the created action. This can then be used for deletion later.
        /// </summary>
        /// <value>The ID of the created action. This can then be used for deletion later.</value>
        public int Id { get; set; }

        /// <summary>
        /// What change will be made to the action.
        /// For creation, this property always has to be 'create'.
        /// For deletion, this property always has to be 'delete'.
        /// </summary>
        /// <value>
        /// What change will be made to the action.
        /// For creation, this property always has to be 'create'.
        /// For deletion, this property always has to be 'delete'.
        /// </value>
        public string Mode { get; set; }

        private string? _action;
        /// <summary>
        /// The action kind. Each kind has a specific set of parameters that it allows.
        ///
        /// This property is set whenever <see cref="Parameters"/> is set.
        /// </summary>
        /// <value>The action kind. Each kind has a specific set of parameters that it allows.</value>
        public string? Action
        {
            get => this._action;
            set
            {
                if (value != null && !ActionNames.Contains(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Action));
                }

                this._action = value;
            }
        }

        private ParamsContainer? _parameters;
        /// <summary>
        /// The parameters of the action.
        ///
        /// When this is set, <see cref="Action"/> is also set to the correct
        /// value.
        /// </summary>
        /// <value>The parameters of the action.</value>
        public ParamsContainer? Parameters
        {
            get => this._parameters;
            set
            {
                this.Action = value?.ActionName;
                this._parameters = value;
            }
        }

        public WhoAction(int id)
        {
            this.Id = id;
            this.Mode = "delete";
        }

        public WhoAction(int id, ParamsContainer parameters)
        {
            this.Id = id;
            this.Mode = "create";
            this.Parameters = parameters;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ActionCreate {\n");
            sb.Append("  Id: ").Append(this.Id).Append("\n");
            sb.Append("  Action: ").Append(this.Action).Append("\n");
            sb.Append("  Mode: ").Append(this.Mode).Append("\n");
            sb.Append("  Parameters: ").Append(this.Parameters).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
