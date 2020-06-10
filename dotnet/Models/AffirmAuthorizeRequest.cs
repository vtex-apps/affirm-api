using Newtonsoft.Json;

namespace Affirm.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AffirmAuthorizeRequest
    {
        public string transaction_id { get; set; }
        public string order_id { get; set; }
    }
}
