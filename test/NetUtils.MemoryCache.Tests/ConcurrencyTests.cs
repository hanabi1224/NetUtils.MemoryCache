using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class ConcurrencyTests
    {
        [Test]
        [TestCase(10000, true)]
        [TestCase(1000, true)]
        [TestCase(100, true)]
        [TestCase(10, true)]
        [TestCase(10000, false)]
        [TestCase(1000, false)]
        [TestCase(100, false)]
        [TestCase(10, false)]
        public void TestCacheFuntionConcurrency(int numberOfConcurrentTasks, bool shouldReloadInBackground)
        {
            var key = Guid.NewGuid().ToString();
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheFuntionConcurrency));
            var counter = 0;
            Parallel.For(0, numberOfConcurrentTasks, i =>
            {
                cache.GetAutoReloadDataWithInterval(
                    key,
                    async () =>
                    {
                        await Task.Delay(RandomUtils.Random.Next(900) + 100).ConfigureAwait(false);
                        Interlocked.Increment(ref counter);
                        return i;
                    },
                    TimeSpan.FromDays(1),
                    TimeSpan.FromHours(1),
                    shouldReloadInBackground: shouldReloadInBackground);
            });

            counter.Should().Be(1);
        }

        [Test]
        [TestCase(10000, true)]
        [TestCase(1000, true)]
        [TestCase(100, true)]
        [TestCase(10, true)]
        [TestCase(10000, false)]
        [TestCase(1000, false)]
        [TestCase(100, false)]
        [TestCase(10, false)]
        public void TestCacheFuntionConcurrencyPerf(int numberOfConcurrentTasks, bool shouldReloadInBackground)
        {
            var key = Guid.NewGuid().ToString();
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheFuntionConcurrency));
            var counter = 0;
            Parallel.For(0, numberOfConcurrentTasks, i =>
            {
                cache.GetAutoReloadDataWithInterval(
                    key,
                    async () =>
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                        Interlocked.Increment(ref counter);
                        return i;
                    },
                    TimeSpan.FromDays(1),
                    TimeSpan.FromHours(1),
                    shouldReloadInBackground: shouldReloadInBackground);
            });

            counter.Should().Be(1);
        }
    }
}
