using Microsoft.AspNetCore.Components;

namespace BlazorAppHuellero.Components.Utils
{
    public partial class LoaderComponent
    {
        [Parameter] public bool Loading { get; set; } = false;
    }
}
