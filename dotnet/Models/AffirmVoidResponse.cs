namespace Affirm.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    #nullable enable
    public class AffirmVoidResponse
    {
        // Success response fields
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("created")]
        public DateTime? Created { get; set; }

        [JsonProperty("amount")]
        public int? Amount { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        // Error response fields
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("status_code")]
        public string? StatusCode { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }

}

