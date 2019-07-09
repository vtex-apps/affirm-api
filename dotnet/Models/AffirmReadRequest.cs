namespace Affirm.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AffirmReadRequest
    {
        public string public_key { get; set; }
        public string private_key { get; set; }
    }
}
