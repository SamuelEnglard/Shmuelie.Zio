using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Zio;

namespace Shmuelie.Zio.FileSystems.Tests
{
    [TestFixture]
    [TestOf(typeof(FlatFileSystem))]
    public class FlatFileSystemTests
    {
        private static readonly UPath[] paths = { "/a.txt", "/a/b.txt", "/a/c.text" };

        private class TestFlatFS : FlatFileSystem
        {
            protected override IEnumerable<UPath> Paths { get; }

            public TestFlatFS(IEnumerable<UPath> paths) => Paths = paths;

            protected override UPath ConvertPathFromInternalImpl(string innerPath) => throw new NotImplementedException();
            protected override string ConvertPathToInternalImpl(UPath path) => throw new NotImplementedException();
            protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => throw new NotImplementedException();
            protected override void CreateDirectoryImpl(UPath path) => throw new NotImplementedException();
            protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) => throw new NotImplementedException();
            protected override void DeleteFileImpl(UPath path) => throw new NotImplementedException();
            protected override FileAttributes GetAttributesImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetCreationTimeImpl(UPath path) => throw new NotImplementedException();
            protected override long GetFileLengthImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetLastAccessTimeImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetLastWriteTimeImpl(UPath path) => throw new NotImplementedException();
            protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) => throw new NotImplementedException();
            protected override void MoveFileImpl(UPath srcPath, UPath destPath) => throw new NotImplementedException();
            protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) => throw new NotImplementedException();
            protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => throw new NotImplementedException();
            protected override void SetAttributesImpl(UPath path, FileAttributes attributes) => throw new NotImplementedException();
            protected override void SetCreationTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override void SetLastAccessTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override void SetLastWriteTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override IFileSystemWatcher WatchImpl(UPath path) => throw new NotImplementedException();
        }

        [TestCaseSource(typeof(FlatFileSystemTests), nameof(DirectoryExistsTestData))]
        public bool DirectoryExistsTest(UPath testPath) => new TestFlatFS(paths).DirectoryExists(testPath);

        public static IEnumerable<TestCaseData> DirectoryExistsTestData()
        {
            yield return new TestCaseData(new UPath("/a")).Returns(true);
            yield return new TestCaseData(new UPath("/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/b.txt")).Returns(false);
            yield return new TestCaseData(UPath.Root).Returns(true);
            yield return new TestCaseData(new UPath(null)).Returns(false);
        }

        [Test]
        public void DirectoryExistsTest() => Assert.That(() => new TestFlatFS(paths).DirectoryExists(UPath.Empty), Throws.ArgumentException);

        [TestCaseSource(typeof(FlatFileSystemTests), nameof(FileExistsTestData))]
        public bool FileExistsTest(UPath testPath) => new TestFlatFS(paths).FileExists(testPath);

        public static IEnumerable<TestCaseData> FileExistsTestData()
        {
            yield return new TestCaseData(new UPath("/a")).Returns(false);
            yield return new TestCaseData(new UPath("/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/c.text")).Returns(true);
            yield return new TestCaseData(new UPath("/a/b.txt")).Returns(true);
            yield return new TestCaseData(new UPath(null)).Returns(false);
        }

        [Test]
        public void FileExistsTest() => Assert.That(() => new TestFlatFS(paths).FileExists(UPath.Empty), Throws.ArgumentException);

        [TestCaseSource(typeof(FlatFileSystemTests), nameof(EnumeratePathsData))]
        public int EnumeratePaths(UPath testPath, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) => new TestFlatFS(paths).EnumeratePaths(testPath, searchPattern, searchOption, searchTarget).Count();

        public static IEnumerable<TestCaseData> EnumeratePathsData()
        {
            yield return new TestCaseData(UPath.Root, "*.txt", SearchOption.AllDirectories, SearchTarget.File).Returns(2);
            yield return new TestCaseData(UPath.Root, "*.txt", SearchOption.TopDirectoryOnly, SearchTarget.File).Returns(1);
        }
    }
}
