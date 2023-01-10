﻿using Datahub.Core.Data;
using System.Collections.Generic;

namespace Datahub.Core.Model.Datahub
{
    public class Project_Constants
    {
        public static List<DropDownContainer> Project_Status = new List<DropDownContainer>()
        {
            new DropDownContainer() { Id = 1, EnglishText = "Open", FrenchText = "Open"},
            new DropDownContainer() { Id = 2, EnglishText = "Closed", FrenchText = "Closed"},            
        };
    }
}
