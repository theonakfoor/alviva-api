using Microsoft.AspNetCore.Mvc;

using PlayMakerAPI.Services;
using Microsoft.AspNetCore.Cors;
using PlayMakerAPI.Models.Request;

namespace PlayMakerAPI.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("visitors")]
    [ApiController]
    public class VisitorController : Controller
    {
        private static VisitorService? _visitorService;

        public VisitorController()
        {
            _visitorService = _visitorService ?? new VisitorService();
        }

        [HttpPost]
        public IActionResult GetFormInformationByAccessKey([FromBody] VisitRequest request)
        {
            try
            {
                bool result = _visitorService.LogVisitByIP(request);

                if (result)
                    return StatusCode(200);
                else
                    return StatusCode(500);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
