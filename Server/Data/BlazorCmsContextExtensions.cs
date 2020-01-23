using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BlazorCMS.Server.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorCMS.Server.Data
{
    public static class BlazorCmsContextExtensions
    {
        public static IdentityResult SeedRole(
        this BlazorCmsContext context,
        RoleManager<IdentityRole<long>> roleManager)
        {
            if (context.Roles.Any(r => r.Name == Roles.USER))
            {
                return null;
            }

            return roleManager.CreateAsync(new IdentityRole<long>(Roles.USER)).Result;
        }

        public static IdentityResult SeedUser(this BlazorCmsContext context, UserManager<User> userManager)
        {
            if (context.Users.Any(e => e.UserName == "admin"))
            {
                return null;
            }

            var password = DotNetEnv.Env.GetString(EnvironmentVariables.ADMIN_PASSWORD);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ApplicationException("Failed to read admin password from environment.");
            }

            var user         = new User {UserName = "admin"};
            var result       = userManager.CreateAsync(user, password).Result;
            var roleResult   = userManager.AddToRoleAsync(user, Roles.USER);
            var claimsResult = userManager.AddClaimAsync(user, new Claim(Roles.USER, Roles.USER)).Result;
            return result;
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
