using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NetUtils.MemoryCache.Utils;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class ConcurrencyTests
    {
        [Test]
        [TestCase(true, 100)]
        [TestCase(false, 3)]
        public void TestCacheFuntionConcurrency(bool useStrictThreadSafeMode, int numberOfConcurrentTasks)
        {
            var key = Guid.NewGuid().ToString();
            var cache = MemoryCache.GetNamedInstance(nameof(TestCacheFuntionConcurrency));
            cache.UseStrictThreadSafeModeForAutoReload = useStrictThreadSafeMode;
            var counter = 0;
            Parallel.For(0, numberOfConcurrentTasks, i =>
            {
                cache.GetAutoReloadDataWithInterval(
                    key,
                    () =>
                    {
                        Thread.Sleep(RandomUtil.Random.Next(900) + 100);
                        Interlocked.Increment(ref counter);
                        return i;
                    },
                    TimeSpan.FromDays(1),
                    TimeSpan.FromHours(1));
            });

            counter.Should().Be(1);
        }

        ////[Test]
        ////public async Task TestCacheAsyncFuntionConcurrency()
        ////{
        ////    var key = Guid.NewGuid().ToString();
        ////    var cache = MemoryCache.GetNamedInstance(nameof(TestCacheAsyncFuntionConcurrency));

        ////    var counter = 0;
        ////    var tasks = new List<Task>();
        ////    for (var i = 0; i < 100; i++)
        ////    {
        ////        var t = cache.GetDataOrCreateAsync(key, async () =>
        ////        {
        ////            await Task.Delay(RandomUtil.Random.Next(900) + 100);
        ////            Interlocked.Increment(ref counter);
        ////            return i;
        ////        }, TimeSpan.FromDays(1)).ContinueWith(async _ => (await _.ConfigureAwait(false)).Should().BeGreaterOrEqualTo(0));

        ////        tasks.Add(t);
        ////    }

        ////    await Task.WhenAll(tasks);

        ////    counter.Should().Be(1);
        ////}
    }
}
