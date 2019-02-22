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
        [Test]
        public async Task TestCacheDelete_DataShouldBeDisposedAsync()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_DataShouldBeDisposedAsync));
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
        public async Task TestCacheDelete_LazyDataShouldBeDisposedAsync()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestCacheDelete_LazyDataShouldBeDisposedAsync));
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
    }
}
