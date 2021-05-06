using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace GPIGCommon
{
    public struct ActionRequest
    {
        public string action;
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionMode mode;
        public Dictionary<string, object> parameters;
    }
}
