using AutoMapper;
using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MrLubeCMS.CustomHandler;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;
using Serilog.Enrichers;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

//KeyVault URI
var keyVaultEndpoint = new Uri(builder.Configuration.GetSection("VaultUri").Value);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new Azure.Identity.DefaultAzureCredential());

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                   .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

var logger = new LoggerConfiguration()
  .Enrich.WithProperty("Application : ", "MrLubeCMS")
  .Enrich.WithProperty("Process Path: ", Environment.ProcessPath)
  .Enrich.WithProperty("Environment: ", Environment.MachineName)
  .Enrich.WithMachineName()
  .Enrich.WithProcessName()
  .Enrich.WithEnvironmentName()
  .Enrich.WithEnvironmentUserName()
  .Enrich.WithThreadName()
  .Enrich.WithThreadId()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Configuration.AddApplicationInsightsSettings("ApplicationInsights");

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
var mapper = config.CreateMapper(); 
builder.Services.AddSingleton(mapper);

builder.Services.AddApplicationInsightsTelemetry();

//Add Session
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddSession();

//Uer External Services for Application
//builder.Services.ApplicationService(builder.Configuration);
builder.Services.AddScoped<IbannerService, bannerService>();
builder.Services.AddScoped<IShopTireRepository, ShopTireRepository>();
builder.Services.AddScoped<IShopTireAllRepository, ShopTireAllRepository>();
builder.Services.AddScoped<IFloatingImageRepository, FloatingImageRepository>();
builder.Services.AddScoped<IPublishService, PublishService>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IPromosRepository, PromoPagesReposiory>();
builder.Services.AddScoped<IPromoImagesRepository, PromoImagesRepository>();
builder.Services.AddScoped<ILeftAdRepository, LeftAdRepository>();
builder.Services.AddScoped<ICouponsRepository, CouponPagesRepository>();
builder.Services.AddScoped<ICouponImagesRepository, CouponImagesRepository>();

string blobAccessToken = builder.Configuration.GetSection("BlobAccessToken").Value;
string blobBaseUrl = builder.Configuration.GetSection("BlobStorageAPIUrl").Value;

builder.Services.AddHttpClient("blobClient", client =>
{
    string token = "";
    if (!BlobStorageAPIService.isEmptyOrInvalid(Convert.ToString(builder.Configuration.GetSection("BlobAccessToken").Value ?? "")))
    {
        token = Convert.ToString(builder.Configuration.GetSection("BlobAccessToken").Value ?? "");
    }
    else
    {
        string clientId = builder.Configuration.GetSection("ClientId").Value; 
        string authority = builder.Configuration.GetSection("Authority").Value;  
        string clientSecret = builder.Configuration.GetSection("ClientSecret").Value; 
        string resource = builder.Configuration.GetSection("Resource").Value;  

        AuthenticationContext context = new AuthenticationContext(authority);
        ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

        token = context.AcquireTokenAsync(resource, clientCredential).Result.AccessToken;
        //HttpContext.Session.SetString("token", token);
        builder.Configuration.GetSection("BlobAccessToken").Value = token;
    }
    client.DefaultRequestHeaders.Authorization =
                                  new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    //client.DefaultRequestHeaders.Add("BlobAccessToken", blobAccessToken);//TODO:uncomment after decide
    client.BaseAddress = new Uri(blobBaseUrl);
});


builder.Services.AddScoped<AzureService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(1);//You can set Time   
});
string ConenctionString = builder.Configuration.GetConnectionString("CmsConnectionLocal");
//builder.Services.Add(new ServiceDescriptor(typeof(CMSDbContext),new CMSDbContext(ConenctionString)));
builder.Services.AddDbContext<CMSDbContext>(options =>
options.UseMySQL(builder.Configuration.GetConnectionString("CmsConnectionLocal")));
//Auto Mapper


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

builder.Services.AddControllersWithViews();

var app = builder.Build();
//app.UseSession();

//fM- configure the required services for static BlobStorageAPIService class.
BlobStorageAPIService.BlobStorageAPIServiceConfigurations(app.Services.GetRequiredService<IConfiguration>(), app.Services.GetRequiredService<System.Net.Http.IHttpClientFactory>());


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMvc();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//     name: "MyArea",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

//    endpoints.MapRazorPages();
//});
//app.UseMvc(routes => {
//    routes.MapRoute(name: "default",
//    template: "{controller=UserLoginModel}/{action=Index}/{id?}");
//    });

app.MapControllerRoute(
name: "default",
pattern: "{controller}/{action}/{id?}",
defaults: new { controller = "Banner", action = "Banner" });
app.MapRazorPages();
//app.UseSerilogRequestLogging();
app.Run();
