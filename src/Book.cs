﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using Reactive.Bindings;

namespace KindleViewer
{
    public class Book
    {
        public static readonly DateTime UnknownDateTime = new DateTime(1900, 1, 1, 0, 0, 0);

        public int Index { get; private set; } = -1;

        public string ASIN { get; private set; } = "";

        public string Title { get; private set; } = "";

        public string TitlePronunciation { get; private set; } = "";

        public class AuthorData
        {
            public string Author { get; private set; } = "";
            public string Pronunciation { get; private set; } = "";

            public AuthorData(string author, string pronunciation)
            {
                this.Author = author;
                this.Pronunciation = pronunciation;
            }
        }

        public List<AuthorData> Authors { get; private set; } = new List<AuthorData>();

        public List<string> Publishers { get; private set; } = new List<string>();

        public DateTime PublicationDate { get; private set; } = UnknownDateTime;

        public DateTime PurchaseDate { get; private set; } = UnknownDateTime;

        public string TextbookType { get; private set; } = "";

        public string CdeContenttype { get; private set; } = "";

        public string ContentType { get; private set; } = "";

        public List<string> Origins { get; private set; } = new List<string>();

        public ReactiveProperty<BitmapImage> CoverImage { get; private set; } = new ReactiveProperty<BitmapImage>();

        public bool IsCoverImageLoaded { get; private set; } = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Book()
        {
        }

        /// <summary>
        /// XML パース
        /// </summary>
        /// <param name="index"></param>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public bool ParseXML(int index, XmlNode xmlNode)
        {
            Index = index;

            try
            {
                XmlUtils.SelectNode(xmlNode, "ASIN", node =>
                {
                    ASIN = XmlUtils.GetInnerText(node);
                });


                XmlUtils.SelectNode(xmlNode, "title", node =>
                {
                    Title = XmlUtils.GetInnerText(node);
                    TitlePronunciation = XmlUtils.GetAttributeInnerText(node, "pronunciation");
                });

                XmlUtils.SelectNodes(xmlNode, "authors/author", node =>
                {
                    Authors.Add(new AuthorData(XmlUtils.GetInnerText(node), XmlUtils.GetAttributeInnerText(node, "pronunciation")));
                });

                XmlUtils.SelectNodes(xmlNode, "publishers/publisher", node =>
                {
                    Publishers.Add(XmlUtils.GetInnerText(node));
                });

                XmlUtils.SelectNode(xmlNode, "publication_date", node =>
                {
                    var dt = default(DateTime);
                    if (DateTime.TryParse(XmlUtils.GetInnerText(node), out dt))
                    {
                        PublicationDate = dt;
                    }
                });

                XmlUtils.SelectNode(xmlNode, "purchase_date", node =>
                {
                    var dt = default(DateTime);
                    if (DateTime.TryParse(XmlUtils.GetInnerText(node), out dt))
                    {
                        PurchaseDate = dt;
                    }
                });

                XmlUtils.SelectNode(xmlNode, "textbook_type", node =>
                {
                    TextbookType = XmlUtils.GetInnerText(node);
                });

                XmlUtils.SelectNode(xmlNode, "cde_contenttype", node =>
                {
                    CdeContenttype = XmlUtils.GetInnerText(node);
                });

                XmlUtils.SelectNode(xmlNode, "content_type", node =>
                {
                    ContentType = XmlUtils.GetInnerText(node);
                });

                XmlUtils.SelectNodes(xmlNode, "origins/origin", node =>
                {
                    Origins.Add(XmlUtils.GetInnerText(node));
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
                return false;
            }

            return PurchaseDate != UnknownDateTime;
        }

        /// <summary>
        /// カバー画像読み込み
        /// </summary>
        public void LoadCoverImage()
        {
            if (IsCoverImageLoaded)
            {
                return;
            }

            IsCoverImageLoaded = true;

            if (string.IsNullOrEmpty(ASIN))
            {
                return;
            }

            // ASIN から カバー画像ファイル名取得
            var md5 = MD5.Create();
            var fhash = md5.ComputeHash(Encoding.UTF8.GetBytes(ASIN));
            md5.Clear();
            var sb = new StringBuilder();
            foreach (var b in fhash)
            {
                sb.Append(b.ToString("X2"));
            }
            var fname = sb.ToString();

            // 画像ローダーでロード
            ImageLoader.Instance.Load(
                $"{Kindle.CoverCacheFolderPath}{Path.DirectorySeparatorChar}{fname}.jpg",
                image =>
                {
                    if (image == null)
                    {
                        return;
                    }

                    CoverImage.Value = image;
                }
            );
        }
    }
}