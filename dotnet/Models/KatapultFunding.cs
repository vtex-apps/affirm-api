using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Affirm.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class KatapultFunding
    {
        public string start { get; set; }
        public string end { get; set; }
        public string date_filter { get; set; }
        public long application_id { get; set; }
        public long limit { get; set; }
        public long offset { get; set; }
        public bool refund { get; set; }
        public bool payment_settled { get; set; }
        public string order_id { get; set; }
    }
}
