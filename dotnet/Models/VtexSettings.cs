namespace Affirm.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class VtexSettings
    {
        public bool isLive { get; set; }
        public string companyName { get; set; }
        public string publicApiKey { get; set; }
        public int delayToAutoSettle { get; set; }
        public int delayToAutoSettleAfterAntifraud { get; set; }
        public int delayToCancel { get; set; }
        public string delayInterval { get; set; }
    }
}
