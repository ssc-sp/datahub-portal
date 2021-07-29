using System;
using System.Collections.Generic;
using System.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MimeKit;
using Moq;
using NRCan.Datahub.Shared;
using NRCan.Datahub.Shared.Services;
using Xunit;

namespace DatahubTest
{
    public class EmailNotificationTestFixture: IDisposable
    {
        public EmailConfiguration EmailConfig { get; private set; }
        public EmailNotificationService EmailNotificationService { get; private set; }

        public IDictionary<string, object> EmailNotificationParameters { get; private set; }
        public IDictionary<string, object> EmailNotificationParametersNoUsername { get; private set; }

        public EmailNotificationTestFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

            EmailConfig = new EmailConfiguration();
            config.Bind(EmailNotificationService.EMAIL_CONFIGURATION_ROOT_KEY, EmailConfig);

            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();

            EmailNotificationService = new EmailNotificationService(localizerMock.Object, config);

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

        public void Dispose() { }
    }

    public class EmailNotificationTest: IClassFixture<EmailNotificationTestFixture>
    {
        private EmailNotificationTestFixture _fixture;

        public EmailNotificationTest(EmailNotificationTestFixture fixture)
        {
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

            var html = await _fixture.EmailNotificationService.RenderTemplate<NRCan.Datahub.Shared.Templates.TestEmailTemplate>();

            Assert.Equal(expectedRender, html);
        }
        
        [Fact]
        public async void TestTemplatingWithParam()
        {
            var expectedRender = "<h3>NRCan DataHub Notification Test</h3><p>Hello Test,</p><p>This is a test of the NRCan DataHub email notification.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.TestEmailTemplate>(new Dictionary<string,object>(){{"Name", "Test"}});

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRevokedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Access Revoked</h3>\r\n\r\n<p>Your access to service <b>Form Builder</b> in data project <b>PIP</b> has been revoked. The service links in the data project page will no longer be available.</p>\r\n\r\n<hr>\r\n\r\n<h3>Accès au Service Révoqué</h3>\r\n\r\n<p>Votre accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été révoqué. Les liens menant au service dans la page du projet de données ne seront plus accessibles.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.ServiceAccessRevoked>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRequestTemplate()
        {
            var expectedRender = "<h3>Form Builder Access Request for Project PIP</h3>\r\n\r\n<p>User <b>Peter Parker</b> has requested access to service <b>Form Builder</b> in data project <b>PIP</b>. Please visit the admin page with proper credentials to approve or deny the request.</p>\r\n\r\n<p>To revoke the access, please contact the DataHub team with the DataHub Data Project code and user name.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande D’accès pour le Projet PIP (FR)</h3>\r\n\r\n<p>L’utilisateur <b>Peter Parker</b> a demandé l’accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b>. Veuillez visiter la page de l’administrateur en saisissant les identifiants appropriés pour approuver ou refuser la demande.</p>\r\n\r\n<p>Pour révoquer l’accès, veuillez communiquer avec l’équipe du DataHub et assurez-vous d’avoir en main le code de projet de données du DataHub et le nom d’utilisateur.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.ServiceAccessRequest>(_fixture.EmailNotificationParameters);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestAccessRequestApprovedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Access Request Approved</h3>\r\n\r\n<p>Your request for the access to service <b>Form Builder</b> in data project <b>PIP</b> has been approved. The service links in the data project page will now be active.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande D’accès au Service Approuvée</h3>\r\n\r\n<p>Votre demande d'accès au service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été approuvée. Les liens menant au service dans la page du projet de données seront maintenant actifs.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.ServiceAccessRequestApproved>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestServiceCreationRequestApprovedTemplate()
        {
            var expectedRender = "<h3>Form Builder Service Request Approved</h3>\r\n\r\n<p>Your request for the creation of service <b>Form Builder</b> in data project <b>PIP</b> has been approved. The service links in the data project page will now be active.</p>\r\n\r\n<hr>\r\n\r\n<h3>Demande de Service Approuvée</h3>\r\n\r\n<p>Votre demande pour la création du service <b>Form Builder</b> dans le projet de données <b>PIP (FR)</b> a été approuvée. Les liens menant au service dans la page du projet de données seront maintenant actifs.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.ServiceRequestApproved>(_fixture.EmailNotificationParametersNoUsername);

            Assert.Equal(expectedRender, html);
        }

        [Fact]
        public async void TestServiceCreationRequestTemplate()
        {
            var expectedRender = "<h3>New Form Builder Service Request</h3>\r\n\r\n<p>User <b>Peter Parker</b> has requested the creation of service <b>Form Builder</b> in data project <b>PIP</b>. Please visit the admin page with proper credentials to approve or deny the request.</p>";

            var html = await _fixture.EmailNotificationService
                .RenderTemplate<NRCan.Datahub.Shared.Templates.ServiceCreationRequest>(_fixture.EmailNotificationParameters);

            Assert.Equal(expectedRender, html);
        }
    }
}