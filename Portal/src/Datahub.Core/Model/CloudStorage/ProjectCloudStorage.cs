﻿using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using System.Collections.Generic;

namespace Datahub.Core.Model.CloudStorage;

public class ProjectCloudStorage
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public virtual Datahub_Project Project { get; set; }
    public string Provider { get; set; }
    public string Name { get; set; }
    public string ConnectionData { get; set; }
    public bool Enabled { get; set; }

    public IList<OpenDataSubmission> PublishingSubmissions { get; set; }
}
