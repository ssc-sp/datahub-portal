namespace Datahub.Portal.Data;

public class FileList
{
    public string FileName { get; set; }
    public string ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }
    public string OwnedBy { get; set; }
    public string FileSize { get ; set; }
    public string Sharing { get; set; }
    public string FileType => GetExtension(FileName);

    private string GetExtension(string FileName)
    {
        int pos = FileName.LastIndexOf(".") + 1;
        return FileName.Substring(pos, FileName.Length - pos);                
    }
}