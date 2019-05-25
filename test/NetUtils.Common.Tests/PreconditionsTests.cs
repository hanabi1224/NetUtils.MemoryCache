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
    }
}
