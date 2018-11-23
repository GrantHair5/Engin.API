using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Engin.API.Configuration;
using Engin.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Engin.API.Helpers
{
    public class AlprServiceHelper
    {
        private readonly EnginSettings _settings;

        public AlprServiceHelper(IOptions<EnginSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<AlprResult> CallAlprService(Models.Engin item)
        {
            using (var client = new HttpClient())
            {
                var obj = new { Base64Image = item.Image };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                var response = await client
                    .PostAsync(
                        _settings.AlprUrl, content)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var byteArray = buffer.ToArray();
                var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                return JsonConvert.DeserializeObject<AlprResult>(responseString);
            }
        }
    }
}