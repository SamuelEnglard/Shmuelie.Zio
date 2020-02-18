using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Zio;

namespace Shmuelie.Zio.Glob
{
    /// <summary>
    ///     Represents a directory backed by a <see cref="DirectoryEntry"/>.
    /// </summary>
    /// <seealso cref="DirectoryInfoBase"/>
    /// <seealso cref="DirectoryEntry"/>
    public sealed class DirectoryInfo : DirectoryInfoBase
    {
        private readonly DirectoryEntry directoryEntry;

        /// <summary>
        ///     Creates a new instance of the <see cref="DirectoryInfo"/> class.
        /// </summary>
        /// <param name="directoryEntry">Backing <see cref="DirectoryEntry"/>.</param>\
        /// <exception cref="ArgumentNullException"><paramref name="directoryEntry"/> is <see langword="null"/>.</exception>
        public DirectoryInfo(DirectoryEntry directoryEntry)
        {
            this.directoryEntry = directoryEntry ?? throw new ArgumentNullException(nameof(directoryEntry));
        }

        /// <summary>
        ///     A string containing the name of the directory.
        /// </summary>
        public override string Name => directoryEntry.Name;

        /// <summary>
        ///     A string containing the full path of the directory.
        /// </summary>
        public override string FullName => directoryEntry.FullName;

        /// <summary>
        ///     The parent directory for the current directory.
        /// </summary>
        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(directoryEntry.Parent);

        /// <summary>
        ///     Enumerates all files and directories in the directory.
        /// </summary>
        /// <returns>Collection of files and directories</returns>
        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos() => directoryEntry.EnumerateEntries().Select(ConvertToZio);

        private static FileSystemInfoBase ConvertToZio(FileSystemEntry fileSystemEntry)
        {
            if (fileSystemEntry is DirectoryEntry directoryEntry)
            {
                return new DirectoryInfo(directoryEntry);
            }
            if (fileSystemEntry is FileEntry fileEntry)
            {
                return new FileInfo(fileEntry);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Returns an instance of <see cref="DirectoryInfoBase"/> that represents a subdirectory.
        /// </summary>
        /// <param name="path">The directory name.</param>
        /// <returns>Instance of <see cref="DirectoryInfoBase"/> even if directory does not exist.</returns>
        public override DirectoryInfoBase GetDirectory(string path) => new DirectoryInfo(new DirectoryEntry(directoryEntry.FileSystem, directoryEntry.Path / path));

        /// <summary>
        ///     Returns an instance of <see cref="FileInfoBase"/> that represents a file in the directory.
        /// </summary>
        /// <param name="path">The file name.</param>
        /// <returns>Instance of <see cref="FileInfoBase"/> even if file does not exist.</returns>
        public override FileInfoBase GetFile(string path) => new FileInfo(new FileEntry(directoryEntry.FileSystem, directoryEntry.Path / path));
    }
}
