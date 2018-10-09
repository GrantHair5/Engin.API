using Engin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Engin.API.Helpers;

namespace Engin.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Engin")]
    public class EnginController : Controller
    {
        private readonly VisionServiceHelper _visionHelper;
        private readonly AlprServiceHelper _alprHelper;
        private readonly HpiServiceHelper _hpiHelper;

        public EnginController(VisionServiceHelper visionHelper, AlprServiceHelper alprHelper, HpiServiceHelper hpiHelper)
        {
            _visionHelper = visionHelper;
            _alprHelper = alprHelper;
            _hpiHelper = hpiHelper;
        }

        // POST: api/Engin
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Engin item)
        {
            //Call Vision API to determine if image is a vehicle
            try
            {
                var visionResult = await _visionHelper.CallVisionService(item);
                if (visionResult.Count == 0)
                {
                    return BadRequest("Please send an image of a vehicle");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            //Successfully identified that the image is a vehicle, attempt to read the reg plate
            AlprResults result;

            try
            {
                result = await _alprHelper.CallAlprService(item);

                if (result != null)
                {
                    if ((result.Results[0].Confidence < 89))
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
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