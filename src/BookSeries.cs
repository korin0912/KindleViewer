using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace KindleViewer
{
    public class BookSeries : IBook
    {
        public string BookshelfTitle => Books.Count > 0 ? null : Books[Books.Keys.Min(k => k)].Title;

        public string BookshelfTitlePronunciation => Books.Count > 0 ? null : Books[Books.Keys.Min(k => k)].TitlePronunciation;

        public DateTime BookshelfPurchaseDate => Books.Count > 0 ? Books.Values.Max(b => b.PurchaseDate) : Consts.UnknownDateTime;

        public ReactiveProperty<BitmapImage> BookshelfImage => Books.Count > 0 ? null : Books[Books.Keys.Min(k => k)].CoverImage;

        public bool IsShow { get; private set; } = false;

        public Dictionary<int, BookItem> Books { get; private set; } = new Dictionary<int, BookItem>();

        /// <summary>
        /// テキスト一致
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool MatchText(string text)
        {
            if (text.Length <= 0)
            {
                return true;
            }

            return Books.Values.Any(book => {
                return
                    book.Title.IndexOf(text) != -1 ||
                    book.TitlePronunciation.IndexOf(text) != -1 ||
                    book.Authors.Any(a => a.Author.IndexOf(text) != -1 || a.Pronunciation.IndexOf(text) != -1) ||
                    book.Publishers.Any(p => p.Publisher.IndexOf(text) != -1);
            });
        }

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="showAction"></param>
        public void Show(Action<IBook> showAction)
        {
            if (IsShow)
            {
                return;
            }

            IsShow = true;

            // カバー画像更新
            Books[Books.Keys.Min(k => k)].LoadCoverImage();

            Books[Books.Keys.Min(k => k)].CoverImage.Subscribe(img =>
            {
                showAction?.Invoke(this);
            });
        }

        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            if (!IsShow)
            {
                return;
            }

            IsShow = false;

            // カバー画像
            Books[Books.Keys.Min(k => k)].UnloadCoverImage();
        }
    }
}
