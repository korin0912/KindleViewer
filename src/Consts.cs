using System;
using System.IO;

namespace KindleViewer
{
    public static class Consts
    {
        public static readonly DateTime UnknownDateTime = new DateTime(1900, 1, 1, 0, 0, 0);

        public static readonly string CacheFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}Amazon{Path.DirectorySeparatorChar}Kindle{Path.DirectorySeparatorChar}Cache";

        public static readonly string XMLCacheFilePath = $"{CacheFolderPath}{Path.DirectorySeparatorChar}KindleSyncMetadataCache.xml";

        public static readonly string CoverCacheFolderPath = $"{CacheFolderPath}{Path.DirectorySeparatorChar}covers";

        public static readonly string KindleCloudReaderUriPrefix = "https://read.amazon.co.jp/manga/";
    }
}
