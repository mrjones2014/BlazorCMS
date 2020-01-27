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
    public class ArticleAuthorizationConductorTest : IAuthorizationConductorTest<Article>
    {
        private Mock<IRepositoryReadConductor<Article>> _readConductor = new Mock<IRepositoryReadConductor<Article>>();

        [Fact]
        public void When_UserId_Equals_SectionUserId_Then_Returns_True()
        {
            // Arrange
            var articleId  = 1;
            _readConductor = new Mock<IRepositoryReadConductor<Article>>();
            _readConductor.Setup(m => m.FindById(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsGivenResult(
                    new Article()
                    {
                        Id      = articleId,
                        Section = new Section()
                        {
                            UserId = CurrentUserId
                        }
                    }
                );
            var sut = Sut();

            // Act
            var result = sut.IsAuthorized(articleId, CurrentUserId);

            // Assert
            result.ShouldNotBeNull();
            result.ResultObject.ShouldBeTrue();
        }

        [Fact]
        public void When_UserId_NotEqual_SectionUserId_Then_Returns_False()
        {
            // Arrange
            var articleId  = 1;
            _readConductor = new Mock<IRepositoryReadConductor<Article>>();
            _readConductor.Setup(m => m.FindById(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsGivenResult(
                    new Article()
                    {
                        Id = articleId,
                        Section = new Section()
                        {
                            UserId = CurrentUserId + 1
                        }
                    }
                );
            var sut = Sut();

            // Act
            var result = sut.IsAuthorized(articleId, CurrentUserId);

            // Assert
            result.ShouldNotBeNull();
            result.ResultObject.ShouldBeFalse();
        }

        [Fact]
        public void FilterByUserId_Filters_By_Section_UserId()
        {
            // Arrange
            var articles = new List<Article>()
            {
                new Article()
                {
                    Id      = 1,
                    Section = new Section()
                    {
                        UserId = CurrentUserId
                    }
                },
                new Article()
                {
                    Id      = 2,
                    Section = new Section()
                    {
                        UserId = CurrentUserId + 1
                    }
                }
            };
            var sut = Sut();

            // Act
            var results = articles.Where(sut.FilterByUserId(CurrentUserId).Compile()).ToList();

            // Assert
            results.Count.ShouldBe(1);
            results.First().Id.ShouldBe(1);
        }

        protected override IAuthorizationConductor<Article> Sut()
        {
            return new ArticleAuthorizationConductor(_readConductor.Object);
        }
    }
}
