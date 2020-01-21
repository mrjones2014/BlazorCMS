using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using AutoMapper;
using BlazorCMS.Server.Data.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/{sectionId:long}/articles")]
    public class ArticlesController : BaseController
    {
        #region Properties

        private readonly IRepositoryCreateConductor<Article> _createConductor;
        private readonly IRepositoryDeleteConductor<Article> _deleteConductor;
        private readonly IRepositoryReadConductor<Article>   _readConductor;
        private readonly IRepositoryUpdateConductor<Article> _updateConductor;
        private readonly IMapper                             _mapper;

        #endregion Properties

        #region Constructor

        public ArticlesController(
            IRepositoryCreateConductor<Article> createConductor,
            IRepositoryDeleteConductor<Article> deleteConductor,
            IRepositoryReadConductor<Article>   readConductor,
            IRepositoryUpdateConductor<Article> updateConductor,
            IMapper                             mapper
        )
        {
            _createConductor = createConductor;
            _deleteConductor = deleteConductor;
            _readConductor   = readConductor;
            _updateConductor = updateConductor;
            _mapper          = mapper;
        }

        #endregion Constructor

        #region PUT

        [HttpPut]
        public IActionResult Put([FromBody] ArticleDto article)
        {
            var newArticle = new Article
            {
                Title     = article.Title,
                Body      = article.Body,
                SectionId = article.SectionId
            };
            var createResult = _createConductor.Create(newArticle);
            if (createResult.HasErrors)
            {
                return InternalError<ArticleDto>(null, createResult.Errors);
            }

            return Ok(createResult.ResultObject, null);
        }

        #endregion PUT

        #region POST

        [HttpPost("{articleId:long}")]
        public IActionResult Post([FromRoute] long articleId, [FromBody] ArticleDto article)
        {
            article.Id = articleId;
            var getResult = _readConductor.FindById(article.Id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return NotFound(false, getResult.Errors);
            }

            var updatedArticle   = getResult.ResultObject;
            updatedArticle.Title = article.Title;
            updatedArticle.Body  = article.Body;

            var updateResult = _updateConductor.Update(updatedArticle);
            if (updateResult.HasErrors)
            {
                return InternalError(updateResult.ResultObject, updateResult.Errors);
            }

            return Ok(true, null);
        }

        #endregion Post

        #region GET

        [HttpGet]
        public IActionResult Index([FromRoute] long sectionId)
        {
            Expression<Func<Article, bool>> filter = e => e.SectionId == sectionId;
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
            var findResult = _readConductor.FindById(id);
            if (findResult.HasErrorsOrResultIsNull())
            {
                return NotFound<ArticleDto>(null, findResult.Errors);
            }

            return Ok(_mapper.Map<ArticleDto>(findResult.ResultObject), null);
        }

        #endregion GET

        #region DELETE

        [HttpDelete("{id:long}")]
        public IActionResult Delete([FromRoute] long sectionId, [FromRoute] long id)
        {
            var deleteResult = _deleteConductor.Delete(id: id, soft: false);
            if (deleteResult.HasErrorsOrResultIsNull())
            {
                return InternalError(false, deleteResult.Errors);
            }

            return Ok(deleteResult.ResultObject, deleteResult.Errors);
        }

        #endregion DELETE
    }
}
