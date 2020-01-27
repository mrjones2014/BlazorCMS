using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core.Enumerations;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using AndcultureCode.CSharp.Core.Models;
using AutoMapper;
using BlazorCMS.Server.Conductors;
using BlazorCMS.Server.Data.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/sections/{sectionId:long}/articles")]
    [Authorize]
    public class ArticlesController : BaseController
    {
        #region Properties

        private readonly IAuthorizationConductor<Article>    _authorizationConductor;
        private readonly IRepositoryCreateConductor<Article> _createConductor;
        private readonly IRepositoryDeleteConductor<Article> _deleteConductor;
        private readonly IRepositoryReadConductor<Article>   _readConductor;
        private readonly IRepositoryUpdateConductor<Article> _updateConductor;
        private readonly IRepositoryReadConductor<Section>   _sectionReadConductor;
        private readonly IMapper                             _mapper;

        #endregion Properties

        #region Constructor

        public ArticlesController(
            IAuthorizationConductor<Article>    authorizationConductor,
            IRepositoryCreateConductor<Article> createConductor,
            IRepositoryDeleteConductor<Article> deleteConductor,
            IRepositoryReadConductor<Article>   readConductor,
            IRepositoryUpdateConductor<Article> updateConductor,
            IRepositoryReadConductor<Section>   sectionReadConductor,
            IMapper                             mapper,
            UserManager<User>                   userManager
        ) : base(userManager)
        {
            _authorizationConductor = authorizationConductor;
            _createConductor        = createConductor;
            _deleteConductor        = deleteConductor;
            _readConductor          = readConductor;
            _updateConductor        = updateConductor;
            _sectionReadConductor   = sectionReadConductor;
            _mapper                 = mapper;
        }

        #endregion Constructor

        #region PUT

        [HttpPut]
        public IActionResult Put([FromBody] ArticleDto article)
        {
            var authResult = _authorizationConductor.IsAuthorized(article.SectionId, CurrentUser.Id);
            if (authResult.HasErrorsOrResultIsFalse())
            {
                return Ok<ArticleDto>(null, authResult.Errors);
            }

            var newArticle = new Article
            {
                Title     = article.Title,
                Body      = article.Body,
                SectionId = article.SectionId
            };
            var createResult = _createConductor.Create(newArticle);
            if (createResult.HasErrors)
            {
                return Ok<ArticleDto>(null, createResult.Errors);
            }

            return Ok(_mapper.Map<ArticleDto>(createResult.ResultObject), null);
        }

        #endregion PUT

        #region POST

        [HttpPost("{articleId:long}")]
        public IActionResult Post([FromRoute] long articleId, [FromBody] ArticleDto article)
        {
            article.Id     = articleId;
            var authResult = _authorizationConductor.IsAuthorized(article.Id, CurrentUser.Id);
            if (authResult.HasErrorsOrResultIsFalse())
            {
                return Ok<ArticleDto>(null, authResult.Errors);
            }

            var getResult = _readConductor.FindById(article.Id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return Ok(false, getResult.Errors);
            }

            var updatedArticle   = getResult.ResultObject;
            updatedArticle.Title = article.Title;
            updatedArticle.Body  = article.Body;

            var updateResult = _updateConductor.Update(updatedArticle);
            if (updateResult.HasErrors)
            {
                return Ok(updateResult.ResultObject, updateResult.Errors);
            }

            return Ok(true, null);
        }

        #endregion Post

        #region GET

        [HttpGet]
        public IActionResult Index([FromRoute] long sectionId)
        {
            Expression<Func<Article, bool>> filter = _authorizationConductor.FilterByUserId(CurrentUser.Id);
            var findResult                         = _readConductor.FindAll(filter);

            if (findResult.HasErrorsOrResultIsNull())
            {
                return NotFound<IEnumerable<ArticleDto>>(null, findResult.Errors);
            }

            return Ok<IEnumerable<ArticleDto>>(findResult.ResultObject.Select(e => _mapper.Map<ArticleDto>(e)), null);
        }

        [HttpGet("{id:long}")]
        public IActionResult Get([FromRoute] long sectionId, [FromRoute] long id)
        {
            var authResult = _authorizationConductor.IsAuthorized(id, CurrentUser.Id);
            if (authResult.HasErrorsOrResultIsFalse())
            {
                return Ok<ArticleDto>(null, authResult.Errors);
            }

            var findResult = _readConductor.FindById(id);
            if (findResult.HasErrorsOrResultIsNull())
            {
                return Ok<ArticleDto>(null, findResult.Errors);
            }

            return Ok(_mapper.Map<ArticleDto>(findResult.ResultObject), null);
        }

        #endregion GET

        #region DELETE

        [HttpDelete("{id:long}")]
        public IActionResult Delete([FromRoute] long sectionId, [FromRoute] long id)
        {
            var authResult = _authorizationConductor.IsAuthorized(id, CurrentUser.Id);
            if (authResult.HasErrorsOrResultIsFalse())
            {
                return Ok<ArticleDto>(null, authResult.Errors);
            }

            var deleteResult = _deleteConductor.Delete(id: id, soft: false);
            if (deleteResult.HasErrorsOrResultIsNull())
            {
                return Ok(false, deleteResult.Errors);
            }

            return Ok(deleteResult.ResultObject, deleteResult.Errors);
        }

        #endregion DELETE
    }
}
