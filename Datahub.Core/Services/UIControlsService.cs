using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class UIControlsService
    {
        public RenderFragment CurrentRightSidebarRenderFragment { get; set; }
        public RenderFragment CurrentModalRenderFragment { get; set; }
        public bool AllowEscape { get; set; } = true;

        public event Action OnRightSidebarChange;
        public event Action OnModalChange;
        public event Action OnErrorModalShow;
        
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
    }
}