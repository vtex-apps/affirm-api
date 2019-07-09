namespace Affirm.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AffirmUpdateRequest
    {
        public string order_id { get; set; }
        public string shipping_carrier { get; set; }
        public string shipping_confirmation { get; set; }
        public string shipping { get; set; }
    }
}
