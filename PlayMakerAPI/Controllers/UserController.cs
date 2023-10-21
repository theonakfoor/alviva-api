using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Services;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static UserService? _userService;
        public UserController()
        {
            _userService = _userService ?? new UserService();
        }

        [HttpPost]
        [Route("{userId}/{matchId}/rating")]
        public IActionResult ListTeams(string userId, string matchId, [FromBody] RatingRequest request)
        {
            try
            {
                var response = _userService.SubmitHostRating(userId, matchId, request.Rating);
                return (response) ? StatusCode(204) : StatusCode(500);
            } catch (Exception ex) { return StatusCode(500); }
        }

    }
}
