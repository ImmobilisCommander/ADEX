using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
using Adex.Mvc;
using Adex.Mvc.Components;
using Adex.Mvc.Components.Layout;
using Adex.Mvc.Components.Loading;
using Adex.Mvc.Components.Entities;
using Adex.Mvc.Configuration;
using Adex.Mvc.Entities;
using Adex.WebApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Adex.Mvc.Components.Entities;

public partial class JustificationDialog
{
    private ElementReference overlay;
    private bool focusPending;

    [Parameter]
    public FinancialLinkViewModel Link { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override void OnParametersSet() => focusPending = Link is not null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusPending)
        {
            focusPending = false;
            await overlay.FocusAsync();
        }
    }

    private Task OnKeyDown(KeyboardEventArgs args) =>
        args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;
}
