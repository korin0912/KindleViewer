using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KindleViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Kindle kindle = new Kindle();

        private Book[] Books => kindle.Books;

        private ListView bookListView = null;
        private ScrollViewer bookListScrollViewer = null;

        public MainWindow()
        {
            Log.Info("start");

            kindle.ReadCache();

            ImageLoader.Instance.Start();

            DataContext = kindle;
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Log.Info("InitializeComponent loaded");

                bookListView = this.FindName("BookListView") as ListView;
                if (bookListView == null)
                {
                    Log.Error("BookListView not found.");
                    return;
                }

                bookListView.Loaded += (s, e) =>
                {
                    Log.Info("BookListView loaded");

                    UpdateScroll(bookListView, bookListScrollViewer);

                    bookListScrollViewer = bookListView.FindDescendant<ScrollViewer>();
                    Log.Info($"ScrollViewer get {(bookListScrollViewer != null ? "success" : "fail")}");
                    if (bookListScrollViewer != null)
                    {
                        bookListScrollViewer.ScrollChanged += (s, e) =>
                        {
                            UpdateScroll(bookListView, bookListScrollViewer);
                        };
                    }
                };

                bookListView.SizeChanged += (s, e) =>
                {
                    UpdateScroll(bookListView, bookListScrollViewer);
                };

            };
        }

        /// <summary>
        /// スクロール時更新
        /// </summary>
        /// <param name="listView"></param>
        private void UpdateScroll(ListView listView, ScrollViewer scrollViewer)
        {
            if (listView.Items.Count <= 0)
            {
                return;
            }

            // 画面に表示されている本のインデクス計算
            var listSize = listView.DesiredSize;

            var item = listView.FindDescendant<ListViewItem>();
            var itemSize = item != null ? item.DesiredSize : new Size(100, 200);

            var scroll = scrollViewer != null ? scrollViewer.VerticalOffset : 0;

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
