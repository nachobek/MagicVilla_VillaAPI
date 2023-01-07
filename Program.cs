using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

// Since we installed the Serilog package to log messages to a flat file. We need to modify the default Log implementation/settings so it uses Serilog.
// First we set up Serilog the way we want it.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Sets the minimum level of detail, it'll log everything that's Info and above.
    .WriteTo.File("log/VillaLog.txt", rollingInterval:RollingInterval.Day) // Sets up the file path/name as well as when to start creating a new logging file, which is set to Daily in this case.
    .CreateLogger();

// Then we tell the application to use Serilog rather than the built in logger.
builder.Host.UseSerilog();


builder.Services.AddControllers( option => {
    option.ReturnHttpNotAcceptable = true; // Adding this option makes the API to return error 406-Not Acceptable if the format in which we return the data is not the accepted format instructed when executed. I.e. if the data was requested in xml and we returned json.
    })
    .AddNewtonsoftJson() //.Addnewtonsoft added to include support for PATCH verb.
    .AddXmlDataContractSerializerFormatters(); //Adds support to return data in xml format if that's what was requested when calling the api.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
