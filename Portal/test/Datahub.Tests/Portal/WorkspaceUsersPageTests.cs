using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using Xunit;
using Datahub.Tests.Portal;
using NSubstitute;
using MudBlazor.Services;
using Datahub.Portal.Pages.Public;
using MassTransit;
using Datahub.Application.Services.Metadata;
using Datahub.Core.Model.Projects;
using Datahub.Portal.Pages.Workspace.Users;
using System.Reflection;
using Datahub.Core.Components.AuthViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Datahub.Tests
{
    public class WorkspaceUsersPageTests
    {
        private readonly Mock<IProjectUserManagementService> _projectUserManagementServiceMock;
        private readonly Mock<IUserInformationService> _userInformationMock;
        private readonly Mock<ICultureService> _cultureServiceMock;
        private readonly Mock<ISnackbar> _snackBarMock;
        private readonly Mock<IStringLocalizer> _stringLocalizerMock;
        private readonly TestContext _testContext;
        private readonly WorkspaceUsersPage? _component;

        public WorkspaceUsersPageTests()
        {
            _projectUserManagementServiceMock = new Mock<IProjectUserManagementService>();
            _userInformationMock = new Mock<IUserInformationService>();
            _cultureServiceMock = new Mock<ICultureService>();
            _stringLocalizerMock = new Mock<IStringLocalizer> { CallBase = false };
            _snackBarMock = new Mock<ISnackbar>();

            _testContext = SetupTestContext();
            var email = "fake_user@gc.ca";
            var fakeIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock");
            var fakeClaimsPrincipal = new ClaimsPrincipal(fakeIdentity);

            // setup
            _stringLocalizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("test", "test"));
            _userInformationMock.Setup(s => s.GetAuthenticatedUser(It.IsAny<bool>()))
                .ReturnsAsync(fakeClaimsPrincipal);

            var projectAcronym = "ABC"; 

            //_component = (WorkspaceUsersPage?)_testContext.RenderComponent<WorkspaceUsersPage>(parameters => parameters
            //    .Add(p => p.WorkspaceAcronym, projectAcronym)
            //    .AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(new AuthenticationState(fakeClaimsPrincipal))));

            var pageParams = new List<ComponentParameter> { ComponentParameter.CreateParameter("WorkspaceAcronym", projectAcronym) };
            _component = (WorkspaceUsersPage?)_testContext.RenderComponent<WorkspaceUsersPage>(pageParams.ToArray());

        }

        private TestContext SetupTestContext()
        {
            _snackBarMock.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());

            using var ctx = new TestContext();
            ctx.Services.AddSingleton(_projectUserManagementServiceMock.Object);
            ctx.Services.AddSingleton(_userInformationMock.Object);
            ctx.Services.AddSingleton(_cultureServiceMock.Object);
            ctx.Services.AddSingleton(_stringLocalizerMock.Object);
            ctx.Services.AddSingleton(_snackBarMock.Object);
            ctx.Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            ctx.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("AlwaysAllow", policy =>
                {
                    policy.RequireAssertion(_ => true);
                });

                var alwaysAllowPolicy = options.GetPolicy("AlwaysAllow");
                if (alwaysAllowPolicy != null)
                {
                    options.DefaultPolicy = alwaysAllowPolicy;
                }
                else
                {
                    throw new InvalidOperationException("AlwaysAllow policy is not configured.");
                }

            });

            ctx.Services.AddMudServices();
            return ctx;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private bool InvokePrivateMethod(object obj, string methodName)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)method.Invoke(obj, null);
        }

        [Fact]
        public void NothingChanged_NoChanges_ReturnsTrue()
        {
            // Arrange
            var user = new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 };
            SetPrivateField(_component, "_projectUsers", new List<Datahub_Project_User> { user });
            SetPrivateField(_component, "_currentlySelected", new List<Datahub_Project_User> { new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 } });

            // Act
            var result = InvokePrivateMethod(_component, "NothingChanged");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void NothingChanged_ChangeIsDataSteward_ReturnsFalse()
        {
            // Arrange
            var user = new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 };
            SetPrivateField(_component, "_projectUsers", new List<Datahub_Project_User> { user });
            SetPrivateField(_component, "_currentlySelected", new List<Datahub_Project_User> { new Datahub_Project_User { PortalUserId = 1, IsDataSteward = true, RoleId = 1 } });

            // Act
            var result = InvokePrivateMethod(_component, "NothingChanged");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void NothingChanged_ChangeRoleId_ReturnsFalse()
        {
            // Arrange
            var user = new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 };
            SetPrivateField(_component, "_projectUsers", new List<Datahub_Project_User> { user });
            SetPrivateField(_component, "_currentlySelected", new List<Datahub_Project_User> { new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 2 } });

            // Act
            var result = InvokePrivateMethod(_component, "NothingChanged");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void NothingChanged_ChangeBothProperties_ReturnsFalse()
        {
            // Arrange
            var user = new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 };
            SetPrivateField(_component, "_projectUsers", new List<Datahub_Project_User> { user });
            SetPrivateField(_component, "_currentlySelected", new List<Datahub_Project_User> { new Datahub_Project_User { PortalUserId = 1, IsDataSteward = true, RoleId = 2 } });

            // Act
            var result = InvokePrivateMethod(_component, "NothingChanged");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void NothingChanged_RevertChanges_ReturnsTrue()
        {
            // Arrange
            var user = new Datahub_Project_User { PortalUserId = 1, IsDataSteward = false, RoleId = 1 };
            SetPrivateField(_component, "_projectUsers", new List<Datahub_Project_User> { user });
            SetPrivateField(_component, "_currentlySelected", new List<Datahub_Project_User> { new Datahub_Project_User { PortalUserId = 1, IsDataSteward = true, RoleId = 2 } });

            // Act
            var changeDataStewardFlagMethod = _component.GetType().GetMethod("ChangeDataStewardFlag", BindingFlags.NonPublic | BindingFlags.Instance);
            changeDataStewardFlagMethod.Invoke(_component, new object[] { user, false });

            var updateProjectMemberRoleMethod = _component.GetType().GetMethod("UpdateProjectMemberRole", BindingFlags.NonPublic | BindingFlags.Instance);
            updateProjectMemberRoleMethod.Invoke(_component, new object[] { user, 1 });

            var result = InvokePrivateMethod(_component, "NothingChanged");

            // Assert
            Assert.True(result);
        }
    }
}
