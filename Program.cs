using MagicVilla_VillaAPI.Utility;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Adding DbContext service to the container.
builder.Services.AddDbContext<ApplicationDbContext> (option => {
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// Adding AutoMapper service.
builder.Services.AddAutoMapper(typeof(MappingConfig));

// Adding reference to IRepository.
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Since we installed the Serilog package to log messages to a flat file. We need to modify the default Log implementation/settings so it uses Serilog.
// First we set up Serilog the way we want it.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Sets the minimum level of detail, it'll log everything that's Info and above.
    .WriteTo.File("log/VillaLog.txt", rollingInterval:RollingInterval.Day) // Sets up the file path/name as well as when to start creating a new logging file, which is set to Daily in this case.
    .CreateLogger();

// Then we tell the application to use Serilog rather than the built in logger.
builder.Host.UseSerilog();

// Authorization service settings. Required to use app.Authentication. Which is needed to execute the API with token received when calling endpoint /UserAuth/register.
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


builder.Services.AddControllers( option => {
    option.ReturnHttpNotAcceptable = true; // Adding this option makes the API to return error 406-Not Acceptable if the format in which we return the data is not the accepted format instructed when executed. I.e. if the data was requested in xml and we returned json.
    })
    .AddNewtonsoftJson() //.Addnewtonsoft added to include support for PATCH verb.
    .AddXmlDataContractSerializerFormatters(); //Adds support to return data in xml format if that's what was requested when calling the api.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Modifying the AddSwagger implementation to add capability to insert the Bearer token. The below setting will insert a header in the HTTP call with Key=Authorization and Value = Bearer tokenNumber.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization using Bearer scheme. \n Enter: Bearer [space] [token] \n Example: \"Bearer 123abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


// About custom services.
// Let's suppose that rather than using the built in logger, we created our own Interface and Class implementation of a custom logging service/functionality.
// We need a way to inject our custom logger in the Controller class.
// Currently we do "public VillaAPIController(ILogger<VillaAPIController> logger)" and this works fine, because the Ilogger is already built in the "builder" container.
// We can make our own custom logging Interface and Class available in the builder by using the following function:
// builder.Services.AddSingleton<ICustomLogging, CustomLogging>();
// Then we can use dependency injection in the controller constructor as same as we do with the built in logger.


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // For the authentication to work, it must be specified before autorization.

app.UseAuthorization();

app.MapControllers();

app.Run();
