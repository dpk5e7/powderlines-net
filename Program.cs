using Microsoft.Data.SqlClient;
using Powderlines.API;
using Powderlines.Components;
using Powderlines.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PowderLinesConnection") ?? throw new InvalidOperationException("Connection string 'PowderLinesConnection' not found.");
builder.Services.AddTransient<IDbConnection>((sp) => new SqlConnection(connectionString));

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddTransient<ISnotelService, SnotelService>();

builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();	
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapAPIEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

await app.RunAsync();
