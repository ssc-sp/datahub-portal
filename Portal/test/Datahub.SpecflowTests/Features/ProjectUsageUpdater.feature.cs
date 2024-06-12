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
    [Xunit.TraitAttribute("Category", "MockWorkspaceManagement")]
    public partial class ProjectUsageUpdaterFeature : object, Xunit.IClassFixture<ProjectUsageUpdaterFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "MockWorkspaceManagement"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ProjectUsageUpdater.feature"
#line hidden
        
        public ProjectUsageUpdaterFeature(ProjectUsageUpdaterFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "ProjectUsageUpdater", "\tTests for the ProjectUsageUpdater class, which includes simple storage, budget a" +
                    "nd costs updating and also the more complicated rollover feature", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
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
        
        [Xunit.SkippableFactAttribute(DisplayName="When a project usage is updated and the update does not go into a new fiscal year" +
            ", a rollover should not be triggered")]
        [Xunit.TraitAttribute("FeatureTitle", "ProjectUsageUpdater")]
        [Xunit.TraitAttribute("Description", "When a project usage is updated and the update does not go into a new fiscal year" +
            ", a rollover should not be triggered")]
        public async System.Threading.Tasks.Task WhenAProjectUsageIsUpdatedAndTheUpdateDoesNotGoIntoANewFiscalYearARolloverShouldNotBeTriggered()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("When a project usage is updated and the update does not go into a new fiscal year" +
                    ", a rollover should not be triggered", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
 await testRunner.GivenAsync("a project usage update message", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
 await testRunner.AndAsync("an associated project credits record", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 8
 await testRunner.AndAsync("the last update date is in the current fiscal year", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 9
 await testRunner.WhenAsync("the project usage is updated", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 10
 await testRunner.ThenAsync("the rollover should not be triggered", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="When a project usage is updated and the update goes into a new fiscal year, a rol" +
            "lover should be triggered")]
        [Xunit.TraitAttribute("FeatureTitle", "ProjectUsageUpdater")]
        [Xunit.TraitAttribute("Description", "When a project usage is updated and the update goes into a new fiscal year, a rol" +
            "lover should be triggered")]
        public async System.Threading.Tasks.Task WhenAProjectUsageIsUpdatedAndTheUpdateGoesIntoANewFiscalYearARolloverShouldBeTriggered()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("When a project usage is updated and the update goes into a new fiscal year, a rol" +
                    "lover should be triggered", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 12
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 13
 await testRunner.GivenAsync("a project usage update message", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 14
 await testRunner.AndAsync("an associated project credits record", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 15
 await testRunner.AndAsync("the last update date is in the previous fiscal year", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 16
 await testRunner.WhenAsync("the project usage is updated", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 17
 await testRunner.ThenAsync("the rollover should be triggered", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 18
 await testRunner.AndAsync("the project credits should be updated accordingly", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="When a project usage is updated and the update goes into a new fiscal year, but w" +
            "e are unable to determine the correct costs, a rollover should not be triggered")]
        [Xunit.TraitAttribute("FeatureTitle", "ProjectUsageUpdater")]
        [Xunit.TraitAttribute("Description", "When a project usage is updated and the update goes into a new fiscal year, but w" +
            "e are unable to determine the correct costs, a rollover should not be triggered")]
        public async System.Threading.Tasks.Task WhenAProjectUsageIsUpdatedAndTheUpdateGoesIntoANewFiscalYearButWeAreUnableToDetermineTheCorrectCostsARolloverShouldNotBeTriggered()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("When a project usage is updated and the update goes into a new fiscal year, but w" +
                    "e are unable to determine the correct costs, a rollover should not be triggered", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 20
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 21
 await testRunner.GivenAsync("a project usage update message", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 22
 await testRunner.AndAsync("an associated project credits record", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 23
 await testRunner.AndAsync("the last update date is in the previous fiscal year", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 24
 await testRunner.AndAsync("the difference between budget spent and cost captured is too large", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 25
 await testRunner.WhenAsync("the project usage is updated", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 26
 await testRunner.ThenAsync("the rollover should not be triggered", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="When a project usage is queued, the blob download should work properly")]
        [Xunit.TraitAttribute("FeatureTitle", "ProjectUsageUpdater")]
        [Xunit.TraitAttribute("Description", "When a project usage is queued, the blob download should work properly")]
        public async System.Threading.Tasks.Task WhenAProjectUsageIsQueuedTheBlobDownloadShouldWorkProperly()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("When a project usage is queued, the blob download should work properly", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 28
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 29
 await testRunner.GivenAsync("a project usage update message", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 30
 await testRunner.WhenAsync("the subscription costs are downloaded", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 31
 await testRunner.ThenAsync("the blob download should work properly", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
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
                await ProjectUsageUpdaterFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await ProjectUsageUpdaterFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
