using System.Collections.Generic;
using System.Linq;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using AutoMapper;
using BlazorCMS.Server.Data.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/sections")]
    [Authorize]
    public class SectionsController : BaseController
    {
        #region Properties

        private readonly IRepositoryCreateConductor<Section> _createConductor;
        private readonly IRepositoryDeleteConductor<Section> _deleteConductor;
        private readonly IRepositoryReadConductor<Section>   _readConductor;
        private readonly IRepositoryUpdateConductor<Section> _updateConductor;
        private readonly IMapper                             _mapper;

        #endregion Properties

        #region Constructor

        public SectionsController(
            IRepositoryCreateConductor<Section> createConductor,
            IRepositoryDeleteConductor<Section> deleteConductor,
            IRepositoryReadConductor<Section>   readConductor,
            IRepositoryUpdateConductor<Section> updateConductor,
            IMapper                             mapper,
            UserManager<User>                   userManager
        ) : base(userManager)
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
        public IActionResult Put([FromBody] SectionDto section)
        {
            var newSection = new Section
            {
                Name = section.Name
            };
            var createResult = _createConductor.Create(newSection);
            if (createResult.HasErrors)
            {
                return Ok<SectionDto>(null, createResult.Errors);
            }

            return Ok(_mapper.Map<SectionDto>(createResult.ResultObject), null);
        }

        #endregion PUT

        #region POST

        [HttpPost("{sectionId:long}")]
        public IActionResult Post([FromRoute] long sectionId, [FromBody] SectionDto section)
        {
            section.Id = sectionId;
            var getResult = _readConductor.FindById(section.Id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return Ok(false, getResult.Errors);
            }

            var updatedSection  = getResult.ResultObject;
            updatedSection.Name = section.Name;

            var updateResult = _updateConductor.Update(updatedSection);
            if (updateResult.HasErrors)
            {
                return Ok(updateResult.ResultObject, updateResult.Errors);
            }

            return Ok(true, null);
        }

        #endregion POST

        #region GET

        [HttpGet]
        public IActionResult Index()
        {
            var getResult = _readConductor.FindAll();
            if (getResult.HasErrorsOrResultIsNull())
            {
                return Ok<IEnumerable<SectionDto>>(null, getResult.Errors);
            }

            return Ok<IEnumerable<SectionDto>>(getResult.ResultObject.Select(e => _mapper.Map<SectionDto>(e)), null);
        }

        [HttpGet("{id:long}")]
        public IActionResult Get(long id)
        {
            var getResult = _readConductor.FindById(id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return Ok<SectionDto>(null, getResult.Errors);
            }

            return Ok(_mapper.Map<SectionDto>(getResult.ResultObject), null);
        }

        #endregion GET

        #region DELETE

        /// <summary>
        /// Deleting a section will automatically delete all the articles because of our foreign key constraint
        /// and delete behavior in our database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            var deleteResult = _deleteConductor.Delete(id: id, soft: false);
            if (deleteResult.HasErrors)
            {
                return Ok(deleteResult.ResultObject, deleteResult.Errors);
            }

            return Ok(deleteResult.ResultObject, null);
        }

        #endregion DELETE
    }
}
