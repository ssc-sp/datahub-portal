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
namespace Datahub.SpecflowTests.Features.Subscriptions
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "AzureDatahubSubscription")]
    public partial class AzureSubscriptionServiceFeature : object, Xunit.IClassFixture<AzureSubscriptionServiceFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "AzureDatahubSubscription"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "DatahubAzureSubscriptions.feature"
#line hidden
        
        public AzureSubscriptionServiceFeature(AzureSubscriptionServiceFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/Subscriptions", "Azure Subscription Service", "This feature provides the ability to manage Azure subscriptions for the DataHub", ProgrammingLanguage.CSharp, featureTags);
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
        
        [Xunit.SkippableFactAttribute(DisplayName="List all subscriptions")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "List all subscriptions")]
        public async System.Threading.Tasks.Task ListAllSubscriptions()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("List all subscriptions", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
        await testRunner.AndAsync("at least one subscription exists", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 8
        await testRunner.WhenAsync("the list of subscriptions is requested", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 9
        await testRunner.ThenAsync("the list of subscriptions is returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 10
        await testRunner.AndAsync("the list of subscriptions contains at least one subscription", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Add a valid Azure subscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Add a valid Azure subscription")]
        public async System.Threading.Tasks.Task AddAValidAzureSubscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Add a valid Azure subscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 12
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 13
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 14
        await testRunner.WhenAsync("a new subscription is added", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 15
        await testRunner.ThenAsync("the subscription is added to the list of subscriptions", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Add an invalid Azure subscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Add an invalid Azure subscription")]
        public async System.Threading.Tasks.Task AddAnInvalidAzureSubscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Add an invalid Azure subscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 17
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 18
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 19
        await testRunner.WhenAsync("an invalid subscription is added", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 20
        await testRunner.ThenAsync("an error is returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Delete an existing subscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Delete an existing subscription")]
        public async System.Threading.Tasks.Task DeleteAnExistingSubscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Delete an existing subscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 22
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 23
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 24
        await testRunner.AndAsync("there is a subscription with id \"delete-me\"", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 25
        await testRunner.WhenAsync("a subscription with id \"delete-me\" is deleted", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 26
        await testRunner.ThenAsync("there should be no subscriptions with id \"delete-me\"", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Delete a non-existing subscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Delete a non-existing subscription")]
        public async System.Threading.Tasks.Task DeleteANon_ExistingSubscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Delete a non-existing subscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 28
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 29
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 30
        await testRunner.WhenAsync("a non-existing subscription is deleted", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 31
        await testRunner.ThenAsync("an error is returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get the next available suscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Get the next available suscription")]
        public async System.Threading.Tasks.Task GetTheNextAvailableSuscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Get the next available suscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 33
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 34
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 35
        await testRunner.AndAsync("at least one subscription exists", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 36
        await testRunner.WhenAsync("the next available subscription is requested", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 37
        await testRunner.ThenAsync("the next available subscription is returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get the next available suscription when there are no subscriptions")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Get the next available suscription when there are no subscriptions")]
        public async System.Threading.Tasks.Task GetTheNextAvailableSuscriptionWhenThereAreNoSubscriptions()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Get the next available suscription when there are no subscriptions", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 39
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 40
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 41
        await testRunner.AndAsync("there are no subscriptions", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 42
        await testRunner.WhenAsync("the next available subscription is requested", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 43
        await testRunner.ThenAsync("an error is returned", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Update an existing subscription")]
        [Xunit.TraitAttribute("FeatureTitle", "Azure Subscription Service")]
        [Xunit.TraitAttribute("Description", "Update an existing subscription")]
        public async System.Threading.Tasks.Task UpdateAnExistingSubscription()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Update an existing subscription", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
        await testRunner.GivenAsync("a datahub azure subscription service", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 47
        await testRunner.AndAsync("there is a subscription with id \"update-me\"", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 48
        await testRunner.WhenAsync("the subscription with id \"update-me\" is updated", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 49
        await testRunner.ThenAsync("there should be no subscriptions with id \"update-me\"", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
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
                await AzureSubscriptionServiceFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await AzureSubscriptionServiceFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
