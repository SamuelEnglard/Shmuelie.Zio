using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zio;
using Zio.FileSystems;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Represents a file system that is backed by list, not a tree, of files.
    /// </summary>
    /// <seealso cref="FileSystem" />
    public abstract class FlatFileSystem : FileSystem
    {
        /// <summary>
        ///     Gets the list of paths in this file system.
        /// </summary>
        /// <value>
        ///     The list of paths in this file system.
        /// </value>
        protected abstract IEnumerable<UPath> Paths
        {
            get;
        }

        /// <inheritdoc />
        protected sealed override bool FileExistsImpl(UPath path) => SearchPaths(path.GetDirectory().ToRelative(), path.GetName(), SearchOption.TopDirectoryOnly, SearchTarget.File).Any();

        /// <inheritdoc />
        protected sealed override bool DirectoryExistsImpl(UPath path) => SearchPaths(path.GetDirectory().ToRelative(), path.GetName(), SearchOption.TopDirectoryOnly, SearchTarget.Directory).Any();

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="searchPattern"/> is <see langword="null" />.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="path"/> does not exist.</exception>
        protected sealed override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            if (!DirectoryExists(path))
            {
                throw FileSystemExceptionHelper.NewDirectoryNotFoundException(path);
            }

            if (searchPattern is null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            return SearchPaths(path, searchPattern, searchOption, searchTarget);
        }

        private IEnumerable<UPath> SearchPaths(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            SearchPattern search = SearchPattern.Parse(ref path, ref searchPattern);
            var entries = new SortedSet<UPath>(UPath.DefaultComparerIgnoreCase);

            foreach (UPath resourceName in Paths)
            {
                if (resourceName.IsInDirectory(path.ToRelative(), searchOption == SearchOption.AllDirectories))
                {
                    if ((searchTarget == SearchTarget.Both || searchTarget == SearchTarget.File) && search.Match(resourceName))
                    {
                        entries.Add(resourceName);
                    }
                    UPath resourceDirectory = resourceName.GetDirectory();
                    if ((searchTarget == SearchTarget.Both || searchTarget == SearchTarget.Directory) && search.Match(resourceDirectory))
                    {
                        entries.Add(resourceDirectory);
                    }
                }
            }

            return entries;
        }
    }
}
