using AlvivaAPI.Models.Request.TGS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Services;
using System.Security.Claims;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private static AdminService? _adminService;
        public AdminController()
        {
            _adminService = _adminService ?? new AdminService();
        }

        [HttpGet]
        [Authorize]
        public IActionResult VerifyAdmin()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var response = _adminService.VerifyUserIsAdmin(user);
                return (response) ? StatusCode(204) : StatusCode(401);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        [Route("teams/tgs/copy")]
        [Authorize]
        public async Task<IActionResult> CopyTeam([FromBody] CopyTeamRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var result = await _adminService.CopyTeamByID(user, request.OrgID, request.EventID, request.TeamID, request.ClubID);
                return StatusCode(result.StatusCode, result.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        [Route("teams/tgs")]
        [Authorize]
        public async Task<IActionResult> GetTeam([FromBody] CopyTeamRequest request)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var result = await _adminService.GetFullTeam(user, request.OrgID, request.EventID, request.TeamID, request.ClubID);
                return StatusCode(result.StatusCode, result.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        [Route("teams/upload")]
        [Authorize]
        public IActionResult UploadTeamAsCSV()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var result = _adminService.UploadTeamAsCSV(user, Request.Form.Files);
                return StatusCode(result.StatusCode, result.Data);
            }
            catch (Exception ex) { return StatusCode(500); }
        }
    }
}
