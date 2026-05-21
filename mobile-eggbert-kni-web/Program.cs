using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WindowsPhoneSpeedyBlupi
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddScoped(sp => new HttpClient()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
            //builder.Services.AddSingleton<LocalStorageHelper>();
            var app = builder.Build();
            //LocalStorageHelper localStorageHelper = app.Services.GetRequiredService<LocalStorageHelper>();
            //LocalStorageHelperHolder.LocalStorageHelper = localStorageHelper;
            await app.RunAsync();
        }
    }
}
