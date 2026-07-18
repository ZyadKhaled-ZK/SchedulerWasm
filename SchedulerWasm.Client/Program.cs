using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SchedulerWasm.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<SchedulerWasm.Client.App>("#app");

builder.Services.AddScoped<DataService>();

await builder.Build().RunAsync();
