using Engin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Engin.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Engin")]
    public class EnginController : Controller
    {
        // POST: api/Engin
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]Models.Engin item)
        {
            var bitmapData = Convert.FromBase64String(FixBase64ForImage(item.Image));

            const string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/88f75a64-c7f5-45aa-99f2-451e672293cd/image";

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "a33145a3ed154e4ca1d14a1b7e52f6ba");

            var results = new List<Predictions>();

            using (var content = new ByteArrayContent(bitmapData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await client.PostAsync(url, content);
                var resp = await response.Content.ReadAsStringAsync();
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(resp);

                results.AddRange(
                    from r in result.Predictions
                    where Convert.ToDouble(r.Probability) > 0.89 && r.Tag != "car"
                    select new Predictions(r.TagId, r.Tag, r.Probability)
                    );
            }
            return Ok(results);
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