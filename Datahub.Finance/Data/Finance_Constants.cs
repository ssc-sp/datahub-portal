using Datahub.Core.Data;
using Microsoft.Graph.ExternalConnectors;
using Polly;
using static MudBlazor.Colors;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.Xml;
using System;

namespace Datahub.Portal.Data.Finance
{
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
            new DropDownContainer() { Id = 1, EnglishText = "New", FrenchText = "New"},
            new DropDownContainer() { Id = 2, EnglishText = "Replacement", FrenchText = "Replacement"},            
        };
    }
}
