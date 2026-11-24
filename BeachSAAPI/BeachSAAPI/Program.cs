using BeachSAAPI.Dto;
using BeachSAAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContextBeachSA>(options =>
    //options.UseSqlServer(builder.Configuration.GetConnectionString("StringLocal")));
options.UseSqlServer(builder.Configuration.GetConnectionString("StringCloud")));

builder.Services.AddHttpClient<IGometaService, GometaService>(client =>
{
    client.BaseAddress = new Uri("https://apis.gometa.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IAuthorizationServices, AuthorizationServices>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();



var key = builder.Configuration.GetValue<string>("JwtSettings:Key");
//convertir la key en bytes
var keyBytes = Encoding.ASCII.GetBytes(key);

//Se configura la authentication
builder.Services.AddAuthentication(config => {
    config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config => {
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false, 
        ValidateAudience = false, 
        ValidateLifetime = true, 
        ClockSkew = TimeSpan.Zero 
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
