using Application.Mapping;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Scalar + OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Scalar/OpenAPI dev only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();