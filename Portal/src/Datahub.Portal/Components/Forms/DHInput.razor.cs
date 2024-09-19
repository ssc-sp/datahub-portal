#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Components.Forms
{
    public partial class DHInput : ComponentBase
    {
    }


    public enum DHInputType
    {
        Text,
        TextButton,
        Select,
        SelectMultiple,
    }
}