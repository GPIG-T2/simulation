using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The result object of an action.  If an action was sent with a coordinate that does not exist, then this response will have the code 404, and the action will not be applied.  If an action failed to apply (i.e. that action has not been applied to the server), then the reponse will have the code 500. If an action has been _partially_ applied, then these changes must be undone by the server before the response is sent.
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// The ID of the action that was adjusted. This will match the ID that was sent.
        /// </summary>
        /// <value>The ID of the action that was adjusted. This will match the ID that was sent.</value>
        public int Id { get; set; }

        /// <summary>
        /// The response code for the action adjustment. This uses standard HTTP response codes. (e.g. 200 for success, 404 for an invalid location, etc.)
        /// </summary>
        /// <value>The response code for the action adjustment. This uses standard HTTP response codes. (e.g. 200 for success, 404 for an invalid location, etc.)</value>
        public int Code { get; set; }

        /// <summary>
        /// A message containing information about the request reponse. Typically used for debug information in the case of non 2xx status codes.
        /// </summary>
        /// <value>A message containing information about the request reponse. Typically used for debug information in the case of non 2xx status codes.</value>
        public string Message { get; set; }

        public ActionResult(int id, int code, string message)
        {
            this.Id = id;
            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ActionResult {\n");
            sb.Append("  Id: ").Append(this.Id).Append("\n");
            sb.Append("  Code: ").Append(this.Code).Append("\n");
            sb.Append("  Message: ").Append(this.Message).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
