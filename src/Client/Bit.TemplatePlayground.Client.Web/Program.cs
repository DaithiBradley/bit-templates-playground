﻿using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Bit.Butil;
#if BlazorWebAssemblyStandalone
using Microsoft.AspNetCore.Components.Web;
#endif

namespace Bit.TemplatePlayground.Client.Web;

public static partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

#if BlazorWebAssemblyStandalone
        builder.RootComponents.Add<Routes>("#app-container");
        builder.RootComponents.Add<HeadOutlet>("head::after");
#endif

        
        builder.ConfigureServices();

        var host = builder.Build();

        if (AppRenderMode.MultilingualEnabled)
        {
            var uri = new Uri(host.Services.GetRequiredService<NavigationManager>().Uri);

            var cultureCookie = await host.Services.GetRequiredService<Cookie>().GetValue(".AspNetCore.Culture");

            if (cultureCookie is not null)
            {
                cultureCookie = Uri.UnescapeDataString(cultureCookie);
                cultureCookie = cultureCookie[(cultureCookie.IndexOf("|uic=") + 5)..];
            }

            var culture = HttpUtility.ParseQueryString(uri.Query)["culture"] ?? // 1- Culture query string
                          cultureCookie ?? // 2- User settings
                          CultureInfo.CurrentUICulture.Name; // 3- OS/Browser settings

            host.Services.GetRequiredService<CultureInfoManager>().SetCurrentCulture(culture);
        }

        try
        {
            await host.RunAsync();
        }
        catch (JSException exp) when (exp.Message is "Error: Could not find any element matching selector '#app-container'.")
        {
#if BlazorWebAssemblyStandalone
            await System.Console.Error.WriteLineAsync("Either run/publish Client.Web project or set BlazorWebAssemblyStandalone to false.");
#else
            throw;
#endif
        }

    }
}
