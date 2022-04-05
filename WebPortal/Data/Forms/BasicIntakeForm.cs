using System.ComponentModel.DataAnnotations;
using System.Linq;
using Elemental.Components;

namespace Datahub.Portal.Data.Forms;

public class BasicIntakeForm
{
    [Required]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Invalid Email Address")]
    [StringLength(20, ErrorMessage = "Identifier too long (16 character limit).")]
    [AeLabel(placeholder: "Please enter your email")]
    public string Email { get; set; }

    [Required]
    [AeLabel("Department Name", isDropDown: true)] 
    public string DepartmentName { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Project Name too long (200 character limit).")]
    [AeLabel("Project Name", "Please enter the name of your project")]
    public string ProjectName { get; set; }


    [AeFormIgnore] 
    public static Dictionary<string, string> DepartmentDictionary { get; } = new()
    {
        {"Accessibility Standards Canada", "ASC"}, {"Administrative Tribunals Support Service of Canada", "ATSSC"},
        {"Agriculture and Agri-Food Canada", "AAFC"}, {"Atlantic Canada Opportunities Agency", "ACOA"},
        {"Atlantic Pilotage Authority Canada", "APA"}, {"Atomic Energy of Canada Limited", "AECL"},
        {"Auditor General of Canada (Office of the)", "OAG"}, {"Bank of Canada", ""}, {"Bank of Canada Museum", ""},
        {"Blue Water Bridge Canada", "BWB"}, {"Business Development Bank of Canada", "BDC"},
        {"Canada Agricultural Review Tribunal", "CART"}, {"Canada Agriculture and Food Museum", ""},
        {"Canada Aviation and Space Museum", ""}, {"Canada Border Services Agency", "CBSA"},
        {"Canada Council for the Arts", ""}, {"Canada Deposit Insurance Corporation", "CDIC"},
        {"Canada Development Investment Corporation", "CDEV"},
        {"Canada Economic Development for Quebec Regions", "CED"}, {"Canada Employment Insurance Commission", "CEIC"},
        {"Canada Energy Regulator", "CER"}, {"Canada Firearms Centre", "CAFC"},
        {"Canada Industrial Relations Board", "CIRB"}, {"Canada Infrastructure Bank", "CIB"},
        {"Canada Lands Company Limited", "CLC"}, {"Canada Mortgage and Housing Corporation", "CMHC"},
        {"Canada Pension Plan Investment Board", "CPPIB"}, {"Canada Post", "CPC"}, {"Canada Research Chairs", ""},
        {"Canada Revenue Agency", "CRA"}, {"Canada School of Public Service", "CSPS"},
        {"Canada Science and Technology Museum", ""}, {"Canadian Air Transport Security Authority", "CATSA"},
        {"Canadian Army", "CA"}, {"Canadian Centre for Occupational Health and Safety", "CCOHS"},
        {"Canadian Coast Guard", "CCG"}, {"Canadian Commercial Corporation", "CCC"},
        {"Canadian Conservation Institute", "CCI"}, {"Canadian Cultural Property Export Review Board", ""},
        {"Canadian Dairy Commission", "CDC"}, {"Canadian Food Inspection Agency", "CFIA"},
        {"Canadian Grain Commission", "CGC"}, {"Canadian Heritage", "PCH"},
        {"Canadian Heritage Information Network", "CHIN"}, {"Canadian Human Rights Commission", "CHRC"},
        {"Canadian Institutes of Health Research", "CIHR"}, {"Canadian Intellectual Property Office", "CIPO"},
        {"Canadian Intergovernmental Conference Secretariat", "CICS"},
        {"Canadian International Trade Tribunal", "CITT"}, {"Canadian Judicial Council", "CJC"},
        {"Canadian Museum for Human Rights", "CMHR"}, {"Canadian Museum of History", "CMH"},
        {"Canadian Museum of Immigration at Pier 21", "CMIP"}, {"Canadian Museum of Nature", "CMN"},
        {"Canadian Northern Economic Development Agency", "CanNor"}, {"Canadian Nuclear Safety Commission", "CNSC"},
        {"Canadian Pari-Mutuel Agency", "CPMA"}, {"Canadian Police College", "CPC"},
        {"Canadian Race Relations Foundation", "CRRF"},
        {"Canadian Radio-Television and Telecommunications Commission", "CRTC"},
        {"Canadian Security Intelligence Service", "CSIS"}, {"Canadian Space Agency", "CSA"},
        {"Canadian Special Operations Forces Command", "CANSOFCOM"}, {"Canadian Trade Commissioner Service", "TCS"},
        {"Canadian Transportation Agency", "CTA"}, {"Canadian War Museum", ""}, {"CBC/Radio-Canada", "CBC"},
        {"Civilian Review and Complaints Commission for the RCMP", "CRCC"},
        {"Commissioner for Federal Judicial Affairs Canada (Office of the)", "FJA"},
        {"Commissioner of Lobbying of Canada (Office of the)", "OCL"},
        {"Commissioner of Official Languages (Office of the)", "OCOL"},
        {"Communications Research Centre Canada", "CRC"}, {"Communications Security Establishment Canada", "CSEC"},
        {"Competition Bureau Canada", "COBU"}, {"Competition Tribunal", ""},
        {"Conflict of Interest and Ethics Commissioner (Office of the)", "CIEC"}, {"Copyright Board Canada", "CB"},
        {"CORCAN", ""}, {"Correctional Investigator Canada", "OCI"}, {"Correctional Service Canada", "CSC"},
        {"Court Martial Appeal Court of Canada", "CMAC"}, {"Courts Administration Service", "CAS"},
        {"Crown-Indigenous Relations and Northern Affairs Canada", "CIRNAC"}, {"Defence Construction Canada", "DCC"},
        {"Defence Research and Development Canada", "DRDC"}, {"Democratic Institutions", ""},
        {"Destination Canada", "DC"}, {"Elections Canada", "Elections"},
        {"Employment and Social Development Canada", "ESDC"}, {"Environment and Climate Change Canada", "ECCC"},
        {"Environmental Protection Review Canada", "EPRC"}, {"Export Development Canada", "EDC"},
        {"Farm Credit Canada", "FCC"}, {"Farm Products Council of Canada", "FPCC"},
        {"Federal Bridge Corporation", "FBCL"}, {"Federal Court of Appeal", "FCA"}, {"Federal Court of Canada", "FC"},
        {"Federal Economic Development Agency for Southern Ontario", "FedDev Ontario"},
        {"Federal Ombudsman for Victims Of Crime (Office of the)", "OFOVC"}, {"FedNor", ""},
        {"Finance Canada (Department of)", "FIN"}, {"Financial Consumer Agency of Canada", "FCAC"},
        {"Financial Transactions and Reports Analysis Centre of Canada", "FINTRAC"},
        {"Fisheries and Oceans Canada", "DFO"}, {"Freshwater Fish Marketing Corporation", "FFMC"},
        {"Global Affairs Canada", "GAC"}, {"Governor General of Canada", "OSGG"},
        {"Great Lakes Pilotage Authority Canada", "GLPA"}, {"Health Canada", "HC"},
        {"Historic Sites and Monuments Board of Canada", "HSMBC"}, {"Human Rights Tribunal of Canada", "HRTC"},
        {"Immigration and Refugee Board of Canada", "IRB"}, {"Immigration, Refugees and Citizenship Canada", "IRCC"},
        {"Impact Assessment Agency of Canada", "IAAC"}, {"Independent Review Panel for Defence Acquisition", "IRPDA"},
        {"Indian Oil and Gas Canada", ""}, {"Indigenous and Northern Affairs Canada", "INAC"},
        {"Indigenous Services Canada", "ISC"}, {"Industrial Technologies Office", "ITO"},
        {"Information Commissioner (Office of the)", "OIC"}, {"Infrastructure Canada", "INFC"},
        {"Innovation, Science and Economic Development Canada", "ISED"},
        {"Intelligence Commissoner (Office of the)", ""}, {"Intergovernmental Affairs", "IGA"},
        {"International Development Research Centre", "IDRC"}, {"Jacques Cartier and Champlain Bridges", "JCCBI"},
        {"Judicial Compensation and Benefits Commission", ""}, {"Justice Canada (Department of)", "JUS"},
        {"Labour Program", ""}, {"Laurentian Pilotage Authority Canada", "LPA"},
        {"Leader of the Government in the House of Commons", ""}, {"Library and Archives Canada", "LAC"},
        {"Marine Atlantic", "MarineAtlantic"}, {"Measurement Canada", "MC"},
        {"Military Grievances External Review Committee", "MGERC"},
        {"Military Police Complaints Commission of Canada", "MPCC"}, {"National Arts Centre", "NAC"},
        {"National Battlefields Commission", "NBC"}, {"National Capital Commission", "NCC"},
        {"National Defence", "DND"}, {"National Film Board", "NFB"}, {"National Gallery of Canada", "NGC"},
        {"National Research Council Canada", "NRC"}, {"National Security and Intelligence Review Agency", "NSIRA"},
        {"National Seniors Council", ""}, {"Natural Resources Canada", "NRCan"},
        {"Natural Sciences and Engineering Research Canada", "NSERC"}, {"Northern Pipeline Agency Canada", "NPA"},
        {"Occupational Health and Safety Tribunal Canada", "OHSTC"}, {"Office of the Chief Military Judge", "OCMJ"},
        {"Ombudsman for the Department of National Defence and the Canadian Armed Forces (Office of the)", ""},
        {"Pacific Economic Development Canada", "PacifiCan"}, {"Pacific Pilotage Authority Canada", "PPA"},
        {"Parks Canada", "PC"}, {"Parliament of Canada", ""}, {"Parole Board of Canada", "PBC"},
        {"Patented Medicine Prices Review Board Canada", "PMPRB"}, {"Polar Knowledge Canada", "POLAR"},
        {"Prairies Economic Development Canada", "PrairiesCan"}, {"Prime Minister of Canada", ""},
        {"Privacy Commissioner of Canada (Office of the)", "OPC"}, {"Privy Council Office", "PCO"},
        {"Procurement Ombudsman (Office of the)", "OPO"}, {"Public Health Agency of Canada", "PHAC"},
        {"Public Prosecution Service of Canada", "PPSC"}, {"Public Safety Canada", "PS"},
        {"Public Sector Integrity Commissioner of Canada (Office of the)", "PSIC"},
        {"Public Sector Pension Investment Board", "PSP Investments"},
        {"Public Servants Disclosure Protection Tribunal Canada", "PSDPTC"},
        {"Public Service Commission of Canada", "PSC"},
        {"Public Service Labour Relations and Employment Board", "PSLREB"},
        {"Public Services and Procurement Canada", "PSPC"},
        {"Registry of the Specific Claims Tribunal of Canada", "SCT"}, {"Ridley Terminals Inc.", ""},
        {"Royal Canadian Air Force", "RCAF"}, {"Royal Canadian Mint", "Mint"},
        {"Royal Canadian Mounted Police", "RCMP"}, {"Royal Canadian Mounted Police External Review Committee", "ERC"},
        {"Royal Canadian Navy", "RCN"}, {"Royal Military College of Canada", "RMCC"},
        {"Secretariat of the National Security and Intelligence Committee of Parliamentarians", "NSICOP"},
        {"Service Canada", "ServCan"}, {"Shared Services Canada", "SSC"},
        {"Social Sciences and Humanities Research Council of Canada", "SSHRC"},
        {"Social Security Tribunal of Canada", "SST"}, {"Standards Council of Canada", "SCC-CCN"},
        {"Statistics Canada", "StatCan"}, {"Superintendent of Bankruptcy Canada (Office of the)", "OSB"},
        {"Superintendent of Financial Institutions Canada (Office of the)", "OSFI"}, {"Supreme Court of Canada", "SCC"},
        {"Tax Court of Canada", "TCC"}, {"Taxpayers' Ombudsperson (Office of the)", "OTO"}, {"Telefilm Canada", ""},
        {"Translation Bureau", ""}, {"Transport Canada", "TC"}, {"Transportation Appeal Tribunal of Canada", "TATC"},
        {"Transportation Safety Board of Canada", "TSB"}, {"Treasury Board of Canada Secretariat", "TBS"},
        {"Veterans Affairs Canada", "VAC"}, {"Veterans Review and Appeal Board", "VRAB"},
        {"Veterans' Ombudsman (Office of the)", ""}, {"VIA Rail Canada", "VIA Rail"},
        {"Virtual Museum of Canada", "VMC"}, {"Windsor-Detroit Bridge Authority", "WDBA"},
        {"Women and Gender Equality Canada", "WAGE"}, {"Youth", "YOUTH"},
    };
}