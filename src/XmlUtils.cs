using System;
using System.Xml;

namespace KindleViewer
{
    public static class XmlUtils
    {
        /// <summary>
        /// XMLファイル読み込み
        /// </summary>
        /// <returns></returns>
        public static bool Load(string fpath, Action<XmlNode> action)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(fpath);

                action?.Invoke(xmlDoc);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// ノード選択して処理
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="xpath"></param>
        /// <param name="action"></param>
        public static void SelectNode(XmlNode xmlNode, string xpath, Action<XmlNode> action)
        {
            if (xmlNode == null)
            {
                return;
            }

            var node = xmlNode.SelectSingleNode(xpath);
            if (node == null)
            {
                return;
            }

            action?.Invoke(node);
        }

        /// <summary>
        /// ノードリスト選択して処理
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="xpath"></param>
        /// <param name="action"></param>
        public static void SelectNodes(XmlNode xmlNode, string xpath, Action<XmlNode> action)
        {
            if (xmlNode == null)
            {
                return;
            }

            var nodes = xmlNode.SelectNodes(xpath);
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                action?.Invoke(node);
            }
        }

        /// <summary>
        /// InnerText 取得
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static string GetInnerText(XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return "";
            }

            return xmlNode.InnerText;
        }

        /// <summary>
        /// 属性の InnerText 取得
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetAttributeInnerText(XmlNode xmlNode, string attribute)
        {
            if (xmlNode == null)
            {
                return "";
            }

            if (xmlNode.Attributes == null)
            {
                return "";
            }

            foreach (XmlAttribute a in xmlNode.Attributes)
            {
                if (a.Name == attribute)
                {
                    return a.InnerText;
                }
            }

            return "";
        }
    }
}
