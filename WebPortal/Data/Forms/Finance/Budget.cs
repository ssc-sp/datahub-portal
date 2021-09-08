using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Data.Finance
{
    public class Budget
    {
        [Key]
        [AeFormIgnore]
        public int Budget_ID { get; set; }
        /** Section: Division **/
        [Required]
        public double Division_NUM { get; set; }
        /** Section: Sub-Level **/
        [Required]
        [MaxLength(200)] 
        public string SubLevel_TXT { get; set; }
        /** Section: Departmental Priorities **/
        [AeLabel(isDropDown: true)] 
        [Required] 
        public string Departmental_Priorities_TXT { get; set; }
        /** Section: Sector Priorities **/
        [AeLabel(isDropDown: true)]
        [Required]
        public string Sector_Priorities_TXT { get; set; }
        /** Section: Key Activity **/
        [AeLabel(isDropDown: true)]
        [Required]
        public string Key_Activity_TXT { get; set; }
        /** Section: Fund **/
        [Required]
        public int? Fund_NUM { get; set; }
        /** Section: Funding Type **/
        [Required]
        [MaxLength(200)] 
        public string Funding_Type_TXT { get; set; }
        /** Section: Program Activity **/
        [Required]
        [MaxLength(200)] 
        public string Program_Activity_TXT { get; set; }
        /** Section: Budget **/        
        public double? Budget_NUM { get; set; }
        /** Section: Anticipated Transfers **/        
        public double? Anticipated_Transfers_NUM { get; set; }
        /** Section: Revised Budget **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Revised_Budget_NUM => (Budget_NUM ?? 0) + (Anticipated_Transfers_NUM ?? 0);
        /** Section: Allocation Percentage **/
        public double? Allocation_Percentage_NUM { get; set; }

        /** Section: Salary Forecast **/
        public double? Indeterminate_Fte_NUM { get; set; }
        /** Section: Salary Forecast **/
        public double? Indeterminate__Amount_NUM { get; set; }
        /** Section: Salary Forecast **/
        public double? Determinate_Fte_NUM { get; set; }
        /** Section: Salary Forecast **/
        public double? Determinate__Amount_NUM { get; set; }
        /** Section: Salary Forecast **/
        public double? Planned_Staffing_Fte_NUM { get; set; }
        /** Section: Salary Forecast **/
        public double? Planned_Staffing__Amount_NUM { get; set; }
        /** Section: Salary Forecast **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_Salary_Fte_NUM => (Indeterminate_Fte_NUM ?? 0) + (Determinate_Fte_NUM ?? 0) + (Planned_Staffing_Fte_NUM ?? 0);
        /** Section: Salary Forecast **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_Salary__Amount_NUM => (Indeterminate__Amount_NUM ?? 0) + (Determinate__Amount_NUM ?? 0) + (Planned_Staffing__Amount_NUM ?? 0);
        /** Section: O&M Forecast **/
        public double? Information_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Information_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Machine_and_Equipment_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Machine_and_Equipment_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Professional_Seervices_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Professional_SeervicesThree_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Repairs_and_Maintenance_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Repairs_and_MaintenanceThree_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Rentals_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Rentals_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Transportation_and_Communication_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Transportation_and_Communication_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Utilities_Materials_and_Supplies_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Utilities_Materials_and_Supplies_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Other_Payments_and_Ogd_Recoveries_NUM { get; set; }
        /** Section: O&M Forecast **/
        public double? Other_Payments_and_Ogd_Recoveries_Three_Year_Average_NUM { get; set; }
        /** Section: O&M Forecast **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_OnM_Forecast_NUM => (Information_NUM ?? 0) + (Machine_and_Equipment_NUM ?? 0) + (Professional_Seervices_NUM ?? 0) + (Repairs_and_Maintenance_NUM ?? 0) + (Rentals_NUM ?? 0) + (Transportation_and_Communication_NUM ?? 0) + (Utilities_Materials_and_Supplies_NUM ?? 0) + (Other_Payments_and_Ogd_Recoveries_NUM ?? 0);
        /** Section: Capital Forecast **/
        public double? Personnel_NUM { get; set; }
        /** Section: Capital Forecast **/
        public double? NonPersonnel_NUM { get; set; }
        /** Section: Capital Forecast **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_Capital_Forecast_NUM => (Personnel_NUM ?? 0) + (NonPersonnel_NUM ?? 0);
        /** Section: G&C Forecast **/
        public double? Grants_NUM { get; set; }
        /** Section: G&C Forecast **/
        public double? Contributions_NUM { get; set; }
        /** Section: G&C Forecast **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_GnC_Forecast_NUM => (Grants_NUM ?? 0) + (Contributions_NUM ?? 0);
        /** Section: Total Forecasted Expenditures **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Total_Forecasted_Expenditures_NUM => (Total_Salary__Amount_NUM ?? 0) + (Total_OnM_Forecast_NUM ?? 0) + (Total_Capital_Forecast_NUM ?? 0) + (Total_GnC_Forecast_NUM ?? 0);
        /** Section: Adjustments to Forecast **/
        public double? Adjustments_To_Forecast_NUM { get; set; }
        /** Section: Forecast Adjustment for Salary Attrition **/
        public double? Forecast_Adjustment_For_Salary_Attrition_NUM { get; set; }
        /** Section: Forecast Adjustment for Risk Management **/
        public double? Forecast_Adjustment_For_Risk_Management_NUM { get; set; }
        /** Section: Revised Forecasts **/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "c")]
        public double? Revised_Forecasts_NUM => (Adjustments_To_Forecast_NUM ?? 0) + (Forecast_Adjustment_For_Salary_Attrition_NUM ?? 0) + (Forecast_Adjustment_For_Risk_Management_NUM ?? 0);
        /** Section: % of Forecast to Budget **/
        public double? Percent_Of_Forecast_To_Budget_NUM { get; set; }
        /** Section: Comments/Notes for Financial Information  **/
        [MaxLength(7500)] 
        public string Comments_Notes_For_Financial_Information_TXT { get; set; }
        /** Section: Involves an IT or Real Property Component? **/
        [MaxLength(7500)] 
        public string Involves_An_It_Or_Real_Property_Component_TXT { get; set; }
        /** Section: Comments/Notes for Non-Financial Information  **/
        [MaxLength(7500)] 
        public string Comments_Notes_For_NonFinancial_Information_TXT { get; set; }

        [AeFormIgnore]
        public SectorAndBranch SectorAndBranch { get; set; }
        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

    }
}
