// <copyright file="LinkController.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

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
        public ActionResult Search(string txt)
        {
            return new JsonResult(_linkSearchService.LinksToJson(txt, 1000));
        }
    }
}
