namespace Affirm.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AffirmReadRequest
    {
        public string public_key { get; set; }
        public string private_key { get; set; }
    }
}
