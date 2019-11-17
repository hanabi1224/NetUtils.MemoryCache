using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class MemoryCacheInstanceTests
    {
        [Test]
        public void TestGetAutoReloadDataWithInterval_SetDataOnException()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            var obj = cache.GetAutoReloadDataWithInterval(key, () => new object(), TimeSpan.MaxValue, TimeSpan.MaxValue);
            obj.Should().NotBeNull();
            cache.GetAutoReloadDataWithInterval<object>(key, () => throw new InvalidOperationException("dummy"), TimeSpan.MaxValue, TimeSpan.MaxValue).Should().Be(obj);

            Action act = () => cache.GetAutoReloadDataWithInterval<object>(Guid.NewGuid().ToString(), () => throw new InvalidOperationException("dummy"), TimeSpan.MaxValue, TimeSpan.MaxValue);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void TestGetAutoReloadDataWithInterval_SetAsyncDataOnException()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            var obj = cache.GetAutoReloadDataWithInterval(key, async () => new object(), TimeSpan.MaxValue, TimeSpan.MaxValue);
            obj.Should().NotBeNull();
            cache.GetAutoReloadDataWithInterval<object>(key, async () => throw new InvalidOperationException("dummy"), TimeSpan.MaxValue, TimeSpan.MaxValue).Should().Be(obj);

            Action act = () => cache.GetAutoReloadDataWithInterval<object>(Guid.NewGuid().ToString(), async () => throw new InvalidOperationException("dummy"), TimeSpan.MaxValue, TimeSpan.MaxValue);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void TestGetAutoReloadDataWithCache_SetDataOnException()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            var obj = cache.GetAutoReloadDataWithCache(key, () => new object(), () => key, TimeSpan.MaxValue, TimeSpan.MaxValue);
            obj.Should().NotBeNull();
            cache.GetAutoReloadDataWithCache<object>(key, () => throw new InvalidOperationException("dummy"), () => key, TimeSpan.MaxValue, TimeSpan.MaxValue).Should().Be(obj);

            Action act = () => cache.GetAutoReloadDataWithCache<object>(Guid.NewGuid().ToString(), () => throw new InvalidOperationException("dummy"), () => key, TimeSpan.MaxValue, TimeSpan.MaxValue);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void TestGetAutoReloadDataWithCache_SetAsyncDataOnException()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            var obj = cache.GetAutoReloadDataWithCache(key, async () => new object(), async () => key, TimeSpan.MaxValue, TimeSpan.MaxValue);
            obj.Should().NotBeNull();
            cache.GetAutoReloadDataWithCache<object>(key, async () => throw new InvalidOperationException("dummy"), async () => key, TimeSpan.MaxValue, TimeSpan.MaxValue).Should().Be(obj);

            Action act = () => cache.GetAutoReloadDataWithCache<object>(Guid.NewGuid().ToString(), () => throw new InvalidOperationException("dummy"), async () => key, TimeSpan.MaxValue, TimeSpan.MaxValue);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void TestDeleteKey()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, new object(), TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            cache.TryDeleteKey(key).Should().BeFalse();
        }

        [Test]
        public async Task TestDeleteKey_DisposableAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromMilliseconds(200);
            var key = Guid.NewGuid().ToString();
            var data = new DummyDisposable();
            cache.SetData(key, data, TimeSpan.MaxValue);
            data.IsDisposed.Should().BeFalse();
            cache.TryDeleteKey(key).Should().BeTrue();
            cache.TryDeleteKey(key).Should().BeFalse();
            data.IsDisposed.Should().BeFalse();
            await Task.Delay(TimeSpan.FromMilliseconds(300));
            data.IsDisposed.Should().BeTrue();
        }

        ////[Test]
        ////public void TestDeleteKey_Disposable_NoDelay()
        ////{
        ////    var cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
        ////    cache.CleanInternal = TimeSpan.FromMilliseconds(0);
        ////    var key = Guid.NewGuid().ToString();
        ////    var isDisposed = false;
        ////    var data = new DummyDisposable(() => { isDisposed = true; });
        ////    cache.SetData(key, data, TimeSpan.MaxValue);
        ////    isDisposed.Should().BeFalse();
        ////    cache.TryDeleteKey(key).Should().BeTrue();
        ////    cache.TryDeleteKey(key).Should().BeFalse();
        ////    isDisposed.Should().BeTrue();
        ////}

        [Test]
        public async Task TestClearAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromMilliseconds(200);
            var key = Guid.NewGuid().ToString();
            var data = new DummyDisposable();
            cache.SetData(key, data, TimeSpan.MaxValue);
            cache.Size.Should().Be(1);
            data.IsDisposed.Should().BeFalse();
            cache.Clear();
            cache.Size.Should().Be(0);
            data.IsDisposed.Should().BeFalse();
            await Task.Delay(TimeSpan.FromMilliseconds(300));
            cache.CleanIfNeeded();
            data.IsDisposed.Should().BeTrue();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TestAutoReloadOnExceptionAsync(bool shouldReloadInBackground)
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            var key = Guid.NewGuid().ToString();
            var data = cache.GetAutoReloadDataWithInterval(key, () => 8, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(100), shouldReloadInBackground: shouldReloadInBackground);
            data.Should().Be(8);

            var exceptionCount = 0;
            Func<Task<int>> exceptionFunc = async () =>
            {
                await Task.Delay(RandomUtils.Random.Next(20) + 20).ConfigureAwait(false);
                Interlocked.Increment(ref exceptionCount);
                throw new InvalidOperationException();
            };

            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(150);
                using (new PerfMonitorScope(sw => Console.WriteLine($"[{i}] {sw.ElapsedMilliseconds}ms")))
                {
                    data = cache.GetAutoReloadDataWithInterval(key, exceptionFunc, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(100), shouldReloadInBackground: shouldReloadInBackground);
                }

                if (shouldReloadInBackground)
                {
                    await Task.Delay(80);
                    using (new PerfMonitorScope(sw => Console.WriteLine($"[{i}] {sw.ElapsedMilliseconds}ms")))
                    {
                        data = cache.GetAutoReloadDataWithInterval(key, exceptionFunc, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(100), shouldReloadInBackground: shouldReloadInBackground);
                    }
                }

                data.Should().Be(8);
                exceptionCount.Should().Be(i + 1);
            }

            await Task.Delay(150);
            using (new PerfMonitorScope(sw => Console.WriteLine($"{sw.ElapsedMilliseconds}ms")))
            {
                data = cache.GetAutoReloadDataWithInterval(key, () => 6, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(100), shouldReloadInBackground: shouldReloadInBackground);
            }

            if (shouldReloadInBackground)
            {
                await Task.Delay(80);
                data.Should().Be(8);
                using (new PerfMonitorScope(sw => Console.WriteLine($"{sw.ElapsedMilliseconds}ms")))
                {
                    data = cache.GetAutoReloadDataWithInterval(key, exceptionFunc, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(100), shouldReloadInBackground: shouldReloadInBackground);
                }
            }

            data.Should().Be(6);
        }

        ////[Test]
        ////[TestCase(true)]
        ////[TestCase(false)]
        ////public async Task TestGetDataOrCreate(bool shouldReloadInBackground)
        ////{
        ////    var cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
        ////    var key = Guid.NewGuid().ToString();
        ////    var data = cache.GetAutoReloadDataWithInterval(key, () => 8, TimeSpan.MaxValue, shouldReloadInBackground: shouldReloadInBackground);
        ////    data.Should().Be(8);

        ////    cache.SetData(key, 6, TimeSpan.FromMilliseconds(100));
        ////    data = cache.GetAutoReloadDataWithInterval(key, () => 8, TimeSpan.MaxValue, shouldReloadInBackground: shouldReloadInBackground);
        ////    data.Should().Be(6);

        ////    await Task.Delay(101);
        ////    data = cache.GetAutoReloadDataWithInterval(key, () => 8, TimeSpan.MaxValue, shouldReloadInBackground: shouldReloadInBackground);
        ////    if (shouldReloadInBackground)
        ////    {
        ////        data.Should().Be(6);
        ////        await Task.Delay(10);
        ////        data = cache.GetAutoReloadDataWithInterval(key, () => 8, TimeSpan.MaxValue, shouldReloadInBackground: shouldReloadInBackground);
        ////        data.Should().Be(8);
        ////    }
        ////    else
        ////    {
        ////        data.Should().Be(8);
        ////    }
        ////}
    }
}
