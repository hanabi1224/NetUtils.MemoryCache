using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class RandomUtilsTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task TestRandomness_MultiThreadingAsync(bool useThreadPool)
        {
            var counter = new ConcurrentDictionary<int, int>();

            const int Loop = 100000;
            const int NumOfBuckets = 100;
            var tasks = new List<Task>(Loop);
            foreach (var i in Enumerable.Range(0, Loop))
            {
                var task = Task.Run(() =>
                {
                    counter.AddOrUpdate(
                    RandomUtils.Random.Next(NumOfBuckets),
                    addValue: 1,
                    updateValueFactory: (key, oldValue) =>
                    {
                        return oldValue + 1;
                    });
                });
                if (useThreadPool)
                {
                    tasks.Add(task);
                }
                else
                {
                    await task.ConfigureAwait(false);
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var idealCount = (double)Loop / NumOfBuckets;
            var lowerBound = idealCount * 0.85;
            var upperBound = idealCount * 1.15;

            for (var i = 0; i < NumOfBuckets; i++)
            {
                var value = (double)counter[i];
                value.Should().BeGreaterOrEqualTo(lowerBound);
                value.Should().BeLessOrEqualTo(upperBound);
            }
        }
    }
}
