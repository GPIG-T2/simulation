using WHO.Main;

namespace WHO
{
    class Program
    {

        private const string uri = "ws://127.0.0.1";

        static void Main(string[] args)
        {
            HealthOrganisation org = new HealthOrganisation(uri);
            org.Start();
        }
    }
}
