using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class DateTimeOffsetUtilsTests
    {
        [Test]
        [TestCaseSource(nameof(GetTestAddSafeCases))]
        public void TestAddSafe(DateTimeOffset time, TimeSpan valueToAdd, DateTimeOffset expected)
        {
            time.AddSafe(valueToAdd).Should().Be(expected);
        }

        [Test]
        public void TestAddSafe_OverflowMax()
        {
            var time = DateTimeOffset.Parse("2018-01-01T00:00:00Z");
            time.AddSafe(TimeSpan.MaxValue).Should().Be(DateTimeOffset.MaxValue);
        }

        [Test]
        public void TestAddSafe_OverflowMin()
        {
            var time = DateTimeOffset.Parse("2018-01-01T00:00:00Z");
            time.AddSafe(TimeSpan.MinValue).Should().Be(DateTimeOffset.MinValue);
        }

        public static IEnumerable<TestCaseData> GetTestAddSafeCases()
        {
            var time = DateTimeOffset.Parse("2018-01-01T00:00:00Z");
            yield return new TestCaseData(time, TimeSpan.FromDays(1), DateTimeOffset.Parse("2018-01-02T00:00:00Z"));
            yield return new TestCaseData(time, TimeSpan.FromDays(-1), DateTimeOffset.Parse("2017-12-31T00:00:00Z"));
            yield return new TestCaseData(time, TimeSpan.Zero, time);
        }
    }
}
