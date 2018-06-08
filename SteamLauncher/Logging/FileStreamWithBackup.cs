using System;
using System.IO;
using System.Text;

namespace SteamLauncher.Logging
{
    /// <summary>
    /// Adds functionality to FileStream, providing a more robust means of logging to a file; 
    /// adds support for setting max log file size, max # of sequential backup files, and more. 
    /// FileStream documentation: <inheritdoc cref="FileStream"/>
    /// </summary>
    public sealed class FileStreamWithBackup : FileStream
    {
        public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode)
            : base(path, BaseFileMode(mode), FileAccess.Write)
        {
            Init(path, maxFileLength, maxFileCount, mode);
        }

        public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share)
            : base(path, BaseFileMode(mode), FileAccess.Write, share)
        {
            Init(path, maxFileLength, maxFileCount, mode);
        }

        public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share, int bufferSize)
            : base(path, BaseFileMode(mode), FileAccess.Write, share, bufferSize)
        {
            Init(path, maxFileLength, maxFileCount, mode);
        }

        public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share, int bufferSize, bool isAsync)
            : base(path, BaseFileMode(mode), FileAccess.Write, share, bufferSize, isAsync)
        {
            Init(path, maxFileLength, maxFileCount, mode);
        }

        private string _fileDir;
        private string _fileBase;
        private string _fileExt;
        private int _fileDecimals;
        private int _nextFileIndex;

        public override bool CanRead { get; } = false;

        public override void Write(byte[] array, int offset, int count)
        {
            var actualCount = System.Math.Min(count, array.GetLength(0));
            if (Position + actualCount <= MaxFileLength)
            {
                base.Write(array, offset, count);
            }
            else
            {
                if (CanSplitData)
                {
                    var partialCount = (int)(System.Math.Max(MaxFileLength, Position) - Position);
                    base.Write(array, offset, partialCount);
                    offset += partialCount;
                    count = actualCount - partialCount;
                }
                else
                {
                    if (count > MaxFileLength)
                        throw new ArgumentOutOfRangeException(nameof(count));
                }
                BackupAndResetStream();
                Write(array, offset, count);
            }
        }

        public long MaxFileLength { get; private set; }
        public int MaxFileCount { get; private set; }
        public bool CanSplitData { get; set; }

        private void Init(string path, long maxFileLength, int maxFileCount, FileMode mode)
        {
            if (maxFileLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFileLength));
            if (maxFileCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFileCount));

            MaxFileLength = maxFileLength;
            MaxFileCount = maxFileCount;
            CanSplitData = true;

            var fullPath = Path.GetFullPath(path);
            _fileDir = Path.GetDirectoryName(fullPath);
            _fileBase = Path.GetFileNameWithoutExtension(fullPath);
            _fileExt = Path.GetExtension(fullPath);

            _fileDecimals = 1;
            var decimalBase = 10;
            while (decimalBase < MaxFileCount)
            {
                ++_fileDecimals;
                decimalBase *= 10;
            }

            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Truncate:
                    // Delete old files
                    for (var fileCount = 0; fileCount < MaxFileCount; ++fileCount)
                    {
                        string file = GetBackupFileName(fileCount);
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    break;

                default:
                    // Position file pointer to the last backup file
                    for (var fileCount = 0; fileCount < MaxFileCount; ++fileCount)
                    {
                        if (File.Exists(GetBackupFileName(fileCount)))
                            _nextFileIndex = fileCount + 1;
                    }

                    if (_nextFileIndex == MaxFileCount)
                        _nextFileIndex = 0;

                    Seek(0, SeekOrigin.End);
                    break;
            }
        }

        private void BackupAndResetStream()
        {
            Flush();
            File.Copy(Name, GetBackupFileName(_nextFileIndex), true);
            SetLength(0);

            ++_nextFileIndex;
            if (_nextFileIndex >= MaxFileCount)
                _nextFileIndex = 0;
        }

        private string GetBackupFileName(int index)
        {
            var formatStringBuilder = new StringBuilder();
            formatStringBuilder.AppendFormat("D{0}", _fileDecimals);
            var outputStringBuilder = new StringBuilder();

            if (_fileExt.Length > 0)
                outputStringBuilder.AppendFormat("{0}{1}{2}", _fileBase, index.ToString(formatStringBuilder.ToString()), _fileExt);
            else
                outputStringBuilder.AppendFormat("{0}{1}", _fileBase, index.ToString(formatStringBuilder.ToString()));

            return Path.Combine(_fileDir, outputStringBuilder.ToString());
        }

        private static FileMode BaseFileMode(FileMode mode)
        {
            return mode == FileMode.Append ? FileMode.OpenOrCreate : mode;
        }
    }
}
