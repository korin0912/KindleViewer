using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace KindleViewer
{
    public interface IBook
    {
        public string BookshelfTitle { get; }

        public string BookshelfTitlePronunciation { get; }

        public DateTime BookshelfPurchaseDate { get; }

        public ReactiveProperty<BitmapImage> BookshelfImage { get; }

        public bool IsShow { get; }

        /// <summary>
        /// テキスト一致
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool MatchText(string text);

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="showAction"></param>
        public void Show(Action<IBook> showAction);

        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide();
    }
}
