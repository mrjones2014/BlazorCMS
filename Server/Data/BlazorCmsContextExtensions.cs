using System;
using System.Collections.Generic;
using System.Linq;
using BlazorCMS.Server.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorCMS.Server.Data
{
    public static class BlazorCmsContextExtensions
    {
        public static async void SeedUser(this BlazorCmsContext context, UserManager<User> userManager)
        {
            if (context.Users.Any(e => e.UserName == "admin"))
            {
                return;
            }

            var password = DotNetEnv.Env.GetString(EnvironmentVariables.ADMIN_PASSWORD);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ApplicationException("Failed to read admin password from environment.");
            }

            await userManager.CreateAsync(new User {UserName = "admin"}, password);
        }

        public static void SeedHelloWorldSectionAndArticle(this BlazorCmsContext context)
        {
            if (context.Sections.Any())
            {
                return;
            }

            var adminUser = context.Users.FirstOrDefault(e => e.UserName == "admin");
            if (adminUser == null)
            {
                throw new Exception("Couldn't find admin user.");
            }

            var section = new Section
            {
                Name     = "Hello World!",
                UserId   = adminUser.Id,
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Hello World!",
                        Body  = "### Yo\n\n[This](www.google.com) is a link to Google.\n\n- This\n- Is\n- A\n- Bulleted\n- List\n\n```c#\nnamespace BlazorCMS {\n    public class TestClass {\n        public void PrintMessage() => Console.WriteLine(\"This is C# code!\");\n    }\n}\n```\n\n`Console.WriteLine(\"This is inline code!\");`\n\n<div style=\"background-color: black; color: white;\">\nThis is an HTML div with custom inline styles!\n</div>"
                    }
                }
            };

            context.Sections.Add(section);
            context.SaveChanges();
        }
    }
}
