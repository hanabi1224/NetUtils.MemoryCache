using System;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class DisposeBehaviourTests
    {
        [Test]
        public async Task TestCacheDelete_DataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var data = new DummyDisposable();
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            data.IsDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            data.IsDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_CollectionDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var data = new DummyDisposable();
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, new[] { data }, TimeSpan.MaxValue);
            cache.TryDeleteKey(key).Should().BeTrue();
            data.IsDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            data.IsDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_LazyDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var data = LazyUtils.ToLazy(() => new DummyDisposable());
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            Console.WriteLine(data.Value.GetHashCode());
            cache.TryDeleteKey(key).Should().BeTrue();
            data.Value.IsDisposed.Should().Be(false);
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            data.Value.IsDisposed.Should().Be(true);
        }

        [Test]
        public async Task TestCacheDelete_LazyCollectionDataShouldBeDisposedAsync()
        {
            ICacheInstance cache = MemoryCache.GetNamedInstance(Guid.NewGuid().ToString());
            cache.CleanInternal = TimeSpan.FromSeconds(1);
            var data = LazyUtils.ToLazy(() => new[] { new DummyDisposable() });
            var key = Guid.NewGuid().ToString();
            cache.SetData(key, data, TimeSpan.MaxValue);
            Console.WriteLine(data.Value.GetHashCode());
            cache.TryDeleteKey(key).Should().BeTrue();
            data.Value.ForEach(_ => _.IsDisposed.Should().Be(false));
            await Task.Delay(cache.CleanInternal.Add(TimeSpan.FromMilliseconds(500)));
            cache.CleanIfNeeded();
            data.Value.ForEach(_ => _.IsDisposed.Should().Be(true));
        }
    }
}
