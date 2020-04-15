using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpLoadGen
{
    public static class Program
    {
        public static void Main()
        {
            const uint Requests = 800000;
            const uint Parallelism = 8;
            const ushort Concurrency = 6;
            var uri = new Uri("http://localhost:5002/v0.1/locate", UriKind.Absolute);
            using var client = new HttpClient();
            Run(client, uri, Concurrency, Parallelism, Concurrency);
            GC.Collect();
            var perThreadRequests = (Requests + Parallelism - 1) / Parallelism;
            var st = Stopwatch.StartNew();
            Run(client, uri, perThreadRequests, Parallelism, Concurrency);
            Console.WriteLine(Math.Floor(perThreadRequests * Parallelism / st.Elapsed.TotalSeconds));
        }

        private static void Run(HttpClient client, Uri uri, uint requests, uint parallelism, ushort concurrency)
        {
            var tasks = new Task[parallelism];
            for (var i = 0; i < parallelism; i++)
            {
                tasks[i] = Task.Run(() => Run(client, uri, requests, concurrency));
            }

            Task.WaitAll(tasks);
        }

        private static void Run(HttpClient client, Uri uri, uint requests, ushort concurrency)
        {
            if (concurrency < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(concurrency), concurrency, "Concurrency is less than 1.");
            }

            if (requests < concurrency)
            {
                throw new ArgumentOutOfRangeException(nameof(requests), requests, "Number of requests is less than concurrency.");
            }

            using var content = new StringContent("8.8.8.8", Encoding.ASCII);
            var tasks = new Task<HttpResponseMessage>[concurrency];
            for (var i = 0; i < requests; i++)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope -- HttpRequestMessage only disposes content and we would like to share it
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
#pragma warning restore CA2000 // Dispose objects before losing scope
                int taskIndex;
                if (i >= concurrency)
                {
                    taskIndex = Task.WaitAny(tasks);
                    tasks[taskIndex].Result.EnsureSuccessStatusCode();
                }
                else
                {
                    taskIndex = i;
                }

                tasks[taskIndex] = client.SendAsync(requestMessage);
            }

            foreach (var task in tasks)
            {
                task.Result.EnsureSuccessStatusCode();
            }
        }
    }
}
