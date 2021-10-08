using System;
using System.Collections.Generic;

namespace Datahub.Portal.Data
{
    public class ETL_CONTROL_TBL
    {
        public string PROCESS_NM { get; set; }
        public DateTime? START_TS { get; set; }
        public DateTime? END_TS { get; set; }
        public string STATUS_FLAG { get; set; }
        public long? RUN_ID { get; set; }
    }
}