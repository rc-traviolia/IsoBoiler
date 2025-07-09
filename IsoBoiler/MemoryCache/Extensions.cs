using Microsoft.Extensions.Caching.Memory;

namespace IsoBoiler.MemoryCache
{
    public static class Extensions
    {
        public static async Task<TDataModel> Get<TDataModel>(this IMemoryCache memoryCache, string cacheKey, Func<Task<TDataModel>> refreshFunction, TimeSpan? timeToLive = null)
        {
            var cachedResults = memoryCache.Get<TDataModel>(cacheKey);

            if (cachedResults == null)
            {
                var results = await refreshFunction();
                memoryCache.Set(cacheKey, results, timeToLive ?? new TimeSpan(3, 0, 0)); //3 hr default
                return results;
            }
            else
            {
                return cachedResults;
            }
        }

        public static async Task<TDataModel> Get<TDataModel>(this IMemoryCache memoryCache, Func<Task<TDataModel>> refreshFunction, TimeSpan? timeToLive = null)
        {
            var cacheKey = !refreshFunction.Method.Name.Contains('<') ? refreshFunction.Method.Name : throw new Exception("If you want to pass an anonymous lambda function into this method you must provide a cacheKey value as well.");
            var cachedResults = memoryCache.Get<TDataModel>(cacheKey);

            if (cachedResults == null)
            {
                var results = await refreshFunction();
                memoryCache.Set(cacheKey, results, timeToLive ?? new TimeSpan(3, 0, 0)); //3 hr default
                return results;
            }
            else
            {
                return cachedResults;
            }
        }
    }
}
