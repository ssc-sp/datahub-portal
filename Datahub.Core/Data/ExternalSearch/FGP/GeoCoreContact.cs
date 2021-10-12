using System.Collections.Generic;

namespace Datahub.Core.Data.External.FGP
{
    public class GeoCoreContact
    {
        public string Fax { get; set; }
        public BilingualText Pt { get; set; }
        public BilingualText Email { get; set; }
        public BilingualText Address { get; set; }
        public string HoursOfService { get; set; }
        public string Role { get; set; }
        public OnlineResourceInfo OnlineResource { get; set; }
        public BilingualText Position { get; set; }
        public BilingualText Telephone { get; set; }
        public string City { get; set; }
        public BilingualText Country { get; set; }
        public BilingualText Organisation { get; set; }
        public string Individual { get; set; }
        public string PostalCode { get; set; }

        public class OnlineResourceInfo
        {
            public string OnlineResource_Description { get; set; }
            public string OnlineResource { get; set; }
            public string OnlineResource_Name { get; set; }
            public string OnlineResource_Protocol { get; set; }
        }
    }

    public class GeoCoreContactList: List<GeoCoreContact> { }
}