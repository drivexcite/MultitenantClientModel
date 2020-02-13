using System.Collections.Generic;
using System.Linq;

namespace ClientApi.ViewModels
{
    public class ServerSidePagedResult<T>
    {
        public int Total { get; private set; }
        public int Top { get; private set; }
        public int Skip { get; private set; }
        public string BaseUrl { get; private set; }
        public IEnumerable<T> Items { get; private set; }
        public Dictionary<string, string> QueryParams { get; private set; }

        public ServerSidePagedResult(IEnumerable<T> items, string baseUrl, int total, int skip, int top, Dictionary<string, string> queryParams = null)
        {
            Items = items;
            BaseUrl = baseUrl;
            Total = total;
            Skip = skip;
            Top = top;
            QueryParams = queryParams ?? new Dictionary<string, string>();
        }

        public virtual object BuildViewModel()
        {
            var urlParameters = string.Join("&", (from kvp in QueryParams where kvp.Value != null select $"{kvp.Key}={kvp.Value}"));

            var count = Items.Count();
            var prefUrl = $"{BaseUrl}?{urlParameters}";

            var selfUrl = $"{prefUrl}&skip={Skip}&top={Top}";
            var nextUrl = Total > Top + Skip ? $"{prefUrl}&skip={Skip + count}&top={Top}" : null;
            var prevUrl = Skip - count >= 0 ? $"{prefUrl}&skip={Skip - count}&top={Top}" : null;

            return new
            {
                links = new
                {
                    prevUrl,
                    selfUrl,
                    nextUrl
                },
                items = Items,
                pagination = new
                {
                    returned = count,
                    total = Total
                }
            };
        }
    }
}
