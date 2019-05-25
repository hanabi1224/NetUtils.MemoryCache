using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class LazyDisposableTests
    {
        private class Disposable : DisposableBase
        {
            protected override void DisposeResources()
            {
            }
        }

        [Test]
        public void EnsureValueDisposed()
        {
            var disposable = new Disposable();
            disposable.IsDisposed.Should().BeFalse();

            var lazyAsync = LazyUtils.ToLazy(() => disposable);
            lazyAsync.Value.Should().NotBeNull();
            if (lazyAsync is IDisposable disposableLazyAsync)
            {
                disposableLazyAsync.Dispose();
                disposable.IsDisposed.Should().BeTrue();
            }
            else
            {
                Assert.Fail("lazy should be disposable");
            }
        }

        [Test]
        public async Task AsyncLazy_EnsureValueDisposedAsync()
        {
            var disposable = new Disposable();
            disposable.IsDisposed.Should().BeFalse();

            var lazyAsync = LazyUtils.ToAsyncLazy(async () => disposable);
            (await lazyAsync.GetValueAsync().ConfigureAwait(false)).Should().NotBeNull();
            if (lazyAsync is IDisposable disposableLazyAsync)
            {
                disposableLazyAsync.Dispose();
                disposable.IsDisposed.Should().BeTrue();
            }
            else
            {
                Assert.Fail("lazy should be disposable");
            }
        }

        [Test]
        public void EnsureValueDisposed_Collection()
        {
            var disposables = new List<Disposable> { new Disposable(), new Disposable() };
            disposables.ForEach(_ => _.IsDisposed.Should().BeFalse());

            var lazyAsync = LazyUtils.ToLazy(() => disposables);
            lazyAsync.Value.Should().NotBeNull();
            if (lazyAsync is IDisposable disposableLazyAsync)
            {
                disposableLazyAsync.Dispose();
                disposables.ForEach(_ => _.IsDisposed.Should().BeTrue());
            }
            else
            {
                Assert.Fail("lazy should be disposable");
            }
        }

        [Test]
        public async Task AsyncLazy_EnsureValueDisposed_CollectionAsync()
        {
            var disposables = new List<Disposable> { new Disposable(), new Disposable() };
            disposables.ForEach(_ => _.IsDisposed.Should().BeFalse());

            var lazyAsync = LazyUtils.ToAsyncLazy(async () => disposables);
            (await lazyAsync.GetValueAsync().ConfigureAwait(false)).Should().NotBeNull();
            if (lazyAsync is IDisposable disposableLazyAsync)
            {
                disposableLazyAsync.Dispose();
                disposables.ForEach(_ => _.IsDisposed.Should().BeTrue());
            }
            else
            {
                Assert.Fail("lazy should be disposable");
            }
        }

        [Test]
        public void EnsureValueDisposed_NotCreated()
        {
            var disposable = new Disposable();
            disposable.IsDisposed.Should().BeFalse();

            var lazyAsync = LazyUtils.ToLazy(() => disposable);
            if (lazyAsync is IDisposable disposableLazyAsync)
            {
                disposableLazyAsync.Dispose();
                disposable.IsDisposed.Should().BeFalse();
            }
            else
            {
                Assert.Fail("lazy should be disposable");
            }
        }
    }
}
