using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class DisposableContainerTests
    {
        [Test]
        public void EnsureAllItemsDisposed()
        {
            var container = new DisposableContainer();
            var items = new List<DummyDisposable>();
            for (var i = 0; i < 10; i++)
            {
                var item = new DummyDisposable();
                items.Add(item);
                container.Register(item);
            }

            items.ForEach(_ => _.IsDisposed.Should().BeFalse());

            container.Dispose();
            container.IsDisposed.Should().BeTrue();
            items.ForEach(_ => _.IsDisposed.Should().BeTrue());
        }
    }
}
