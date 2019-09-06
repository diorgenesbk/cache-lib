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
                this.LogService.LogError(ae, $"Error when try to add value into Cache - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to add value into Cache! - Key: {key}");
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
                this.LogService.LogError(ae, $"Error when try to set value into Cache - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to set value into Cache! - Key: {key}");
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
                this.LogService.LogError(ae, $"Error when try to get cached item - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to get cached item! - Key: {key}");
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
                this.LogService.LogError(ae, $"Error when try to del value from Cache - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to del value from Cache! - Key: {key}");
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
                this.LogService.LogError(ex, $"Error when try to get DbSize");
            }
            return 0;
        }

        public int Incr(string key)
        {
            try
            {
                this.ValidateKey(key);

                var cachedItem = this.Get<int?>(key);

                if (cachedItem == null || cachedItem.Equals(string.Empty))
                {
                    this.Set(key, 0);
                }
                else
                {
                    var tryResult = int.TryParse(cachedItem.ToString(), out int value);

                    if (!tryResult)
                    {
                        throw new InvalidCastException($"The value represents from key: {key}, is not an int");
                    }
                    else
                    {
                        this.Del(key);
                        this.Set(key, ++value);
                    }
                }

                return this.Get<int>(key);

            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, $"Error when try to exec Increment - key: {key}");
            }
            catch (InvalidCastException ice)
            {
                this.LogService.LogError(ice, $"Error when try to exec Increment - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to exec Increment - key: {key}");
            }

            return 0;
        }

        public int Zadd(string key, string[] scoreMemberWhiteSpaceSeparated)
        {
            try
            {
                this.ValidateKey(key);

                string[] cachedItem = this.Get<string[]>(key);

                if (cachedItem == null || cachedItem.Equals(string.Empty))
                {
                    var scoreMemberOrdered = this.OrderMembersByScore(scoreMemberWhiteSpaceSeparated);

                    this.Set(key, scoreMemberWhiteSpaceSeparated);
                }
                else
                {
                    this.UpdateScoreIfMemberExists(key, scoreMemberWhiteSpaceSeparated);
                    this.InsertMemberIfNotExists(key, scoreMemberWhiteSpaceSeparated);
                }
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, $"Error when try to exec Zadd - key: {key}");
                return 0;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to exec Zadd - key: {key}");
                return 0;
            }

            return 1;
        }

        public int Zcard(string key)
        {
            try
            {
                this.ValidateKey(key);

                string[] scoreMemberList = this.Get<string[]>(key);

                if (scoreMemberList != null && scoreMemberList.ToList().Count >= 0)
                {
                    return scoreMemberList.ToList().Count;
                }
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, $"Error when try to exec Zcard - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to exec Zcard - key: {key}");
            }

            return 0;

        }

        public string ZRank(string key, string member)
        {
            try
            {
                this.ValidateKey(key);

                string[] scoreMemberList = this.Get<string[]>(key);

                if (scoreMemberList != null && scoreMemberList.ToList().Count >= 0)
                {
                    for (int i = 0; i < scoreMemberList.Length; i++)
                    {
                        if (scoreMemberList[i].Split(" ")[1] == member)
                            return i.ToString();
                    }
                }
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, $"Error when try to exec ZRank - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to exec ZRank - key: {key}");
            }

            return "nil";
        }

        public List<string> ZRange(string key, int start, int stop)
        {
            List<string> rangeScoreMember = new List<string>();
            try
            {
                this.ValidateKey(key);

                string[] scoreMemberList = this.Get<string[]>(key);

                int calculatedStart, calculatedStop;

                calculatedStart = start < 0 ? scoreMemberList.ToList().Count + start : start;

                calculatedStop = stop < 0 ? scoreMemberList.ToList().Count + stop : stop;

                if (calculatedStart > calculatedStop)
                    throw new ArgumentNullException($"Invalid range - start: {start} and stop: {stop} - key: {key}");

                if (scoreMemberList != null && scoreMemberList.ToList().Count > 0)
                {
                    for (int i = 0; i < scoreMemberList.ToList().Count; i++)
                    {
                        if (i >= start && i <= stop)
                            rangeScoreMember.Add(scoreMemberList[i]);
                    }
                }
            }
            catch (ArgumentNullException ae)
            {
                this.LogService.LogError(ae, $"Error when try to exec ZRank - key: {key}");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex, $"Error when try to exec ZRank - key: {key}");
            }

            return rangeScoreMember;
        }

        private string[] OrderMembersByScore(string[] scoreMemberWhiteSpaceSeparated)
        {
            for (int i = 0; i < scoreMemberWhiteSpaceSeparated.Length; i++)
            {
                for (int j = 0; j < scoreMemberWhiteSpaceSeparated.Length - 1; j++)
                {
                    string[] principalScoreMemberSplited = scoreMemberWhiteSpaceSeparated[j].Split(' ');
                    string principalScore = principalScoreMemberSplited[0];

                    string[] secondaryScoreMemberSplited = scoreMemberWhiteSpaceSeparated[j + 1].Split(' ');
                    string secondaryScore = secondaryScoreMemberSplited[0];

                    if (decimal.Parse(principalScore) > decimal.Parse(secondaryScore))
                    {
                        var auxiliar = scoreMemberWhiteSpaceSeparated[j];
                        scoreMemberWhiteSpaceSeparated[j] = scoreMemberWhiteSpaceSeparated[j + 1];
                        scoreMemberWhiteSpaceSeparated[j + 1] = auxiliar;
                    }
                }
            }

            return scoreMemberWhiteSpaceSeparated;
        }

        private void UpdateScoreIfMemberExists(string key, string[] scoreMemberParameter)
        {
            var scoreMemberList = this.Get<string[]>(key);

            for (int i = 0; i < scoreMemberParameter.ToArray().Length; i++)
            {
                var parameterSplited = scoreMemberParameter[i].Split(' ');
                for (int j = 0; j < scoreMemberList.ToArray().Length; j++)
                {
                    var scoreMemberSplited = scoreMemberList[j].Split(' ');

                    if (parameterSplited[1] == scoreMemberSplited[1])
                    {
                        decimal scoreParameter = decimal.Parse(parameterSplited[0]);
                        decimal scorePrincipal = decimal.Parse(scoreMemberSplited[0]);

                        scoreMemberSplited[0] = (scoreParameter + scorePrincipal).ToString();

                        string newScoreMember = string.Concat(scoreMemberSplited[0], " ", scoreMemberSplited[1]);

                        scoreMemberList[j] = newScoreMember;
                    }
                }
            }
            this.Del(key);

            scoreMemberList = this.OrderMembersByScore(scoreMemberList);

            this.Set(key, scoreMemberList);
        }

        private void InsertMemberIfNotExists(string key, IList<string> scoreMemberParameter)
        {
            var scoreMemberList = this.Get<string[]>(key);

            List<string> memberList = scoreMemberList.Select(sm => sm.Split(' ')[1]).ToList();

            scoreMemberParameter.ToList().ForEach(smp =>
            {
                if (!memberList.Contains(smp.Split(' ')[1]))
                {
                    scoreMemberList = scoreMemberList.Append(smp).ToArray();
                }
            });

            this.Del(key);

            scoreMemberList = this.OrderMembersByScore(scoreMemberList);

            this.Set(key, scoreMemberList);
        }

        private void ValidateKey(string key)
        {
            this.ValidateKey(key);
                throw new ArgumentNullException("Key can't be null!");
                
        }
    }
}
