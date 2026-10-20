using Microsoft.Data.SqlClient;
using System;
using System.Net;
using System.Threading.Tasks;
namespace Pennywise.Backend
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ApiServer apiServer = new ApiServer();
            try
            {
                await apiServer.StartAsync();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
