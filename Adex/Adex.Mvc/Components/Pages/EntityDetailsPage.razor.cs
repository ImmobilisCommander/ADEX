using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Api;
using Adex.Mvc.Entities;
using Adex.Mvc.Entities.Json;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;

namespace Adex.Mvc.Components.Pages
{
    public partial class EntityDetailsPage
    {
        private static readonly (string Key, string Title, string Label)[] SortColumns =
        {
            ("direction", "Trier par sens", "Sens"),
            ("name", "Trier par contrepartie", "Contrepartie"),
            ("type", "Trier par typologie", "Typologie"),
            ("kind", "Trier par nature", "Nature / objet"),
        };

        private EntityDetailsViewModel model;
        private FinancialLinkViewModel selectedLink;
        private string errorMessage;
        private bool notFound;

        [Parameter]
        public Guid Id { get; set; }

        [SupplyParameterFromQuery]
        public int? Page { get; set; }

        [SupplyParameterFromQuery]
        public int? PageSize { get; set; }

        [SupplyParameterFromQuery]
        public string Sort { get; set; }

        [SupplyParameterFromQuery]
        public bool? Descending { get; set; }

        [SupplyParameterFromQuery(Name = "fromQuery")]
        public string FromQuery { get; set; }

        [CascadingParameter]
        public HttpContext HttpContext { get; set; }

        [Inject]
        private EntityDetailsCache DetailsCache { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            selectedLink = null;
            await RunWithLoadingAsync(LoadAsync);
        }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            var page = Math.Max(Page ?? 1, 1);
            var pageSize = PageSize is > 0 ? PageSize.Value : 20;
            var sort = string.IsNullOrWhiteSpace(Sort) ? "amount" : Sort;
            var descending = Descending ?? true;

            errorMessage = null;
            notFound = false;
            try
            {
                var entity = await DetailsCache.GetAsync(Id, page, pageSize, sort, descending, cancellationToken);
                if (entity is null)
                {
                    model = null;
                    notFound = true;
                    if (HttpContext is { Response.HasStarted: false })
                    {
                        HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    }

                    return;
                }

                var fromQuery = FromQuery?.Trim();
                var viewModel = entity.ToViewModel();
                if (!string.IsNullOrEmpty(fromQuery))
                {
                    viewModel.SearchQuery = fromQuery;
                    foreach (var link in viewModel.FinancialLinks)
                    {
                        link.SearchQuery = fromQuery;
                    }
                }

                viewModel.JsonUrl =
                    $"/data/entities/{Id}.json?page={page}&pageSize={pageSize}&sort={sort}&descending={descending.ToString().ToLowerInvariant()}";
                model = viewModel;
            }
            catch (Exception exception) when (exception is AdexApiException or HttpRequestException)
            {
                model = null;
                errorMessage = exception.Message;
            }
        }

        private void Select(FinancialLinkViewModel link)
        {
            if (link.HasJustification)
            {
                selectedLink = link;
            }
        }

        private void OnRowKeyDown(KeyboardEventArgs args, FinancialLinkViewModel link)
        {
            if (args.Key is "Enter" or " ")
            {
                Select(link);
            }
        }

        private void CloseJustification() => selectedLink = null;

        private void OnPageSizeChanged(ChangeEventArgs args)
        {
            if (int.TryParse(args.Value?.ToString(), out var size))
            {
                Navigation.NavigateTo(model.PageSizeUrl(size));
            }
        }
    }
}
