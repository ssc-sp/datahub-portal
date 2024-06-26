﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "WorkspaceManagement")]
    public partial class WorkspaceBudgetFeature : object, Xunit.IClassFixture<WorkspaceBudgetFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "WorkspaceManagement"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "WorkspaceBudget.feature"
#line hidden
        
        public WorkspaceBudgetFeature(WorkspaceBudgetFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "WorkspaceBudget", "\tTests for the workspace budget management service", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            string testWorkerId = testRunner.TestWorkerId;
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
            global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);
        }
        
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
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
        
        [Xunit.SkippableFactAttribute(DisplayName="Querying budget amount for a workspace should return the right amount")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceBudget")]
        [Xunit.TraitAttribute("Description", "Querying budget amount for a workspace should return the right amount")]
        public async System.Threading.Tasks.Task QueryingBudgetAmountForAWorkspaceShouldReturnTheRightAmount()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Querying budget amount for a workspace should return the right amount", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
 await testRunner.GivenAsync("a workspace with a budget identifier of \"/subscriptions/bc4bcb08-d617-49f4-b6af-6" +
                        "9d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/b" +
                        "udgets/fsdh-test-budget\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
 await testRunner.AndAsync("the budget amount is $1000", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 8
 await testRunner.WhenAsync("the budget amount is queried for the workspace", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 9
 await testRunner.ThenAsync("the result should be the expected amount", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Setting a budget amount for a workspace should update the budget amount")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceBudget")]
        [Xunit.TraitAttribute("Description", "Setting a budget amount for a workspace should update the budget amount")]
        public async System.Threading.Tasks.Task SettingABudgetAmountForAWorkspaceShouldUpdateTheBudgetAmount()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Setting a budget amount for a workspace should update the budget amount", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 11
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 12
 await testRunner.GivenAsync("a workspace with a budget identifier of \"/subscriptions/bc4bcb08-d617-49f4-b6af-6" +
                        "9d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/b" +
                        "udgets/fsdh-test-budget\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 13
 await testRunner.AndAsync("the budget amount is $1000", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 14
 await testRunner.WhenAsync("the budget amount is set to $500 for the workspace", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 15
 await testRunner.ThenAsync("the result should be the expected amount", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Querying budget spent for a workspace should return the right amount")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceBudget")]
        [Xunit.TraitAttribute("Description", "Querying budget spent for a workspace should return the right amount")]
        public async System.Threading.Tasks.Task QueryingBudgetSpentForAWorkspaceShouldReturnTheRightAmount()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Querying budget spent for a workspace should return the right amount", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 17
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 18
 await testRunner.GivenAsync("a workspace with a budget identifier of \"/subscriptions/bc4bcb08-d617-49f4-b6af-6" +
                        "9d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/b" +
                        "udgets/fsdh-test-budget\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 19
 await testRunner.AndAsync("the budget spent is less than $10", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 20
 await testRunner.WhenAsync("the budget spent is queried for the workspace", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 21
 await testRunner.ThenAsync("the result should be less than the expected amount", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Updating budget spent for a workspace should update the budget spent")]
        [Xunit.TraitAttribute("FeatureTitle", "WorkspaceBudget")]
        [Xunit.TraitAttribute("Description", "Updating budget spent for a workspace should update the budget spent")]
        public async System.Threading.Tasks.Task UpdatingBudgetSpentForAWorkspaceShouldUpdateTheBudgetSpent()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Updating budget spent for a workspace should update the budget spent", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 23
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 24
 await testRunner.GivenAsync("a workspace with a budget identifier of \"/subscriptions/bc4bcb08-d617-49f4-b6af-6" +
                        "9d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/b" +
                        "udgets/fsdh-test-budget\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 25
 await testRunner.AndAsync("an existing project credit record", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 26
 await testRunner.WhenAsync("the budget is updated for that workspace", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 27
 await testRunner.ThenAsync("project credit record should be updated", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await WorkspaceBudgetFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await WorkspaceBudgetFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
