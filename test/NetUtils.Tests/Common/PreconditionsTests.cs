using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class PreconditionsTests
    {
        [Test]
        public void TestRequireNotNull()
        {
            var value = new MemoryStream();
            value.RequireNotNull(nameof(value)).Should().NotBeNull();

            value = default(MemoryStream);
            Action act = () => value.RequireNotNull(nameof(value));
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TestRequireNotNullOrEmpty()
        {
            new[] { 1, 2, 3 }.RequireNotNullOrEmpty("1");

            Action act = () => (null as List<int>).RequireNotNullOrEmpty("2");
            act.Should().Throw<ArgumentException>();

            act = () => new List<int>().RequireNotNullOrEmpty("3");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TestStringRequireNotNullOrEmpty()
        {
            "1".RequireNotNullOrEmpty("1");

            Action act = () => (null as string).RequireNotNullOrEmpty("2");
            act.Should().Throw<ArgumentException>();

            act = () => (string.Empty).RequireNotNullOrEmpty("2");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TestStringRequireNotNullOrWhiteSpace()
        {
            "1".RequireNotNullOrEmpty("1");

            Action act = () => (null as string).RequireNotNullOrWhiteSpace("2");
            act.Should().Throw<ArgumentException>();

            act = () => string.Empty.RequireNotNullOrWhiteSpace("2");
            act.Should().Throw<ArgumentException>();

            act = () => " ".RequireNotNullOrWhiteSpace("2");
            act.Should().Throw<ArgumentException>();

            act = () => "\t".RequireNotNullOrWhiteSpace("2");
            act.Should().Throw<ArgumentException>();

            act = () => "\n".RequireNotNullOrWhiteSpace("2");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TestRequireNonNegative()
        {
            0.RequireNonNegative(nameof(TestRequireNonNegative));
            (0d).RequireNonNegative(nameof(TestRequireNonNegative));
            1.RequireNonNegative(nameof(TestRequireNonNegative));
            (1d).RequireNonNegative(nameof(TestRequireNonNegative));

            Action act;
            act = () => (-1).RequireNonNegative(nameof(TestRequireNonNegative));
            act.Should().Throw<ArgumentException>();

            act = () => (-1d).RequireNonNegative(nameof(TestRequireNonNegative));
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TestRequirePositive()
        {
            1.RequirePositive(nameof(TestRequirePositive));
            (1d).RequirePositive(nameof(TestRequirePositive));

            Action act;
            act = () => (0).RequirePositive(nameof(TestRequirePositive));
            act.Should().Throw<ArgumentException>();

            act = () => (0d).RequirePositive(nameof(TestRequirePositive));
            act.Should().Throw<ArgumentException>();

            act = () => (-1).RequirePositive(nameof(TestRequirePositive));
            act.Should().Throw<ArgumentException>();

            act = () => (-1d).RequirePositive(nameof(TestRequirePositive));
            act.Should().Throw<ArgumentException>();
        }
    }
}
