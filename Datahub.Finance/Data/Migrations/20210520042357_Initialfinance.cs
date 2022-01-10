using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class Initialfinance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    SectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectorNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectorNameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectorAndBranchForSectorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.SectorId);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchNameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    SectorAndBranchForBranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "SectorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectorAndBranches",
                columns: table => new
                {
                    SectorBranch_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sector_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    Allocated_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    Unallocated_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    SectorId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorAndBranches", x => x.SectorBranch_ID);
                    table.ForeignKey(
                        name: "FK_SectorAndBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectorAndBranches_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "SectorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Budget_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Division_NUM = table.Column<double>(type: "float", nullable: false),
                    SubLevel_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Departmental_Priorities_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sector_Priorities_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key_Activity_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fund_NUM = table.Column<int>(type: "int", nullable: false),
                    Funding_Type_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Program_Activity_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Budget_NUM = table.Column<double>(type: "float", nullable: true),
                    Anticipated_Transfers_NUM = table.Column<double>(type: "float", nullable: true),
                    Allocation_Percentage_NUM = table.Column<double>(type: "float", nullable: true),
                    Indeterminate_Fte_NUM = table.Column<double>(type: "float", nullable: true),
                    Indeterminate__Amount_NUM = table.Column<double>(type: "float", nullable: true),
                    Determinate_Fte_NUM = table.Column<double>(type: "float", nullable: true),
                    Determinate__Amount_NUM = table.Column<double>(type: "float", nullable: true),
                    Planned_Staffing_Fte_NUM = table.Column<double>(type: "float", nullable: true),
                    Planned_Staffing__Amount_NUM = table.Column<double>(type: "float", nullable: true),
                    Information_NUM = table.Column<double>(type: "float", nullable: true),
                    Information_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Machine_and_Equipment_NUM = table.Column<double>(type: "float", nullable: true),
                    Machine_and_Equipment_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Professional_Seervices_NUM = table.Column<double>(type: "float", nullable: true),
                    Professional_SeervicesThree_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Repairs_and_Maintenance_NUM = table.Column<double>(type: "float", nullable: true),
                    Repairs_and_MaintenanceThree_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Rentals_NUM = table.Column<double>(type: "float", nullable: true),
                    Rentals_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Transportation_and_Communication_NUM = table.Column<double>(type: "float", nullable: true),
                    Transportation_and_Communication_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Utilities_Materials_and_Supplies_NUM = table.Column<double>(type: "float", nullable: true),
                    Utilities_Materials_and_Supplies_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Other_Payments_and_Ogd_Recoveries_NUM = table.Column<double>(type: "float", nullable: true),
                    Other_Payments_and_Ogd_Recoveries_Three_Year_Average_NUM = table.Column<double>(type: "float", nullable: true),
                    Personnel_NUM = table.Column<double>(type: "float", nullable: true),
                    NonPersonnel_NUM = table.Column<double>(type: "float", nullable: true),
                    Grants_NUM = table.Column<double>(type: "float", nullable: true),
                    Contributions_NUM = table.Column<double>(type: "float", nullable: true),
                    Adjustments_To_Forecast_NUM = table.Column<double>(type: "float", nullable: true),
                    Forecast_Adjustment_For_Salary_Attrition_NUM = table.Column<double>(type: "float", nullable: true),
                    Forecast_Adjustment_For_Risk_Management_NUM = table.Column<double>(type: "float", nullable: true),
                    Percent_Of_Forecast_To_Budget_NUM = table.Column<double>(type: "float", nullable: true),
                    Comments_Notes_For_Financial_Information_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Involves_An_It_Or_Real_Property_Component_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Comments_Notes_For_NonFinancial_Information_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    SectorAndBranchSectorBranch_ID = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Budget_ID);
                    table.ForeignKey(
                        name: "FK_Budgets_SectorAndBranches_SectorAndBranchSectorBranch_ID",
                        column: x => x.SectorAndBranchSectorBranch_ID,
                        principalTable: "SectorAndBranches",
                        principalColumn: "SectorBranch_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "SectorId", "SectorAndBranchForSectorId", "SectorNameEn", "SectorNameFr" },
                values: new object[,]
                {
                    { 1, 0, "DIRECTION AND COORDINATION", "DIRECTION AND COORDINATION" },
                    { 2, 0, "INDIGENOUS AFFAIRS AND RECON SECTOR", "INDIGENOUS AFFAIRS AND RECON SECTOR" },
                    { 3, 0, "COMMS PORTFOLIO SEC", "COMMS PORTFOLIO SEC" },
                    { 4, 0, "STRATEGIC PETROLEUM POL & INV OFFICE", "STRATEGIC PETROLEUM POL & INV OFFICE" },
                    { 5, 0, "IND CONSULT FOR TMX", "IND CONSULT FOR TMX" },
                    { 6, 0, "ATOMIC ENERGY SECTOR", "ATOMIC ENERGY SECTOR" },
                    { 7, 0, "MAJOR PROJECT MANAGEMENT OFFICE SECTOR", "MAJOR PROJECT MANAGEMENT OFFICE SECTOR" },
                    { 8, 0, "STRATEGIC POLICY AND INNOVATION", "STRATEGIC POLICY AND INNOVATION" },
                    { 9, 0, "OFFICE OF THE CHIEF SCIENTIST", "OFFICE OF THE CHIEF SCIENTIST" },
                    { 10, 0, "LOW CARBON ENERGY SECTOR", "LOW CARBON ENERGY SECTOR" },
                    { 11, 0, "CORPORATE MANAGEMENT AND SERVICES SECTOR", "CORPORATE MANAGEMENT AND SERVICES SECTOR" },
                    { 12, 0, "DEPARTMENTAL SERVICES", "DEPARTMENTAL SERVICES" },
                    { 13, 0, "CANADIAN FOREST SERVICE", "CANADIAN FOREST SERVICE" },
                    { 14, 0, "ENERGY TECHNOLOGY SECTOR", "ENERGY TECHNOLOGY SECTOR" },
                    { 15, 0, "LANDS AND MINERALS SECTOR", "LANDS AND MINERALS SECTOR" },
                    { 16, 0, "CORPORATE ACCOUNTING", "CORPORATE ACCOUNTING" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorAndBranchForBranchId", "SectorId" },
                values: new object[,]
                {
                    { 1, "AUDIT & EVALUATION BRANCH", "AUDIT & EVALUATION BRANCH", 0, 1 },
                    { 75, "ADM’S OFFICE", "ADM’S OFFICE", 0, 15 },
                    { 74, "SECTOR MANAGEMENT ACCOUNT", "SECTOR MANAGEMENT ACCOUNT", 0, 14 },
                    { 73, "PLANNING & OPERATIONS", "PLANNING & OPERATIONS", 0, 14 },
                    { 72, "OFFICE OF ENERGY RESEARCH AND DEV", "OFFICE OF ENERGY RESEARCH AND DEV", 0, 14 },
                    { 71, "FINANCE MANAGEMENT", "FINANCE MANAGEMENT", 0, 14 },
                    { 70, "ETS ADM'S OFFICE", "ETS ADM'S OFFICE", 0, 14 },
                    { 69, "CANMET MATERIALS", "CANMET MATERIALS", 0, 14 },
                    { 68, "CANMET ENERGY – VARENNES", "CANMET ENERGY – VARENNES", 0, 14 },
                    { 67, "CANMET ENERGY – OTTAWA", "CANMET ENERGY – OTTAWA", 0, 14 },
                    { 66, "ADM’S RESERVE", "ADM’S RESERVE", 0, 14 },
                    { 76, "BUSINESS MANAGEMENT SERVICES AND DATA", "BUSINESS MANAGEMENT SERVICES AND DATA", 0, 15 },
                    { 65, "SCIENCE POLICY INTEGRATION", "SCIENCE POLICY INTEGRATION", 0, 13 },
                    { 63, "PLANNING OPERATIONS AND INFORMATION", "PLANNING OPERATIONS AND INFORMATION", 0, 13 },
                    { 62, "PACIFIC FORESTRY CENTRE", "PACIFIC FORESTRY CENTRE", 0, 13 },
                    { 61, "NORTHERN FORESTRY CENTRE", "NORTHERN FORESTRY CENTRE", 0, 13 },
                    { 60, "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)", "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)", 0, 13 },
                    { 59, "INTEGRATED SYSTEM APPLIED", "INTEGRATED SYSTEM APPLIED", 0, 13 },
                    { 58, "GREAT LAKES FORESTRY CENTER", "GREAT LAKES FORESTRY CENTER", 0, 13 },
                    { 57, "CANADIAN WOOD FIBRE CENTRE", "CANADIAN WOOD FIBRE CENTRE", 0, 13 },
                    { 56, "ATLANTIC FORESTRY CENTRE", "ATLANTIC FORESTRY CENTRE", 0, 13 },
                    { 55, "ADM’S OFFICE CFS", "ADM’S OFFICE CFS", 0, 13 },
                    { 54, "DEPTL SVCS - CORP HR SERVICES & SYSTEMS", "DEPTL SVCS - CORP HR SERVICES & SYSTEMS", 0, 12 },
                    { 64, "POLICY ECONOMICS INTERNATIONAL &INDUSTRY", "POLICY ECONOMICS INTERNATIONAL &INDUSTRY", 0, 13 },
                    { 53, "DEPARTMENTAL SERVICES - SUPPORT TO MINO", "DEPARTMENTAL SERVICES - SUPPORT TO MINO", 0, 12 },
                    { 77, "CANMET MINES", "CANMET MINES", 0, 15 },
                    { 79, "GEOLOGICAL SURVEY OF CANADA", "GEOLOGICAL SURVEY OF CANADA", 0, 15 },
                    { 101, "SFT DEFAULT", "SFT DEFAULT", 0, 16 },
                    { 100, "RF INTEREST EARNED", "RF INTEREST EARNED", 0, 16 },
                    { 99, "RF EMPLOYEE BENEFIT PLAN", "RF EMPLOYEE BENEFIT PLAN", 0, 16 },
                    { 98, "RF CORPORATE ACCOUNTING - ACCRUALS", "RF CORPORATE ACCOUNTING - ACCRUALS", 0, 16 },
                    { 97, "QST PAID ON PURCHASE (8530)", "QST PAID ON PURCHASE (8530)", 0, 16 },
                    { 96, "PST COLLECTED", "PST COLLECTED", 0, 16 },
                    { 95, "PROCEEDS CROWN ASSET DISPOSAL", "PROCEEDS CROWN ASSET DISPOSAL", 0, 16 },
                    { 94, "PAYROLL DEDUCTIONS - PHOENIX DAMAGES", "PAYROLL DEDUCTIONS - PHOENIX DAMAGES", 0, 16 },
                    { 93, "MONIES REC'D AFTER MARCH 31 - OLD YEAR", "MONIES REC'D AFTER MARCH 31 - OLD YEAR", 0, 16 },
                    { 92, "IS SUSPEND-EXPENDITURES", "IS SUSPEND-EXPENDITURES", 0, 16 },
                    { 78, "EXPLOSIVE SAFETY & SECURITY BRANCH", "EXPLOSIVE SAFETY & SECURITY BRANCH", 0, 15 },
                    { 91, "INTEREST EARNED", "INTEREST EARNED", 0, 16 },
                    { 89, "GST COLLECTED", "GST COLLECTED", 0, 16 },
                    { 88, "GARNISHEED SALARIES", "GARNISHEED SALARIES", 0, 16 },
                    { 87, "EMPLOYEE BENEFIT PLAN", "EMPLOYEE BENEFIT PLAN", 0, 16 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorAndBranchForBranchId", "SectorId" },
                values: new object[,]
                {
                    { 86, "CORPORATE ACCOUNTING - ACCRUALS", "CORPORATE ACCOUNTING - ACCRUALS", 0, 16 },
                    { 85, "CASH RECEIPTS SUSPENSE", "CASH RECEIPTS SUSPENSE", 0, 16 },
                    { 84, "CASH IN HANDS & IN TRANSIT", "CASH IN HANDS & IN TRANSIT", 0, 16 },
                    { 83, "SURVEYOR GENERAL BRANCH", "SURVEYOR GENERAL BRANCH", 0, 15 },
                    { 82, "POLICY AND ECONOMICS BRANCH", "POLICY AND ECONOMICS BRANCH", 0, 15 },
                    { 81, "LANDS & MINERALS SECTOR RESERVE", "LANDS & MINERALS SECTOR RESERVE", 0, 15 },
                    { 80, "HAZ ADAP & OPS BR", "HAZ ADAP & OPS BR", 0, 15 },
                    { 90, "GST PAID - PURCHASES", "GST PAID - PURCHASES", 0, 16 },
                    { 102, "SSO AMEX/MASTERCARD DEFAULT", "SSO AMEX/MASTERCARD DEFAULT", 0, 16 },
                    { 52, "DEPARTMENTAL SERVICES - RP & FACILITIES", "DEPARTMENTAL SERVICES - RP & FACILITIES", 0, 12 },
                    { 50, "DEPARTMENTAL SERVICES - FPB", "DEPARTMENTAL SERVICES - FPB", 0, 12 },
                    { 23, "ASSISTANT DEPUTY MINISTER OFFICE", "ASSISTANT DEPUTY MINISTER OFFICE", 0, 8 },
                    { 22, "TMX ACCOMMODATION MEASURES", "TMX ACCOMMODATION MEASURES", 0, 7 },
                    { 21, "MAJOR PROJECT MANAGEMENT OFFICE", "MAJOR PROJECT MANAGEMENT OFFICE", 0, 7 },
                    { 20, "INDIGENOUS PARTNERSHIPS OFFICE - WEST", "INDIGENOUS PARTNERSHIPS OFFICE - WEST", 0, 7 },
                    { 19, "ATOMIC ENERGY", "ATOMIC ENERGY", 0, 6 },
                    { 18, "IND CONSULT FOR TMX", "IND CONSULT FOR TMX", 0, 5 },
                    { 17, "PETROLEUM RESOURCES BRANCH", "PETROLEUM RESOURCES BRANCH", 0, 4 },
                    { 16, "CANMET ENERGY - DEVON", "CANMET ENERGY - DEVON", 0, 4 },
                    { 15, "ADMO - SPPIO", "ADMO - SPPIO", 0, 4 },
                    { 14, "PORTFOLIO MGT & CORP SECRETARIAT BRANCH", "PORTFOLIO MGT & CORP SECRETARIAT BRANCH", 0, 3 },
                    { 24, "EXTERNAL POLICY & PARTNERSHIPS", "EXTERNAL POLICY & PARTNERSHIPS", 0, 8 },
                    { 13, "CPS CORP SERVICES", "CPS CORP SERVICES", 0, 3 },
                    { 11, "PUBLIC AFFAIRS BRANCH", "PUBLIC AFFAIRS BRANCH", 0, 3 },
                    { 10, "ADM’S OFFICE", "ADM’S OFFICE", 0, 3 },
                    { 9, "INDIGENOUS OUTREACH AND ENGAGEMENT", "INDIGENOUS OUTREACH AND ENGAGEMENT", 0, 2 },
                    { 8, "INDIGENOUS AFFAIRS AND RECON", "INDIGENOUS AFFAIRS AND RECON", 0, 2 },
                    { 7, "DGO", "DGO", 0, 2 },
                    { 6, "ADM'S OFFICE IAR SECTOR", "ADM'S OFFICE IAR SECTOR", 0, 2 },
                    { 5, "NRCAN CHARITABLE CAMPAIGN", "NRCAN CHARITABLE CAMPAIGN", 0, 1 },
                    { 4, "MINISTER'S OFFICE", "MINISTER'S OFFICE", 0, 1 },
                    { 3, "LEGAL SERVICES OFFICE", "LEGAL SERVICES OFFICE", 0, 1 },
                    { 2, "DEPUTY MINISTER'S OFFICES", "DEPUTY MINISTER'S OFFICES", 0, 1 },
                    { 12, "ENGAG DIGITAL COMMS", "ENGAG DIGITAL COMMS", 0, 3 },
                    { 51, "DEPARTMENTAL SERVICES - POB", "DEPARTMENTAL SERVICES - POB", 0, 12 },
                    { 25, "INNOVATION BRANCH", "INNOVATION BRANCH", 0, 8 },
                    { 27, "PLANNING, DELIVERY & RESULTS BRANCH", "PLANNING, DELIVERY & RESULTS BRANCH", 0, 8 },
                    { 49, "DEPARTMENTAL SERVICES - CIOSB", "DEPARTMENTAL SERVICES - CIOSB", 0, 12 },
                    { 48, "PLANNING AND OPERATION BRANCH", "PLANNING AND OPERATION BRANCH", 0, 11 },
                    { 47, "HUMAN RESOURCE /SECURITY MGMT BRANCH", "HUMAN RESOURCE /SECURITY MGMT BRANCH", 0, 11 },
                    { 46, "FINANCE AND PROCUREMENT BRANCH", "FINANCE AND PROCUREMENT BRANCH", 0, 11 },
                    { 45, "FACILITIES-REGIONS -EM-OHS", "FACILITIES-REGIONS -EM-OHS", 0, 11 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorAndBranchForBranchId", "SectorId" },
                values: new object[,]
                {
                    { 44, "DEPARTMENTAL PROJECTS", "DEPARTMENTAL PROJECTS", 0, 11 },
                    { 43, "CORPORATE HR SERVICES & SYSTEMS", "CORPORATE HR SERVICES & SYSTEMS", 0, 11 },
                    { 42, "CMSS RESERVE", "CMSS RESERVE", 0, 11 },
                    { 41, "CHIEF INFORMATION AND SECURITY OFFICE", "CHIEF INFORMATION AND SECURITY OFFICE", 0, 11 },
                    { 40, "ADM'S OFFICE", "ADM'S OFFICE", 0, 11 },
                    { 26, "PARDP", "PARDP", 0, 8 },
                    { 39, "OFFICE OF ENERGY EFFICIENCY", "OFFICE OF ENERGY EFFICIENCY", 0, 10 },
                    { 37, "ENERGY POLICY BRANCH", "ENERGY POLICY BRANCH", 0, 10 },
                    { 36, "ELECTRICITY RESOURCES BRANCH", "ELECTRICITY RESOURCES BRANCH", 0, 10 },
                    { 35, "CORPORATE ACCOUNTS", "CORPORATE ACCOUNTS", 0, 10 },
                    { 34, "CLEAN FUELS BRANCH", "CLEAN FUELS BRANCH", 0, 10 },
                    { 33, "ADM’S OFFICE", "ADM’S OFFICE", 0, 10 },
                    { 32, "OFFICE OF THE CHIEF SCIENTIST", "OFFICE OF THE CHIEF SCIENTIST", 0, 9 },
                    { 31, "OCS - OPERATIONS", "OCS - OPERATIONS", 0, 9 },
                    { 30, "CANADA CENTRE FOR MAPPING AND EARTH OBS", "CANADA CENTRE FOR MAPPING AND EARTH OBS", 0, 8 },
                    { 29, "STRATEGIC POLICY BRANCH", "STRATEGIC POLICY BRANCH", 0, 8 },
                    { 28, "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS", "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS", 0, 8 },
                    { 38, "INTERNATIONAL ENERGY BRANCH", "INTERNATIONAL ENERGY BRANCH", 0, 10 },
                    { 103, "TRAVELLERS CHEQUES - CLEARING ACCOUNT", "TRAVELLERS CHEQUES - CLEARING ACCOUNT", 0, 16 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_SectorId",
                table: "Branches",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SectorAndBranchSectorBranch_ID",
                table: "Budgets",
                column: "SectorAndBranchSectorBranch_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SectorAndBranches_BranchId",
                table: "SectorAndBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorAndBranches_SectorId",
                table: "SectorAndBranches",
                column: "SectorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "SectorAndBranches");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Sectors");
        }
    }
}
