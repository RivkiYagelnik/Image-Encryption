
using BL;
using DAL.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MODELS.Model;
using Serilog;
using Image_encryption.middleWare;
using BL.Interface;
using BL.Services;
using Project.middleWare;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);
//
// Add services to the container.
string myCors = "_myCors";
builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
    options.AddPolicy("AdminOrCustomer", policy => policy.RequireRole("Admin", "Customer"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter JWT token with Bearer into field"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddCors(op =>
{
    op.AddPolicy(myCors,
        builder =>
        {
            builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});


//connection string
//builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultDataBase"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, ServiceLifetime.Transient);
       


builder.Services.AddDbContext< DbContext>(options =>
{
    options.UseSqlServer("name=ConnectionStrings:WarOfMindsDB");
   
}, ServiceLifetime.Transient);



builder.Services.AddScoped<IUserData, UserData>();
   builder.Services.AddScoped<IPictureData, PictureData>();
       builder.Services.AddScoped<IPictureService, PictureService>();
      builder.Services.AddScoped<IUserService, UserService>();// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(@"C:\myLogDoc.txt",
    rollingInterval: RollingInterval.Day)
    .CreateLogger();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(myCors);
app.UseHttpsRedirection();
app.UseMiddleware<ErrorGlobalMiddleWare>();
app.UseMiddleware<JWTmiddlware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseMiddleware<IdValidationMiddleware>();


app.Run();