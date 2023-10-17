using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Services;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("players")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private static PlayerService? _playerService;
        public PlayerController()
        {
            _playerService = _playerService ?? new PlayerService();
        }

        [HttpGet]
        [Route("leaderboard")]
        public IActionResult ListTeams([FromQuery] string type = "goals", [FromQuery] int offset = 0)
        {
            try
            {
                var response = _playerService.GetLeaderboard(type, offset);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
