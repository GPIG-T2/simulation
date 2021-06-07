using System;
namespace Models.WebSocket
{
    public class Response
    {
        public int? Id { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }

        public Response(int? id, int status, string message)
        {
            this.Id = id;
            this.Status = status;
            this.Message = message;
        }
    }
}
