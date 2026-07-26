//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CMS.Core.DTOs
{
    public class StoreViewModel
    {
        [JsonProperty(PropertyName = "storeNumber")]
        public int StoreNumber { get; set; }
        public string Remarks { get; set; }
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        [JsonProperty(PropertyName = "provinceAbbr")]
        public string ProvinceAbbr { get; set; }
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }
        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string eMail { get; set; }
        public DateTime? DateOpened { get; set; }
        public DateTime? DateClosed { get; set; }
        public byte? NumberofBays { get; set; }
        public string POSEMAIL { get; set; }
        public DateTime? DateTransferTo { get; set; }
        public DateTime? DateTransferFrom { get; set; }
        public string GeoGroupName { get; set; }
        public string Operator { get; set; }
        public string Franch_name { get; set; }
        public string LP_Manager_Fullname { get; set; }
        public string LP_Manager_Title { get; set; }
        public string LP_Manager_Title_Fr { get; set; }
        public string LP_Hours_French { get; set; }
        public string LP_Hours_English { get; set; }
        public string StoreName { get; set; }
        public string StoreEmail { get; set; }
        public string LP_Language { get; set; }
        public DateTime? LastModifiedForETL { get; set; }
        public string StoreStatus { get; set; }
        public int? MarketingRegionId { get; set; }
        public int? OldStoreNumber { get; set; }
        public string StoreType { get; set; }
        [JsonProperty(PropertyName = "siteName")]
        public string SiteName { get; set; }
        public string StoreOrigin { get; set; }
        public string BuildingType { get; set; }
        public decimal? SquareFeet { get; set; }
        [JsonProperty(PropertyName = "storeLatitude")]
        public string StoreLatitude { get; set; }
        [JsonProperty(PropertyName = "storeLongitude")]
        public string StoreLongitude { get; set; }
        public string ClosureReason { get; set; }
        public string PipelineActionables { get; set; }
        public string LastModifiedUser { get; set; }
        public string LastModifiedApp { get; set; }
        //public bool? ComingSoon { get; set; }
        public string HolidayNotes { get; set; }
        public double Distance { get; set; }
        public bool ShopTires { get; set; }
        public string TireConnectID { get; set; }
        public string RebateWidgetApiKey { get; set; }
        public string storeImage { get; set; }
        public string LP_Hours_French_content { get; set; }
        public string LP_Hours_English_content { get; set; }
        public string ProvinceFullName { get; set; }

    }
}
