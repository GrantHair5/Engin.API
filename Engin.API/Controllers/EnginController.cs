using Engin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Engin.API.Helpers;
using Serilog;

namespace Engin.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Engin")]
    public class EnginController : Controller
    {
        private readonly AlprServiceHelper _alprHelper;
        private readonly HpiServiceHelper _hpiHelper;
        private readonly ILogger _logger;

        public EnginController(AlprServiceHelper alprHelper, HpiServiceHelper hpiHelper, ILogger logger)
        {
            _alprHelper = alprHelper;
            _hpiHelper = hpiHelper;
            _logger = logger;
        }

        // POST: api/Engin
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Engin item)
        {
            AlprResult result;
            try
            {
                _logger.Debug("Received image");

                result = await _alprHelper.CallAlprService(item);

                if (result != null)
                {
                    if ((result.Confidence < 75))
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return BadRequest("Please send an image of a vehicle");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            //Successfully read number plate, call vehicle lookup service with reg number
            try
            {
                var response = await _hpiHelper.CallHpiService(result);
                if (response != null)
                {
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return NotFound();
        }
    }
}