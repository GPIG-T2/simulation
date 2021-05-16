using System;

namespace Models.WebSocket
{
    public class Request
    {
        public int Id { get; set; }
        public string Endpoint { get; set; }
        public HttpMethod Method { get; set; }
        public string? Message { get; set; }

        public Request(int id, string endpoint, HttpMethod method)
        {
            this.Id = id;
            this.Endpoint = endpoint;
            this.Method = method;
        }
    }
}
