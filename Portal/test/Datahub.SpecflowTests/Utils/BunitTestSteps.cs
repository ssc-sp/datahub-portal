using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Reqnroll;

namespace Datahub.SpecflowTests.Utils
{
    public class BunitTestSteps: BunitContext
    {
        protected BunitTestSteps()
        {
            var gcdsModule = JSInterop.SetupModule("./_content/GcdsWrapper.Blazor/gcdsInterop.js");
            gcdsModule.Mode = JSRuntimeMode.Loose;
        }

        [AfterScenario]
        public async Task DisposeAsyncBunit()
        {
            try
            {
                await DisposeComponentsAsync();
                await DisposeAsync();
            }
            catch (Exception)
            {
                // Ignore exceptions during disposal
            }
        }
    }
}
