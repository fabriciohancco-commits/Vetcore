using Microsoft.AspNetCore.Mvc;
using Vetcare.Application.Common;

namespace Vetcare.API.Controllers;

internal static class ControllerResultMapper
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, OperationResult<T> result)
    {
        if (result.StatusCode == 400 && result.Errors is not null)
        {
            foreach (var (field, messages) in result.Errors)
                foreach (var message in messages)
                    controller.ModelState.AddModelError(field, message);
            return controller.ValidationProblem();
        }

        return result.StatusCode switch
        {
            200 => controller.Ok(result.Value),
            204 => controller.NoContent(),
            400 => controller.BadRequest(new { message = result.Message }),
            404 => controller.NotFound(),
            409 => controller.Conflict(new { message = result.Message }),
            _ => controller.StatusCode(result.StatusCode)
        };
    }
}
