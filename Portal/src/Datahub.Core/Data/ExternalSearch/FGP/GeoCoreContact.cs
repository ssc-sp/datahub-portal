namespace Datahub.Core.Data.ExternalSearch.FGP;

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
        public string OnlineResourceDescription { get; set; }
        public string OnlineResource { get; set; }
        public string OnlineResourceName { get; set; }
        public string OnlineResourceProtocol { get; set; }
    }
}

public class GeoCoreContactList : List<GeoCoreContact> { }