using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Components.Forms
{
    public partial class DHInput : ComponentBase
    {
        [Parameter]
        public string Label { get; set; }
        [Parameter]
        public string Description { get; set; }
        [Parameter]
        public string Hint { get; set; }
        [Parameter]
        public string Type { get; set; }
        [Parameter]
        public string Placeholder { get; set; }
        [Parameter]
        public string Value { get; set; }
        [Parameter]
        public bool Required { get; set; }
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        [Parameter]
        public string EndButton { get; set; }
        
        private string _id => $"{Label}-id-{Guid.NewGuid()}";
        private string _helpId => $"{Label}-help-{Guid.NewGuid()}";
        private string _hintId => $"{Label}-hint-{Guid.NewGuid()}";
        
        
    }
}