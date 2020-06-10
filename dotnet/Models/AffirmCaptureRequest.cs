namespace Affirm.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AffirmCaptureRequest
    {
        public int amount { get; set; }
        public string order_id { get; set; }
        public string shipping_carrier { get; set; }
        public string shipping_confirmation { get; set; }
    }
}