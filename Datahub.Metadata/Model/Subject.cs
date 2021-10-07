using System.Collections.Generic;

namespace NRCan.Datahub.Metadata.Model
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string Subject_TXT { get; set; }
        public virtual ICollection<SubSubject> SubSubjects { get; set; }
    }

    public class SubSubject
    {
        public int SubSubjectId { get; set; }
        public string Name_English_TXT { get; set; }
        public string Name_French_TXT { get; set; }
        public virtual ICollection<Subject> Subjects { get; set; }
    }
}
