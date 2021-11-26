namespace Datahub.Portal.Data
{
    public record PowerBIActionBar
    {
        public bool visible { get; set; } = true;
    }

    public record PowerBIBars
    {
        public PowerBIActionBar actionBar { get; set; } = new PowerBIActionBar();
    }

    public record PowerBISettings
    {
        public bool filterPaneEnabled { get; set; } = true;
        public bool navContentPaneEnabled { get; set; } = true;

        public PowerBIBars bars { get; set; } = new PowerBIBars();

    }



}
