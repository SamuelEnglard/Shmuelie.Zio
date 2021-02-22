using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Zio;
using Zio.FileSystems;
using System.IO;

namespace Shmuelie.Zio.FileSystems
{
    public class CompressedFileSystem : FlatFileSystem
    {
        private readonly ZipArchive archive;

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path)
        {
            //NOOP
        }

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path) => IfFileExists(path, entry => entry.Delete());

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            foreach (UPath filePath in this.EnumerateFiles(path, "*", isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                GetEntry(filePath).Delete();
            }
        }

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path) => IfFileExists(path, entry => entry.Length);

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path) => IfFileExists(path, _ => DefaultFileTime);

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path) => IfFileExists(path, entry => entry.LastWriteTime.UtcDateTime);

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path) => IfFileExists(path, _ => DefaultFileTime);

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => IfFileExists(srcPath, srcEntry => CopyEntry(destPath, overwrite, srcEntry));

        private void CopyEntry(UPath destPath, bool overwrite, ZipArchiveEntry srcEntry)
        {
            ZipArchiveEntry destEntry = GetEntry(destPath);
            if (destEntry is null)
            {
                destEntry = archive.CreateEntry(destPath.ToRelative().FullName);
            }
            else if (!overwrite)
            {
                throw FileSystemExceptionHelper.NewDestinationFileExistException(destPath);
            }
            using (Stream src = srcEntry.Open())
            using (Stream dest = destEntry.Open())
            {
                src.CopyTo(dest);
            }
        }

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath) => IfFileExists(srcPath, srcEntry => MoveEntry(destPath, srcEntry));

        private void MoveEntry(UPath destPath, ZipArchiveEntry srcEntry)
        {
            CopyEntry(destPath, false, srcEntry);
            srcEntry.Delete();
        }

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            if (!DirectoryExists(srcPath))
            {
                throw FileSystemExceptionHelper.NewDirectoryNotFoundException(srcPath);
            }
            List<IOException> exceptions = new List<IOException>();
            foreach (UPath srcFilePath in EnumeratePaths(srcPath, "*", SearchOption.AllDirectories, SearchTarget.File))
            {
                UPath destFilePath = destPath / srcFilePath.GetName();
                try
                {
                    MoveEntry(destFilePath, GetEntry(srcFilePath));
                }
                catch (IOException ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
        }

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) => IfFileExists(path, entry => entry.Open());

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => IfFileExists(srcPath, srcEntry =>
        {
            if (FileExists(destPath))
            {
                throw FileSystemExceptionHelper.NewDestinationFileExistException(destPath);
            }
            ZipArchiveEntry destEntry = GetEntry(destPath);
            if (!destBackupPath.IsNull)
            {
                CopyEntry(destBackupPath, false, destEntry);
            }
            MoveEntry(destPath, srcEntry);
        });

        /// <inheritdoc />
        protected override IEnumerable<UPath> Paths => archive.Entries.Select(e => new UPath(e.FullName));

        private ZipArchiveEntry GetEntry(UPath path) => archive.GetEntry(path.ToRelative().FullName);


        private void IfFileExists(UPath path, Action<ZipArchiveEntry> func)
        {
            if (FileExists(path))
            {
                func(GetEntry(path));
            }
            throw FileSystemExceptionHelper.NewFileNotFoundException(path);
        }

        private T IfFileExists<T>(UPath path, Func<ZipArchiveEntry, T> func)
        {
            if (FileExists(path))
            {
                return func(GetEntry(path));
            }
            throw FileSystemExceptionHelper.NewFileNotFoundException(path);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    archive?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
