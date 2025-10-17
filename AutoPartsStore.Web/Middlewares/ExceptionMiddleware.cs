using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Models.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace AutoPartsStore.Web.Middlewares
{
    /// <summary>
    /// Global exception handler middleware
    /// Catches all unhandled exceptions and returns standardized error responses
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Check if response has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Cannot handle exception - response has already started. Exception: {ExceptionType}: {Message}",
                    exception.GetType().Name,
                    exception.Message);
                return;
            }

            var errorResponse = CreateErrorResponse(context, exception);

            // Log the exception with appropriate severity
            LogException(exception, errorResponse);

            // Set response
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = errorResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment(),
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }

        private ErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
        {
            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            // Handle different exception types
            switch (exception)
            {
                case ValidationException validationEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = validationEx.ErrorCode;
                    errorResponse.Message = validationEx.Message;
                    errorResponse.Errors = validationEx.Errors;
                    errorResponse.ValidationErrors = validationEx.ValidationErrors;
                    break;

                case BusinessException businessEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = businessEx.ErrorCode;
                    errorResponse.Message = businessEx.Message;
                    errorResponse.AdditionalData = businessEx.AdditionalData;
                    break;

                case NotFoundException notFoundEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = notFoundEx.ErrorCode;
                    errorResponse.Message = notFoundEx.Message;
                    errorResponse.AdditionalData = new Dictionary<string, object>();
                    if (notFoundEx.ResourceType != null)
                        errorResponse.AdditionalData["resourceType"] = notFoundEx.ResourceType;
                    if (notFoundEx.ResourceId != null)
                        errorResponse.AdditionalData["resourceId"] = notFoundEx.ResourceId;
                    break;

                case ConflictException conflictEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.ErrorCode = conflictEx.ErrorCode;
                    errorResponse.Message = conflictEx.Message;
                    errorResponse.AdditionalData = new Dictionary<string, object>();
                    if (conflictEx.ConflictingField != null)
                        errorResponse.AdditionalData["conflictingField"] = conflictEx.ConflictingField;
                    if (conflictEx.ConflictingValue != null)
                        errorResponse.AdditionalData["conflictingValue"] = conflictEx.ConflictingValue;
                    break;

                case UnauthorizedException unauthorizedEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = unauthorizedEx.ErrorCode;
                    errorResponse.Message = unauthorizedEx.Message;
                    break;

                case ForbiddenException forbiddenEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.ErrorCode = forbiddenEx.ErrorCode;
                    errorResponse.Message = forbiddenEx.Message;
                    errorResponse.AdditionalData = new Dictionary<string, object>();
                    if (forbiddenEx.RequiredRole != null)
                        errorResponse.AdditionalData["requiredRole"] = forbiddenEx.RequiredRole;
                    if (forbiddenEx.RequiredPermission != null)
                        errorResponse.AdditionalData["requiredPermission"] = forbiddenEx.RequiredPermission;
                    break;

                case ExternalServiceException externalEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    errorResponse.ErrorCode = externalEx.ErrorCode;
                    errorResponse.Message = externalEx.Message;
                    errorResponse.AdditionalData = new Dictionary<string, object>();
                    if (externalEx.ServiceName != null)
                        errorResponse.AdditionalData["serviceName"] = externalEx.ServiceName;
                    if (externalEx.ServiceErrorCode != null)
                        errorResponse.AdditionalData["serviceErrorCode"] = externalEx.ServiceErrorCode;
                    break;

                case DatabaseException dbEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = dbEx.ErrorCode;
                    errorResponse.Message = _environment.IsDevelopment()
                        ? dbEx.Message
                        : "حدث خطأ في قاعدة البيانات. يرجى المحاولة لاحقاً.";
                    break;

                // Handle EF Core and SQL exceptions
                case DbUpdateException dbUpdateEx:
                    errorResponse = HandleDbUpdateException(dbUpdateEx);
                    break;

                case SqlException sqlEx:
                    errorResponse = HandleSqlException(sqlEx);
                    break;

                case TimeoutException timeoutEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse.ErrorCode = "REQUEST_TIMEOUT";
                    errorResponse.Message = "انتهت مهلة الطلب. يرجى المحاولة مرة أخرى.";
                    break;

                case UnauthorizedAccessException unauthorizedAccessEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = "UNAUTHORIZED_ACCESS";
                    errorResponse.Message = "غير مصرح. يرجى تسجيل الدخول.";
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = "RESOURCE_NOT_FOUND";
                    errorResponse.Message = keyNotFoundEx.Message;
                    break;

                case ArgumentException argumentEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "INVALID_ARGUMENT";
                    errorResponse.Message = argumentEx.Message;
                    break;

                case InvalidOperationException invalidOpEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "INVALID_OPERATION";
                    errorResponse.Message = invalidOpEx.Message;
                    break;

                case InternalServerException internalEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = internalEx.ErrorCode;
                    errorResponse.Message = internalEx.Message;
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                    errorResponse.Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً.";
                    break;
            }

            // Add stack trace and inner exception details in development
            if (_environment.IsDevelopment())
            {
                errorResponse.StackTrace = exception.StackTrace;
                errorResponse.InnerException = exception.InnerException?.Message;
            }

            return errorResponse;
        }

        private ErrorResponse HandleDbUpdateException(DbUpdateException exception)
        {
            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ErrorCode = "DATABASE_UPDATE_ERROR"
            };

            // Check for specific SQL Server errors
            if (exception.InnerException is SqlException sqlException)
            {
                return HandleSqlException(sqlException);
            }

            errorResponse.Message = _environment.IsDevelopment()
                ? exception.Message
                : "فشل تحديث قاعدة البيانات. يرجى التحقق من البيانات المدخلة.";

            return errorResponse;
        }

        private ErrorResponse HandleSqlException(SqlException sqlException)
        {
            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ErrorCode = "SQL_ERROR"
            };

            // Handle specific SQL Server error codes
            switch (sqlException.Number)
            {
                case 2627: // Unique constraint violation
                case 2601: // Duplicate key
                    errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.ErrorCode = "DUPLICATE_ENTRY";
                    errorResponse.Message = "العنصر موجود بالفعل. يرجى استخدام قيمة مختلفة.";
                    break;

                case 547: // Foreign key constraint violation
                    errorResponse.ErrorCode = "FOREIGN_KEY_VIOLATION";
                    errorResponse.Message = "لا يمكن تنفيذ العملية بسبب ارتباط هذا العنصر بعناصر أخرى.";
                    break;

                case 515: // NULL value violation
                    errorResponse.ErrorCode = "NULL_VALUE_ERROR";
                    errorResponse.Message = "القيمة المطلوبة مفقودة. يرجى توفير جميع الحقول الإلزامية.";
                    break;

                case -1: // Timeout
                case -2:
                    errorResponse.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse.ErrorCode = "DATABASE_TIMEOUT";
                    errorResponse.Message = "انتهت مهلة الاتصال بقاعدة البيانات. يرجى المحاولة مرة أخرى.";
                    break;

                case 4060: // Cannot open database
                case 18456: // Login failed
                    errorResponse.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    errorResponse.ErrorCode = "DATABASE_CONNECTION_ERROR";
                    errorResponse.Message = "فشل الاتصال بقاعدة البيانات. يرجى المحاولة لاحقاً.";
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = _environment.IsDevelopment()
                        ? sqlException.Message
                        : "حدث خطأ في قاعدة البيانات. يرجى المحاولة لاحقاً.";
                    break;
            }

            return errorResponse;
        }

        private void LogException(Exception exception, ErrorResponse errorResponse)
        {
            var logLevel = errorResponse.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            _logger.Log(
                logLevel,
                exception,
                "Exception caught: {ErrorCode} - {Message}. TraceId: {TraceId}, Path: {Path}",
                errorResponse.ErrorCode,
                errorResponse.Message,
                errorResponse.TraceId,
                errorResponse.Path);
        }
    }
}