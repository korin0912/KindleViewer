using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Windows;
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

        public class PublisherData
        {
            public string Publisher { get; private set; } = "";

            public PublisherData(string publisher)
            {
                this.Publisher = publisher;
            }
        }

        public List<PublisherData> Publishers { get; private set; } = new List<PublisherData>();

        public DateTime PublicationDate { get; private set; } = UnknownDateTime;

        public string PublicationDateFormat => PublicationDate.ToString("yyyy/MM/dd");

        public DateTime PurchaseDate { get; private set; } = UnknownDateTime;

        public string PurchaseDateFormat => PurchaseDate.ToString("yyyy/MM/dd");

        public string TextbookType { get; private set; } = "";

        public string CdeContenttype { get; private set; } = "";

        public string ContentType { get; private set; } = "";

        public class OriginData
        {
            public string Origin { get; private set; } = "";

            public OriginData(string origin)
            {
                this.Origin = origin;
            }
        }

        public List<OriginData> Origins { get; private set; } = new List<OriginData>();

        public bool IsShow { get; private set; } = false;

        public ReactiveProperty<BitmapImage> CoverImage { get; private set; } = new ReactiveProperty<BitmapImage>();

        public Uri ReaderURI { get; private set; } = null;

        public Visibility Visibility { get; private set; } = Visibility.Hidden;

        public string TitlePronunciationTrim { get; private set; } = "";

        public int Volume = 0;

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
                    Publishers.Add(new PublisherData(XmlUtils.GetInnerText(node)));
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
                    Origins.Add(new OriginData(XmlUtils.GetInnerText(node)));
                });

                ReaderURI = new Uri($"{Kindle.KindleCloudReaderUriPrefix}{ASIN}?language={System.Globalization.CultureInfo.CurrentCulture.Name}");
                // ReaderURI = new Uri("https://www.google.co.jp");

                // タイトル仮名読み整形と巻数
                if (!string.IsNullOrEmpty(TitlePronunciation))
                {
                    TitlePronunciationTrim = TitlePronunciation.Trim();
                    var match = Regex.Match(TitlePronunciationTrim, "([0-9]{1,3})");
                    if (match != null && match != Match.Empty)
                    {
                        Volume = int.Parse(TitlePronunciationTrim.Substring(match.Index, match.Length), System.Globalization.NumberStyles.Integer);
                        TitlePronunciationTrim = TitlePronunciationTrim.Substring(0, match.Index + 1);
                    }
                    else
                    {
                        TitlePronunciationTrim = TitlePronunciationTrim.Split(' ')[0];
                    }
                }
                else
                {
                    TitlePronunciationTrim = Title;
                }

                Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
                return false;
            }

            return PurchaseDate != UnknownDateTime;
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        public void Show()
        {
            if (IsShow)
            {
                return;
            }

            IsShow = true;

            // カバー画像更新
            if (!string.IsNullOrEmpty(ASIN))
            {
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

        /// <summary>
        /// 非表示更新
        /// </summary>
        public void Hide()
        {
            if (!IsShow)
            {
                return;
            }

            IsShow = false;

            // カバー画像
            CoverImage = new ReactiveProperty<BitmapImage>();
        }
    }
}
