using AlvivaAPI.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Services;
using System.Security.Claims;

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

        [HttpPost]
        [Authorize]
        [Route("{id}")]
        public IActionResult UpdatePlayerInformation(Int32 id, [FromBody] UpdatePlayerRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                bool result = _playerService.UpdatePlayerByID(id, user, request);
                return (result) ? StatusCode(204) : StatusCode(401);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}")]
        public IActionResult DeletePlayer(Int32 id)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                bool result = _playerService.DeletePlayerByID(id, user);
                return (result) ? StatusCode(204) : StatusCode(401);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreatePlayers([FromBody] UpdatePlayerRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var result = _playerService.CreatePlayer(user, request);
                return StatusCode(result.StatusCode, result.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpGet]
        [Route("{id}/statistics")]
        public IActionResult GetPlayerStatisticsByID(int id)
        {
            try
            {
                var response = _playerService.GetPlayerStatistics(id);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
