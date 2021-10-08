using System;
using System.Collections.Generic;
using System.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MimeKit;
using Moq;
using Datahub.Core;
using Datahub.Core.Data;
using Datahub.Core.Services;
using Xunit;
using Xunit.Abstractions;

namespace Datahub.Tests
{
    public class EmailNotificationTestFixture: IDisposable
    {
        public static readonly string USER_1_ID = "753c8790-a3ab-4b7e-a0d6-69e228853994";
        public static readonly string USER_1_NAME = "Fred Flintstone";
        public static readonly string USER_1_ADDR = "fred@bedrock.com";
        public static readonly string USER_2_ID = "ff47397d-fe84-414d-8029-4ec8eacbbb2b";
        public static readonly string USER_2_NAME = "Barney Rubble";
        public static readonly string USER_2_ADDR = "barney@bedrock.com";

        public EmailConfiguration EmailConfig { get; private set; }
        public EmailNotificationService EmailNotificationService { get; private set; }
        
        private ITestOutputHelper _output = null;
        private Mock<IStringLocalizer> _localizerMock;
        private Mock<IMSGraphService> _graphServiceMock;
        private IConfigurationRoot _config;

        public IDictionary<string, object> EmailNotificationParameters { get; private set; }
        public IDictionary<string, object> EmailNotificationParametersNoUsername { get; private set; }

        public EmailNotificationTestFixture()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

            EmailConfig = new EmailConfiguration();
            _config.Bind(EmailNotificationService.EMAIL_CONFIGURATION_ROOT_KEY, EmailConfig);

            _localizerMock = new Mock<IStringLocalizer>();

            var fakeUser1 = GenerateTestUser(USER_1_ID, USER_1_NAME, USER_1_ADDR);
            var fakeUser2 = GenerateTestUser(USER_2_ID, USER_2_NAME, USER_2_ADDR);

            _graphServiceMock = new Mock<IMSGraphService>();
            _graphServiceMock.Setup(g => g.GetUser(USER_1_ID)).Returns(fakeUser1);
            _graphServiceMock.Setup(g => g.GetUser(USER_2_ID)).Returns(fakeUser2);
            _graphServiceMock.Setup(g => g.GetUserIdFromEmail(USER_1_ADDR)).Returns(USER_1_ID);
            _graphServiceMock.Setup(g => g.GetUserIdFromEmail(USER_2_ADDR)).Returns(USER_2_ID);

            var testProject = new DatahubProjectInfo() { ProjectNameEn = "PIP", ProjectNameFr = "PIP (FR)" };

            EmailNotificationParameters = new Dictionary<string, object>() 
            {
                { "Service", "Form Builder" },
                { "DataProject", testProject },
                { "Username", "Peter Parker" } 
            };

            EmailNotificationParametersNoUsername = new Dictionary<string, object>() 
            {
                { "Service", "Form Builder" },
                { "DataProject", testProject },
            };
        }

        private GraphUser GenerateTestUser(string id, string name, string email)
        {
            var user = new GraphUser()
            {
                Id = id,
                DisplayName = name
            };
            var mailAddress = new System.Net.Mail.MailAddress(email);

            // this is horrible
            var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var mailAddressProp = user.GetType().GetProperty("mailAddress", bindingFlags);
            mailAddressProp.SetValue(user, mailAddress);

            return user;


            /*
            var I = foo.GetType().GetProperty(nameof(Foo.I), BindFlags.Public | BindingFlag.Instance);
I.SetValue(foo, 8675309);
            */
        }

        public void InitOutput(ITestOutputHelper output)
        {
            if (_output == null)
            {
                _output = output;
                EmailNotificationService = new EmailNotificationService(
                    _localizerMock.Object, 
                    _config, 
                    _output.BuildLoggerFor<EmailNotificationService>(),
                    _graphServiceMock.Object);
            }
        }

