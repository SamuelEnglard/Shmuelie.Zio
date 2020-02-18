using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Zio;

namespace Shmuelie.Zio.Glob
{
    /// <summary>
    ///     Represents a file backed by a <see cref="FileEntry"/>.
    /// </summary>
    /// <seealso cref="FileInfoBase"/>
    /// <seealso cref="FileEntry"/>
    public sealed class FileInfo : FileInfoBase
    {
        private readonly FileEntry fileEntry;

        /// <summary>
        ///     Creates a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        /// <param name="fileEntry">Backing <see cref="FileEntry"/>.</param>\
        /// <exception cref="ArgumentNullException"><paramref name="fileEntry"/> is <see langword="null"/>.</exception>
        public FileInfo(FileEntry fileEntry)
        {
            this.fileEntry = fileEntry ?? throw new ArgumentNullException(nameof(fileEntry));
        }

        /// <summary>
        ///     A string containing the name of the file.
        /// </summary>
        public override string Name => fileEntry.Name;

        /// <summary>
        ///     A string containing the full path of the file.
        /// </summary>
        public override string FullName => fileEntry.FullName;

        /// <summary>
        ///     The parent directory for the current file.
        /// </summary>
        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(fileEntry.Parent);
    }
}
