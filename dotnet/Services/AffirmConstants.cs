using System;
using System.Collections.Generic;
using System.Text;

namespace Affirm.Services
{
    public class AffirmConstants
    {
        public const string Sandbox = "https://sandbox.affirm.com/api/v2/";
        public const string Live = " https://api.affirm.com/api/v2/";

        public const string Authorize = "";
        public const string Read = "";
        public const string ReadCharge = "";
        public const string Capture = "";
        public const string Void = "";
        public const string Refund = "";
        public const string Update = "charges/{charge_id}/update";

        public const string SuccessResponseCode = "auth";

        public const string PrivateKeyHeader = "X-PROVIDER-API-AppToken";
        public const string PublicKeyHeader = "X-PROVIDER-API-AppKey";
        public const string IsProduction = "X-Vtex-Workspace-Is-Production";
    }
}
