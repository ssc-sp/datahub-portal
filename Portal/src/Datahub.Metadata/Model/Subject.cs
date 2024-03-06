using System.Collections.Generic;

namespace Datahub.Metadata.Model;

public class Subject
{
    public int SubjectId { get; set; }
    public string SubjectTXT { get; set; }
    public virtual ICollection<SubSubject> SubSubjects { get; set; }
}

public class SubSubject
{
    public int SubSubjectId { get; set; }
    public string NameEnglishTXT { get; set; }
    public string NameFrenchTXT { get; set; }
    public virtual ICollection<Subject> Subjects { get; set; }
}