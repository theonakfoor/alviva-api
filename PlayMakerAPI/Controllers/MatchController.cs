using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Services;
using System.Security.Claims;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("matches")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private static MatchService? _matchService;
        private static UserService? _userService;
        public MatchController()
        {
            _matchService = _matchService ?? new MatchService();
            _userService = _userService ?? new UserService();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = _matchService.CreateMatch(user, request);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMatchByID(string id, [FromBody] UpdateMatchRequest request)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
            var response = await _matchService.UpdateMatchById(user, id, request);
            return (response) ? StatusCode(204) : StatusCode(401);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMatches([FromQuery] int dateOffset, [FromQuery] int offset = 0, [FromQuery] string type = "upcoming")
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = _matchService.GetAllMatches(dateOffset, offset, type, user, null);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMatchByID(string id)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = _matchService.DeleteMatchById(user, id);
                return (response) ? StatusCode(204) : StatusCode(401);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpGet]
        [Authorize]
        [Route("scorekeeper/{id}")]
        public async Task<IActionResult> GetMatchByIDAuthorized(string id)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = _matchService.GetMatchById(user, id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPut]
        [Authorize]
        [Route("{id}/attendance")]
        public async Task<IActionResult> ProcessMatchAttendance(string id, [FromBody] AttendanceRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = await _matchService.ProcessAttendance(user, id, request);
                return (response) ? StatusCode(204) : StatusCode(401);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        [Authorize]
        [Route("{id}/events")]
        public async Task<IActionResult> AddMatchEvent(string id, [FromBody] NewEventRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = await _matchService.AddMatchEvent(user, id, request);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPut]
        [Authorize]
        [Route("{id}/events/{eventId}")]
        public async Task<IActionResult> UpdateMatchEvent(string id, string eventId, [FromBody] NewEventRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = await _matchService.UpdateMatchEvent(user, id, eventId, request);
                return (response) ? StatusCode(201) : StatusCode(401);
            }
            catch(Exception ex) { return StatusCode(500); }
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}/events/{eventId}")]
        public async Task<IActionResult> DeleteMatchEvent(string id, string eventId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                await _userService.VerifyOrInsertUser(user, Request.Headers[HeaderNames.Authorization]);
                var response = await _matchService.DeleteMatchEvent(user, id, eventId);
                return (response) ? StatusCode(204) : StatusCode(401);
            }
            catch(Exception ex) { return StatusCode(500); }
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetMatchByID(string id)
        {
            try
            {
                var response = _matchService.GetMatchById(id);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500);  }
        }

        [HttpPost]
        [Route("list")]
        public IActionResult ListMatches([FromQuery] int dateOffset, [FromBody] ListMatchesRequest? request = null, [FromQuery] int offset = 0, [FromQuery] string type = "upcoming")
        {
            try
            {
                var response = _matchService.GetAllMatches(dateOffset, offset, type, null, request);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }
    }
}
