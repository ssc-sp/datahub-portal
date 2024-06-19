using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Projects;

public class Project_SyncStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTimeOffset SyncTSUTC { get; set; }
    public Datahub_Project Project { get; set; }
}
