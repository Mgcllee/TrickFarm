using TrickFarmWebApp.Client.Pages;
using TrickFarmWebApp.Components;
using Microsoft.AspNetCore.ResponseCompression;
using TrickFarmWebApp.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

var hubContext = app.Services.GetRequiredService<IHubContext<ChatHub>>();
GlobalHubContext.Initialize(hubContext);

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

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TrickFarmWebApp.Client._Imports).Assembly);

app.MapHub<ChatHub>("/chathub");

app.Run();
