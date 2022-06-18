using System;
using System.IO;
using System.Collections.Generic;

namespace KindleViewer
{
    public class Kindle
    {
        public static readonly string CacheFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}Amazon{Path.DirectorySeparatorChar}Kindle{Path.DirectorySeparatorChar}Cache";

        public static readonly string XMLCacheFilePath = $"{CacheFolderPath}{Path.DirectorySeparatorChar}KindleSyncMetadataCache.xml";

        public static readonly string CoverCacheFolderPath = $"{CacheFolderPath}{Path.DirectorySeparatorChar}covers";

        public static readonly string KindleCloudReaderUriPrefix = "https://read.amazon.co.jp/manga/";

        private static Kindle _singleton = new Kindle();

        public static Kindle Instance => _singleton;

        public Dictionary<string, Book> Books { get; private set; } = new Dictionary<string, Book>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private Kindle()
        {
        }

        /// <summary>
        /// キャッシュ読み込み
        /// </summary>
        /// <returns></returns>
        public bool ReadCache()
        {
            Log.Info($"read cache start.");

            // XML 読み込み
            return XmlUtils.Load(XMLCacheFilePath, rootNode =>
            {
                XmlUtils.SelectNodes(rootNode, "response/add_update_list/meta_data", node =>
                {
                    var book = new Book();
                    if (book.ParseXML(Books.Count, node))
                    {
                        if (!Books.ContainsKey(book.ASIN))
                        {
                            Books.Add(book.ASIN, book);
                        }
                    }
                });

                Log.Info($"read cache end. {Books.Count}");
            });
        }
    }
}
