using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<AlprResults> CallAlprService(Models.Engin item)
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
                return JsonConvert.DeserializeObject<AlprResults>(responseString);
            }
        }
    }
}