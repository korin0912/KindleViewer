using System;
using System.Windows;
using System.Windows.Controls;

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

            InitializeComponent();

            this.kindle = kindle;

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
                    UpdateScroll();
                };

                listScrollViewer = listView.FindDescendant<ScrollViewer>();
                Log.Info($"ScrollViewer get {(listScrollViewer != null ? "success" : "fail")}");
                if (listScrollViewer != null)
                {
                    listScrollViewer.ScrollChanged += (s, e) =>
                    {
                        UpdateScroll();
                    };
                }

                UpdateScroll();
            };
        }

        /// <summary>
        /// スクロール時更新
        /// </summary>
        private void UpdateScroll()
        {
            if (listView.Items.Count <= 0)
            {
                return;
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

            // Log.Info($"{begin} - {end}, {listSize.Width}, {listSize.Height}, {itemSize.Width}");

            // カバー画像ロード
            for (var i = begin; i <= end; i++)
            {
                if (!kindle.Books[i].IsCoverImageLoaded)
                {
                    kindle.Books[i].LoadCoverImage();
                }
            }
        }
    }
}
