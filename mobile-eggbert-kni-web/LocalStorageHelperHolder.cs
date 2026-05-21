using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WindowsPhoneSpeedyBlupi
{
    public class LocalStorageHelperHolder
    {
        public static LocalStorageHelper LocalStorageHelper { get; set; }
     
    }
}
