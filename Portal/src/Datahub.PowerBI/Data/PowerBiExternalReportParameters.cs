using Datahub.Core.Model.Datahub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.PowerBI.Data
{
	public class PowerBiExternalReportParameters
	{
		public ExternalPowerBiReport App;
		public string AppUrl;
		public List<string> AdminEmailAddresses;
	}

}
