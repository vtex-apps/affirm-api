namespace Affirm.Models
{
    public class AffirmAuthorizeRequest
    {
        public string transaction_id { get; set; }
        public string order_id { get; set; }
    }
}
