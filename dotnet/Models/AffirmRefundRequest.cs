namespace Affirm.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AffirmRefundRequest
    {
        public int amount { get; set; }
    }
}
