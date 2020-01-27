using System.Collections.Generic;
using System.Linq;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using BlazorCMS.Server.Conductors;
using BlazorCMS.Server.Data.Models;
using BlazorCMS.Tests.Extensions;
using Moq;
using Shouldly;
using Xunit;

namespace BlazorCMS.Tests.Conductors
{
    public class SectionAuthorizationConductorTest : IAuthorizationConductorTest<Section>
    {
        private Mock<IRepositoryReadConductor<Section>> _readConductor = new Mock<IRepositoryReadConductor<Section>>();

        [Fact]
        public void When_UserId_Equals_SectionUserId_Then_Returns_False()
        {
            // Arrange
            var sectionId  = 1;
            _readConductor = new Mock<IRepositoryReadConductor<Section>>();
            _readConductor.Setup(m => m.FindById(It.IsAny<long>()))
                .ReturnsGivenResult(new Section() {Id = sectionId, UserId = CurrentUserId});
            var sut = Sut();

            // Act
            var result = sut.IsAuthorized(sectionId, CurrentUserId);

            // Assert
            result.ShouldNotBeNull();
            result.ResultObject.ShouldBeTrue();
        }

        [Fact]
        public void When_UserId_NotEqualTo_SectionUserId_Then_Returns_False()
        {
            // Arrange
            var sectionId  = 1;
            _readConductor = new Mock<IRepositoryReadConductor<Section>>();
            _readConductor.Setup(m => m.FindById(It.IsAny<long>()))
                .ReturnsGivenResult(new Section() {Id = sectionId, UserId = CurrentUserId + 1});
            var sut = Sut();

            // Act
            var result = sut.IsAuthorized(sectionId, CurrentUserId);

            // Assert
            result.ShouldNotBeNull();
            result.ResultObject.ShouldBeFalse();
        }

        [Fact]
        public void FilterByUserId_Filters_By_Section_UserId()
        {
            // Arrange
            var sections = new List<Section>()
            {
                new Section()
                {
                    Id     = 1,
                    UserId = CurrentUserId
                },
                new Section()
                {
                    Id     = 2,
                    UserId = CurrentUserId + 1
                }
            };
            var sut = Sut();

            // Act
            var result = sections.Where(sut.FilterByUserId(CurrentUserId).Compile()).ToList();

            // Assert
            result.Count().ShouldBe(1);
            result.First().Id.ShouldBe(1);
        }

        public override IAuthorizationConductor<Section> Sut()
        {
            return new SectionAuthorizationConductor(_readConductor.Object);
        }
    }
}
