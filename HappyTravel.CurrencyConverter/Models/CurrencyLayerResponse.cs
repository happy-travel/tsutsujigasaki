using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.CurrencyConverter.Models
{
    #nullable disable
    public readonly struct CurrencyLayerResponse
    {
        [JsonConstructor]
        public CurrencyLayerResponse(ErrorContainer error, bool isSuccessful, Dictionary<string, decimal> quotes)
        {
            Error = error;
            IsSuccessful = isSuccessful;
            Quotes = quotes;
        }


        [JsonProperty("error")]
        public ErrorContainer Error { get; }
        [JsonProperty("success")]
        public bool IsSuccessful { get; }
        [JsonProperty("quotes")]
        public Dictionary<string, decimal> Quotes { get; }


        public readonly struct ErrorContainer
        {
            [JsonConstructor]
            public ErrorContainer(int code, string message)
            {
                Code = code;
                Message = message;
            }


            [JsonProperty("code")]
            public int Code { get; }
            [JsonProperty("info")]
            public string Message { get; }
        }
    }
    #nullable restore
}
