using Microsoft.EntityFrameworkCore;
using Db;
using Service_Contracts;
using Service_Classes;
using Repository_Contracts;
using Repository_Classes;
using Serilog;
using Serilog.AspNetCore;
using ContactManagement.Middlewares;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider service, LoggerConfiguration configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration) // take log setting from appsettings.json
    .ReadFrom.Services(service); // make our services available to Serilog
} );

//builder.Host.ConfigureLogging(provider =>
//{
//    provider.ClearProviders();  // clear default providers
//    provider.AddConsole(); // add console logging
//    provider.AddDebug(); // add debug logging

//});
builder.Services.AddLogging(); // add logging as service

builder.Services.AddHttpLogging(logging => // add HttpLogging Service
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>(); // add antiforgery token validation to all controllers
});

builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();


builder.Services.AddScoped<ICountriesAdderService, CountriesAdderService>();
builder.Services.AddScoped<ICountriesGetterService, CountriesGetterService>();
builder.Services.AddScoped<IPersonsAdderService, PersonsAdderService>();
builder.Services.AddScoped<IPersonsDeleterService, PersonsDeleteService>();
builder.Services.AddScoped<IPersonsGetterService, PersonsGetterService>();
builder.Services.AddScoped<IPersonsSorterService, PersonsSorterService>();
builder.Services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();

// add DbCOntext
if(builder.Environment.IsEnvironment("Testing") == false)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
}

// add Identity Context
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6; // min length
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 3;

})// User class name & role class name
    .AddEntityFrameworkStores<ApplicationDbContext>()// dbcontext name
    .AddDefaultTokenProviders() // add default token providers like email confirmation, password reset, glogin, fblogin etc.
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()// for user repository -- repository is generated automatically
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
// for role repository -- repository is generated automatically


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser()
        .Build(); // fallback policy for all endpoints

    options.AddPolicy("NotAuthorized", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return !context.User.Identity.IsAuthenticated; // if user is not authenticated will return true
            // to allow anonymous access to the endpoints
        });
    });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // redirect to login page if user is not authenticated
});

var app = builder.Build();
// 1. ExceptionalHandlingMiddleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); // add built in error handling middleware 
    app.UseExceptionalHandlingMiddleware(); // add custom middleware to handle exceptions
}

// 2. HTTPS Middleware
app.UseHsts(); // add HSTS middleware to enforce use HTTPS in browser
app.UseHttpsRedirection(); // add HTTPS redirection middleware to redirect HTTP requests to HTTPS

app.UseHttpLogging();
app.UseSerilogRequestLogging(); // add Serilog middleware to log requests

// logs
//app.Logger.LogInformation("Log Info message");
//app.Logger.LogWarning("Log Warning message");
//app.Logger.LogError("Log Error message");
//app.Logger.LogCritical("Log Critical message");
//app.Logger.LogDebug("Log Debug message");
//app.Logger.LogTrace("Log Trace message");


app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // to carry cookie in subsequent requests 
app.UseAuthorization(); // to check if user is authorized to access the resource
app.MapControllers();

app.UseEndpoints(endPoints =>
{
    endPoints.MapControllerRoute(
            name : "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}" // default route for areas is Admin/Home/Index, even if you do Admin it will redirect to Admin/Home/Index
    ); 
});

app.Run();

public partial class Program { } // Make the Program class partial to allow for extension in tests
