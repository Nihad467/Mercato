using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Products.Commands.CreateProduct;
using Mercato.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<MercatoDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")
));

// Interface → Implementation
builder.Services.AddScoped<IApplicationDbContext>(provider =>
provider.GetRequiredService<MercatoDbContext>());

// MediatR registration (MediatR v11 üçün düzgün syntax)
builder.Services.AddMediatR(typeof(CreateProductCommand).Assembly);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
