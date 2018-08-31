using Engin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Engin.API.Controllers
{
    [Produces("application/json")]
    public class EnginController : Controller
    {
        // POST: api/Engin
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> PostAsync([FromBody]Models.Engin item)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(item.Image);

                var response = await client
                    .PostAsync(
                        "https://api.openalpr.com/v2/recognize_bytes?recognize_vehicle=1&country=gb&secret_key=" +
                        "sk_e2a0698f47251457aab69e96", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var byteArray = buffer.ToArray();
                    var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    var result = JsonConvert.DeserializeObject<AlprResults>(responseString);
                    if (result.Results[0].Confidence > 89)
                    {
                        var hpiResponse = await client.GetAsync(
                            $"http://dev.api.menupricing.arnoldclark.com/api/Servicing/GetModelGroupDescriptions?registrationNumber={result.Results[0].Plate}");

                        if (hpiResponse.IsSuccessStatusCode)
                        {
                            var hpiBuffer = await hpiResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                            var hpiByteArray = hpiBuffer.ToArray();
                            var hpiResponseString = Encoding.UTF8.GetString(hpiByteArray, 0, hpiByteArray.Length);
                            var hpiResult = JsonConvert.DeserializeObject<HpiResults>(hpiResponseString);

                            if (hpiResult.Vehicle != null)
                            {
                                var model = hpiResult.Vehicle.Model.Substring(0,
                                    hpiResult.Vehicle.Model.IndexOf(" ", StringComparison.Ordinal));
                                var enginResponse = new Response
                                {
                                    Manufacturer = hpiResult.Vehicle.Manufacturer,
                                    Model = model
                                };
                                return Ok(enginResponse);
                            }
                        }
                    }
                }

                //HPI lookup failed, checking Azure Vision API

                var bitmapData = Convert.FromBase64String(FixBase64ForImage(item.Image));

                const string url =
                    "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/88f75a64-c7f5-45aa-99f2-451e672293cd/image";

                client.DefaultRequestHeaders.Add("Prediction-Key", "a33145a3ed154e4ca1d14a1b7e52f6ba");

                var results = new List<Predictions>();

                using (var visionContent = new ByteArrayContent(bitmapData))
                {
                    visionContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var visionResponse = await client.PostAsync(url, visionContent);
                    if (visionResponse.IsSuccessStatusCode)
                    {
                        var resp = await visionResponse.Content.ReadAsStringAsync();
                        var visionResult = JsonConvert.DeserializeObject<Result>(resp);

                        results.AddRange(
                            from r in visionResult.Predictions
                            where Convert.ToDouble(r.Probability) > 0.89 && r.Tag != "car"
                            select new Predictions(r.TagId, r.Tag, r.Probability)
                        );

                        var enginResponseFallback = new Response();
                        switch (results.Count)
                        {
                            case 1:
                                enginResponseFallback.Manufacturer = results[0].Tag;
                                break;

                            case 2:
                                enginResponseFallback.Manufacturer = results[0].Tag;
                                enginResponseFallback.Model = results[1].Tag;
                                break;

                            default:
                                break;
                        }

                        if (enginResponseFallback.Manufacturer == null)
                        {
                            return NotFound();
                        }
                        return Ok(enginResponseFallback);
                    }
                }
            }

            return NotFound();
        }

        private static string FixBase64ForImage(string image)
        {
            var sbText = new StringBuilder(image, image.Length);
            sbText.Replace("\r\n", string.Empty);
            sbText.Replace(" ", string.Empty);
            return sbText.ToString();
        }
    }
}