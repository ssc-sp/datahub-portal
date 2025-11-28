using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using NSubstitute;

namespace Datahub.SpecflowTests.Utils
{
    internal static class MudBlazorUtils
    {

        public static void SetupFakeMudBlazorServices(this BunitServiceProvider Services)
        {
            var mockPopOver = Substitute.For<IPopoverService>();
            Services.AddSingleton(mockPopOver);
            var mockSnackBar = Substitute.For<ISnackbar>();
            Services.AddSingleton(mockSnackBar);
        }
        public static void SetupMudBlazor(this BunitJSInterop JSInterop)
        {
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            JSInterop.SetupModule("./_content/Datahub.Portal/Components/SkipLink.razor.js");
            var markdown = JSInterop.SetupModule("./_content/Datahub.Core/Components/DHMarkdown.razor.js");
            markdown.SetupVoid("styleCodeblocks", _ => true).SetVoidResult();
            markdown.SetupVoid("highlightCodeElement", _ => true).SetVoidResult();
            markdown.SetupVoid("setHighlightStylesheet", _ => true).SetVoidResult();
            JSInterop.SetupVoid("setHighlightStylesheet", _ => true).SetVoidResult();
            JSInterop.SetupVoid("mudPopover.initialize", _ => true);
            // Setup handler to return an int result (number of providers)
            JSInterop.Setup<int>("mudpopoverHelper.countProviders").SetResult(1);
            JSInterop.SetupVoid("mudPopover.connect", _ => true).SetVoidResult(); 
            JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true).SetVoidResult();
            JSInterop.SetupVoid("mudPointerEventsNone.dispose").SetVoidResult();
            JSInterop.SetupVoid("mudPopover.dispose").SetVoidResult();
            JSInterop.SetupVoid("mudKeyInterceptor.dispose").SetVoidResult();
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true).SetVoidResult();
            JSInterop.SetupVoid("mudKeyInterceptor.disconnect", _ => true).SetVoidResult();
            JSInterop.SetupVoid("mudElementRef.removeOnBlurEvent", _ => true).SetVoidResult();
            JSInterop.SetupVoid("highlightCodeElement", _ => true).SetVoidResult();            
            JSInterop.SetupVoid("mudPointerEventsNone.cancelListener", _ => true).SetVoidResult();
            JSInterop.SetupVoid("mudKeyInterceptor.disconnect", _ => true).SetVoidResult();
        }
    }
}
