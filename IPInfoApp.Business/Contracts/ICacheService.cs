using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Contracts
{
    public interface ICacheService
    {
        Task<T?> TryGetValueFromCacheAsync<T>(string key) where T : class;
        Task SetCacheItemAsync<T>(string key, T value,int expirationInMinutes = 10);

        Task ClearCacheAsync(string key);
    }
}
