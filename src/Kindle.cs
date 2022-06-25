using System;
using System.IO;
using System.Collections.Generic;

namespace KindleViewer
{
    public class Kindle
    {
        private static Kindle _singleton = new Kindle();

        public static Kindle Instance => _singleton;

        public Dictionary<string, IBook> Books { get; private set; } = new Dictionary<string, IBook>();

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
            return XmlUtils.Load(Consts.XMLCacheFilePath, rootNode =>
            {
                XmlUtils.SelectNodes(rootNode, "response/add_update_list/meta_data", node =>
                {
                    var book = new BookItem();
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
