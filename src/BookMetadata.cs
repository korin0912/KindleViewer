using System.Text.RegularExpressions;

namespace KindleViewer
{
    public class BookMetadata
    {
        /// <summary>
        /// シリーズタイトル
        /// </summary>
        /// <value></value>
        public string SeriesTitle { get; set; } = "";

        /// <summary>
        /// 巻数
        /// </summary>
        /// <value></value>
        public int Volume { get; set; } = 0;

        /// <summary>
        /// 本情報を元に初期化
        /// </summary>
        /// <param name="book"></param>
        public void InitializeByBook(Book book)
        {
            // シリーズタイトルと巻数
            if (!string.IsNullOrEmpty(book.TitlePronunciation))
            {
                // タイトル読み仮名があるので、日本版タイトル(のつもり)

                // スペース区切りの0要素目をシリーズタイトルとして扱う
                SeriesTitle = book.Title.Trim().Split(' ', '　')[0];

                // 読み仮名の中に数値1～3桁があればそれを巻数として扱う
                var t = book.TitlePronunciation.Trim();
                var match = Regex.Match(t, "([0-9]{1,3})");
                if (match != null && match != Match.Empty)
                {
                    Volume = int.Parse(t.Substring(match.Index, match.Length), System.Globalization.NumberStyles.Integer);
                }
                else
                {
                    Volume = 1;
                }
            }
            else
            {
                // 読み仮名が無い = 外国版タイトル
                SeriesTitle = book.Title;
                Volume = 1;
            }
        }
    }
}
