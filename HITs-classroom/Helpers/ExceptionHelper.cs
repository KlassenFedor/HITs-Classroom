using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Helpers
{
    static class ExceptionHelper
    {
        public static IActionResult HandleException(Exception e)
        {
            return StatusCodeResult(500, "ei");
        }
    }
}
