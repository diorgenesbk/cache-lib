namespace Cache.Domain.Constants
{
    public class CacheConstants
    {
        public static readonly string ErrorMessageInvalidKey = "Key can't be null!";

        public static readonly string ErrorMessageSet = "Error when try to add value into Cache - key: {0}";

        public static readonly string ErrorMessageGet = "Error when try to get cached item - key: {0}";

        public static readonly string ErrorMessageDel = "Error when try to del value from Cache - key: {0}";

        public static readonly string ErrorMessageDbSize = "Error when try to get DbSize";

        public static readonly string ErrorMessageIncrInvalidCast = "The value represents from key: {0}, is not an int.";

        public static readonly string ErrorMessageIncr = "Error when try to exec Increment - key: {0}";

        public static readonly string ErrorMessageZadd = "Error when try to exec Zadd - key: {0}";

        public static readonly string ErrorMessageZcard = "Error when try to exec Zcard - key: {0}";

        public static readonly string ErrorMessageZRank = "Error when try to exec ZRank - key: {0}";

        public static readonly string ErrorMessageZRange = "Error when try to exec ZRank - key: {0}";
    }
}
