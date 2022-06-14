using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace KindleViewer
{
    public partial class BookListView : UserControl
    {
        private Kindle kindle = null;

        private bool isInitialized = false;
        private ListView listView = null;
        private ScrollViewer listScrollViewer = null;

        private Size bookItemSize = new Size(0, 0);

        private AsyncLock updateLock = new AsyncLock();

        public BookListView(Kindle kindle)
        {
            Log.Info("bookList start");

            DataContext = kindle;

            this.kindle = kindle;

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                if (isInitialized)
                {
                    return;
                }
                isInitialized = true;

                Log.Info($"BookListView loaded.");

                listView = this.FindName("BookListView_ListView") as ListView;
                if (listView == null)
                {
                    Log.Error("BookListView not found.");
                    return;
                }

                // サイズ変わったら更新
                listView.SizeChanged += (s, e) =>
                {
                    // Log.Info("DataContextChanged");
                    UpdateBooks(GetShowRange());
                };

                // スクロールしたら更新
                listScrollViewer = listView.FindDescendant<ScrollViewer>();
                if (listScrollViewer != null)
                {
                    listScrollViewer.ScrollChanged += (s, e) =>
                    {
                        // Log.Info("ScrollChanged");
                        UpdateBooks(GetShowRange());
                    };
                }

                // ListViewアイテムをロード
                var books = GetSortBooksByPurchaseDate(kindle.Books, ListSortDirection.Descending);
                Log.Info($"book add start.");
                var mainWindow = Application.Current.MainWindow as MainWindow;
                foreach (var book in books)
                {
                    listView.Items.Add(book);
                }
                Log.Info($"book add end. {listView.Items.Count}");
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

            if (bookItemSize.Width <= 0)
            {
                var item = listView.FindDescendant<ListViewItem>();
                bookItemSize = item != null ? item.DesiredSize : new Size(100, 200);
            }

            var scroll = listScrollViewer != null ? listScrollViewer.VerticalOffset : 0;

            var vCount = (int)Math.Floor(listSize.Width / bookItemSize.Width);

            var begin = (int)Math.Floor(scroll / bookItemSize.Height) * vCount;
            var end = ((int)Math.Floor((scroll + listSize.Height) / bookItemSize.Height) + 1) * vCount - 1;

            begin = Math.Clamp(begin, 0, listView.Items.Count - 1);
            end = Math.Clamp(end, 0, listView.Items.Count - 1);

            // Log.Info($"{listSize.Width}, {listSize.Height}, {bookItemSize.Width}, {bookItemSize.Height}, {scroll}, {vCount}, {begin}, {end}");

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

            // Log.Info($"{range.Start} - {range.End}");

            // 本情報更新
            var wrapPanel = listView.FindDescendant<WrapPanel>();

            Task.Run(async () =>
            {
                using (await updateLock.LockAsync())
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        for (var i = range.Start.Value; i <= range.End.Value; i++)
                        {
                            var book = listView.Items[i] as Book;
                            if (!book.IsUpdateShow)
                            {
                                var stackPanel = wrapPanel.Children[i].FindDescendant<StackPanel>();

                                var image = new Image();
                                book.CoverImage.Subscribe(async img =>
                                {
                                    using (await updateLock.LockAsync())
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            image.Source = img;
                                        });
                                    }
                                });
                                image.Width = 100;
                                image.Height = 150;
                                image.Margin = new Thickness(0, 0, 0, 0);
                                stackPanel.Children.Add(image);

                                var textBlock = new TextBlock();
                                textBlock.Text = book.Title;
                                textBlock.Width = 100;
                                textBlock.Height = 50;
                                textBlock.Margin = new Thickness(0, 0, 0, 0);
                                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                                textBlock.TextWrapping = TextWrapping.Wrap;
                                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                                stackPanel.Children.Add(textBlock);

                                book.UpdateShow();
                            }
                        }
                    });
                }
            });
        }

        /// <summary>
        /// イベント - アイテムをマウスダブルクリック
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListViewItem_MouseDoubleClick(object s, MouseEventArgs e)
        {
            Log.Info("ListViewItem_MouseDoubleClick");

            var item = s as ListViewItem;
            if (item == null)
            {
                return;
            }

            var book = item.DataContext as Book;
            if (book == null)
            {
                return;
            }

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.AddLayoutDocument(book.Title, new ReaderWewbView(book));
        }

        /// <summary>
        /// ListView ソート
        /// </summary>
        /// <param name="field"></param>
        /// <param name="direction"></param>
        private void SortListView(string field, ListSortDirection direction)
        {
            Log.Info("sort start");

            this.Dispatcher.Invoke(() =>
            {
                // 一旦 StackPanel の中身を全部消す
                var wrapPanel = listView.FindDescendant<WrapPanel>();
                for (var i = 0; i < wrapPanel.Children.Count; i++)
                {
                    var stackPanel = wrapPanel.Children[i].FindDescendant<StackPanel>();
                    stackPanel.Children.Clear();
                }

                // ソートして、本を全部非表示状態に戻して置き換える
                var books = ((field == "Title") ? GetSortBooksByTitle(kindle.Books, direction) : GetSortBooksByPurchaseDate(kindle.Books, direction));
                for (var i = 0; i < books.Count; i++)
                {
                    books[i].UpdateHide();
                    listView.Items[i] = books[i];
                }
            });

            UpdateBooks(GetShowRange());

            Log.Info("sort end");
        }

        /// <summary>
        /// 購入日でソートした本リスト取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public List<Book> GetSortBooksByPurchaseDate(IEnumerable<Book> books, ListSortDirection direction)
            => ((direction == ListSortDirection.Ascending) ? books.OrderBy(b => b.PurchaseDate) : books.OrderByDescending(b => b.PurchaseDate)).ToList();

        /// <summary>
        /// 購入日でソートした本リスト取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public List<Book> GetSortBooksByTitle(IEnumerable<Book> books, ListSortDirection direction)
            => ((direction == ListSortDirection.Ascending) ? books.OrderBy(b => b.TitlePronunciation) : books.OrderByDescending(b => b.TitlePronunciation)).ToList();

        /// <summary>
        /// イベント - ソート - 名前順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByName_ButtonClick(object s, EventArgs e)
        {
            Log.Info("SortByName_ButtonClick");
            SortListView("Title", ListSortDirection.Ascending);
        }

        /// <summary>
        /// イベント - ソート - 購入日順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByPurchase_ButtonClick(object s, EventArgs e)
        {
            Log.Info("SortByPurchase_ButtonClick");
            SortListView("PurchaseDate", ListSortDirection.Descending);
        }
    }
}
