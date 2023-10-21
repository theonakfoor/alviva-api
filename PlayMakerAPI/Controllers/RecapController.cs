using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Services;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("recaps")]
    [ApiController]
    public class RecapController : ControllerBase
    {
        private static RecapService? _recapService;
        public RecapController()
        {
            _recapService = _recapService ?? new RecapService();
        }

        [HttpGet]
        public IActionResult ListRecaps([FromQuery] int offset)
        {
            try
            {
                var response = _recapService.GetAllRecaps(offset);
                return StatusCode(response.StatusCode, response.Data);
            } catch (Exception ex) { return StatusCode(500); }
        }

        [HttpPost]
        public IActionResult FetchRecaps([FromBody] FetchRecapRequest request)
        {
            try
            {
                var response = _recapService.FetchRecaps(request.recapIDs);
                return StatusCode(response.StatusCode, response.Data);
            } catch(Exception ex) { return StatusCode(500); }
        }

    }
}
