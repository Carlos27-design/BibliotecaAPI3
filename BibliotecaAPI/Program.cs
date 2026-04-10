using BibliotecaAPI.Data;
using BibliotecaAPI.Enitities;
using BibliotecaAPI.Services;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



//área de servicios;
/*builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});*/

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

builder.Services.AddDataProtection();

var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(optionsCors =>
    {
        optionsCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("cantidad-total-registros");
    });
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FiltroTiempoEjecucion>();
    options.Conventions.Add(new ConvencionAgrupaPorVersion());
}).AddNewtonsoftJson();

builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddIdentityCore<Usuario>().AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<Usuario>>();

builder.Services.AddScoped<SignInManager<Usuario>>();

builder.Services.AddTransient<IServiciosUsuarios, ServiciosUsuarios>();

builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorDeArchivosLocal>();

builder.Services.AddScoped<MiFiltroDeAccion>();

builder.Services.AddScoped<FiltroValidacionLibro>();

builder.Services.AddScoped<BibliotecaAPI.Services.V1.IServicioAutores, BibliotecaAPI.Services.V1.ServicioAutores>();

builder.Services.AddScoped<BibliotecaAPI.Services.V1.IGeneradorEnlaces, BibliotecaAPI.Services.V1.GeneradorEnlaces>();

builder.Services.AddScoped<HATEOASAutorAttribute>();

builder.Services.AddScoped<HATEOASAutoresAttribute>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["keyjwt"]!)),
        ClockSkew = TimeSpan.Zero

    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin"));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Biblioteca API",
        Description = "Este es una web api para trabajar con datos de autores y libros",
        Contact = new OpenApiContact
        {
            Email = "carlos@gmail.com",
            Name = "Carlos Coronado",
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "Biblioteca API",
        Description = "Este es una web api para trabajar con datos de autores y libros",
        Contact = new OpenApiContact
        {
            Email = "carlos@gmail.com",
            Name = "Carlos Coronado",
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    options.OperationFilter<FiltroAutorizacion>();

    /* options.AddSecurityRequirement(new OpenApiSecurityRequirement
     {
         {
             new OpenApiSecurityScheme
             {
                 Reference = new OpenApiReference
                 {
                     Type = ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },

             new string[]{}

         }
     });*/
});


var app = builder.Build();

// area de middlewares


//El orden de los middlewares es importante, ya que se ejecutan en el orden en el que se registran.
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;
    var error = new Error
    {
        MensajeDeError = exception.Message,
        StrackTrace = exception.StackTrace,
        Fecha = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDBContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new { tipo = "Error", mensaje = "Ha ocurrido un error inesperado", estatus = 500 }).ExecuteAsync(context);
}));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API V2");
});

app.UseStaticFiles();

app.UseCors();

app.UseOutputCache();

app.MapControllers();

app.Run();
