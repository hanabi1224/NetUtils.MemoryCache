﻿
using System;

namespace NetUtils.MemoryCache.Utils
{
    public static class RandomUtil
    {
        [ThreadStatic]
        private static Random t_random;
        public static Random Random
        {
            get
            {
                if (t_random == null)
                {
                    t_random = new Random(Guid.NewGuid().GetHashCode());
                }
                return t_random;
            }
        }
    }
}
