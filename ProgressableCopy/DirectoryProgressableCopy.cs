using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProgressableCopy
{
    public class DirectoryProgressableCopy : ProgressableCopyBase, IProgressableCopy
    {
        private long _copiedBytes = 0;
        private DirectoryInfo _destinationInfo;
        private int _fileCount = 0;
        private List<FileProgressableCopy> _files = new List<FileProgressableCopy>();
        private int _folderCount = 0;
        private List<DirectoryProgressableCopy> _folders = new List<DirectoryProgressableCopy>();
        private bool _populated = false;
        private DirectoryInfo _sourceInfo;
        private long _totalBytes = 0;

        #region Constructors

        public DirectoryProgressableCopy() : this(null, null) { }
        public DirectoryProgressableCopy(string sourcePath) : this(sourcePath, null) { }
        public DirectoryProgressableCopy(string sourcePath, string destinationPath) : base(sourcePath, destinationPath) { }

        #endregion

        #region Properties

        public override bool DestinationExists
        {
            get { return _destinationInfo != null ? _destinationInfo.Exists : false; }
        }

        public override string DestinationPath
        {
            get
            {
                return _destinationInfo != null ? _destinationInfo.FullName : null;
            }
            set
            {
                try
                {
                    _destinationInfo = new DirectoryInfo(value);
                }
                catch (Exception)
                {
                    _destinationInfo = null;
                }
                finally
                {
                    Reset();
                }
            }
        }

        public int FileCount
        {
            get
            {
                Populate();
                return _fileCount;
            }
        }

        public List<FileProgressableCopy> Files
        {
            get
            {
                Populate();
                return _files;
            }
        }

        public int FolderCount
        {
            get
            {
                Populate();
                return _folderCount;
            }
        }

        public List<DirectoryProgressableCopy> Folders
        {
            get
            {
                Populate();
                return _folders;
            }
        }

        public override bool SourceExists
        {
            get { return _sourceInfo != null ? _sourceInfo.Exists : false; }
        }

        public override string SourcePath
        {
            get
            {
                return _sourceInfo != null ? _sourceInfo.FullName : null;
            }
            set
            {
                try
                {
                    _sourceInfo = new DirectoryInfo(value);
                }
                catch (Exception)
                {
                    _sourceInfo = null;
                }
                finally
                {
                    Reset();
                }
            }
        }

        public override long TotalBytes
        {
            get
            {
                Populate();
                return _totalBytes;
            }
        }

        #endregion

        #region Instance Methods

        public override void Copy()
        {
            try
            {
                base.Copy();
            }
            catch (Exception ex)
            {
                RaiseCompleted(this, new CompletedEventArgs() { Successful = false, Exception = ex });
                return;
            }
            
            Populate();

            lock (_lock)
            {
                foreach (var file in _files)
                {
                    if (!RunCopier(file))
                        return;
                }
                foreach (var folder in _folders)
                {
                    if (folder.CreateDirectory && !folder.DestinationExists)
                        Directory.CreateDirectory(folder.DestinationPath);

                    if (!RunCopier(folder))
                        return;
                }
                RaiseCompleted(this, new CompletedEventArgs() { Successful = true });
            }
        }

        #endregion

        #region Helper Methods
        
        private void Populate()
        {
            if (_populated || _sourceInfo == null || _destinationInfo == null) return;

            lock (_lock)
            {
                foreach (FileInfo file in _sourceInfo.GetFiles())
                {
                    FileProgressableCopy sfc = new FileProgressableCopy(file.FullName, Path.Combine(DestinationPath, file.Name)) 
                    {
                        CreateDirectory = true,
                        Overwrite = this.Overwrite
                    };
                    _files.Add(sfc);
                    _fileCount++;
                    _totalBytes += file.Length;
                }

                foreach (DirectoryInfo directory in _sourceInfo.GetDirectories())
                {
                    DirectoryProgressableCopy dc = new DirectoryProgressableCopy(directory.FullName, Path.Combine(DestinationPath, directory.Name))
                    {
                        CreateDirectory = this.CreateDirectory,
                        Overwrite = this.Overwrite
                    };
                    _folders.Add(dc);
                    _folderCount++;
                    _fileCount += dc.FileCount;
                    _folderCount += dc.FolderCount;
                    _totalBytes += dc.TotalBytes;
                }

                _populated = true;
            }
        }

        private void Reset()
        {
            if (_populated)
            {
                lock (_lock)
                {
                    _files.Clear();
                    _folders.Clear();
                    _fileCount = 0;
                    _folderCount = 0;
                    _copiedBytes = 0;
                    _totalBytes = 0;
                    _populated = false;
                }
            }
        }

        private bool RunCopier(IProgressableCopy copier)
        {
            long offset = _copiedBytes;
            BytesChangedEventArgs args = new BytesChangedEventArgs()
            {
                SourcePath = copier.SourcePath,
                DestinationPath = copier.DestinationPath,
                CopiedBytes = _copiedBytes,
                TotalBytes = _totalBytes
            };
            copier.BytesChanged += (s, e) =>
            {
                _copiedBytes = offset + e.CopiedBytes;
                args.SourcePath = e.SourcePath;
                args.DestinationPath = e.DestinationPath;
                args.CopiedBytes = _copiedBytes;
                RaiseBytesChanged(this, args);
            };
            copier.Completed += (s, e) =>
            {
                if (!e.Successful)
                {
                    Console.WriteLine("ERROR while copying {0}: {1}", (s as IProgressableCopy).SourcePath, e.Exception.Message);
                }
            };
            copier.Copy();
            return true;
        }

        #endregion
    }

}
