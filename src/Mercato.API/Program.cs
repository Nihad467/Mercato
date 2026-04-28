using FluentValidation;
using MediatR;
using Mercato.API.Middlewares;
using Mercato.Application.Common.Behaviors;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Products.Commands.CreateProduct;
using Mercato.Domain.Entities;
using Mercato.Infrastructure.Extensions;
using Mercato.Infrastructure.Persistence;
using Mercato.Infrastructure.Persistence.Context;
using Mercato.Infrastructure.Services;
using Mercato.Infrastructure.Services.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mercato API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header istifadə et. Nümunə: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    builder.Services.AddDbContext<MercatoDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<MercatoDbContext>());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddScoped<IProductQueryService, ProductQueryService>();

    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddMinioStorage(builder.Configuration);
    builder.Services.AddScoped<IFileStorageService, S3MinioFileStorageService>();

    builder.Services.AddRedisServices(builder.Configuration);

    builder.Services.Configure<JwtOptions>(
        builder.Configuration.GetSection("Jwt"));
    builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

    builder.Services.Configure<StripeOptions>(
        builder.Configuration.GetSection(StripeOptions.SectionName));

    builder.Services.Configure<EmailOptions>(
        builder.Configuration.GetSection("Email"));
    builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));

    builder.Services.AddHttpClient<IAiRequirementAnalyzer, OpenAiRequirementAnalyzer>();

    builder.Services.AddScoped<IAiProductRecommendationService, AiProductRecommendationService>();

    builder.Services.AddScoped<IEmailService, EmailService>();

    builder.Services.AddScoped<MockPaymentService>();
    builder.Services.AddScoped<StripePaymentService>();
    builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

    builder.Services
        .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<MercatoDbContext>()
        .AddDefaultTokenProviders();

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSection["SecretKey"]!;

    builder.Services
        .AddAuthentication(options =>
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
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddMediatR(typeof(CreateProductCommand).Assembly);

    builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);

    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    var app = builder.Build();

    Log.Information("Mercato API started successfully.");

    using (var scope = app.Services.CreateScope())
    {
        await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
    }

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
            }
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}