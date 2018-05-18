using System;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Self)]
namespace NetUtils.MemoryCache.Tests
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            MemoryCache.CacheCleanCheckInternal = TimeSpan.FromMilliseconds(10);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            MemoryCache.CacheCleanCheckInternal = MemoryCache.DefaultCacheCleanCheckInternal;
        }
    }
}
