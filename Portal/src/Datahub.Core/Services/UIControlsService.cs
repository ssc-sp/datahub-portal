using Datahub.Core.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Datahub.Core.Services;

public class UiControlsService
{
    public RenderFragment CurrentRightSidebarRenderFragment { get; set; }
    public RenderFragment CurrentModalRenderFragment { get; set; }
    public bool AllowEscape { get; set; } = true;

    public event Action OnRightSidebarChange;
    public event Action OnModalChange;
    public event Action OnErrorModalShow;

    private IDialogService _dialogService;
    private IDialogReference _dialogReference = null;

    public UiControlsService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    private void NotifyRightSidebarChange() => OnRightSidebarChange?.Invoke();
    private void NotifyModalChange() => OnModalChange?.Invoke();
    private void NotifyErrorModalShow() => OnErrorModalShow?.Invoke();

    public void ToggleRightSidebar(RenderFragment rightSidebarRenderFragment = null)
    {
        CurrentRightSidebarRenderFragment = (CurrentRightSidebarRenderFragment == rightSidebarRenderFragment) ? null : rightSidebarRenderFragment;
        NotifyRightSidebarChange();
    }

    public async Task ToggleModal(RenderFragment modalRenderFragment = null)
    {
        await Task.Run(() =>
        {
            CurrentModalRenderFragment = (CurrentModalRenderFragment == modalRenderFragment) ? null : modalRenderFragment;
            NotifyModalChange();
        });
    }

    public void ShowErrorModal()
    {
        NotifyErrorModalShow();
    }

    public void ShowDialog(string dialogTitle, RenderFragment contentFragment)
    {
        var parameters = new DialogParameters();
        parameters.Add("Content", contentFragment);

        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            CloseOnEscapeKey = true,
            BackdropClick = false
        };

        _dialogReference = _dialogService.Show<DialogModalFrame>(dialogTitle, parameters, options);
    }

    public void HideDialog()
    {
        _dialogReference?.Close();
        _dialogReference = default;
    }
}