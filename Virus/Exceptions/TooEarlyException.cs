using System;
namespace Virus.Exceptions
{
    public class TooEarlyException : BaseException
    {
        public TooEarlyException() : base(425, "Virus is still processing")
        {
        }
    }
}
