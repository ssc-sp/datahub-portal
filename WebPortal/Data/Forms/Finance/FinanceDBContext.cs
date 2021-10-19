using Microsoft.EntityFrameworkCore;
using Datahub.Portal.Pages.Forms.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class FinanceDBContext : DbContext
    {
        public FinanceDBContext(DbContextOptions<FinanceDBContext> options) : base(options)
        { }

        public DbSet<SectorAndBranch> SectorAndBranches { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        public DbSet<Sector> Sectors { get; set; }

        public DbSet<Branch> Branches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildSectors(modelBuilder);
            BuildBranches(modelBuilder);

            //modelBuilder.Entity<SectorAndBranch>()
            //.HasOne(a => a.Sector)
            //.WithOne(b => b.SectorAndBranch)
            //.HasForeignKey<Sector>(b => b.SectorAndBranchForSectorId);

            //modelBuilder.Entity<SectorAndBranch>()
            //.HasOne(a => a.Branch)
            //.WithOne(b => b.SectorAndBranch)
            //.HasForeignKey<Branch>(b => b.SectorAndBranchForBranchId);
        }

        private void BuildBranches(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>().HasData(
                new Branch { BranchId = 1, SectorId = 1, BranchNameEn = "AUDIT & EVALUATION BRANCH", BranchNameFr = "AUDIT & EVALUATION BRANCH" },
                new Branch { BranchId = 2, SectorId = 1, BranchNameEn = "DEPUTY MINISTER'S OFFICES", BranchNameFr = "DEPUTY MINISTER'S OFFICES" },
                new Branch { BranchId = 3, SectorId = 1, BranchNameEn = "LEGAL SERVICES OFFICE", BranchNameFr = "LEGAL SERVICES OFFICE" },
                new Branch { BranchId = 4, SectorId = 1, BranchNameEn = "MINISTER'S OFFICE", BranchNameFr = "MINISTER'S OFFICE" },
                new Branch { BranchId = 5, SectorId = 1, BranchNameEn = "NRCAN CHARITABLE CAMPAIGN", BranchNameFr = "NRCAN CHARITABLE CAMPAIGN" },
                new Branch { BranchId = 6, SectorId = 2, BranchNameEn = "ADM'S OFFICE IAR SECTOR", BranchNameFr = "ADM'S OFFICE IAR SECTOR" },
                new Branch { BranchId = 7, SectorId = 2, BranchNameEn = "DGO", BranchNameFr = "DGO" },
                new Branch { BranchId = 8, SectorId = 2, BranchNameEn = "INDIGENOUS AFFAIRS AND RECON", BranchNameFr = "INDIGENOUS AFFAIRS AND RECON" },
                new Branch { BranchId = 9, SectorId = 2, BranchNameEn = "INDIGENOUS OUTREACH AND ENGAGEMENT", BranchNameFr = "INDIGENOUS OUTREACH AND ENGAGEMENT" },
                new Branch { BranchId = 10, SectorId = 3, BranchNameEn = "ADM’S OFFICE", BranchNameFr = "ADM’S OFFICE" },
                new Branch { BranchId = 11, SectorId = 3, BranchNameEn = "PUBLIC AFFAIRS BRANCH", BranchNameFr = "PUBLIC AFFAIRS BRANCH" },
                new Branch { BranchId = 12, SectorId = 3, BranchNameEn = "ENGAG DIGITAL COMMS", BranchNameFr = "ENGAG DIGITAL COMMS" },
                new Branch { BranchId = 13, SectorId = 3, BranchNameEn = "CPS CORP SERVICES", BranchNameFr = "CPS CORP SERVICES" },
                new Branch { BranchId = 14, SectorId = 3, BranchNameEn = "PORTFOLIO MGT & CORP SECRETARIAT BRANCH", BranchNameFr = "PORTFOLIO MGT & CORP SECRETARIAT BRANCH" },
                new Branch { BranchId = 15, SectorId = 4, BranchNameEn = "ADMO - SPPIO", BranchNameFr = "ADMO - SPPIO" },
                new Branch { BranchId = 16, SectorId = 4, BranchNameEn = "CANMET ENERGY - DEVON", BranchNameFr = "CANMET ENERGY - DEVON" },
                new Branch { BranchId = 17, SectorId = 4, BranchNameEn = "PETROLEUM RESOURCES BRANCH", BranchNameFr = "PETROLEUM RESOURCES BRANCH" },
                new Branch { BranchId = 18, SectorId = 5, BranchNameEn = "IND CONSULT FOR TMX", BranchNameFr = "IND CONSULT FOR TMX" },
                new Branch { BranchId = 19, SectorId = 6, BranchNameEn = "ATOMIC ENERGY", BranchNameFr = "ATOMIC ENERGY" },
                new Branch { BranchId = 20, SectorId = 7, BranchNameEn = "INDIGENOUS PARTNERSHIPS OFFICE - WEST", BranchNameFr = "INDIGENOUS PARTNERSHIPS OFFICE - WEST" },
                new Branch { BranchId = 21, SectorId = 7, BranchNameEn = "MAJOR PROJECT MANAGEMENT OFFICE", BranchNameFr = "MAJOR PROJECT MANAGEMENT OFFICE" },
                new Branch { BranchId = 22, SectorId = 7, BranchNameEn = "TMX ACCOMMODATION MEASURES", BranchNameFr = "TMX ACCOMMODATION MEASURES" },
                new Branch { BranchId = 23, SectorId = 8, BranchNameEn = "ASSISTANT DEPUTY MINISTER OFFICE", BranchNameFr = "ASSISTANT DEPUTY MINISTER OFFICE" },
                new Branch { BranchId = 24, SectorId = 8, BranchNameEn = "EXTERNAL POLICY & PARTNERSHIPS", BranchNameFr = "EXTERNAL POLICY & PARTNERSHIPS" },
                new Branch { BranchId = 25, SectorId = 8, BranchNameEn = "INNOVATION BRANCH", BranchNameFr = "INNOVATION BRANCH" },
                new Branch { BranchId = 26, SectorId = 8, BranchNameEn = "PARDP", BranchNameFr = "PARDP" },
                new Branch { BranchId = 27, SectorId = 8, BranchNameEn = "PLANNING, DELIVERY & RESULTS BRANCH", BranchNameFr = "PLANNING, DELIVERY & RESULTS BRANCH" },
                new Branch { BranchId = 28, SectorId = 8, BranchNameEn = "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS", BranchNameFr = "SECTOR FINANCIAL ADVISOR'S OFFICE SPRS" },
                new Branch { BranchId = 29, SectorId = 8, BranchNameEn = "STRATEGIC POLICY BRANCH", BranchNameFr = "STRATEGIC POLICY BRANCH" },
                new Branch { BranchId = 30, SectorId = 8, BranchNameEn = "CANADA CENTRE FOR MAPPING AND EARTH OBS", BranchNameFr = "CANADA CENTRE FOR MAPPING AND EARTH OBS" },
                new Branch { BranchId = 31, SectorId = 9, BranchNameEn = "OCS - OPERATIONS", BranchNameFr = "OCS - OPERATIONS" },
                new Branch { BranchId = 32, SectorId = 9, BranchNameEn = "OFFICE OF THE CHIEF SCIENTIST", BranchNameFr = "OFFICE OF THE CHIEF SCIENTIST" },
                new Branch { BranchId = 33, SectorId = 10, BranchNameEn = "ADM’S OFFICE", BranchNameFr = "ADM’S OFFICE" },
                new Branch { BranchId = 34, SectorId = 10, BranchNameEn = "CLEAN FUELS BRANCH", BranchNameFr = "CLEAN FUELS BRANCH" },
                new Branch { BranchId = 35, SectorId = 10, BranchNameEn = "CORPORATE ACCOUNTS", BranchNameFr = "CORPORATE ACCOUNTS" },
                new Branch { BranchId = 36, SectorId = 10, BranchNameEn = "ELECTRICITY RESOURCES BRANCH", BranchNameFr = "ELECTRICITY RESOURCES BRANCH" },
                new Branch { BranchId = 37, SectorId = 10, BranchNameEn = "ENERGY POLICY BRANCH", BranchNameFr = "ENERGY POLICY BRANCH" },
                new Branch { BranchId = 38, SectorId = 10, BranchNameEn = "INTERNATIONAL ENERGY BRANCH", BranchNameFr = "INTERNATIONAL ENERGY BRANCH" },
                new Branch { BranchId = 39, SectorId = 10, BranchNameEn = "OFFICE OF ENERGY EFFICIENCY", BranchNameFr = "OFFICE OF ENERGY EFFICIENCY" },
                new Branch { BranchId = 40, SectorId = 11, BranchNameEn = "ADM'S OFFICE", BranchNameFr = "ADM'S OFFICE" },
                new Branch { BranchId = 41, SectorId = 11, BranchNameEn = "CHIEF INFORMATION AND SECURITY OFFICE", BranchNameFr = "CHIEF INFORMATION AND SECURITY OFFICE" },
                new Branch { BranchId = 42, SectorId = 11, BranchNameEn = "CMSS RESERVE", BranchNameFr = "CMSS RESERVE" },
                new Branch { BranchId = 43, SectorId = 11, BranchNameEn = "CORPORATE HR SERVICES & SYSTEMS", BranchNameFr = "CORPORATE HR SERVICES & SYSTEMS" },
                new Branch { BranchId = 44, SectorId = 11, BranchNameEn = "DEPARTMENTAL PROJECTS", BranchNameFr = "DEPARTMENTAL PROJECTS" },
                new Branch { BranchId = 45, SectorId = 11, BranchNameEn = "FACILITIES-REGIONS -EM-OHS", BranchNameFr = "FACILITIES-REGIONS -EM-OHS" },
                new Branch { BranchId = 46, SectorId = 11, BranchNameEn = "FINANCE AND PROCUREMENT BRANCH", BranchNameFr = "FINANCE AND PROCUREMENT BRANCH" },
                new Branch { BranchId = 47, SectorId = 11, BranchNameEn = "HUMAN RESOURCE /SECURITY MGMT BRANCH", BranchNameFr = "HUMAN RESOURCE /SECURITY MGMT BRANCH" },
                new Branch { BranchId = 48, SectorId = 11, BranchNameEn = "PLANNING AND OPERATION BRANCH", BranchNameFr = "PLANNING AND OPERATION BRANCH" },
                new Branch { BranchId = 49, SectorId = 12, BranchNameEn = "DEPARTMENTAL SERVICES - CIOSB", BranchNameFr = "DEPARTMENTAL SERVICES - CIOSB" },
                new Branch { BranchId = 50, SectorId = 12, BranchNameEn = "DEPARTMENTAL SERVICES - FPB", BranchNameFr = "DEPARTMENTAL SERVICES - FPB" },
                new Branch { BranchId = 51, SectorId = 12, BranchNameEn = "DEPARTMENTAL SERVICES - POB", BranchNameFr = "DEPARTMENTAL SERVICES - POB" },
                new Branch { BranchId = 52, SectorId = 12, BranchNameEn = "DEPARTMENTAL SERVICES - RP & FACILITIES", BranchNameFr = "DEPARTMENTAL SERVICES - RP & FACILITIES" },
                new Branch { BranchId = 53, SectorId = 12, BranchNameEn = "DEPARTMENTAL SERVICES - SUPPORT TO MINO", BranchNameFr = "DEPARTMENTAL SERVICES - SUPPORT TO MINO" },
                new Branch { BranchId = 54, SectorId = 12, BranchNameEn = "DEPTL SVCS - CORP HR SERVICES & SYSTEMS", BranchNameFr = "DEPTL SVCS - CORP HR SERVICES & SYSTEMS" },
                new Branch { BranchId = 55, SectorId = 13, BranchNameEn = "ADM’S OFFICE CFS", BranchNameFr = "ADM’S OFFICE CFS" },
                new Branch { BranchId = 56, SectorId = 13, BranchNameEn = "ATLANTIC FORESTRY CENTRE", BranchNameFr = "ATLANTIC FORESTRY CENTRE" },
                new Branch { BranchId = 57, SectorId = 13, BranchNameEn = "CANADIAN WOOD FIBRE CENTRE", BranchNameFr = "CANADIAN WOOD FIBRE CENTRE" },
                new Branch { BranchId = 58, SectorId = 13, BranchNameEn = "GREAT LAKES FORESTRY CENTER", BranchNameFr = "GREAT LAKES FORESTRY CENTER" },
                new Branch { BranchId = 59, SectorId = 13, BranchNameEn = "INTEGRATED SYSTEM APPLIED", BranchNameFr = "INTEGRATED SYSTEM APPLIED" },
                new Branch { BranchId = 60, SectorId = 13, BranchNameEn = "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)", BranchNameFr = "LAURENTIAN FOR CTR (INCL SUSPENSE ACCNT)" },
                new Branch { BranchId = 61, SectorId = 13, BranchNameEn = "NORTHERN FORESTRY CENTRE", BranchNameFr = "NORTHERN FORESTRY CENTRE" },
                new Branch { BranchId = 62, SectorId = 13, BranchNameEn = "PACIFIC FORESTRY CENTRE", BranchNameFr = "PACIFIC FORESTRY CENTRE" },
                new Branch { BranchId = 63, SectorId = 13, BranchNameEn = "PLANNING OPERATIONS AND INFORMATION", BranchNameFr = "PLANNING OPERATIONS AND INFORMATION" },
                new Branch { BranchId = 64, SectorId = 13, BranchNameEn = "POLICY ECONOMICS INTERNATIONAL &INDUSTRY", BranchNameFr = "POLICY ECONOMICS INTERNATIONAL &INDUSTRY" },
                new Branch { BranchId = 65, SectorId = 13, BranchNameEn = "SCIENCE POLICY INTEGRATION", BranchNameFr = "SCIENCE POLICY INTEGRATION" },
                new Branch { BranchId = 66, SectorId = 14, BranchNameEn = "ADM’S RESERVE", BranchNameFr = "ADM’S RESERVE" },
                new Branch { BranchId = 67, SectorId = 14, BranchNameEn = "CANMET ENERGY – OTTAWA", BranchNameFr = "CANMET ENERGY – OTTAWA" },
                new Branch { BranchId = 68, SectorId = 14, BranchNameEn = "CANMET ENERGY – VARENNES", BranchNameFr = "CANMET ENERGY – VARENNES" },
                new Branch { BranchId = 69, SectorId = 14, BranchNameEn = "CANMET MATERIALS", BranchNameFr = "CANMET MATERIALS" },
                new Branch { BranchId = 70, SectorId = 14, BranchNameEn = "ETS ADM'S OFFICE", BranchNameFr = "ETS ADM'S OFFICE" },
                new Branch { BranchId = 71, SectorId = 14, BranchNameEn = "FINANCE MANAGEMENT", BranchNameFr = "FINANCE MANAGEMENT" },
                new Branch { BranchId = 72, SectorId = 14, BranchNameEn = "OFFICE OF ENERGY RESEARCH AND DEV", BranchNameFr = "OFFICE OF ENERGY RESEARCH AND DEV" },
                new Branch { BranchId = 73, SectorId = 14, BranchNameEn = "PLANNING & OPERATIONS", BranchNameFr = "PLANNING & OPERATIONS" },
                new Branch { BranchId = 74, SectorId = 14, BranchNameEn = "SECTOR MANAGEMENT ACCOUNT", BranchNameFr = "SECTOR MANAGEMENT ACCOUNT" },
                new Branch { BranchId = 75, SectorId = 15, BranchNameEn = "ADM’S OFFICE", BranchNameFr = "ADM’S OFFICE" },
                new Branch { BranchId = 76, SectorId = 15, BranchNameEn = "BUSINESS MANAGEMENT SERVICES AND DATA", BranchNameFr = "BUSINESS MANAGEMENT SERVICES AND DATA" },
                new Branch { BranchId = 77, SectorId = 15, BranchNameEn = "CANMET MINES", BranchNameFr = "CANMET MINES" },
                new Branch { BranchId = 78, SectorId = 15, BranchNameEn = "EXPLOSIVE SAFETY & SECURITY BRANCH", BranchNameFr = "EXPLOSIVE SAFETY & SECURITY BRANCH" },
                new Branch { BranchId = 79, SectorId = 15, BranchNameEn = "GEOLOGICAL SURVEY OF CANADA", BranchNameFr = "GEOLOGICAL SURVEY OF CANADA" },
                new Branch { BranchId = 80, SectorId = 15, BranchNameEn = "HAZ ADAP & OPS BR", BranchNameFr = "HAZ ADAP & OPS BR" },
                new Branch { BranchId = 81, SectorId = 15, BranchNameEn = "LANDS & MINERALS SECTOR RESERVE", BranchNameFr = "LANDS & MINERALS SECTOR RESERVE" },
                new Branch { BranchId = 82, SectorId = 15, BranchNameEn = "POLICY AND ECONOMICS BRANCH", BranchNameFr = "POLICY AND ECONOMICS BRANCH" },
                new Branch { BranchId = 83, SectorId = 15, BranchNameEn = "SURVEYOR GENERAL BRANCH", BranchNameFr = "SURVEYOR GENERAL BRANCH" },
                new Branch { BranchId = 84, SectorId = 16, BranchNameEn = "CASH IN HANDS & IN TRANSIT", BranchNameFr = "CASH IN HANDS & IN TRANSIT" },
                new Branch { BranchId = 85, SectorId = 16, BranchNameEn = "CASH RECEIPTS SUSPENSE", BranchNameFr = "CASH RECEIPTS SUSPENSE" },
                new Branch { BranchId = 86, SectorId = 16, BranchNameEn = "CORPORATE ACCOUNTING - ACCRUALS", BranchNameFr = "CORPORATE ACCOUNTING - ACCRUALS" },
                new Branch { BranchId = 87, SectorId = 16, BranchNameEn = "EMPLOYEE BENEFIT PLAN", BranchNameFr = "EMPLOYEE BENEFIT PLAN" },
                new Branch { BranchId = 88, SectorId = 16, BranchNameEn = "GARNISHEED SALARIES", BranchNameFr = "GARNISHEED SALARIES" },
                new Branch { BranchId = 89, SectorId = 16, BranchNameEn = "GST COLLECTED", BranchNameFr = "GST COLLECTED" },
                new Branch { BranchId = 90, SectorId = 16, BranchNameEn = "GST PAID - PURCHASES", BranchNameFr = "GST PAID - PURCHASES" },
                new Branch { BranchId = 91, SectorId = 16, BranchNameEn = "INTEREST EARNED", BranchNameFr = "INTEREST EARNED" },
                new Branch { BranchId = 92, SectorId = 16, BranchNameEn = "IS SUSPEND-EXPENDITURES", BranchNameFr = "IS SUSPEND-EXPENDITURES" },
                new Branch { BranchId = 93, SectorId = 16, BranchNameEn = "MONIES REC'D AFTER MARCH 31 - OLD YEAR", BranchNameFr = "MONIES REC'D AFTER MARCH 31 - OLD YEAR" },
                new Branch { BranchId = 94, SectorId = 16, BranchNameEn = "PAYROLL DEDUCTIONS - PHOENIX DAMAGES", BranchNameFr = "PAYROLL DEDUCTIONS - PHOENIX DAMAGES" },
                new Branch { BranchId = 95, SectorId = 16, BranchNameEn = "PROCEEDS CROWN ASSET DISPOSAL", BranchNameFr = "PROCEEDS CROWN ASSET DISPOSAL" },
                new Branch { BranchId = 96, SectorId = 16, BranchNameEn = "PST COLLECTED", BranchNameFr = "PST COLLECTED" },
                new Branch { BranchId = 97, SectorId = 16, BranchNameEn = "QST PAID ON PURCHASE (8530)", BranchNameFr = "QST PAID ON PURCHASE (8530)" },
                new Branch { BranchId = 98, SectorId = 16, BranchNameEn = "RF CORPORATE ACCOUNTING - ACCRUALS", BranchNameFr = "RF CORPORATE ACCOUNTING - ACCRUALS" },
                new Branch { BranchId = 99, SectorId = 16, BranchNameEn = "RF EMPLOYEE BENEFIT PLAN", BranchNameFr = "RF EMPLOYEE BENEFIT PLAN" },
                new Branch { BranchId = 100, SectorId = 16, BranchNameEn = "RF INTEREST EARNED", BranchNameFr = "RF INTEREST EARNED" },
                new Branch { BranchId = 101, SectorId = 16, BranchNameEn = "SFT DEFAULT", BranchNameFr = "SFT DEFAULT" },
                new Branch { BranchId = 102, SectorId = 16, BranchNameEn = "SSO AMEX/MASTERCARD DEFAULT", BranchNameFr = "SSO AMEX/MASTERCARD DEFAULT" },
                new Branch { BranchId = 103, SectorId = 16, BranchNameEn = "TRAVELLERS CHEQUES - CLEARING ACCOUNT", BranchNameFr = "TRAVELLERS CHEQUES - CLEARING ACCOUNT" }
                );
        }

        private void BuildSectors(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sector>().HasData(
                new Sector { SectorId = 1, SectorNameEn = "DIRECTION AND COORDINATION", SectorNameFr = "DIRECTION AND COORDINATION" },
                new Sector { SectorId = 2, SectorNameEn = "INDIGENOUS AFFAIRS AND RECON SECTOR", SectorNameFr = "INDIGENOUS AFFAIRS AND RECON SECTOR" },
                new Sector { SectorId = 3, SectorNameEn = "COMMS PORTFOLIO SEC", SectorNameFr = "COMMS PORTFOLIO SEC" },
                new Sector { SectorId = 4, SectorNameEn = "STRATEGIC PETROLEUM POL & INV OFFICE", SectorNameFr = "STRATEGIC PETROLEUM POL & INV OFFICE" },
                new Sector { SectorId = 5, SectorNameEn = "IND CONSULT FOR TMX", SectorNameFr = "IND CONSULT FOR TMX" },
                new Sector { SectorId = 6, SectorNameEn = "ATOMIC ENERGY SECTOR", SectorNameFr = "ATOMIC ENERGY SECTOR" },
                new Sector { SectorId = 7, SectorNameEn = "MAJOR PROJECT MANAGEMENT OFFICE SECTOR", SectorNameFr = "MAJOR PROJECT MANAGEMENT OFFICE SECTOR" },
                new Sector { SectorId = 8, SectorNameEn = "STRATEGIC POLICY AND INNOVATION", SectorNameFr = "STRATEGIC POLICY AND INNOVATION" },
                new Sector { SectorId = 9, SectorNameEn = "OFFICE OF THE CHIEF SCIENTIST", SectorNameFr = "OFFICE OF THE CHIEF SCIENTIST" },
                new Sector { SectorId = 10, SectorNameEn = "LOW CARBON ENERGY SECTOR", SectorNameFr = "LOW CARBON ENERGY SECTOR" },
                new Sector { SectorId = 11, SectorNameEn = "CORPORATE MANAGEMENT AND SERVICES SECTOR", SectorNameFr = "CORPORATE MANAGEMENT AND SERVICES SECTOR" },
                new Sector { SectorId = 12, SectorNameEn = "DEPARTMENTAL SERVICES", SectorNameFr = "DEPARTMENTAL SERVICES" },
                new Sector { SectorId = 13, SectorNameEn = "CANADIAN FOREST SERVICE", SectorNameFr = "CANADIAN FOREST SERVICE" },
                new Sector { SectorId = 14, SectorNameEn = "ENERGY TECHNOLOGY SECTOR", SectorNameFr = "ENERGY TECHNOLOGY SECTOR" },
                new Sector { SectorId = 15, SectorNameEn = "LANDS AND MINERALS SECTOR", SectorNameFr = "LANDS AND MINERALS SECTOR" },
                new Sector { SectorId = 16, SectorNameEn = "CORPORATE ACCOUNTING", SectorNameFr = "CORPORATE ACCOUNTING" }
                );
        }
    }
}
