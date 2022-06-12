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

        private List<Book> books = new List<Book>();
        public Book[] Books => books.ToArray();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Kindle()
        {
        }

        /// <summary>
        /// キャッシュ読み込み
        /// </summary>
        /// <returns></returns>
        public bool ReadCache()
        {
            Log.Info($"read cache start.");

            return XmlUtils.Load(XMLCacheFilePath, rootNode =>
            {
                XmlUtils.SelectNodes(rootNode, "response/add_update_list/meta_data", node =>
                {
                    // // デバッグ用
                    // if (books.Count >= 500)
                    // {
                    //     return;
                    // }

                    var book = new Book();
                    if (book.ParseXML(books.Count, node))
                    {
                        books.Add(book);
                    }
                });

                Log.Info($"read cache end. {books.Count}");
            });
        }
    }
}
