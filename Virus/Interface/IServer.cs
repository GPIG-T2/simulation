using System;

namespace Virus.Interface
{
    public interface IServer : IDisposable
    {
        void Start(IHandler handler);
        void Stop();
    }
}
