using System;
using System.Threading.Tasks;
using FluentAssertions;
using NetUtils.MemoryCache.Utils;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class DisposeBehaviourTests
    {
        public class DummyDisposable : DisposableBase
        {
            private readonly Action _callbackOnDispose;

            public DummyDisposable(Action callbackOnDispose)
            {
                _callbackOnDispose = callbackOnDispose;
            }

            protected override void DisposeResources()
            {
                _callbackOnDispose?.Invoke();
            }
        }

        [Test]
        public async Task TestCacheDelete_DataShouldBeDisposed()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_DataShouldBeDisposed), CacheExpirePolicy.ExpireOnLastUpdate);
            cache.DataDisposeDelay = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = new DummyDisposable(() => { isDisposed = true; });
            var key = Guid.NewGuid().ToString();
            cache.TrySetData(key, data, TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.DataDisposeDelay.Add(TimeSpan.FromMilliseconds(500)));
            isDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_LazyDataShouldBeDisposed()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_LazyDataShouldBeDisposed), CacheExpirePolicy.ExpireOnLastUpdate);
            cache.DataDisposeDelay = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = LazyUtils.ToLazy(() => new DummyDisposable(() => { isDisposed = true; }));
            var key = Guid.NewGuid().ToString();
            cache.TrySetData(key, data, TimeSpan.MaxValue);
            Console.WriteLine(data.Value.GetHashCode());
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.DataDisposeDelay.Add(TimeSpan.FromMilliseconds(500)));
            isDisposed.Should().Be(true);
        }
    }
}
