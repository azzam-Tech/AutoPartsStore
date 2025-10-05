using AutoPartsStore.Web.Filters;
using AutoPartsStore.Web.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsStore.Web.Extensions
{
    /// <summary>
    /// Extension methods for configuring error handling
    /// </summary>
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Add error handling services to the service collection
        /// </summary>
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            // Register model validation filter
            services.AddScoped<ModelValidationFilter>();

            // Configure API behavior options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disable automatic model validation (we'll use our filter instead)
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }

        /// <summary>
        /// Use error handling middleware in the application pipeline
        /// </summary>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            // Exception middleware should be first to catch all errors
            app.UseMiddleware<ExceptionMiddleware>();
            
            return app;
        }
    }
}
