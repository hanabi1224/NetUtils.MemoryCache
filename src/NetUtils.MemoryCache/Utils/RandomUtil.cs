namespace NetUtils.MemoryCache.Utils
{
    using System;

    public static class RandomUtil
    {
        [ThreadStatic]
        private static Random _random;
        public static Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random(Guid.NewGuid().GetHashCode());
                }
                return _random;
            }
        }
    }
}
