using AutoPartsStore.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message = "تمت العملية بنجاح")
        {
            return Ok(ApiResponse<T>.SuccessResult(data, message));
        }

        protected IActionResult Success(string message = "تمت العملية بنجاح")
        {
            return Ok(ApiResponse.SuccessResult(message));
        }

        protected IActionResult BadRequest(string message, List<string> errors = null)
        {
            return BadRequest(ApiResponse.FailureResult(message, errors));
        }

        protected IActionResult NotFound(string message = "لم يتم العثور على المورد")
        {
            return NotFound(ApiResponse.FailureResult(message));
        }

        protected IActionResult Unauthorized(string message = "غير مصرح بالوصول")
        {
            return Unauthorized(ApiResponse.FailureResult(message));
        }

        protected IActionResult Forbidden(string message = "ممنوع الوصول")
        {
            return StatusCode(403, ApiResponse.FailureResult(message));
        }

        protected IActionResult InternalServerError(string message = "حدث خطأ داخلي في الخادم")
        {
            return StatusCode(500, ApiResponse.FailureResult(message));
        }
    }
}