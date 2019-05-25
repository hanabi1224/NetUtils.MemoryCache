using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.Common.Tests
{
    [TestFixture]
    public class TempFileTests
    {
        [Test]
        public void TestTempFile_CreationWithNoData()
        {
            string path;
            using (var tempFile = new TempFile("wav"))
            {
                path = tempFile.FullPath;
                File.Exists(path).Should().BeFalse();
            }

            File.Exists(path).Should().BeFalse();
        }

        [Test]
        public void TestTempFile_CreationWithData()
        {
            string path;
            using (var tempFile = new TempFile(new byte[] { 1, 2, 3 }, "wav"))
            {
                path = tempFile.FullPath;
                File.Exists(path).Should().BeTrue();
            }

            File.Exists(path).Should().BeFalse();
        }
    }
}
