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
    public partial class IPAddressWhitelistFeature : object, Xunit.IClassFixture<IPAddressWhitelistFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "IPAddressWhitelist.feature"
#line hidden
        
        public IPAddressWhitelistFeature(IPAddressWhitelistFeature.FixtureData fixtureData, Datahub_SpecflowTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "IP Address Whitelist", "Allows a user to add an IP address to the whitelist of a cloud resource", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Add my IP address to the whitelist")]
        [Xunit.TraitAttribute("FeatureTitle", "IP Address Whitelist")]
        [Xunit.TraitAttribute("Description", "Add my IP address to the whitelist")]
        [Xunit.InlineDataAttribute("my-firewall", "123.123.123.123", new string[0])]
        public void AddMyIPAddressToTheWhitelist(string name, string ip_Address, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("name", name);
            argumentsOfScenario.Add("ip_address", ip_Address);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add my IP address to the whitelist", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 4
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 5
        testRunner.Given(string.Format("a current user with a name of {0}", name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 6
        testRunner.And(string.Format("an IP address of {0}", ip_Address), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 7
        testRunner.When("the user adds their current IP to the whitelist", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 8
        testRunner.Then(string.Format("the {0} should be in the whitelist", ip_Address), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 9
        testRunner.And(string.Format("the {0} associated with {1}", name, ip_Address), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Names should conform to Azure Firewall naming conventions")]
        [Xunit.TraitAttribute("FeatureTitle", "IP Address Whitelist")]
        [Xunit.TraitAttribute("Description", "Names should conform to Azure Firewall naming conventions")]
        [Xunit.InlineDataAttribute("my-firewall", "my-firewall", new string[0])]
        [Xunit.InlineDataAttribute("my_firewall", "my-firewall", new string[0])]
        [Xunit.InlineDataAttribute("my.firewall", "my-firewall", new string[0])]
        [Xunit.InlineDataAttribute("my firewall", "my-firewall", new string[0])]
        public void NamesShouldConformToAzureFirewallNamingConventions(string name, string cleaned_Name, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("name", name);
            argumentsOfScenario.Add("cleaned_name", cleaned_Name);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Names should conform to Azure Firewall naming conventions", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 15
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 16
        testRunner.Given(string.Format("a firewall {0}", name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 17
        testRunner.When("the name is cleaned up for compliance", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 18
        testRunner.Then(string.Format("the {0} should be {1}", name, cleaned_Name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Add my existing IP address to the whitelist")]
        [Xunit.TraitAttribute("FeatureTitle", "IP Address Whitelist")]
        [Xunit.TraitAttribute("Description", "Add my existing IP address to the whitelist")]
        [Xunit.InlineDataAttribute("test", "123.123.123.123", "my-firewall", new string[0])]
        public void AddMyExistingIPAddressToTheWhitelist(string name, string ip_Address, string existing_Name, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("name", name);
            argumentsOfScenario.Add("ip_address", ip_Address);
            argumentsOfScenario.Add("existing_name", existing_Name);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add my existing IP address to the whitelist", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 27
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 28
        testRunner.Given(string.Format("a current user with a name of {0}", name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 29
        testRunner.And(string.Format("an IP address of {0}", ip_Address), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 30
        testRunner.And(string.Format("the {0} is already in the whitelist with {1}", ip_Address, existing_Name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 31
        testRunner.When("the user adds their current IP to the whitelist", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 32
        testRunner.Then(string.Format("the {0} should be in the whitelist with {1}", ip_Address, name), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                IPAddressWhitelistFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                IPAddressWhitelistFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
