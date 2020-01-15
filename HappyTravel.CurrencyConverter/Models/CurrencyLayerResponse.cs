using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.CurrencyConverter.Models
{
    #nullable disable
    public class CurrencyLayerResponse
    {
        [JsonProperty("error")]
        public ErrorContainer Error { get; set; }
        [JsonProperty("success")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("quotes")]
        public Dictionary<string, decimal> Quotes { get; set; }


        public class ErrorContainer
        {
            [JsonProperty("code")]
            public int Code { get; set; }
            [JsonProperty("info")]
            public string Message { get; set; }
        }
    }
    #nullable restore
}
