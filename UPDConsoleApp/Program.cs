using System;
using System.Collections.Generic;
using System.Text;
using Classes;

namespace UPDConsoleApp
{
    internal static class Program
    {
        private const int Port = 8888;

        public static void Main(string[] args)
        {
            var server = new MetricsUdpServer(Port, DisplayMessage, DisplayMetrics);
            ConsoleKeyInfo keyInfo;

            server.Start();
            do
            {
                keyInfo = Console.ReadKey();
            } while (keyInfo.Key != ConsoleKey.Enter);

            server.Stop();
        }

        private static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static void DisplayMetrics(Dictionary<string, double> metrics)
        {
            var sb = new StringBuilder("[METRIC] ");

            if (metrics.Count > 0)
            {
                foreach (var metric in metrics) sb.AppendFormat("{0} = {1} | ", metric.Key, metric.Value);
                var nonUsefulElemsStartIndex = sb.Length - 3;
                sb.Remove(nonUsefulElemsStartIndex, 3);
            }
            else
            {
                sb.Append(Errors.NoMetrics);
            }

            Console.WriteLine(sb.ToString());
        }
    }
}