using System;
namespace Virus.Exceptions
{
    public class BaseException : Exception
    {
        public int Code { get; }

        public BaseException(int code, string message) : base(message)
        {
            this.Code = code;
        }
    }
}
