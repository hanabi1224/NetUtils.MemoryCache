
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NetUtils.MemoryCache.Utils;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class HttpClientCacheTests
    {
        [Ignore("Flacky")]
        [Test]
        public async Task HttpGetETagCacheTestsAsync()
        {
            var cache = MemoryCache.GetNamedInstance(nameof(HttpGetETagCacheTestsAsync));
            using (var client = new HttpClient())
            {
                var baseUrl = $"https://httpbin.org/etag/";
                var etag = Guid.NewGuid();

                int counter1 = 0;
                int counter2 = 0;

                var lazyResponse = LazyUtils.ToLazy(async () =>
                {
                    Interlocked.Increment(ref counter1);
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{etag}");
                    return await client.SendAsync(request).ConfigureAwait(false);
                });
                cache.GetAutoReloadDataWithCache(
                    baseUrl,
                    async () =>
                    {
                        Interlocked.Increment(ref counter2);
                        return await lazyResponse.Value.Content.ReadAsStringAsync().ConfigureAwait(false);
                    },
                    () => Task.FromResult(lazyResponse.Value.Headers.ETag?.Tag),
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMilliseconds(100));
                counter1.Should().Be(1);
                counter2.Should().Be(1);

                lazyResponse = LazyUtils.ToLazy(async () =>
                {
                    Interlocked.Increment(ref counter1);
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{etag}");
                    return await client.SendAsync(request).ConfigureAwait(false);
                });
                cache.GetAutoReloadDataWithCache(
                    baseUrl,
                    async () =>
                    {
                        Interlocked.Increment(ref counter2);
                        return await lazyResponse.Value.Content.ReadAsStringAsync().ConfigureAwait(false);
                    },
                    () => Task.FromResult(lazyResponse.Value.Headers.ETag?.Tag),
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMilliseconds(100));
                counter1.Should().Be(1);
                counter2.Should().Be(1);

                await Task.Delay(150);
                lazyResponse = LazyUtils.ToLazy(async () =>
                {
                    Interlocked.Increment(ref counter1);
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{etag}");
                    return await client.SendAsync(request).ConfigureAwait(false);
                });
                cache.GetAutoReloadDataWithCache(
                    baseUrl,
                    async () =>
                    {
                        Interlocked.Increment(ref counter2);
                        return await lazyResponse.Value.Content.ReadAsStringAsync().ConfigureAwait(false);
                    },
                    () => Task.FromResult(lazyResponse.Value.Headers.ETag?.Tag),
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMilliseconds(100));
                counter1.Should().Be(1);
                counter2.Should().Be(1);

                etag = Guid.NewGuid();
                await Task.Delay(50);
                lazyResponse = LazyUtils.ToLazy(async () =>
                {
                    Interlocked.Increment(ref counter1);
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{etag}");
                    return await client.SendAsync(request).ConfigureAwait(false);
                });
                cache.GetAutoReloadDataWithCache(
                    baseUrl,
                    async () =>
                    {
                        Interlocked.Increment(ref counter2);
                        return await lazyResponse.Value.Content.ReadAsStringAsync().ConfigureAwait(false);
                    },
                    () => Task.FromResult(lazyResponse.Value.Headers.ETag?.Tag),
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMilliseconds(100));
                counter1.Should().Be(2);
                counter2.Should().Be(1);

                await Task.Delay(150);
                lazyResponse = LazyUtils.ToLazy(async () =>
                {
                    Interlocked.Increment(ref counter1);
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{etag}");
                    return await client.SendAsync(request).ConfigureAwait(false);
                });
                cache.GetAutoReloadDataWithCache(
                    baseUrl,
                    async () =>
                    {
                        Interlocked.Increment(ref counter2);
                        return await lazyResponse.Value.Content.ReadAsStringAsync().ConfigureAwait(false);
                    },
                    () => Task.FromResult(lazyResponse.Value.Headers.ETag?.Tag),
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMilliseconds(100));
                counter1.Should().Be(3);
                counter2.Should().Be(2);
            }
        }
    }
}
