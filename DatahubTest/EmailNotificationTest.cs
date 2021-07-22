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
    }
}