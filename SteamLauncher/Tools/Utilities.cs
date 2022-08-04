using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace SteamLauncher.Tools
{
    public static class Utilities
    {
        public static Assembly LoadAssembly(string path)
        {
            Assembly assembly;
#if NETCOREAPP
            assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
#else
            assembly = System.Reflection.Assembly.LoadFile(path);
#endif
            return assembly;
        }

        //public static Assembly AssemblyResolveHandler(object source, ResolveEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(e.Name) || e.RequestingAssembly == null)
        //        return null;

        //    var name = new AssemblyName(e.Name);
        //    if (name.Name.ToLower().EndsWith("resources"))
        //        return null;

        //    var basePath = Path.GetDirectoryName(e.RequestingAssembly.Location);

        //    if (string.IsNullOrWhiteSpace(basePath))
        //        return null;

        //    if (name.Name.ToLower() == "system.management")
        //    {
                
        //    }

        //    return null;
        //}

        /// <summary>
        /// Handles cases where the loading of an assembly fails. LaunchBox depends upon 'System.Management' but BigBox
        /// does not, so whenever the plugin attempts to reference it while running under BigBox, it can't be found (as
        /// SteamLauncher does not include it). This locates the 'System.Management' assembly that is already included
        /// with LaunchBox and loads it into the default context. NOTE: This is no longer being used to load
        /// 'System.Management' after migrating code to use 'Microsoft.Management.Infrastructure' which is now included
        /// with SteamLauncher.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Assembly ResolvingHandler(AssemblyLoadContext context, AssemblyName name)
        {
            // Removed after switching over to 'Microsoft.Management.Infrastructure'
            //if (name.Name?.ToLower() == "system.management")
            //{
            //    Logger.Info("Resolving 'System.Management' dependency");

            //    foreach (var a in FindAssembliesInLaunchBoxDir("System.Management.dll", true, true).Reverse().ToList())
            //    {
            //        // If the 'System.Management.dll' assembly references 'System.Runtime', it is the correct assembly.
            //        var findReference = "system.runtime";

            //        // If you include publicKey in the call to 'CheckAssemblyUnload', it must match the exact assembly.
            //        // If you instead set it to null, it should be able to load a compatible assembly even if it is not
            //        // the exact version the plugin references.
            //        //var publicKey = name.FullName.Split("PublicKeyToken=").LastOrDefault();
            //        string publicKey = null;

            //        var weakRef = CheckAssemblyUnload(out var isCorrectRef, a, new[] {findReference}, publicKey);
            //        for (var i = 0; i < 8 && weakRef.IsAlive; i++)
            //        {
            //            GC.Collect();
            //            GC.WaitForPendingFinalizers();
            //        }

            //        if (weakRef.IsAlive)
            //            Logger.Error("Failed to unload assembly 'System.Management' from disposable LoadContext.");

            //        if (isCorrectRef)
            //        {
            //            var loadedAssembly = Utilities.LoadAssembly(a);
            //            return loadedAssembly;
            //        }
            //    }
            //}

            return null;
        }

        /// <summary>
        /// Loads an assembly, checks if the assembly's properties match those provided, and assigns the result to
        /// <paramref name="isCorrectRef"/>. The assembly is unloaded before the function returns.
        /// </summary>
        /// <param name="isCorrectRef">The result is assigned to this out argument.</param>
        /// <param name="assemblyPath">The path to the assembly to load.</param>
        /// <param name="matchRefs">A list of assembly names to be checked against the loaded assembly's references.
        /// Will be ignored if null or empty.</param>
        /// <param name="matchPubKey">A public key value to be checked against the loaded assemblies public key value.
        /// Will be ignored if null or empty.</param>
        /// <returns>A WeakReference value that can be checked to ensure that the assembly loaded inside of the
        /// function is definitely unloaded.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CheckAssemblyUnload(out bool isCorrectRef, 
                                                         string assemblyPath, 
                                                         IReadOnlyCollection<string> matchRefs, 
                                                         string matchPubKey)
        {
            isCorrectRef = false;
            if (string.IsNullOrEmpty(matchPubKey) && matchRefs.IsNullOrEmpty())
                throw new ArgumentException($"One or both of the following arguments must contain a valid, non-null " +
                                            $"value: '{nameof(matchRefs)}', '{nameof(matchPubKey)}'");

            var loadContext = new DisposableAssemblyLoadContext();
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var publicKeyToken = assembly.FullName?.Split("PublicKeyToken=").LastOrDefault();
            if (!string.IsNullOrEmpty(matchPubKey) && (matchPubKey == publicKeyToken))
                isCorrectRef = true;

            if (!matchRefs.IsNullOrEmpty())
            {
                matchRefs = matchRefs.Select(x => x.ToLower().TrimStrEnd(".dll")).ToList();
                var referencedAssemblies = assembly.GetReferencedAssemblies().Select(x => x.Name.ToLower());
                isCorrectRef = referencedAssemblies.ContainsAllItems(matchRefs);
            }

            loadContext.Unload();
            return new WeakReference(loadContext);
        }

        /// <summary>
        /// Scans the LaunchBox directory to locate the designated assembly.
        /// </summary>
        /// <param name="name">Name of the assembly that is to be searched for ('*' and '?' are supported).</param>
        /// <param name="recursive">By default, the LaunchBox directory is searched recursively. If this value is set
        /// to false, only the base LaunchBox directory and its child 'Core' directory will be searched (or only the
        /// 'Core' directory if <paramref name="useCoreAsBaseDir"/> is true.</param>
        /// <param name="useCoreAsBaseDir">If true, 'LaunchBox\Core' will be treated as the base directory.</param>
        /// <returns>List of paths that match the search term or an empty list if no matches are found.</returns>
        public static IList<string> FindAssembliesInLaunchBoxDir(string name, bool recursive=true, bool useCoreAsBaseDir=false)
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var coreDir = Path.Join(Info.LaunchBoxDir, "Core");
            var baseDir = useCoreAsBaseDir ? coreDir : Info.LaunchBoxDir;
            if (useCoreAsBaseDir && !Directory.Exists(coreDir))
                throw new DirectoryNotFoundException($"The directory '{coreDir}' does not exist.");

            if (recursive)
                return Directory.GetFiles(baseDir, name, searchOption);

            var results = Directory.GetFiles(baseDir, name, searchOption).ToList();
            if (!useCoreAsBaseDir)
                results.AddRange(Directory.GetFiles(Info.LaunchBoxDir, name, searchOption));

            return results;
        }


        /// <summary>
        /// Reads an embedded resource stream from an assembly.
        /// </summary>
        /// <typeparam name="TSource">The type of any class in the assembly you want to read the resource
        /// from.</typeparam>
        /// <param name="embeddedFilename">The filename of the resource you want to read.</param>
        /// <returns></returns>
        public static string ReadManifestData<TSource>(string embeddedFilename) where TSource : class
        {
            var assembly = typeof(TSource).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().First(
                s => s.EndsWith(embeddedFilename, StringComparison.CurrentCultureIgnoreCase));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load manifest resource stream.");
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Specifies whether the provided path is an absolute path including root directory information (as opposed to
        /// a relative path).
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the provided path is an absolute path; otherwise, false.</returns>
        public static bool IsAbsolutePath(string path)
        {
            // New .NETCore version that works cross platform (tested and verified)
            return Path.IsPathFullyQualified(path);

            // Old non-.NETCore, non-CrossPlatform version
            //return !string.IsNullOrWhiteSpace(path) &&
            //       path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1 &&
            //       Path.IsPathRooted(path) && 
            //       !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// If the provided path is a relative path, this function converts it into an absolute path (it assumes the
        /// path is relative to the LaunchBox directory). If the provided path is already absolute, it returns the path
        /// unchanged.
        /// </summary>
        /// <param name="path">The string value to convert to an absolute path.</param>
        /// <returns>A new string representing an absolute path value.</returns>
        public static string GetAbsolutePath(string path)
        {
            if (!IsAbsolutePath(path))
                return Path.GetFullPath(Path.Combine(Info.LaunchBoxDir, path));

            return path;
        }

        /// <summary>
        /// If the provided path is an absolute path, this function converts it into a path relative to
        /// <paramref name="workingDir"/>. If <paramref name="workingDir"/> is not provided, the current working
        /// directory is used. If <paramref name="absolutePath"/> is not an absolute path, it returns the path
        /// unchanged.
        /// </summary>
        /// <param name="absolutePath">The path to convert to a relative path.</param>
        /// <param name="workingDir">The base working directory path to calculate the relative path from.</param>
        /// <returns>A relative path string value.</returns>
        public static string GetRelativePath(string absolutePath, string workingDir = null)
        {
            if (string.IsNullOrEmpty(workingDir))
                workingDir = Directory.GetCurrentDirectory();

            if (!IsAbsolutePath(absolutePath))
                return absolutePath;

            var result = string.Empty;
            int offset;

            // Simple case: the file is inside of the working directory.
            if (absolutePath.StartsWith(workingDir))
            {
                return absolutePath.Substring(workingDir.Length + 1);
            }

            // Hard case: have to back out of the working directory.
            var baseDirs = workingDir.Split(':', '\\', '/');
            var fileDirs = absolutePath.Split(':', '\\', '/');

            // if we failed to split (empty strings?) or the drive letter does not match
            if (baseDirs.Length <= 0 || fileDirs.Length <= 0 || baseDirs[0] != fileDirs[0])
            {
                // can't create a relative path between separate hard drives/partitions.
                return absolutePath;
            }

            // skip all leading directories that match
            for (offset = 1; offset < baseDirs.Length; offset++)
            {
                if (baseDirs[offset] != fileDirs[offset])
                    break;
            }

            // back out of the working directory
            for (var i = 0; i < (baseDirs.Length - offset); i++)
            {
                result += "..\\";
            }

            // step into the file path
            for (var i = offset; i < fileDirs.Length - 1; i++)
            {
                result += fileDirs[i] + "\\";
            }

            // append the file
            result += fileDirs[fileDirs.Length - 1];

            return result;
        }

        /// <summary>
        /// Creates a backup of <paramref name="origFilePath"/> to <paramref name="backupFilePath"/>. If <paramref
        /// name="backupFilePath"/> already exists, is the same file size as <paramref name="origFilePath"/>, and has
        /// the same modified timestamp as <paramref name="origFilePath"/>, the existing backup will not be modified.
        /// </summary>
        /// <param name="origFilePath">The path of the file to create a backup of.</param>
        /// <param name="backupFilePath">The path of the file to be created as a backup.</param>
        /// <returns>True if a new backup was created or the old backup was replaced; otherwise, false.</returns>
        public static bool CreateFileBackup(string origFilePath, string backupFilePath)
        {
            var origFileInfo = new FileInfo(origFilePath);

            if (!origFileInfo.Exists)
                throw new FileNotFoundException($"Cannot backup file '{origFilePath}' because it does not exist.", origFilePath);

            if (FileSizeAndModifiedTimestampMatch(origFilePath, backupFilePath))
                return false;

            origFileInfo.CopyTo(backupFilePath, true);
            return true;
        }

        /// <summary>
        /// Checks if 2 files have matching modified timestamps and have identical file sizes.
        /// </summary>
        /// <param name="filePath1"></param>
        /// <param name="filePath2"></param>
        /// <returns>True if the files match; otherwise, false.</returns>
        public static bool FileSizeAndModifiedTimestampMatch(string filePath1, string filePath2)
        {
            var fileInfo1 = new FileInfo(filePath1);
            var fileInfo2 = new FileInfo(filePath2);

            return fileInfo1.Exists && 
                   fileInfo2.Exists && 
                   (fileInfo1.Length == fileInfo2.Length) && 
                   (fileInfo1.LastWriteTime == fileInfo2.LastWriteTime);
        }
    }
}