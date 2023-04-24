using System;

namespace Datahub.Core.Model.Achievements;

public class TelemetryEvent
{
    public int Id { get; set; }
    public int PortalUserId { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }

    #region Navigation props
    public virtual PortalUser PortalUser { get; set; }
    #endregion
}
