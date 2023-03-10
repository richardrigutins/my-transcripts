using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Rigutins.MyTranscripts.Server.Data;
using Rigutins.MyTranscripts.Server.Notifications;
using Rigutins.MyTranscripts.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from Key Vault.
builder.Configuration.TryAddAzureKeyVault(builder.Configuration["KeyVaultName"]);

// Add services to the container.
var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
	.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
		.EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
			.AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
			.AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
	.AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
	// By default, all incoming requests will be authorized according to the default policy
	options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
	.AddMicrosoftIdentityConsentHandler();

builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddScoped<IUserService, GraphUserService>();
builder.Services.AddScoped<IOneDriveService, GraphOneDriveService>();
builder.Services.AddScoped<ITodoService, GraphTodoService>();
builder.Services.AddSpeechRecognition(builder.Configuration.GetSection("SpeechRecognition"));
builder.Services.AddToasts();
builder.Services.AddScoped<NotificationsState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseWebSockets();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
