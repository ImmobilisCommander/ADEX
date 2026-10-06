// <copyright file="LinkController.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Adex.Business;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        private readonly ILinkSearchService _linkSearchService;

        public LinkController(ILinkSearchService linkSearchService)
        {
            _linkSearchService = linkSearchService;
        }

        /// <summary>
        /// This method enable user to search for codes of companies or beneficiaries.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/{txt}")]
        public async Task<ActionResult> Search(string txt, CancellationToken cancellationToken)
        {
            return new JsonResult(
                await _linkSearchService.LinksToJsonAsync(txt, 1000, cancellationToken)
            );
        }
    }
}
