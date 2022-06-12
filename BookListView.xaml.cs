using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace KindleViewer
{
    public partial class BookListView : UserControl
    {
        private Kindle kindle = null;

        private ListView listView = null;
        private ScrollViewer listScrollViewer = null;

        public BookListView(Kindle kindle)
        {
            Log.Info("bookList start");

            DataContext = kindle;

            this.kindle = kindle;

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Log.Info("InitializeComponent loaded");

                listView = this.FindName("BookListView_ListView") as ListView;
                if (listView == null)
                {
                    Log.Error("BookListView not found.");
                    return;
                }

                listView.SizeChanged += (s, e) =>
                {
                    UpdateBooks(GetShowRange());
                };

                listScrollViewer = listView.FindDescendant<ScrollViewer>();
                Log.Info($"ScrollViewer get {(listScrollViewer != null ? "success" : "fail")}");
                if (listScrollViewer != null)
                {
                    listScrollViewer.ScrollChanged += (s, e) =>
                    {
                        UpdateBooks(GetShowRange());
                    };
                }

                // 起動を速くするため、スレッドでアイテムをちょっとずつ追加する
                this.Dispatcher.Invoke(async () =>
                {
                    for (var i = 0; i < kindle.Books.Length;)
                    {
                        for (var j = 0; j < 100 && i < kindle.Books.Length; j++, i++)
                        {
                            listView.Items.Add(kindle.Books[i]);
                        }
                        await Task.Delay(1);
                    }
                });
            };
        }

        /// <summary>
        /// 表示範囲取得
        /// </summary>
        /// <returns></returns>
        private Range GetShowRange()
        {
            if (listView.Items.Count <= 0)
            {
                return new Range(0, -1);
            }

            // 画面に表示されている本のインデクス計算
            var listSize = listView.DesiredSize;

            var item = listView.FindDescendant<ListViewItem>();
            var itemSize = item != null ? item.DesiredSize : new Size(100, 200);

            var scroll = listScrollViewer != null ? listScrollViewer.VerticalOffset : 0;

            var vCount = (int)Math.Floor(listSize.Width / itemSize.Width);

            var begin = (int)Math.Floor(scroll / itemSize.Height) * vCount;
            var end = ((int)Math.Floor((scroll + listSize.Height) / itemSize.Height) + 1) * vCount - 1;

            begin = Math.Clamp(begin, 0, listView.Items.Count - 1);
            end = Math.Clamp(end, 0, listView.Items.Count - 1);

            // Log.Info($"{listSize.Width}, {listSize.Height}, {itemSize.Width}, {itemSize.Height}, {scroll}, {vCount}, {begin}, {end}");

            return new Range(begin, end);
        }

        /// <summary>
        /// 本更新
        /// </summary>
        private void UpdateBooks(Range range)
        {
            if (listView.Items.Count <= 0)
            {
                return;
            }

            // カバー画像ロード
            for (var i = range.Start.Value; i <= range.End.Value; i++)
            {
                var book = listView.Items[i] as Book;
                if (!book.IsCoverImageLoaded)
                {
                    book.LoadCoverImage();
                }
            }
        }
    }
}
