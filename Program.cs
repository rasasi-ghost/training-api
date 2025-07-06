/*
 * Program.cs
 * 
 * This file configures and launches the ASP.NET Core web application using the minimal hosting model (introduced in .NET 6).
 * It handles all application setup including:
 * - Service registration
 * - Swagger/OpenAPI configuration
 * - Authentication with Firebase JWT
 * - CORS policy setup
 * - HTTP request pipeline configuration
 * 
 * The minimal hosting model consolidates startup logic that was previously split between Program.cs and Startup.cs.
 */

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TrainingApi.Repositories;
using TrainingApi.Services;
using TrainingApi.Auth;

// Create the application builder which provides the API for configuring the application's services
var builder = WebApplication.CreateBuilder(args);

// =====================================
// SERVICE CONFIGURATION SECTION
// =====================================

/*
 * Add ASP.NET MVC controllers to the service container.
 * This enables handling HTTP requests through controller classes marked with [ApiController] attribute.
 */
builder.Services.AddControllers();

/*
 * Register services to generate API endpoint metadata for OpenAPI specification.
 * This enables tools like Swagger to discover API endpoints automatically.
 */
builder.Services.AddEndpointsApiExplorer();

/*
 * Configure Swagger/OpenAPI for API documentation.
 * This setup enables:
 * - API metadata display in Swagger UI
 * - XML comments integration
 * - JWT authentication in the Swagger interface
 */
builder.Services.AddSwaggerGen(c =>
{
    // Configure basic API information displayed in Swagger UI
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Training API", 
        Version = "v1",
        Description = "API for training platform with courses, teachers, students and enrollments",
        Contact = new OpenApiContact
        {
            Name = "Admin",
            Email = "admin@trainingapi.com",
            Url = new Uri("https://trainingapi.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Enable XML documentation comments display in Swagger UI
    // Note: Requires <GenerateDocumentationFile>true</GenerateDocumentationFile> in project file
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // Make XML comments loading conditional to prevent file not found exception
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Configure JWT authentication UI elements in Swagger
    // This allows testing authenticated endpoints directly from Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    // Apply the JWT security requirement to all API operations
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

/*
 * Configure Firebase JWT authentication.
 * This setup validates JWT tokens issued by Firebase Authentication service.
 */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure token validation parameters with Firebase-specific settings
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectId"]}",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Firebase:ProjectId"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false, // Firebase uses its own key validation
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow for some clock drift
        };

        // Setup event handlers to troubleshoot token validation
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token successfully validated");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"Token received: {context.Token?.Substring(0, Math.Min(context.Token.Length, 20))}...");
                return Task.CompletedTask;
            }
        };
    });

/*
 * Replace the default JWT handler with custom Firebase handler
 * This allows for custom processing and validation of Firebase tokens
 */
builder.Services.AddTransient<JwtBearerHandler, FirebaseAuthenticationHandler>();

/*
 * Register application services with dependency injection container.
 * Using Singleton lifetime which creates a single instance for the application lifetime.
 */
builder.Services.AddSingleton<IFirebaseService, FirebaseService>();       // Firebase initialization service
builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>(); // Authentication service
builder.Services.AddSingleton<IUserRepository, UserRepository>();           // User data access
builder.Services.AddSingleton<ICourseRepository, CourseRepository>();       // Course data access
builder.Services.AddSingleton<IEnrollmentRepository, EnrollmentRepository>(); // Enrollment data access

/*
 * Configure Cross-Origin Resource Sharing (CORS)
 * This allows browsers to make cross-origin requests to this API
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()   // Allow requests from any origin
               .AllowAnyMethod()   // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
               .AllowAnyHeader();  // Allow any HTTP headers
    });
});

// =====================================
// APPLICATION PIPELINE CONFIGURATION 
// =====================================

// Build the application from the configured services
var app = builder.Build();

/*
 * Configure the HTTP request pipeline with middleware.
 * The order of middleware registration is important - they execute in the order added.
 */
if (app.Environment.IsDevelopment())
{
    // Show detailed exception information in development environment
    app.UseDeveloperExceptionPage();
    
    // Enable Swagger documentation UI and JSON endpoint in development
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Training API v1"));
}

// Redirect HTTP requests to HTTPS for security
app.UseHttpsRedirection();

// Enable routing middleware to match requests to endpoints
app.UseRouting();

// Apply configured CORS policies to allow cross-origin requests
app.UseCors("AllowAll");

// Enable authentication middleware to validate JWT tokens
app.UseAuthentication();

// Enable authorization middleware to enforce access policies
app.UseAuthorization();

// Map controller endpoints to handle HTTP requests
app.MapControllers();

// Start the application and listen for incoming requests
app.Run();
