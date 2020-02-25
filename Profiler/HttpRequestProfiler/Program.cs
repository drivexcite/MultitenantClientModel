using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Net.Http;

namespace HttpRequestProfiler
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [PlainExporter]
    public class BenchMarkAccountsGet
    {
        private static HttpClient HttpClient = new HttpClient();

        [Benchmark]
        public string GetAccountsAspNet()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44321/accounts");
            var response = HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
            var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            return content;
        }

        [Benchmark]
        public string GetAccountsWithAzureFunctions()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:7071/accounts");
            var response = HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
            var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            return content;
        }
    }

    class Program
    {
        public static void Main()
        {
            _ = BenchmarkRunner.Run<BenchMarkAccountsGet>();            
        }
    }
}
