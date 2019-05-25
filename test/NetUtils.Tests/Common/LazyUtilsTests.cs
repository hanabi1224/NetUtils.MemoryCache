using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class LazyUtilsTests
    {
        [Test]
        public void TestToLazy_NonDisposable()
        {
            var lazy = LazyUtils.ToLazy(() => 3);
            (lazy is IDisposable).Should().BeFalse();
        }

        [Test]
        public void TestToLazy_AsyncNonDisposable()
        {
            var lazy = LazyUtils.ToLazy(async () => 3);
            (lazy is IDisposable).Should().BeFalse();
        }

        [Test]
        public void TestToLazy_Disposable()
        {
            var lazy = LazyUtils.ToLazy(() => new MemoryStream());
            (lazy is IDisposable).Should().BeTrue();
        }

        [Test]
        public void TestToLazy_AsyncDisposable()
        {
            var lazy = LazyUtils.ToLazy(async () => new MemoryStream());
            (lazy is IDisposable).Should().BeTrue();
        }

        [Test]
        public void TestToLazy_Collection()
        {
            var lazy = LazyUtils.ToLazy(() => new List<int> { 1, 2, 3 });
            (lazy is IDisposable).Should().BeTrue();
        }

        [Test]
        public void TestToLazy_AsyncCollection()
        {
            var lazy = LazyUtils.ToLazy(async () => new List<MemoryStream> { new MemoryStream(), new MemoryStream(), new MemoryStream() });
            (lazy is IDisposable).Should().BeTrue();
        }

        [Test]
        public void TestToAsyncLazy_NonDisposable()
        {
            var lazy = LazyUtils.ToAsyncLazy(async () => 3);
            (lazy is IDisposable).Should().BeTrue();
        }

        [Test]
        public void TestToAsyncLazy_Disposable()
        {
            var lazy = LazyUtils.ToAsyncLazy(async () => new MemoryStream());
            (lazy is IDisposable).Should().BeTrue();
        }
    }
}
