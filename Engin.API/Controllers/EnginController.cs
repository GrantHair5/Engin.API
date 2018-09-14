using Engin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Engin.API.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Engin.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Engin")]
    public class EnginController : Controller
    {
        private readonly EnginSettings _settings;

        public EnginController(IOptions<EnginSettings> settings)
        {
            _settings = settings.Value;
        }

        // POST: api/Engin
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Engin item)
        {
            AlprResults result;

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(item.Image);

                    var response = await client
                        .PostAsync(
                            _settings.AlprUrl +
                            _settings.AlprSecret, content)
                        .ConfigureAwait(false);

                    var buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var byteArray = buffer.ToArray();
                    var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    result = JsonConvert.DeserializeObject<AlprResults>(responseString);
                }
            }
            catch (Exception ex)
            {
                return Ok("Failed in OCR Call");
            }

            if ((result.Results[0].Confidence < 89))
            {
                return NotFound();
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var hpiResponse = await client.GetAsync(
                        $"{_settings.HpiUrl}{result.Results[0].Plate}");

                    if (hpiResponse.IsSuccessStatusCode)
                    {
                        var hpiBuffer = await hpiResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        var hpiByteArray = hpiBuffer.ToArray();
                        var hpiResponseString = Encoding.UTF8.GetString(hpiByteArray, 0, hpiByteArray.Length);
                        var hpiResult = JsonConvert.DeserializeObject<HpiResults>(hpiResponseString);

                        if (hpiResult.Model != null)
                        {
                            var enginResponse = new Response
                            {
                                Manufacturer = hpiResult.Model.Make,
                                Model = hpiResult.Model.Model,
                                Registration = hpiResult.Model.RegNumber,
                                Colour = hpiResult.Model.Colour,
                                ChassisNumber = hpiResult.Model.ChassisNumber,
                                CapCode = hpiResult.Model.CapCode
                            };
                            return Ok(enginResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("Failed within HPI Check");//do logs here
            }

            return NotFound();

            ////HPI lookup failed, checking Azure Vision API
            //try
            //{
            //    using (var client = new HttpClient())
            //    {
            //        var bitmapData = Convert.FromBase64String(FixBase64ForImage(item.Image));

            //        const string url =
            //            "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/88f75a64-c7f5-45aa-99f2-451e672293cd/image";

            //        client.DefaultRequestHeaders.Add("Prediction-Key", "a33145a3ed154e4ca1d14a1b7e52f6ba");

            //        var results = new List<Predictions>();

            //        using (var visionContent = new ByteArrayContent(bitmapData))
            //        {
            //            visionContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //            var visionResponse = await client.PostAsync(url, visionContent);
            //            if (visionResponse.IsSuccessStatusCode)
            //            {
            //                var resp = await visionResponse.Content.ReadAsStringAsync();
            //                var visionResult = JsonConvert.DeserializeObject<Result>(resp);

            //                results.AddRange(
            //                    from r in visionResult.Predictions
            //                    where Convert.ToDouble(r.Probability) > 0.89 && r.Tag != "car"
            //                    select new Predictions(r.TagId, r.Tag, r.Probability)
            //                );

            //                var enginResponseFallback = new Response();
            //                switch (results.Count)
            //                {
            //                    case 1:
            //                        enginResponseFallback.Manufacturer = results[0].Tag;
            //                        break;

            //                    case 2:
            //                        enginResponseFallback.Manufacturer = results[0].Tag;
            //                        enginResponseFallback.Model = results[1].Tag;
            //                        break;

            //                    default:
            //                        break;
            //                }

            //                if (enginResponseFallback.Manufacturer == null)
            //                {
            //                    return NotFound();
            //                }

            //                return Ok(enginResponseFallback);
            //            }
            //        }

            //        return NotFound();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return Ok("Failed inside Azure Vision Api");
            //}
        }

        //private static string FixBase64ForImage(string image)
        //{
        //    var sbText = new StringBuilder(image, image.Length);
        //    sbText.Replace("\r\n", string.Empty);
        //    sbText.Replace(" ", string.Empty);
        //    return sbText.ToString();
        //}
    }
}