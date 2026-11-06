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

namespace Adex.Mvc.Components.Pages;

public partial class NotFoundPage
{
    // Les pages MVC ne font pas partie du routeur Blazor : une navigation interne vers l'une d'elles
    // aboutit ici, il faut alors laisser le serveur la servir par un chargement complet.
    protected override void OnInitialized()
    {
        if (RendererInfo.IsInteractive
            && !new Uri(Navigation.Uri).AbsolutePath.Equals("/not-found", StringComparison.OrdinalIgnoreCase))
        {
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        }
    }
}
