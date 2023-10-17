using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Services;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("teams")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private static TeamService? _teamService;
        public TeamController()
        {
            _teamService = _teamService ?? new TeamService();
        }

        [HttpGet]
        public IActionResult ListTeams([FromQuery] int offset = 0)
        {
            try
            {
                var response = _teamService.GetAllTeams(offset);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetTeamByID(int id)
        {
            try
            {
                var response = _teamService.GetTeamByID(id, true);
                return StatusCode(response.StatusCode, response.Data);
            } catch(Exception ex) { return StatusCode(500); }
        }

        [HttpGet]
        [Route("{id}/playercards")]
        public IActionResult GetPlayerCardsByID(int id)
        {
            try
            {
                var response = _teamService.GetPlayerCardsByID(id);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }
    }
}
