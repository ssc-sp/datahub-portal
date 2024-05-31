using Datahub.Application.Services.UserManagement;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Datahub.Portal.Pages.Project
{
#nullable enable
	public class ProjectMemberBlock : ComponentBase
    {

        [Inject]
        public IUserInformationService _userInformationService { get; set; } = null!;

        [Inject]
        public ILogger<ProjectMemberBlock> logger { get; set; } = null!;

        [Parameter]
        public string ProjectAcronym { get; set; } = null!;

        [Parameter]
        public bool AdminOnly { get; set; } = true;

        /// <summary>
        /// The content that will be displayed if the user is authorized.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The content that will be displayed while asynchronous authorization is in progress.
        /// </summary>
        [Parameter] public RenderFragment? Authorizing { get; set; }

        [Parameter] public RenderFragment? Authorized { get; set; }

        [Parameter] public RenderFragment? NotAuthorized { get; set; }

        private bool? isAuthorized = null;

        /// <inheritdoc />
        /// See https://github.com/dotnet/aspnetcore/blob/main/src/Components/Authorization/src/AuthorizeViewCore.cs
        protected override async Task OnParametersSetAsync()
        {
            // We allow 'ChildContent' for convenience in basic cases, and 'Authorized' for symmetry
            // with 'NotAuthorized' in other cases. Besides naming, they are equivalent. To avoid
            // confusion, explicitly prevent the case where both are supplied.
            if (ChildContent != null && Authorized != null)
            {
                throw new InvalidOperationException($"Do not specify both '{nameof(Authorized)}' and '{nameof(ChildContent)}'.");
            }

            if (ProjectAcronym is null)
                throw new InvalidOperationException($"'{nameof(ProjectAcronym)}' is required");
            var userId = (await _userInformationService.GetCurrentGraphUserAsync()).Id;
            var isDatahubAdmin = await _userInformationService.IsUserDatahubAdmin();
            var isProjectAdmin = await _userInformationService.IsUserProjectAdmin(ProjectAcronym);
            var isProjectMember = await _userInformationService.IsUserProjectMember(ProjectAcronym);
            logger.LogTrace($"User {userId} in ProjectMemberBlock - isDatahubAdmin: {isDatahubAdmin} isProjectAdmin: {isProjectAdmin} ");
            if (AdminOnly)
                isAuthorized = isDatahubAdmin || isProjectAdmin;
            else
                isAuthorized = isDatahubAdmin || isProjectMember;

            //bool IsDatahubAdmin(string? _userId, System.Security.Claims.ClaimsPrincipal authUser)
            //{
            //    return !_serviceAuthManager.GetViewingAsGuest(_userId) && authUser.IsInRole(RoleConstants.DATAHUB_ROLE_ADMIN);
            //}

            //bool IsProjectAdmin(string? _userId, System.Security.Claims.ClaimsPrincipal authUser)
            //{
            //    return !_serviceAuthManager.GetViewingAsGuest(_userId) && authUser.IsInRole($"{ProjectAcronym}-admin");
            //}

            //bool IsProjectMember(string? _userId, System.Security.Claims.ClaimsPrincipal authUser)
            //{
            //    return !_serviceAuthManager.GetViewingAsGuest(_userId) && authUser.IsInRole($"{ProjectAcronym}");
            //}
        }


        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // We're using the same sequence number for each of the content items here
            // so that we can update existing instances if they are the same shape
            if (isAuthorized == null)
            {
                builder.AddContent(0, Authorizing);
            }
            else if (isAuthorized == true)
            {
                var authorized = Authorized ?? ChildContent;
                builder.AddContent(0, authorized);
            }
            else
            {
                builder.AddContent(0, NotAuthorized);
            }
        }

    }
#nullable disable
}
