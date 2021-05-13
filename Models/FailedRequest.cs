using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The object returned in the case that a request failed for any reason.
    /// </summary>
    public class FailedRequest
    {
        /// <summary>
        /// The status code of the request. Will always be 4xx or 5xx, as reponse is only used in these conditions.
        /// </summary>
        /// <value>The status code of the request. Will always be 4xx or 5xx, as reponse is only used in these conditions.</value>
        public int Code { get; set; }

        /// <summary>
        /// A message containing extra information on what went wrong.
        /// </summary>
        /// <value>A message containing extra information on what went wrong.</value>
        public string Message { get; set; }

        public FailedRequest(int code, string message)
        {
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
            sb.Append("class FailedRequest {\n");
            sb.Append("  Code: ").Append(this.Code).Append("\n");
            sb.Append("  Message: ").Append(this.Message).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
