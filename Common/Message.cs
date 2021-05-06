using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GPIGCommon
{
    public struct Message
    {

        [JsonConverter(typeof(StringEnumConverter))]
        public MessagePath path;
        public string body;

    }
}
