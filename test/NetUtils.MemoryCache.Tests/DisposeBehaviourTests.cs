using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class DisposeBehaviourTests
    {
        [Test]
        public async Task TestCacheDelete_DataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_DataShouldBeDisposedAsync));
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = new DummyDisposable(() => { isDisposed = true; });
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            isDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_CollectionDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_DataShouldBeDisposedAsync));
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = new DummyDisposable(() => { isDisposed = true; });
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, new[] { data }, TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            isDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_LazyDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_LazyDataShouldBeDisposedAsync));
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = LazyUtils.ToLazy(() => new DummyDisposable(() => { isDisposed = true; }));
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            Console.WriteLine(data.Value.GetHashCode());
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            isDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_LazyCollectionDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_LazyDataShouldBeDisposedAsync));
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var isDisposed = false;
            var data = LazyUtils.ToLazy(() => new[] { new DummyDisposable(() => { isDisposed = true; }) });
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            Console.WriteLine(data.Value.GetHashCode());
            cache.TryDeleteKey(key).Should().BeTrue();
            isDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            isDisposed.Should().Be(true);
        }
    }
}
