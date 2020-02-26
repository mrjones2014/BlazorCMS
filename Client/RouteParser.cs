using System;

namespace BlazorCMS.Client
{
    public class RouteParams
    {
        public long SectionId { get; set; }
        public long ArticleId { get; set; }
    }

    public class RouteParser
    {
        public static RouteParams ParseRoute(string route)
        {
            try
            {
                var parts      = route.Split("/");
                var sectionIdx = findIndex(parts, "section");
                var sectionId  = -1L;
                if (sectionIdx > -1)
                {
                    long.TryParse(parts[sectionIdx + 1], out sectionId);
                }

                var articleIdx = findIndex(parts, "article");
                var articleId  = -1L;
                if (articleIdx > -1)
                {
                    long.TryParse(parts[sectionIdx + 1], out articleId);
                }

                return new RouteParams
                {
                    SectionId = sectionId,
                    ArticleId = articleId
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static int findIndex(string[] parts, string query)
        {
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i]?.ToLower() == query?.ToLower())
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
