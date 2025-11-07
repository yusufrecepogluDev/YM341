using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ClupApi.Models;
using ClupApi.Services;
using System.ComponentModel.DataAnnotations;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T? result, string? errorMessage = null)
        {
            if (result == null)
            {
                return NotFound(ApiResponse.ErrorResponse(
                    errorMessage ?? "Resource not found", 
                    new[] { "The requested resource could not be found" }));
            }

            return Ok(ApiResponse<T>.SuccessResponse(result));
        }

        protected IActionResult HandleValidationErrors(ModelStateDictionary modelState)
        {
            var errors = modelState
                .SelectMany(x => x.Value?.Errors ?? new ModelErrorCollection())
                .Select(x => x.ErrorMessage)
                .ToArray();

            return BadRequest(ApiResponse.ValidationErrorResponse(errors));
        }

        protected IActionResult HandleValidationResult(ValidationResult validationResult)
        {
            if (validationResult != null && validationResult != ValidationResult.Success)
            {
                return BadRequest(ApiResponse.ValidationErrorResponse(new[] { validationResult.ErrorMessage ?? "Validation failed" }));
            }

            return Ok();
        }


        protected IActionResult HandleCreatedResult<T>(T result, string actionName, object routeValues)
        {
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.SuccessResponse(result, "Resource created successfully"));
        }

        protected IActionResult HandleUpdatedResult<T>(T result)
        {
            return Ok(ApiResponse<T>.SuccessResponse(result, "Resource updated successfully"));
        }

        protected IActionResult HandleDeletedResult()
        {
            return Ok(ApiResponse.SuccessResponse("Resource deleted successfully"));
        }

        protected IActionResult HandleBadRequest(string message, string[] errors)
        {
            return BadRequest(ApiResponse.ErrorResponse(message, errors));
        }

        protected IActionResult HandleNotFound(string message = "Resource not found")
        {
            return NotFound(ApiResponse.ErrorResponse(message, new[] { "The requested resource could not be found" }));
        }

        protected IActionResult HandleInternalServerError(string message = "An internal server error occurred")
        {
            return StatusCode(500, ApiResponse.ErrorResponse(message, new[] { "Please try again later" }));
        }

        protected IActionResult HandleServerError(string message)
        {
            return StatusCode(500, ApiResponse.ErrorResponse(message, new[] { "Please try again later" }));
        }

        protected IActionResult HandleBadRequest(string message)
        {
            return BadRequest(ApiResponse.ErrorResponse(message, new[] { message }));
        }

        protected IActionResult HandleCreated<T>(T result, string location)
        {
            Response.Headers.Append("Location", location);
            return StatusCode(201, ApiResponse<T>.SuccessResponse(result, "Resource created successfully"));
        }

        protected IActionResult HandleNoContent()
        {
            return NoContent();
        }
    }
}