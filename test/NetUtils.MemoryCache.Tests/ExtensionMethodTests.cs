using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
        [Test]
        public async Task TestGetData_GenericAsync()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestGetData_GenericAsync));
            var key = Guid.NewGuid().ToString();

            var data = (1, 2, 3);
            cache.SetData(key, data, TimeSpan.FromMilliseconds(100));
            var dataRef = cache.GetData<(int, int, int)>(key);
            dataRef.Should().NotBeNull();
            dataRef.Should().Be(data);

            await Task.Delay(TimeSpan.FromMilliseconds(101));

            cache.GetData<(int, int, int)>(key).Should().Be(default((int, int, int)));
        }

        [Test]
        public void TestGetData_Generic_InvalidCast()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestGetData_Generic_InvalidCast));
            var key = Guid.NewGuid().ToString();

            var data = (1, 2, 3);
            cache.SetData(key, data, TimeSpan.MaxValue);

            var dataRef = cache.GetData<(int, int, int)>(key);
            dataRef.Should().NotBeNull();
            dataRef.Should().Be(data);

            var dataRefInvalid = cache.GetData<string>(key);
            dataRefInvalid.Should().Be(default(string));

            dataRef = cache.GetData<(int, int, int)>(key);
            dataRef.Should().NotBeNull();
            dataRef.Should().Be(data);
        }

        [Test]
        public async Task TestGetData_NonGenericAsync()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(TestGetData_NonGenericAsync));
            var key = Guid.NewGuid().ToString();

            var data = (1, 2, 3);
            cache.SetData(key, data, TimeSpan.FromMilliseconds(100));
            cache.GetData(key).Should().NotBeNull();

            await Task.Delay(TimeSpan.FromMilliseconds(101));

            cache.GetData(key).Should().BeNull();
        }
    }
}
