using System.Collections.Generic;
using BlazorCMS.Server.Data.Models;

namespace BlazorCMS.Server.Data
{
    public static class BlazorCmsContextExtensions
    {
        public static void SeedHelloWorldSectionAndArticle(this BlazorCmsContext context)
        {
            var section = new Section
            {
                Name     = "Hello World!",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Hello World!",
                        Body  = "This is an article!"
                    }
                }
            };

            context.Sections.Add(section);
            context.SaveChanges();
        }
    }
}
