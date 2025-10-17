using AutoPartsStore.Core.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace AutoPartsStore.Web.Filters
{
    /// <summary>
    /// Action filter to validate model state before executing action
    /// </summary>
    public class ModelValidationFilter : IActionFilter
    {
        private readonly ILogger<ModelValidationFilter> _logger;

        public ModelValidationFilter(ILogger<ModelValidationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                var validationErrors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorResponse = new ErrorResponse
                {
                    ErrorCode = "MODEL_VALIDATION_ERROR",
                    Message = "›‘· «· Õﬁﬁ „‰ ’Õ… «·»Ì«‰«  «·„œŒ·….",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = errors,
                    ValidationErrors = validationErrors,
                    TraceId = context.HttpContext.TraceIdentifier,
                    Path = context.HttpContext.Request.Path,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogWarning(
                    "Model validation failed for {ActionName}. Errors: {Errors}",
                    context.ActionDescriptor.DisplayName,
                    JsonSerializer.Serialize(errors));

                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}