        public void Dispose() { }
    }

    public class EmailNotificationTest: IClassFixture<EmailNotificationTestFixture>
    {
        private EmailNotificationTestFixture _fixture;

        public EmailNotificationTest(EmailNotificationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            fixture.InitOutput(outputHelper);
            _fixture = fixture;
        }

        [Fact]
        public async void TestAuthentication()
        {
            var config = _fixture.EmailConfig;

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(config.SmtpHost, config.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(new NetworkCredential(config.SmtpUsername, config.SmtpPassword));
            
                Assert.True(smtpClient.IsAuthenticated);

                await smtpClient.DisconnectAsync(true);
            }
        }

        [Fact]
        public async void TestTemplating()
        {
            var expectedRender = "<h3>NRCan DataHub Notification Test</h3><p>Hello,</p><p>This is a test of the NRCan DataHub email notification.</p>";

            var html = await _fixture.EmailNotificationService.RenderTemplate<Datahub.Core.Templates.TestEmailTemplate>();

            Assert.Equal(expectedRender, html);
        }
        
        [Fact]
        public async void TestTemplatingWithParam()
        {
            var expectedRender = "<h3>NRCan DataHub Notification Test</h3><p>Hello Test,</p><p>This is a test of the NRCan DataHub email notification.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.TestEmailTemplate>(new Dictionary<string,object>(){{"Name", "Test"}});

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRevokedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Access Revoked</h3>\r\n\r\n<p>Your access to service <b>Form Builder</b> in data project <b>PIP</b> has been revoked. The service links in the data project page will no longer be available.</p>\r\n\r\n<hr>\r\n\r\n<h3>Accès au Service Révoqué</h3>\r\n\r\n<p>Votre accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été révoqué. Les liens menant au service dans la page du projet de données ne seront plus accessibles.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.ServiceAccessRevoked>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRequestTemplate()
        {
            var expectedRender = "<h3>Form Builder Access Request for Project PIP</h3>\r\n\r\n<p>User <b>Peter Parker</b> has requested access to service <b>Form Builder</b> in data project <b>PIP</b>. Please visit the admin page with proper credentials to approve or deny the request.</p>\r\n\r\n<p>To revoke the access, please contact the DataHub team with the DataHub Data Project code and user name.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande D’accès pour le Projet PIP (FR)</h3>\r\n\r\n<p>L’utilisateur <b>Peter Parker</b> a demandé l’accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b>. Veuillez visiter la page de l’administrateur en saisissant les identifiants appropriés pour approuver ou refuser la demande.</p>\r\n\r\n<p>Pour révoquer l’accès, veuillez communiquer avec l’équipe du DataHub et assurez-vous d’avoir en main le code de projet de données du DataHub et le nom d’utilisateur.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.ServiceAccessRequest>(_fixture.EmailNotificationParameters);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRequestApprovedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Access Request Approved</h3>\r\n\r\n<p>Your request for the access to service <b>Form Builder</b> in data project <b>PIP</b> has been approved. The service links in the data project page will now be active.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande D’accès au Service Approuvée</h3>\r\n\r\n<p>Votre demande d'accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été approuvée. Les liens menant au service dans la page du projet de données seront maintenant actifs.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.ServiceAccessRequestApproved>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestServiceCreationRequestApprovedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Request Approved</h3>\r\n\r\n<p>Your request for the creation of service <b>Form Builder</b> in data project <b>PIP</b> has been approved. The service links in the data project page will now be active.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande de Service Approuvée</h3>\r\n\r\n<p>Votre demande pour la création du service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été approuvée. Les liens menant au service dans la page du projet de données seront maintenant actifs.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.ServiceRequestApproved>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestServiceCreationRequestTemplate()
        {
            var expectedRender = "<h3>New Form Builder Service Request</h3>\r\n\r\n<p>User <b>Peter Parker</b> has requested the creation of service <b>Form Builder</b> in data project <b>PIP</b>. Please visit the admin page with proper credentials to notify project users when it has been created.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<Datahub.Core.Templates.ServiceCreationRequest>(_fixture.EmailNotificationParameters);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public void TestLookupOneRecipientById()
        {
            var recipients = new List<(string address, string name)>() { (EmailNotificationTestFixture.USER_1_ID, null) };
            var result = _fixture.EmailNotificationService.TestUsernameEmailConversion(recipients);

            Assert.Single(result);
            
            var singleResult = result[0];
            Assert.Equal(EmailNotificationTestFixture.USER_1_ADDR, singleResult.Address);
            Assert.Equal(EmailNotificationTestFixture.USER_1_NAME, singleResult.Name);
        }

        [Fact]
        public void TestLookupOneRecipientByEmail()
        {
            var recipients = new List<(string address, string name)>() { (EmailNotificationTestFixture.USER_1_ADDR, null) };
            var result = _fixture.EmailNotificationService.TestUsernameEmailConversion(recipients);

            Assert.Single(result);

            var singleResult = result[0];
            Assert.Equal(EmailNotificationTestFixture.USER_1_ADDR, singleResult.Address);
            Assert.Equal(EmailNotificationTestFixture.USER_1_NAME, singleResult.Name);
        }

        [Fact]
        public void TestMakeRecipientUsingAddressAndName()
        {
            var fakeName = "Hank Hill";
            var recipients = new List<(string address, string name)>() { (EmailNotificationTestFixture.USER_1_ADDR, fakeName) };
            var result = _fixture.EmailNotificationService.TestUsernameEmailConversion(recipients);

            Assert.Single(result);

            var singleResult = result[0];
            Assert.Equal(EmailNotificationTestFixture.USER_1_ADDR, singleResult.Address);
            Assert.Equal(fakeName, singleResult.Name);
        }

    }
}