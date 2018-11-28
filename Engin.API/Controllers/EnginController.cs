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

        public EnginController(AlprServiceHelper alprHelper,
            HpiServiceHelper hpiHelper, ILogger logger)
        {
            _alprHelper = alprHelper;
            _hpiHelper = hpiHelper;
            _logger = logger;
        }

        // POST: api/Engin
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Engin request)
        {
            var uniqueId = Guid.NewGuid();
            AlprResult result;
            try
            {
                _logger.Debug($"Request Id: {uniqueId} - Received image");

                result = await _alprHelper.CallAlprService(request.Image, uniqueId);

                if (result != null)
                {
                    _logger.Information($"Request Id: {uniqueId} - Found registration - {result.Registration} , Confidence - {result.Confidence}");
                    if ((result.Confidence < 75))
                    {
                        _logger.Information($"Request Id: {uniqueId} - Found registration - {result.Registration} , Confidence - {result.Confidence} below 75, returning NotFound");

                        return NotFound();
                    }
                }
                else
                {
                    _logger.Information($"Request Id: {uniqueId} - No registration found, returning Bad Request");

                    return BadRequest("Please send an image of a vehicle");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Request Id: {uniqueId} - Error occured trying to detect registration");
                return StatusCode(500, ex.Message);
            }

            //Successfully read number plate, call vehicle lookup service with reg number
            try
            {
                _logger.Debug($"Request Id: {uniqueId} - Found Registration - {result.Registration} , trying to find vehicle using vehicle lookup service");

                var response = await _hpiHelper.CallHpiService(result.Registration);
                if (response != null)
                {
                    _logger.Debug($"Request Id: {uniqueId} - Successfully found a vehicle with Registration - {result.Registration} ");
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Request Id: {uniqueId} - Error occured trying to lookup vehicle from registration found");

                return StatusCode(500, ex.Message);
            }
            _logger.Information($"Request Id: {uniqueId} - No vehicle found with Registration - {result.Registration} , returning NotFound");
            return NotFound();
        }
    }
}