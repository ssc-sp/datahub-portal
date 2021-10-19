using Microsoft.EntityFrameworkCore;
using Datahub.Portal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FormTests
{
    public class ETLStatusTest
    {
        [Fact]
        public async Task TestLoadETLStatus()
        {
            var contextOptions = new DbContextOptionsBuilder<DatahubETLStatusContext>()
                .UseSqlServer(@"Server=sqlserver-ciosb-datahub.database.windows.net;Database=sql-ciosb-datahub-etldb;Integrated Authentication=true;applicationintent=readonly")
                .Options;
            using var etlctx = new DatahubETLStatusContext(contextOptions);
            var results = await etlctx.ETL_CONTROL_TBL.Where(p => p.PROCESS_NM == "CITSM - Elsevier Publications and Citations").OrderByDescending(t => t.END_TS).ToListAsync();
            Assert.Equal(2, results.Count);
            
            var status = results.GroupBy(t => t.PROCESS_NM).Select(gp => gp.OrderByDescending(g => g.END_TS).First()).OrderBy(s => s.PROCESS_NM).ToList();
        }

    }
}
