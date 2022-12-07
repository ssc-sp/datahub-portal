using Datahub.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    internal class Project_Constants
    {
        public static List<DropDownContainer> Project_Status = new List<DropDownContainer>()
        {
            new DropDownContainer() { Id = 1, EnglishText = "Open", FrenchText = "Open"},
            new DropDownContainer() { Id = 2, EnglishText = "Closed", FrenchText = "Closed"},            
        };
    }
}
