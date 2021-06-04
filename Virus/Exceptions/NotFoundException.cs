using System;
namespace Virus.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(404, message)
        {
        }
    }
}
