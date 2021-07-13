using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ApiDemo.NET5
{
    /// <summary>
    ///
    /// </summary>
    public class Program
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = WebFramework.Program.CreateHostBuilder(args).Build();

            // Run MVC Host, REST API Services.
            host.Run();
        }
    }
}
