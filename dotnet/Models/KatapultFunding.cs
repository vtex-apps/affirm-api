using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Affirm.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class KatapultFunding
    {
        [JsonProperty("funding_report")]
        public FundingReport FundingReport { get; set; }
    }

    public class FundingReport
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("objects")]
        public List<FundingObject> FundingObjects { get; set; }
    }

    public class Meta
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }

    public class FundingObject
    {
        [JsonProperty("application_id")]
        public long ApplicationId { get; set; }

        [JsonProperty("buyout_fee")]
        public decimal BuyoutFee { get; set; }

        [JsonProperty("consumer_discount")]
        public decimal ConsumerDiscount { get; set; }

        [JsonProperty("delivery")]
        public decimal Delivery { get; set; }

        [JsonProperty("delivery_date")]
        public string DeliveryDate { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; }

        [JsonProperty("effective_date")]
        public string EffectiveDate { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("funded")]
        public string Funded { get; set; }

        [JsonProperty("funded_date")]
        public string FundedDate { get; set; }

        [JsonProperty("funding_id")]
        public long FundingId { get; set; }

        [JsonProperty("funding_required_items_status")]
        public string FundingRequiredItemsStatus { get; set; }

        [JsonProperty("gross_funding_amount")]
        public decimal GrossFundingAmount { get; set; }

        [JsonProperty("interchange_payable")]
        public decimal InterchangePayable { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("leasable")]
        public decimal Leasable { get; set; }

        [JsonProperty("lease_status")]
        public string LeaseStatus { get; set; }

        [JsonProperty("net_funding_amount")]
        public decimal NetFundingAmount { get; set; }

        [JsonProperty("nonleasable")]
        public decimal Nonleasable { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("origination_date")]
        public DateTimeOffset OriginationDate { get; set; }

        [JsonProperty("rebate")]
        public decimal Rebate { get; set; }

        [JsonProperty("store")]
        public string Store { get; set; }

        [JsonProperty("transaction_detail")]
        public string TransactionDetail { get; set; }
    }
}
