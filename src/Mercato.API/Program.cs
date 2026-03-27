using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Products.Commands.CreateProduct;
using Mercato.Infrastructure.Extensions;
using Mercato.Infrastructure.Persistence.Context;
using Mercato.Infrastructure.Services;
using Mercato.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<MercatoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Interface registrations
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<MercatoDbContext>());

builder.Services.AddScoped<IProductQueryService, ProductQueryService>();

// MinIO
builder.Services.AddMinioStorage(builder.Configuration);
builder.Services.AddScoped<IFileStorageService, S3MinioFileStorageService>();

// MediatR
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