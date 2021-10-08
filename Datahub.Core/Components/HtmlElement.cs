using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Shared.Components
{
    public class HtmlElement : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> InputAttributes { get; set; }
        public Dictionary<string, object> InputAttributesWithoutClass { get; set; }
        protected string _inputClass => InputAttributes != null && InputAttributes.ContainsKey("class") ? InputAttributes["class"] as string : "";


        protected override void OnInitialized()
        {
            base.OnInitialized();

            InputAttributesWithoutClass = InputAttributes?
                .Keys
                .Where(k => k != "class")
                .ToDictionary(_ => _, _ => InputAttributes[_]);
        }
    }
}
