using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class SeedingMaybe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Sectors_SectorId",
                table: "Branches");

            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorId",
            //    table: "Sectors",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorBranch_ID",
            //    table: "SectorAndBranches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Budget_ID",
            //    table: "Budgets",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "BranchId",
            //    table: "Branches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "SectorId", "SectorNameEn", "SectorNameFr" },
                values: new object[,]
                {
                    { 1, "DIRECTION AND COORDINATION", "DIRECTION AND COORDINATION" },
                    { 2, "INDIGENOUS AFFAIRS AND RECON SECTOR", "INDIGENOUS AFFAIRS AND RECON SECTOR" },
                    { 3, "COMMS PORTFOLIO SEC", "COMMS PORTFOLIO SEC" },
                    { 4, "STRATEGIC PETROLEUM POL & INV OFFICE", "STRATEGIC PETROLEUM POL & INV OFFICE" },
                    { 5, "IND CONSULT FOR TMX", "IND CONSULT FOR TMX" },
                    { 6, "ATOMIC ENERGY SECTOR", "ATOMIC ENERGY SECTOR" },
                    { 7, "MAJOR PROJECT MANAGEMENT OFFICE SECTOR", "MAJOR PROJECT MANAGEMENT OFFICE SECTOR" },
                    { 8, "STRATEGIC POLICY AND INNOVATION", "STRATEGIC POLICY AND INNOVATION" },
                    { 9, "OFFICE OF THE CHIEF SCIENTIST", "OFFICE OF THE CHIEF SCIENTIST" },
                    { 10, "LOW CARBON ENERGY SECTOR", "LOW CARBON ENERGY SECTOR" },
                    { 11, "CORPORATE MANAGEMENT AND SERVICES SECTOR", "CORPORATE MANAGEMENT AND SERVICES SECTOR" },
                    { 12, "DEPARTMENTAL SERVICES", "DEPARTMENTAL SERVICES" },
                    { 13, "CANADIAN FOREST SERVICE", "CANADIAN FOREST SERVICE" },
                    { 14, "ENERGY TECHNOLOGY SECTOR", "ENERGY TECHNOLOGY SECTOR" },
                    { 15, "LANDS AND MINERALS SECTOR", "LANDS AND MINERALS SECTOR" },
                    { 16, "CORPORATE ACCOUNTING", "CORPORATE ACCOUNTING" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorId" },
                values: new object[,]
                {
                    { 1, "AUDIT & EVALUATION BRANCH", "AUDIT & EVALUATION BRANCH", 1 },
                    { 75, "ADM’S OFFICE", "ADM’S OFFICE", 15 },
                    { 74, "SECTOR MANAGEMENT ACCOUNT", "SECTOR MANAGEMENT ACCOUNT", 14 },
                    { 73, "PLANNING & OPERATIONS", "PLANNING & OPERATIONS", 14 },
                    { 72, "OFFICE OF ENERGY RESEARCH AND DEV", "OFFICE OF ENERGY RESEARCH AND DEV", 14 },
                    { 71, "FINANCE MANAGEMENT", "FINANCE MANAGEMENT", 14 },
                    { 70, "ETS ADM'S OFFICE", "ETS ADM'S OFFICE", 14 },
                    { 69, "CANMET MATERIALS", "CANMET MATERIALS", 14 },
                    { 68, "CANMET ENERGY – VARENNES", "CANMET ENERGY – VARENNES", 14 },
                    { 67, "CANMET ENERGY – OTTAWA", "CANMET ENERGY – OTTAWA", 14 },
                    { 66, "ADM’S RESERVE", "ADM’S RESERVE", 14 },
                    { 76, "BUSINESS MANAGEMENT SERVICES AND DATA", "BUSINESS MANAGEMENT SERVICES AND DATA", 15 },
                    { 65, "SCIENCE POLICY INTEGRATION", "SCIENCE POLICY INTEGRATION", 13 },
                    { 63, "PLANNING OPERATIONS AND INFORMATION", "PLANNING OPERATIONS AND INFORMATION", 13 },
                    { 62, "PACIFIC FORESTRY CENTRE", "PACIFIC FORESTRY CENTRE", 13 },
                    { 61, "NORTHERN FORESTRY CENTRE", "NORTHERN FORESTRY CENTRE", 13 },
                    { 60, "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)", "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)", 13 },
                    { 59, "INTEGRATED SYSTEM APPLIED", "INTEGRATED SYSTEM APPLIED", 13 },
                    { 58, "GREAT LAKES FORESTRY CENTER", "GREAT LAKES FORESTRY CENTER", 13 },
                    { 57, "CANADIAN WOOD FIBRE CENTRE", "CANADIAN WOOD FIBRE CENTRE", 13 },
                    { 56, "ATLANTIC FORESTRY CENTRE", "ATLANTIC FORESTRY CENTRE", 13 },
                    { 55, "ADM’S OFFICE CFS", "ADM’S OFFICE CFS", 13 },
                    { 54, "DEPTL SVCS - CORP HR SERVICES & SYSTEMS", "DEPTL SVCS - CORP HR SERVICES & SYSTEMS", 12 },
                    { 64, "POLICY ECONOMICS INTERNATIONAL &INDUSTRY", "POLICY ECONOMICS INTERNATIONAL &INDUSTRY", 13 },
                    { 53, "DEPARTMENTAL SERVICES - SUPPORT TO MINO", "DEPARTMENTAL SERVICES - SUPPORT TO MINO", 12 },
                    { 77, "CANMET MINES", "CANMET MINES", 15 },
                    { 79, "GEOLOGICAL SURVEY OF CANADA", "GEOLOGICAL SURVEY OF CANADA", 15 },
                    { 101, "SFT DEFAULT", "SFT DEFAULT", 16 },
                    { 100, "RF INTEREST EARNED", "RF INTEREST EARNED", 16 },
                    { 99, "RF EMPLOYEE BENEFIT PLAN", "RF EMPLOYEE BENEFIT PLAN", 16 },
                    { 98, "RF CORPORATE ACCOUNTING - ACCRUALS", "RF CORPORATE ACCOUNTING - ACCRUALS", 16 },
                    { 97, "QST PAID ON PURCHASE (8530)", "QST PAID ON PURCHASE (8530)", 16 },
                    { 96, "PST COLLECTED", "PST COLLECTED", 16 },
                    { 95, "PROCEEDS CROWN ASSET DISPOSAL", "PROCEEDS CROWN ASSET DISPOSAL", 16 },
                    { 94, "PAYROLL DEDUCTIONS - PHOENIX DAMAGES", "PAYROLL DEDUCTIONS - PHOENIX DAMAGES", 16 },
                    { 93, "MONIES REC'D AFTER MARCH 31 - OLD YEAR", "MONIES REC'D AFTER MARCH 31 - OLD YEAR", 16 },
                    { 92, "IS SUSPEND-EXPENDITURES", "IS SUSPEND-EXPENDITURES", 16 },
                    { 78, "EXPLOSIVE SAFETY & SECURITY BRANCH", "EXPLOSIVE SAFETY & SECURITY BRANCH", 15 },
                    { 91, "INTEREST EARNED", "INTEREST EARNED", 16 },
                    { 89, "GST COLLECTED", "GST COLLECTED", 16 },
                    { 88, "GARNISHEED SALARIES", "GARNISHEED SALARIES", 16 },
                    { 87, "EMPLOYEE BENEFIT PLAN", "EMPLOYEE BENEFIT PLAN", 16 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorId" },
                values: new object[,]
                {
                    { 86, "CORPORATE ACCOUNTING - ACCRUALS", "CORPORATE ACCOUNTING - ACCRUALS", 16 },
                    { 85, "CASH RECEIPTS SUSPENSE", "CASH RECEIPTS SUSPENSE", 16 },
                    { 84, "CASH IN HANDS & IN TRANSIT", "CASH IN HANDS & IN TRANSIT", 16 },
                    { 83, "SURVEYOR GENERAL BRANCH", "SURVEYOR GENERAL BRANCH", 15 },
                    { 82, "POLICY AND ECONOMICS BRANCH", "POLICY AND ECONOMICS BRANCH", 15 },
                    { 81, "LANDS & MINERALS SECTOR RESERVE", "LANDS & MINERALS SECTOR RESERVE", 15 },
                    { 80, "HAZ ADAP & OPS BR", "HAZ ADAP & OPS BR", 15 },
                    { 90, "GST PAID - PURCHASES", "GST PAID - PURCHASES", 16 },
                    { 102, "SSO AMEX/MASTERCARD DEFAULT", "SSO AMEX/MASTERCARD DEFAULT", 16 },
                    { 52, "DEPARTMENTAL SERVICES - RP & FACILITIES", "DEPARTMENTAL SERVICES - RP & FACILITIES", 12 },
                    { 50, "DEPARTMENTAL SERVICES - FPB", "DEPARTMENTAL SERVICES - FPB", 12 },
                    { 23, "ASSISTANT DEPUTY MINISTER OFFICE", "ASSISTANT DEPUTY MINISTER OFFICE", 8 },
                    { 22, "TMX ACCOMMODATION MEASURES", "TMX ACCOMMODATION MEASURES", 7 },
                    { 21, "MAJOR PROJECT MANAGEMENT OFFICE", "MAJOR PROJECT MANAGEMENT OFFICE", 7 },
                    { 20, "INDIGENOUS PARTNERSHIPS OFFICE - WEST", "INDIGENOUS PARTNERSHIPS OFFICE - WEST", 7 },
                    { 19, "ATOMIC ENERGY", "ATOMIC ENERGY", 6 },
                    { 18, "IND CONSULT FOR TMX", "IND CONSULT FOR TMX", 5 },
                    { 17, "PETROLEUM RESOURCES BRANCH", "PETROLEUM RESOURCES BRANCH", 4 },
                    { 16, "CANMET ENERGY - DEVON", "CANMET ENERGY - DEVON", 4 },
                    { 15, "ADMO - SPPIO", "ADMO - SPPIO", 4 },
                    { 14, "PORTFOLIO MGT & CORP SECRETARIAT BRANCH", "PORTFOLIO MGT & CORP SECRETARIAT BRANCH", 3 },
                    { 24, "EXTERNAL POLICY & PARTNERSHIPS", "EXTERNAL POLICY & PARTNERSHIPS", 8 },
                    { 13, "CPS CORP SERVICES", "CPS CORP SERVICES", 3 },
                    { 11, "PUBLIC AFFAIRS BRANCH", "PUBLIC AFFAIRS BRANCH", 3 },
                    { 10, "ADM’S OFFICE", "ADM’S OFFICE", 3 },
                    { 9, "INDIGENOUS OUTREACH AND ENGAGEMENT", "INDIGENOUS OUTREACH AND ENGAGEMENT", 2 },
                    { 8, "INDIGENOUS AFFAIRS AND RECON", "INDIGENOUS AFFAIRS AND RECON", 2 },
                    { 7, "DGO", "DGO", 2 },
                    { 6, "ADM'S OFFICE IAR SECTOR", "ADM'S OFFICE IAR SECTOR", 2 },
                    { 5, "NRCAN CHARITABLE CAMPAIGN", "NRCAN CHARITABLE CAMPAIGN", 1 },
                    { 4, "MINISTER'S OFFICE", "MINISTER'S OFFICE", 1 },
                    { 3, "LEGAL SERVICES OFFICE", "LEGAL SERVICES OFFICE", 1 },
                    { 2, "DEPUTY MINISTER'S OFFICES", "DEPUTY MINISTER'S OFFICES", 1 },
                    { 12, "ENGAG DIGITAL COMMS", "ENGAG DIGITAL COMMS", 3 },
                    { 51, "DEPARTMENTAL SERVICES - POB", "DEPARTMENTAL SERVICES - POB", 12 },
                    { 25, "INNOVATION BRANCH", "INNOVATION BRANCH", 8 },
                    { 27, "PLANNING, DELIVERY & RESULTS BRANCH", "PLANNING, DELIVERY & RESULTS BRANCH", 8 },
                    { 49, "DEPARTMENTAL SERVICES - CIOSB", "DEPARTMENTAL SERVICES - CIOSB", 12 },
                    { 48, "PLANNING AND OPERATION BRANCH", "PLANNING AND OPERATION BRANCH", 11 },
                    { 47, "HUMAN RESOURCE /SECURITY MGMT BRANCH", "HUMAN RESOURCE /SECURITY MGMT BRANCH", 11 },
                    { 46, "FINANCE AND PROCUREMENT BRANCH", "FINANCE AND PROCUREMENT BRANCH", 11 },
                    { 45, "FACILITIES-REGIONS -EM-OHS", "FACILITIES-REGIONS -EM-OHS", 11 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchNameEn", "BranchNameFr", "SectorId" },
                values: new object[,]
                {
                    { 44, "DEPARTMENTAL PROJECTS", "DEPARTMENTAL PROJECTS", 11 },
                    { 43, "CORPORATE HR SERVICES & SYSTEMS", "CORPORATE HR SERVICES & SYSTEMS", 11 },
                    { 42, "CMSS RESERVE", "CMSS RESERVE", 11 },
                    { 41, "CHIEF INFORMATION AND SECURITY OFFICE", "CHIEF INFORMATION AND SECURITY OFFICE", 11 },
                    { 40, "ADM'S OFFICE", "ADM'S OFFICE", 11 },
                    { 26, "PARDP", "PARDP", 8 },
                    { 39, "OFFICE OF ENERGY EFFICIENCY", "OFFICE OF ENERGY EFFICIENCY", 10 },
                    { 37, "ENERGY POLICY BRANCH", "ENERGY POLICY BRANCH", 10 },
                    { 36, "ELECTRICITY RESOURCES BRANCH", "ELECTRICITY RESOURCES BRANCH", 10 },
                    { 35, "CORPORATE ACCOUNTS", "CORPORATE ACCOUNTS", 10 },
                    { 34, "CLEAN FUELS BRANCH", "CLEAN FUELS BRANCH", 10 },
                    { 33, "ADM’S OFFICE", "ADM’S OFFICE", 10 },
                    { 32, "OFFICE OF THE CHIEF SCIENTIST", "OFFICE OF THE CHIEF SCIENTIST", 9 },
                    { 31, "OCS - OPERATIONS", "OCS - OPERATIONS", 9 },
                    { 30, "CANADA CENTRE FOR MAPPING AND EARTH OBS", "CANADA CENTRE FOR MAPPING AND EARTH OBS", 8 },
                    { 29, "STRATEGIC POLICY BRANCH", "STRATEGIC POLICY BRANCH", 8 },
                    { 28, "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS", "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS", 8 },
                    { 38, "INTERNATIONAL ENERGY BRANCH", "INTERNATIONAL ENERGY BRANCH", 10 },
                    { 103, "TRAVELLERS CHEQUES - CLEARING ACCOUNT", "TRAVELLERS CHEQUES - CLEARING ACCOUNT", 16 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Sectors_SectorId",
                table: "Branches",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "SectorId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Sectors_SectorId",
                table: "Branches");

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "SectorId",
                keyValue: 16);

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Sectors",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "SectorBranch_ID",
                table: "SectorAndBranches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Budget_ID",
                table: "Budgets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Branches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "Branches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Sectors_SectorId",
                table: "Branches",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "SectorId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
