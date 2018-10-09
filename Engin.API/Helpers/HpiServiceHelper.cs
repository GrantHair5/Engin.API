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
    public class HpiServiceHelper
    {
        private readonly EnginSettings _settings;

        public HpiServiceHelper(IOptions<EnginSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<Response> CallHpiService(AlprResults result)
        {
            using (var client = new HttpClient())
            {
                var hpiResponse = await client.GetAsync(
                    $"{_settings.HpiUrl}{result.Results[0].Plate}");

                if (!hpiResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Call to HPI failed");
                }

                var hpiBuffer = await hpiResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var hpiByteArray = hpiBuffer.ToArray();
                var hpiResponseString = Encoding.UTF8.GetString(hpiByteArray, 0, hpiByteArray.Length);
                var hpiResult = JsonConvert.DeserializeObject<HpiResults>(hpiResponseString);

                if (hpiResult.Model == null)
                {
                    return null;
                }

                var enginResponse = new Response
                {
                    Make = hpiResult.Model.Make,
                    Model = hpiResult.Model.Model,
                    RegNumber = hpiResult.Model.RegNumber,
                    Colour = hpiResult.Model.Colour,
                    ChassisNumber = hpiResult.Model.ChassisNumber,
                    CapCode = hpiResult.Model.CapCode,
                    Spec = hpiResult.Model.Spec,
                    EngineSize = hpiResult.Model.EngineSize
                };

                return enginResponse;
            }
        }
    }
}