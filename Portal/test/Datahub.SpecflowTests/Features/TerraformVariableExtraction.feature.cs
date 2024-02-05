﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Datahub.SpecflowTests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Terraform Variable Extraction")]
    [NUnit.Framework.CategoryAttribute("terraform")]
    public partial class TerraformVariableExtractionFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "terraform"};
        
#line 1 "TerraformVariableExtraction.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Terraform Variable Extraction", @"Static class to extract specific variables from the output json of a terraform resource. Simply pass in the project with its resources
and call the method to extract the variable you want. The method will return either a null or the value of the variable you want.", ProgrammingLanguage.CSharp, new string[] {
                        "terraform"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 6
    #line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Resource Type",
                        "Json Content"});
            table1.AddRow(new string[] {
                        "terraform:azure-databricks",
                        @"{ ""workspace_id"": ""/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_die6_dev_rg/providers/Microsoft.Databricks/workspaces/fsdh-dbk-die6-dev"", ""workspace_url"": ""adb-4970741514517449.9.azuredatabricks.net"", ""workspace_name"": ""fsdh-dbk-die6-dev"" }"});
            table1.AddRow(new string[] {
                        "terraform:new-project-template",
                        "{  }"});
            table1.AddRow(new string[] {
                        "terraform:azure-storage-blob",
                        "{ \"storage_account\": \"fsdhprojyt31dev\", \"container\": \"datahub\", \"storage_type\": \"" +
                            "blob\", \"resource_group_name\": \"fsdh_proj_yt31_dev_rg\" }"});
            table1.AddRow(new string[] {
                        "terraform:azure-postgres",
                        @"{ ""postgres_id"": ""/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_dr1_dev_rg/providers/Microsoft.DBforPostgreSQL/flexibleServers/fsdh-dr1-psql-dev"", ""postgres_dns"": ""fsdh-dr1-psql-dev.postgres.database.azure.com"", ""postgres_db_name"": ""fsdh"", ""postgres_secret_name_admin"": ""datahub-psql-admin"", ""postgres_secret_name_password"": ""datahub-psql-password"", ""postgres_server_name"": ""fsdh-dr1-psql-dev"" }"});
#line 7
        testRunner.Given("a datahub project with the following resources", ((string)(null)), table1, "Given ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Databricks Workspace Id")]
        public virtual void ExtractingADatabricksWorkspaceId()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Databricks Workspace Id", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 14
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 15
        testRunner.When("I call the method to extract the databricks workspace id", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table2.AddRow(new string[] {
                            "databricksWorkspaceId",
                            "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_die6" +
                                "_dev_rg/providers/Microsoft.Databricks/workspaces/fsdh-dbk-die6-dev"});
#line 16
        testRunner.Then("I should get the following value", ((string)(null)), table2, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Databricks Workspace Url")]
        public virtual void ExtractingADatabricksWorkspaceUrl()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Databricks Workspace Url", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 20
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 21
        testRunner.When("I call the method to extract the databricks workspace url", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table3.AddRow(new string[] {
                            "databricksUrl",
                            "https://adb-4970741514517449.9.azuredatabricks.net"});
#line 22
        testRunner.Then("I should get the following value", ((string)(null)), table3, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Postgres Host")]
        public virtual void ExtractingAPostgresHost()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Postgres Host", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 26
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 27
        testRunner.When("I call the method to extract the postgres host", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table4.AddRow(new string[] {
                            "postgresHost",
                            "fsdh-dr1-psql-dev.postgres.database.azure.com"});
#line 28
        testRunner.Then("I should get the following value", ((string)(null)), table4, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Postgres Database Name")]
        public virtual void ExtractingAPostgresDatabaseName()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Postgres Database Name", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 32
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 33
        testRunner.When("I call the method to extract the postgres database name", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table5.AddRow(new string[] {
                            "postgresDatabaseName",
                            "fsdh"});
#line 34
        testRunner.Then("I should get the following value", ((string)(null)), table5, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Postgres Admin Secret Name")]
        public virtual void ExtractingAPostgresAdminSecretName()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Postgres Admin Secret Name", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 38
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 39
        testRunner.When("I call the method to extract the postgres username secret name", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table6.AddRow(new string[] {
                            "postgresUsernameSecretName",
                            "datahub-psql-admin"});
#line 40
        testRunner.Then("I should get the following value", ((string)(null)), table6, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Extracting a Postgres Password Secret Name")]
        public virtual void ExtractingAPostgresPasswordSecretName()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Extracting a Postgres Password Secret Name", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 44
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
    this.FeatureBackground();
#line hidden
#line 45
        testRunner.When("I call the method to extract the postgres password secret name", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "Value"});
                table7.AddRow(new string[] {
                            "postgresPasswordSecretName",
                            "datahub-psql-password"});
#line 46
        testRunner.Then("I should get the following value", ((string)(null)), table7, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
