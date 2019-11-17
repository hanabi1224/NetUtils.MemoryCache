using System;
using System.Diagnostics;
using System.IO;

namespace NetUtils
{
    public class TempFile : DisposableBase
    {
        private const string FolderName = ".tempfiles";
        public static string TempFilesFolder { get; private set; } = string.Empty;

        static TempFile()
        {
            SetTempFilesFolder(Path.GetTempPath());
            Init();
        }

        private static void Init()
        {
            if (!Directory.Exists(TempFilesFolder))
            {
                Directory.CreateDirectory(TempFilesFolder);
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(TempFilesFolder, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        if (fi.LastWriteTimeUtc + TimeSpan.FromDays(1) < DateTime.UtcNow)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }
            }
        }

        public static void SetTempFilesFolder(string folder)
        {
            TempFilesFolder = Path.Combine(folder.RequireNotNullOrEmpty(nameof(folder)), FolderName);
            Init();
        }

        public TempFile(
            string fileExtension = "")
            : this(new byte[0], fileExtension)
        {
        }

        public TempFile(
            ReadOnlyMemory<byte> bytes,
            string fileExtension = "")
        {
            var normalizedExtension = string.IsNullOrEmpty(fileExtension) ? string.Empty : $".{fileExtension.TrimStart('.')}";
            FullPath = Path.Combine(TempFilesFolder, $"{Path.GetRandomFileName()}{normalizedExtension}");
            Trace.TraceInformation($"Creating temp file {FullPath}");

            if (bytes.Length > 0)
            {
                try
                {
                    File.WriteAllBytes(FullPath, bytes.ToArray());
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(TempFilesFolder);
                    File.WriteAllBytes(FullPath, bytes.ToArray());
                }
            }
        }

        public string FullPath { get; }

        public override string ToString() => FullPath;

        protected override void DisposeResources()
        {
            try
            {
                if (File.Exists(FullPath))
                {
                    Trace.TraceInformation($"Deleting temp file {FullPath}");
                    File.Delete(FullPath);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }
    }
}
