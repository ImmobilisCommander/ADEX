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

namespace Adex.Mvc.Components.Layout;

public partial class NavMenu
{
    private bool open;
    private ElementReference toggleButton;

    private void Toggle() => open = !open;

    private void Close() => open = false;

    private async Task OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && open)
        {
            open = false;
            await toggleButton.FocusAsync();
        }
    }
}
