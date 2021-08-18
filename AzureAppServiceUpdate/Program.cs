using System;
using System.Threading.Tasks;

namespace AzureAppServiceUpdate
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Updating Function Apps Settings");
            await AppServiceUpdate.UpdateFunctionAppSettings();

            Console.WriteLine("Stopping AppService");
            await AppServiceUpdate.StopAppService();

            Console.WriteLine("Starting AppService");
            await AppServiceUpdate.StartAppService();
         
        }
    }
}
