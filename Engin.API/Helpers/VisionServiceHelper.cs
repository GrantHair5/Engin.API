using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Engin.API.Configuration;
using Engin.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Engin.API.Helpers
{
    public class VisionServiceHelper
    {
        private readonly EnginSettings _settings;

        public VisionServiceHelper(IOptions<EnginSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<List<Predictions>> CallVisionService(Models.Engin item)
        {
            using (var client = new HttpClient())
            {
                var bitmapData = Convert.FromBase64String(FixBase64ForImage(item.Image));
                var results = new List<Predictions>();

                using (var visionContent = new ByteArrayContent(bitmapData))
                {
                    visionContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var visionResponse = await client.PostAsync(_settings.VisionApiUrl, visionContent);
                    if (visionResponse.IsSuccessStatusCode)
                    {
                        var resp = await visionResponse.Content.ReadAsStringAsync();
                        var visionResult = JsonConvert.DeserializeObject<Rootobject>(resp);

                        results.AddRange(
                            from r in visionResult.tags
                            where Convert.ToDouble(r.confidence) > 0.89 && r.name == "car"
                            select new Predictions(r.name, r.confidence.ToString(CultureInfo.InvariantCulture))
                        );
                    }
                    else
                    {
                        throw new Exception("Error in azure vison api");
                    }

                    return results;
                }
            }
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