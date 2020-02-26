using System.Collections.Generic;
using System.Linq;

namespace ClientApi.ViewModels
{
    public static class ServerSidePagedResultExtensions
    {        
        public static object CreateServerSidePagedResult<T>(this List<T> items, string baseUrl, int total, int skip, int top, Dictionary<string, string> queryParams = null)
        {
            queryParams ??= new Dictionary<string, string>();
            var urlParameters = string.Join("&", (from kvp in queryParams where kvp.Value != null select $"{kvp.Key}={kvp.Value}"));

            var count = items.Count;
            var prefUrl = $"{baseUrl}?{urlParameters}";

            var selfUrl = $"{prefUrl}&skip={skip}&top={top}";
            var nextUrl = total > top + skip ? $"{prefUrl}&skip={skip + count}&top={top}" : null;
            var prevUrl = skip - count >= 0 ? $"{prefUrl}&skip={skip - count}&top={top}" : null;

            return new
            {
                links = new
                {
                    prevUrl,
                    selfUrl,
                    nextUrl
                },
                items,
                pagination = new
                {
                    returned = count,
                    total
                }
            };
        }
    }
}
