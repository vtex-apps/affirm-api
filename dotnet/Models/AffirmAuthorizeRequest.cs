namespace Affirm.Models
{
    public class AffirmAuthorizeRequest
    {
        public string checkout_token { get; set; }
        public string order_id { get; set; }
    }
}
