using System.Collections.Generic;
using System.Linq;
using BlazorCMS.Server.Data.Models;

namespace BlazorCMS.Server.Data
{
    public static class BlazorCmsContextExtensions
    {
        public static void SeedHelloWorldSectionAndArticle(this BlazorCmsContext context)
        {
            if (context.Sections.Any())
            {
                return;
            }
            var section = new Section
            {
                Name     = "Hello World!",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Hello World!",
                        Body  = "### Yo\n\n[This](www.google.com) is a link to Google.\n\n- This\n- Is\n- A\n- Bulleted\n- List\n\n```c#\nnamespace BlazorCMS {\npublic class TestClass {\npublic void PrintMessage() => Console.WriteLine(\"This is C# code!\");\n}\n}\n```\n\n`Console.WriteLine(\"This is inline code!\");`\n\n<div style=\"background-color: black; color: white;\">\nThis is an HTML div with custom inline styles!\n</div>"
                    }
                }
            };

            context.Sections.Add(section);
            context.SaveChanges();
        }
    }
}
