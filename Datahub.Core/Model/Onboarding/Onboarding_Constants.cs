using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Onboarding
{
    public class Onboarding_Constants
    {
        public static readonly string[] CATEGORY = { "Data Pipeline",
                                                    "Data Science",
                                                    "Full Stack",
                                                    "Guidance",
                                                    "Power BI Reports",
                                                    "Storage",
                                                    "Web Forms",
                                                    "Unknown",
                                                    "Other"
        };

        public static readonly string[] SECURITYLEVEL = {   "Classified", 
                                                            "Protected A",
                                                            "Protected B",
                                                            "Protected C",
                                                            "Unclassified",};
}
}
