using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Present an assembly's resources as a file system.
    /// </summary>
    /// <remarks><para>Exposes Managed Resources only.</para></remarks>
    /// <seealso cref="FlatFileSystem" />
    public class AssemblyFileSystem : FlatFileSystem
    {
        /// <summary>
        ///     The assembly to access resources of.
        /// </summary>
        private readonly Assembly assembly;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyFileSystem"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to access resources of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null" />.</exception>
        public AssemblyFileSystem(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Name = assembly.FullName;
        }

        private static IOException FileSystemIsReadOnly() => new IOException("This filesystem is read-only");

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetAttributesImpl(UPath path, FileAttributes attributes) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetCreationTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetLastAccessTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetLastWriteTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override bool CanWatchImpl(UPath path) => false;

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path) => null;

        /// <inheritdoc />
        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            if (FileExists(path))
            {
                return FileAttributes.ReadOnly;
            }
            if (DirectoryExists(path))
            {
                return FileAttributes.Directory | FileAttributes.ReadOnly;
            }
            throw FileSystemExceptionHelper.NewDirectoryNotFoundException(path);
        }

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path) => DefaultFileTime;

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path)
        {
            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
            using (Stream strm = assembly.GetManifestResourceStream(path.ToRelative().FullName))
            {
                return strm.Length;
            }
        }

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path) => DefaultFileTime;

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path) => DefaultFileTime;

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            if (mode != FileMode.Open || ((access & FileAccess.Write) != 0))
            {
                throw FileSystemIsReadOnly();
            }

            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
            return assembly.GetManifestResourceStream(path.ToRelative().FullName);
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath) => innerPath;

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path) => path.FullName;

        /// <inheritdoc />
        protected override IEnumerable<UPath> Paths => assembly.GetManifestResourceNames().Select(n => new UPath(n));
    }
}
