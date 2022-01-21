using Datahub.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests
{
    public class ModuleManagerTest
    {
        [Fact]
        public async Task GivenModuleName_FindClass()
        {
            List<Type> allTypes = Datahub.Portal.Startup.LoadModules("*");
            //var objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
            //                   from type in asm.GetTypes()
            //                   where type.IsClass && type.Name == expectedClass
            //                  select type).Single();
            Assert.Contains(typeof(Datahub.Finance.FinanceModule), allTypes);
            Assert.Contains(typeof(Datahub.M365Forms.M365FormsModule), allTypes);
            Assert.Contains(typeof(Datahub.LanguageTraining.LanguageTrainingModule), allTypes);
        }

        [Fact]
        public async Task GivenFilterStar_FindModule()
        {
            var allModules = Datahub.Portal.Startup.LoadModules("*");
            Assert.Contains(typeof(Datahub.Finance.FinanceModule), allModules);
            Assert.Contains(typeof(Datahub.M365Forms.M365FormsModule), allModules);
            Assert.Contains(typeof(Datahub.LanguageTraining.LanguageTrainingModule), allModules);
        }

        [Fact]
        public async Task GivenFilterSingle_FindModule()
        {
            var allModules = Datahub.Portal.Startup.LoadModules("financemodule");
            Assert.Single(allModules);
            Assert.Contains(typeof(Datahub.Finance.FinanceModule), allModules);
        }

    }
}
