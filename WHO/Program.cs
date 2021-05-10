using System.Threading.Tasks;

namespace WHO
{
    class Program
    {

        private const string uri = "ws://127.0.0.1";

        async static ValueTask Main(string[] args)
        {
            await using HealthOrganisation org = new(uri);
            // TODO: process WHO
        }
    }
}
