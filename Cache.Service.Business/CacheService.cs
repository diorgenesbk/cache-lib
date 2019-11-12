using Cache.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cache.Service.Business
{
    public class CacheService
    {
        MemoryCache Cache { get; set; }
        LogService LogService { get; set; }

        public CacheService()
        {
            this.Cache = new MemoryCache(new MemoryCacheOptions());
            this.LogService = new LogService();
        }

        public bool Set<T>(string key, T value, int timeInSeconds)
        {
            try
            {
                this.ValidateKey(key);

                this.Cache.Set(key, value, DateTimeOffset.Now.AddSeconds(timeInSeconds));
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, string.Format(CacheConstants.ErrorMessageSet, key));
                return false;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, string.Format(CacheConstants.ErrorMessageSet, key));
                return false;
            }

            return true;
        }

        public bool Set<T>(string key, T value)
        {
            try
            {
                this.ValidateKey(key);
 
                this.Set(key, value, 1800);
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, string.Format(CacheConstants.ErrorMessageSet, key));
                return false;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, string.Format(CacheConstants.ErrorMessageSet, key));
                return false;
            }

            return true;
        }

        public T Get<T>(string key)
        {
            try
            {
                this.ValidateKey(key);
 
                return this.Cache.Get<T>(key);
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, string.Format(CacheConstants.ErrorMessageGet, key));
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, string.Format(CacheConstants.ErrorMessageGet, key));
            }

            return default;
        }

        public bool Del(string key)
        {
            try
            {
                this.ValidateKey(key);

                this.Cache.Remove(key);
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, string.Format(CacheConstants.ErrorMessageDel, key));
                return false;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, string.Format(CacheConstants.ErrorMessageDel, key));
                return false;
            }

            return true;
        }

        public bool Del(IList<string> keyList)
        {
            keyList.ToList().ForEach(key =>
            {
                this.Del(key);
            });

            return true;
        }

        public int DbSize()
        {
            try
            {
                return this.Cache.Count;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, CacheConstants.ErrorMessageDbSize);
            }
            return 0;
        }
    }
}
