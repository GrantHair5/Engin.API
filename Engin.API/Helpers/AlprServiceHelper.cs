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
using Serilog;

namespace Engin.API.Helpers
{
    public class AlprServiceHelper
    {
        private readonly EnginSettings _settings;
        private readonly Serilog.ILogger _logger;

        public AlprServiceHelper(IOptions<EnginSettings> settings, ILogger logger)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<AlprResult> CallAlprService(string base64Image, Guid uniqueId)
        {
            using (var client = new HttpClient())
            {
                var obj = new
                {
                    Base64Image = base64Image
                };

                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_settings.AlprUrl, content);

                _logger.Debug($"Request Id: {uniqueId} Response from ALPR - {response.StatusCode} , {response.ReasonPhrase}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var buffer = await response.Content.ReadAsByteArrayAsync();
                var byteArray = buffer.ToArray();
                var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                var result = JsonConvert.DeserializeObject<AlprResult>(responseString);
                return result;
            }
        }
    }
}