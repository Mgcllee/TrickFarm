using TrickFarmWebApp.Client.Pages;
using TrickFarmWebApp.Components;
using Microsoft.AspNetCore.ResponseCompression;
using TrickFarmWebApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"))
    .SetApplicationName("TrickFarmWebApp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // options.KnownNetworks.Clear();
    // options.KnownProxies.Clear();

    options.KnownProxies.Add(System.Net.IPAddress.Parse("172.17.0.1"));
});

builder.WebHost.UseUrls("http://0.0.0.0:8081");

var app = builder.Build();

var hubContext = app.Services.GetRequiredService<IHubContext<ChatHub>>();
GlobalHubContext.Initialize(hubContext);

app.UseForwardedHeaders();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TrickFarmWebApp.Client._Imports).Assembly);

app.MapHub<ChatHub>("/chathub");

app.Run();
