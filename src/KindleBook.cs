using System;
using System.Collections.Generic;
using System.Xml;

namespace KindleViewer
{
    public class KindleBook
    {
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

        public static readonly DateTime UnknownDateTime = new DateTime(1900, 1, 1, 0, 0, 0);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KindleBook()
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
    }
}
