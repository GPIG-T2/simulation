using System;
namespace Virus.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(400, message)
        {
        }
    }
}
