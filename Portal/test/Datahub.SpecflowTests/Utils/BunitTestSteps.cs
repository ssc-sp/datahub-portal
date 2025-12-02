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
