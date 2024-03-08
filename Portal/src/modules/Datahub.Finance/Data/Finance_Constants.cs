using Datahub.Core.Data;

namespace Datahub.Finance.Data;

internal static class Finance_Constants
{
	public static List<DropDownContainer> Potential_Hiring_Process = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "Candidate identified - finalizing appointment", FrenchText = "Candidate Identified - Finalizing Appointment"},
		new DropDownContainer() { Id = 2, EnglishText = "Process currently being run / expected appointment from pool", FrenchText = "Process currently being run / expected appointment from pool"},
		new DropDownContainer() { Id = 3, EnglishText = "Process to be run - external to the public service", FrenchText = "Process to be run - external to the public service"},
		new DropDownContainer() { Id = 4, EnglishText = "Process to be run - internal to the public service or department", FrenchText = "Process to be run - internal to the public service or department"},
		new DropDownContainer() { Id = 5, EnglishText = "TBD", FrenchText = "TBD"},
	};

	public static List<DropDownContainer> Key_Activities = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "Research, Development and Demonstration to support innovation", FrenchText = "Research, Development and Demonstration to support innovation"},
		new DropDownContainer() { Id = 2, EnglishText = "Science and Technology to better understand Canada’s resources and environment", FrenchText = "Science and Technology to better understand Canada’s resources and environment"},
		new DropDownContainer() { Id = 3, EnglishText = "Policy Advice and Development", FrenchText = "Policy Advice and Development"},
		new DropDownContainer() { Id = 4, EnglishText = "Engagement and Consultations", FrenchText = "Engagement and Consultations"},
		new DropDownContainer() { Id = 5, EnglishText = "Delivering Transfer Payment Programs and Statutory payments", FrenchText = "Delivering Transfer Payment Programs and Statutory payments"},
		new DropDownContainer() { Id = 6, EnglishText = "Development and Administration of regulations", FrenchText = "Development and Administration of regulations"},
		new DropDownContainer() { Id = 7, EnglishText = "Providing Services to Canadians", FrenchText = "Providing Services to Canadians"},
		new DropDownContainer() { Id = 8, EnglishText = "Maintaining External Infrastructure and the Natural Environment", FrenchText = "Maintaining External Infrastructure and the Natural Environment"},
		new DropDownContainer() { Id = 9, EnglishText = "Internal Services: Management and Oversight", FrenchText = "Internal Services: Management and Oversight"},
		new DropDownContainer() { Id = 10, EnglishText = "Internal Services: Communications Services", FrenchText = "Internal Services: Communications Services"},
		new DropDownContainer() { Id = 11, EnglishText = "Internal Services: Legal Services", FrenchText = "Internal Services: Legal Services"},
		new DropDownContainer() { Id = 12, EnglishText = "Internal Services – Corporate Services", FrenchText = "Internal Services – Corporate Services"},
	};

	public static List<DropDownContainer> Key_Drivers = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "Research, Development and Demonstration to support innovation", FrenchText = "Research, Development and Demonstration to support innovation"},
		new DropDownContainer() { Id = 2, EnglishText = "Science and Technology to better understand Canada’s resources and environment", FrenchText = "Science and Technology to better understand Canada’s resources and environment"},
		new DropDownContainer() { Id = 3, EnglishText = "Policy Advice and Development", FrenchText = "Policy Advice and Development"},
		new DropDownContainer() { Id = 4, EnglishText = "Engagement and Consultations", FrenchText = "Engagement and Consultations"},
		new DropDownContainer() { Id = 5, EnglishText = "Delivering Transfer Payment Programs and Statutory payments", FrenchText = "Delivering Transfer Payment Programs and Statutory payments"},
		new DropDownContainer() { Id = 6, EnglishText = "Development and Administration of regulations", FrenchText = "Development and Administration of regulations"},
		new DropDownContainer() { Id = 7, EnglishText = "Providing Services to Canadians", FrenchText = "Providing Services to Canadians"},
		new DropDownContainer() { Id = 8, EnglishText = "Maintaining External Infrastructure and the Natural Environment", FrenchText = "Maintaining External Infrastructure and the Natural Environment"},
		new DropDownContainer() { Id = 9, EnglishText = "Internal Services: Management and Oversight", FrenchText = "Internal Services: Management and Oversight"},
		new DropDownContainer() { Id = 10, EnglishText = "Internal Services: Communications Services", FrenchText = "Internal Services: Communications Services"},
		new DropDownContainer() { Id = 11, EnglishText = "Internal Services: Legal Services", FrenchText = "Internal Services: Legal Services"},
		new DropDownContainer() { Id = 12, EnglishText = "Internal Services – Corporate Services", FrenchText = "Internal Services – Corporate Services"},
	};

	public static List<DropDownContainer> Incremental_Replacements = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "New-Incremental", FrenchText = "New-Incremental"},
		new DropDownContainer() { Id = 2, EnglishText = "Replacement", FrenchText = "Replacement"},
	};

	public static List<DropDownContainer> FTE_Accomodation_Requirements = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "Anticipate using existing accommodations; little to no alterations needed", FrenchText = "Anticipate using existing accommodations; little to no alterations needed"},
		new DropDownContainer() { Id = 2, EnglishText = "Anticipate using existing footprint but altered (e.g. new cubicle layout)", FrenchText = "Anticipate using existing footprint but altered (e.g. new cubicle layout)"},
		new DropDownContainer() { Id = 3, EnglishText = "Additional space needed", FrenchText = "Additional space needed"},
		new DropDownContainer() { Id = 4, EnglishText = "Unknown", FrenchText = "Unknown"},
	};

	public static List<DropDownContainer> Position_Workspace_Type = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "Role can be unassigned seating*", FrenchText = "Role can be unassigned seating*"},
		new DropDownContainer() { Id = 2, EnglishText = "Assigned seating", FrenchText = "Assigned seating"},
		new DropDownContainer() { Id = 3, EnglishText = "Unknown", FrenchText = "Unknown"},
	};

	public static List<DropDownContainer> FTE_Intended_Location = new List<DropDownContainer>()
	{
		new DropDownContainer() { Id = 1, EnglishText = "TBD - pending staffing plan and results", FrenchText = "TBD - pending staffing plan and results"},
		new DropDownContainer() { Id = 2, EnglishText = "Full-time off-site", FrenchText = "Full-time off-site"},
		new DropDownContainer() { Id = 3, EnglishText = "AMHERST, Amherst Plaza, 136 Victoria Street East", FrenchText = "AMHERST, Amherst Plaza, 136 Victoria Street East"},
		new DropDownContainer() { Id = 4, EnglishText = "AMHERST, Government of Canada Building, 38 - 40 Havelock Street", FrenchText = "AMHERST, Government of Canada Building, 38 - 40 Havelock Street"},
		new DropDownContainer() { Id = 5, EnglishText = "CALGARY, Geological Survey of Canada, 3303 33 Street Northwest", FrenchText = "CALGARY, Geological Survey of Canada, 3303 33 Street Northwest"},
		new DropDownContainer() { Id = 6, EnglishText = "CANTLEY, Gatineau Satellite Station, 75 McClelland Road", FrenchText = "CANTLEY, Gatineau Satellite Station, 75 McClelland Road"},
		new DropDownContainer() { Id = 7, EnglishText = "CHALK RIVER, Petawawa Research Forest, 1000 Clouthier Road, P.O. Box 2000", FrenchText = "CHALK RIVER, Petawawa Research Forest, 1000 Clouthier Road, P.O. Box 2000"},
		new DropDownContainer() { Id = 8, EnglishText = "CORNER BROOK, Forest Centre - Sir Wilfred Grenfell College, 26 University Avenue, P.O. Box 960", FrenchText = "CORNER BROOK, Forest Centre - Sir Wilfred Grenfell College, 26 University Avenue, P.O. Box 960"},
		new DropDownContainer() { Id = 9, EnglishText = "DARTMOUTH, Bedford Institute of Oceanograph, 1 Challenger Drive, P.O. Box 1006", FrenchText = "DARTMOUTH, Bedford Institute of Oceanograph, 1 Challenger Drive, P.O. Box 1006"},
		new DropDownContainer() { Id = 10, EnglishText = "DEVON, Devon Research Centre, 1 Oil Patch Drive", FrenchText = "DEVON, Devon Research Centre, 1 Oil Patch Drive"},
		new DropDownContainer() { Id = 11, EnglishText = "EDMONTON, Northern Forestry Centre, 5320 122 Street Northwest", FrenchText = "EDMONTON, Northern Forestry Centre, 5320 122 Street Northwest"},
		new DropDownContainer() { Id = 12, EnglishText = "FREDERICTON, Atlantic Forestry Centre, 1350 Regent Street, P.O. Box 4000", FrenchText = "FREDERICTON, Atlantic Forestry Centre, 1350 Regent Street, P.O. Box 4000"},
		new DropDownContainer() { Id = 13, EnglishText = "HAMILTON, CANMET Materials Technology Lab, 183 Longwood Road South", FrenchText = "HAMILTON, CANMET Materials Technology Lab, 183 Longwood Road South"},
		new DropDownContainer() { Id = 14, EnglishText = "INUVIK, Canada Centre for Mapping and Earth Observation, 191, Mackenzie Road, P.O. Box 1450", FrenchText = "INUVIK, Canada Centre for Mapping and Earth Observation, 191, Mackenzie Road, P.O. Box 1450"},
		new DropDownContainer() { Id = 15, EnglishText = "INUVIK, Inuvik Satellite Station Facility, Phase 2, Lot 4, Block 101", FrenchText = "INUVIK, Inuvik Satellite Station Facility, Phase 2, Lot 4, Block 101"},
		new DropDownContainer() { Id = 16, EnglishText = "IQALUIT, Governor Building, 100 - 1093 Governor, P.O. Box 2380", FrenchText = "IQALUIT, Governor Building, 100 - 1093 Governor, P.O. Box 2380"},
		new DropDownContainer() { Id = 17, EnglishText = "IQALUIT, Inuksugait Plaza Site, PO Box 2319, 1106 Inuksugait Plaza", FrenchText = "IQALUIT, Inuksugait Plaza Site, PO Box 2319, 1106 Inuksugait Plaza"},
		new DropDownContainer() { Id = 18, EnglishText = "NOONAN, Acadia Research Forest, Highway 10", FrenchText = "NOONAN, Acadia Research Forest, Highway 10"},
		new DropDownContainer() { Id = 19, EnglishText = "OTTAWA, Anderson Road, 2617 Anderson Road", FrenchText = "OTTAWA, Anderson Road, 2617 Anderson Road"},
		new DropDownContainer() { Id = 20, EnglishText = "OTTAWA, Bells Corners Complex, 1 Haanel Drive - Building 1", FrenchText = "OTTAWA, Bells Corners Complex, 1 Haanel Drive - Building 1"},
		new DropDownContainer() { Id = 21, EnglishText = "OTTAWA, Booth Street Complex - 555 Booth St, 555 Booth Street", FrenchText = "OTTAWA, Booth Street Complex - 555 Booth St, 555 Booth Street"},
		new DropDownContainer() { Id = 22, EnglishText = "OTTAWA, Booth Street Complex - 580 Booth Street", FrenchText = "OTTAWA, Booth Street Complex - 580 Booth Street"},
		new DropDownContainer() { Id = 23, EnglishText = "OTTAWA, Booth Street Complex - 601 Booth St, 601 Booth Street", FrenchText = "OTTAWA, Booth Street Complex - 601 Booth St, 601 Booth Street"},
		new DropDownContainer() { Id = 24, EnglishText = "OTTAWA, Booth Street Complex - 615 Booth St, 615 Booth Street", FrenchText = "OTTAWA, Booth Street Complex - 615 Booth St, 615 Booth Street"},
		new DropDownContainer() { Id = 25, EnglishText = "OTTAWA, Central Experimental Farm, 930 Carling Avenue ", FrenchText = "OTTAWA, Central Experimental Farm, 930 Carling Avenue "},
		new DropDownContainer() { Id = 26, EnglishText = "OTTAWA, Limebank Road, 3484 Limebank Road", FrenchText = "OTTAWA, Limebank Road, 3484 Limebank Road"},
		new DropDownContainer() { Id = 27, EnglishText = "OTTAWA, Warner Building, Sheffield Road, 2464 Sheffield Road", FrenchText = "OTTAWA, Warner Building, Sheffield Road, 2464 Sheffield Road"},
		new DropDownContainer() { Id = 28, EnglishText = "PASADENA, Pasadena Field Station, Midland Row Street", FrenchText = "PASADENA, Pasadena Field Station, Midland Row Street"},
		new DropDownContainer() { Id = 29, EnglishText = "PRINCE ALBERT, Prince Albert Satellite Station, Prince Albert Satellite Station, P.O. Box 1150", FrenchText = "PRINCE ALBERT, Prince Albert Satellite Station, Prince Albert Satellite Station, P.O. Box 1150"},
		new DropDownContainer() { Id = 30, EnglishText = "QUÉBEC, Jacques Cartier Place Complex, 320 Saint-Joseph Street East", FrenchText = "QUÉBEC, Jacques Cartier Place Complex, 320 Saint-Joseph Street East"},
		new DropDownContainer() { Id = 31, EnglishText = "QUÉBEC, Laurentian Forestry Centre, 1055 Du P.E.P.S. Street, P.O. Box 10380", FrenchText = "QUÉBEC, Laurentian Forestry Centre, 1055 Du P.E.P.S. Street, P.O. Box 10380"},
		new DropDownContainer() { Id = 32, EnglishText = "QUÉBEC, National Institute for Scientific Research, 490 De la Couronne Street", FrenchText = "QUÉBEC, National Institute for Scientific Research, 490 De la Couronne Street"},
		new DropDownContainer() { Id = 33, EnglishText = "REGINA, Alvin Hamilton Building, 701 - 1783 Hamilton Street", FrenchText = "REGINA, Alvin Hamilton Building, 701 - 1783 Hamilton Street"},
		new DropDownContainer() { Id = 34, EnglishText = "RESOLUTE, Polar Continental Shelf Facility - North Camp, NRCan Complex North Camp", FrenchText = "RESOLUTE, Polar Continental Shelf Facility - North Camp, NRCan Complex North Camp"},
		new DropDownContainer() { Id = 35, EnglishText = "SAINT-GABRIEL-DE, Valcartier Forest Research Station, 41 Murphy Road", FrenchText = "SAINT-GABRIEL-DE, Valcartier Forest Research Station, 41 Murphy Road"},
		new DropDownContainer() { Id = 36, EnglishText = "SAULT STE. MARIE, Great Lakes Forestry Centre, 1219 Queen Street East", FrenchText = "SAULT STE. MARIE, Great Lakes Forestry Centre, 1219 Queen Street East"},
		new DropDownContainer() { Id = 37, EnglishText = "SHERBROOKE, Sherbrooke Office Site, 212 - 50 Place de la Cité, P.O. Box 162", FrenchText = "SHERBROOKE, Sherbrooke Office Site, 212 - 50 Place de la Cité, P.O. Box 162"},
		new DropDownContainer() { Id = 38, EnglishText = "SIDNEY, Pacific Geoscience Centre, 9860 West Saanich Road, P.O. Box 6000", FrenchText = "SIDNEY, Pacific Geoscience Centre, 9860 West Saanich Road, P.O. Box 6000"},
		new DropDownContainer() { Id = 39, EnglishText = "SMITHERS, Provincial Government Office Building, 3793 Alfred Avenue, Bag 5000", FrenchText = "SMITHERS, Provincial Government Office Building, 3793 Alfred Avenue, Bag 5000"},
		new DropDownContainer() { Id = 40, EnglishText = "ST. JOHN'S, Baine Johnston Centre, 10 Fort William Place", FrenchText = "ST. JOHN'S, Baine Johnston Centre, 10 Fort William Place"},
		new DropDownContainer() { Id = 41, EnglishText = "SUDBURY, Sudbury leasehold, 1079 Kelly Lake Road", FrenchText = "SUDBURY, Sudbury leasehold, 1079 Kelly Lake Road"},
		new DropDownContainer() { Id = 42, EnglishText = "SWIFT CURRENT, Swift Current Research and Development Centre, P.O Box 1030", FrenchText = "SWIFT CURRENT, Swift Current Research and Development Centre, P.O Box 1030"},
		new DropDownContainer() { Id = 43, EnglishText = "TORONTO, Bay Street Site, 300 - 655 Bay Street, P.O. Box 15", FrenchText = "TORONTO, Bay Street Site, 300 - 655 Bay Street, P.O. Box 15"},
		new DropDownContainer() { Id = 44, EnglishText = "VAL-D'OR, CANMET-MMSL Experimental, 1 Peter Ferderber Road, P.O. Box 1300", FrenchText = "VAL-D'OR, CANMET-MMSL Experimental, 1 Peter Ferderber Road, P.O. Box 1300"},
		new DropDownContainer() { Id = 45, EnglishText = "VANCOUVER, Burrard Street Office, 219 - 800 Burrard Street", FrenchText = "VANCOUVER, Burrard Street Office, 219 - 800 Burrard Street"},
		new DropDownContainer() { Id = 46, EnglishText = "VANCOUVER, Melville Office, 1501 - 1138 Melville Street", FrenchText = "VANCOUVER, Melville Office, 1501 - 1138 Melville Street"},
		new DropDownContainer() { Id = 47, EnglishText = "VANCOUVER, Vancouver Centre, 650 Georgia Street West", FrenchText = "VANCOUVER, Vancouver Centre, 650 Georgia Street West"},
		new DropDownContainer() { Id = 48, EnglishText = "VANCOUVER, Vancouver House, 1500 - 605 Robson Street", FrenchText = "VANCOUVER, Vancouver House, 1500 - 605 Robson Street"},
		new DropDownContainer() { Id = 49, EnglishText = "VARENNES, CANMET Energy Technology Centre (Varennes), 1615 Lionel-Boulet Boulevard, P.O. Box 4800", FrenchText = "VARENNES, CANMET Energy Technology Centre (Varennes), 1615 Lionel-Boulet Boulevard, P.O. Box 4800"},
		new DropDownContainer() { Id = 50, EnglishText = "VICTORIA, Pacific Forestry Centre, 506 Burnside Road West", FrenchText = "VICTORIA, Pacific Forestry Centre, 506 Burnside Road West"},
		new DropDownContainer() { Id = 51, EnglishText = "WHITEHORSE, Elijah Smith Building, 225 - 300 Main Street", FrenchText = "WHITEHORSE, Elijah Smith Building, 225 - 300 Main Street"},
		new DropDownContainer() { Id = 52, EnglishText = "WINNIPEG, Winnipeg Office, 365 Hargrave Street", FrenchText = "WINNIPEG, Winnipeg Office, 365 Hargrave Street"},
		new DropDownContainer() { Id = 53, EnglishText = "YELLOWKNIFE, NWT Regional Operations Centre, 5101 50th Avenue, P.O. Box 668", FrenchText = "YELLOWKNIFE, NWT Regional Operations Centre, 5101 50th Avenue, P.O. Box 668"},
		new DropDownContainer() { Id = 54, EnglishText = "YELLOWKNIFE, Yellowknife Geophysical Observatory, Lot 1018, P.O. Box 2399", FrenchText = "YELLOWKNIFE, Yellowknife Geophysical Observatory, Lot 1018, P.O. Box 2399"},
		new DropDownContainer() { Id = 55, EnglishText = "Other", FrenchText = "Autre"},
	};
}