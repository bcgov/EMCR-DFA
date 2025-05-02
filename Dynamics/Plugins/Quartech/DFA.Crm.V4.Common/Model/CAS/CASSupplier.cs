using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFA.Crm.V3.Common.Model
{
    public class CASSupplierResult
    {
        [JsonPropertyName("items")]
        public List<CASSupplier> Items { get; set; }

        [JsonPropertyName("hasMore")]
        public bool HasMore { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("links")]
        public List<Link> Links { get; set; }
    }

    public class CASSupplier
    {
        [JsonPropertyName("suppliernumber")]
        public string SupplierNumber { get; set; }

        [JsonPropertyName("suppliername")]
        public string SupplierName { get; set; }

        [JsonPropertyName("subcategory")]
        public string SubCategory { get; set; }

        [JsonPropertyName("sin")]
        public string Sin { get; set; }

        [JsonPropertyName("providerid")]
        public string ProviderId { get; set; }

        [JsonPropertyName("businessnumber")]
        public string BusinessNumber { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("supplierprotected")]
        public string SupplierProtected { get; set; }

        [JsonPropertyName("standardindustryclassification")]
        public string StandardIndustryClassification { get; set; }

        [JsonPropertyName("lastupdated")]
        public string LastUpdated { get; set; }

        [JsonPropertyName("supplieraddress")]
        public List<SupplierAddress> SupplierAddress { get; set; }
    }

    public class SupplierAddress
    {
        [JsonPropertyName("suppliersitecode")]
        public string SupplierSiteCode { get; set; }

        [JsonPropertyName("addressline1")]
        public string AddressLine1 { get; set; }

        [JsonPropertyName("addressline2")]
        public string AddressLine2 { get; set; }

        [JsonPropertyName("addressline3")]
        public string AddressLine3 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("province")]
        public string Province { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("postalcode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("emailaddress")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("eftadvicepref")]
        public string EftAdvicePref { get; set; }

        [JsonPropertyName("providerid")]
        public string ProviderId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("siteprotected")]
        public string SiteProtected { get; set; }

        [JsonPropertyName("lastupdated")]
        public string LastUpdated { get; set; }
    }

    public class Link
    {
        [JsonPropertyName("rel")]
        public string Rel { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

}
