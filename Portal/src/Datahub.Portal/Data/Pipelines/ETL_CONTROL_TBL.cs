namespace Datahub.Portal.Data.Pipelines;

public class ETLCONTROLTBL
{
    public string PROCESSNM { get; set; }
    public DateTime? STARTTS { get; set; }
    public DateTime? ENDTS { get; set; }
    public string STATUSFLAG { get; set; }
    public long? RUNID { get; set; }
}