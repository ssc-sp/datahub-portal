﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:1.0.0.0
//      Reqnroll Generator Version:1.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Datahub.SpecflowTests.Features
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "WorkspaceManagement")]
    public partial class WorkspaceCostsFeature : object, Xunit.IClassFixture<WorkspaceCostsFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "WorkspaceManagement"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "WorkspaceCosts.feature"
#line hidden
        
        public WorkspaceCostsFeature(WorkspaceCostsFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "WorkspaceCosts", "\tTesting systems related to getting and updating workspace costs", ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            string testWorkerId = testRunner.TestWorkerId;
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
            Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);
        }
        
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            await this.TestInitializeAsync();
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Querying subscription costs should return right amount")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Querying subscription costs should return right amount")]
        public async System.Threading.Tasks.Task QueryingSubscriptionCostsShouldReturnRightAmount()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Querying subscription costs should return right amount", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
 await testRunner.GivenAsync("a workspace with a subscription cost of at most 100", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
 await testRunner.WhenAsync("the subscription cost is mock queried", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 8
 await testRunner.ThenAsync("the result should not exceed the expected value", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Querying subscription costs with an invalid date range should return an error")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Querying subscription costs with an invalid date range should return an error")]
        public async System.Threading.Tasks.Task QueryingSubscriptionCostsWithAnInvalidDateRangeShouldReturnAnError()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Querying subscription costs with an invalid date range should return an error", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 10
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 11
 await testRunner.GivenAsync("a workspace with a subscription cost of at most 100", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 12
 await testRunner.WhenAsync("the subscription cost is mock queried with an invalid date range", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 13
 await testRunner.ThenAsync("an error should be returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Grouping costs by source should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Grouping costs by source should work properly")]
        public async System.Threading.Tasks.Task GroupingCostsBySourceShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Grouping costs by source should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 15
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 16
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 17
 await testRunner.WhenAsync("the costs are grouped by source", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 18
 await testRunner.ThenAsync("the result should have the expected count and total of source costs", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Grouping costs by date should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Grouping costs by date should work properly")]
        public async System.Threading.Tasks.Task GroupingCostsByDateShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Grouping costs by date should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 20
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 21
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 22
 await testRunner.WhenAsync("the costs are grouped by date", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 23
 await testRunner.ThenAsync("the result should have the expected count and total of daily costs", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Filtering the current fiscal year should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Filtering the current fiscal year should work properly")]
        public async System.Threading.Tasks.Task FilteringTheCurrentFiscalYearShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Filtering the current fiscal year should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 25
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 26
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 27
 await testRunner.WhenAsync("the costs are filtered by the current fiscal year", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 28
 await testRunner.ThenAsync("the result should have the expected count and total of fiscal year costs", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Filtering the last fiscal year should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Filtering the last fiscal year should work properly")]
        public async System.Threading.Tasks.Task FilteringTheLastFiscalYearShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Filtering the last fiscal year should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 30
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 31
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 32
 await testRunner.WhenAsync("the costs are filtered by the last fiscal year", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 33
 await testRunner.ThenAsync("the result should have the expected count and total of last fiscal year costs", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Filtering mock costs for a specific date range should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Filtering mock costs for a specific date range should work properly")]
        public async System.Threading.Tasks.Task FilteringMockCostsForASpecificDateRangeShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Filtering mock costs for a specific date range should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 35
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 36
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 37
 await testRunner.WhenAsync("the costs are filtered by a specific date range", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 38
 await testRunner.ThenAsync("the result should have the expected count and total of costs in the date range", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Filtering mock costs by workspace should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Filtering mock costs by workspace should work properly")]
        public async System.Threading.Tasks.Task FilteringMockCostsByWorkspaceShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Filtering mock costs by workspace should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 40
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 41
 await testRunner.GivenAsync("a list of mock costs", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 42
 await testRunner.WhenAsync("the costs are filtered by a specific workspace", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 43
 await testRunner.ThenAsync("the result should have the expected count and total of costs in the workspace", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Updating workspace costs should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceCosts")]
        [Xunit.TraitAttribute("Description", "Updating workspace costs should work properly")]
        public async System.Threading.Tasks.Task UpdatingWorkspaceCostsShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Updating workspace costs should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 45
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 46
 await testRunner.GivenAsync("a workspace, with populated costs and credits", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 47
 await testRunner.WhenAsync("the costs are updated", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 48
 await testRunner.ThenAsync("the costs table and credits table should be updated accordingly and correctly", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await WorkspaceCostsFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await WorkspaceCostsFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
