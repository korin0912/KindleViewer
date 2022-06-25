using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading.Tasks;

namespace KindleViewer
{
    public partial class BookshelfDocument : UserControl
    {
        private bool isInitialized = false;
        private ListView listView = null;
        private TreeView treeView = null;
        private ScrollViewer listScrollViewer = null;

        private Size bookItemSize = new Size(0, 0);

        private AsyncLock updateLock = new AsyncLock();

        private List<IBook> listViewItemsSource => listView.ItemsSource as List<IBook>;

        private List<IBook> showBooks = new List<IBook>();

        private bool isSeries = false;

        public BookshelfDocument()
        {
            Log.Info("bookList start");

            DataContext = this;

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                if (isInitialized)
                {
                    return;
                }
                isInitialized = true;

                Log.Info($"BookListView loaded.");

                // -------------------------
                // ListView
                listView = this.FindName("BookListView_ListView") as ListView;
                if (listView == null)
                {
                    Log.Error("BookListView not found.");
                    return;
                }

                var books = GetSortBooksByPurchaseDate(Kindle.Instance.Books.Values.Where(b => !isSeries ? b is BookItem : b is BookSeries), ListSortDirection.Descending);
                Log.Info($"listView set books {books.Count}");
                listView.ItemsSource = books;

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

                // フィルタ設定
                showBooks.Clear();
                var textBoxFilter = this.FindName("BookListView_TextBox_Filter") as TextBox;
                CollectionViewSource.GetDefaultView(listView.ItemsSource).Filter = (item) =>
                {
                    var book = item as IBook;

                    var filterText = textBoxFilter.Text.Trim();
                    // Log.Info(filterText);

                    var ret = book.MatchText(filterText);

                    if (ret)
                    {
                        // Log.Info($"filter: {book.Title}");
                        showBooks.Add(book);
                        book.Hide();
                    }

                    return ret;
                };

                // -------------------------
                // TreeView 
                treeView = this.FindName("Sorter_TreeView") as TreeView;
            };
        }

        /// <summary>
        /// 表示範囲取得
        /// </summary>
        /// <returns></returns>
        private Range GetShowRange()
        {
            if (showBooks.Count <= 0)
            {
                return new Range(0, 0);
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

            begin = Math.Clamp(begin, 0, showBooks.Count - 1);
            end = Math.Clamp(end, 0, showBooks.Count - 1);

            // Log.Info($"{listSize.Width}, {listSize.Height}, {bookItemSize.Width}, {bookItemSize.Height}, {scroll}, {vCount}, {begin}, {end}");

            return new Range(begin, end);
        }

        /// <summary>
        /// 本更新
        /// </summary>
        private void UpdateBooks(Range range)
        {
            if (showBooks.Count <= 0)
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
                            var book = showBooks[i];
                            if (book.IsShow)
                            {
                                continue;
                            }

                            var grid = wrapPanel.Children[i].FindDescendant<Grid>();
                            book.Show(async book =>
                            {
                                using (await updateLock.LockAsync())
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        if (book is BookItem)
                                        {
                                            grid.Children.Add(new BookItemContent(book as BookItem));
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }

        /// <summary>
        /// 本フィルター更新
        /// </summary>
        private void UpdateBooksFilter()
        {
            Log.Info("update books filter");
            showBooks.Clear();
            CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
        }

        /// <summary>
        /// 本ソート設定
        /// </summary>
        /// <param name="sortDescription"></param>
        private void SetBooksSort(Func<IEnumerable<IBook>, ListSortDirection, List<IBook>> sorter, ListSortDirection direction)
        {
            if (listView == null)
            {
                return;
            }

            Log.Info($"set books sort. {direction.ToString()}");

            this.Dispatcher.Invoke(() =>
            {
                // 一旦 StackPanel の中身を全部消す
                var wrapPanel = listView.FindDescendant<WrapPanel>();
                if (wrapPanel == null)
                {
                    return;
                }

                foreach (var child in wrapPanel.Children)
                {
                    var stackPanel = (child as UIElement).FindDescendant<StackPanel>();
                    if (stackPanel != null)
                    {
                        stackPanel.Children.Clear();
                    }
                }

                // ソートして、本を全部非表示状態に戻して置き換える
                var sortBooks = sorter(listViewItemsSource, direction);
                for (var i = 0; i < listViewItemsSource.Count; i++)
                {
                    sortBooks[i].Hide();
                    listViewItemsSource[i] = sortBooks[i];
                }

                // 表示中の本もソート
                showBooks = sorter(showBooks, direction);
            });

            UpdateBooks(GetShowRange());

            Log.Info("sort books end");
        }

        /// <summary>
        /// タイトルでソートした本リスト取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private List<IBook> GetSortBooksByTitle(IEnumerable<IBook> books, ListSortDirection direction)
            => GetSortBooksOrder<string>(books, direction).Invoke(b => b.BookshelfTitlePronunciation).ToList();

        /// <summary>
        /// 購入日でソートした本リスト取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private List<IBook> GetSortBooksByPurchaseDate(IEnumerable<IBook> books, ListSortDirection direction)
            => GetSortBooksOrder<DateTime>(books, direction).Invoke(b => b.BookshelfPurchaseDate).ToList();

        /// <summary>
        /// 本リストソーター取得
        /// </summary>
        /// <param name="books"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Func<Func<IBook, T>, IOrderedEnumerable<IBook>> GetSortBooksOrder<T>(IEnumerable<IBook> books, ListSortDirection direction)
            => (direction == ListSortDirection.Ascending) ? books.OrderBy : books.OrderByDescending;

        private IBook listViewItemRightButtonBook = null;

        /// <summary>
        /// イベント - リストビューアイテム - マウス右ダウン
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListViewItem_PreviewMouseRightButtonDown(object s, MouseButtonEventArgs e)
        {
            Log.Info($"ListViewItem_PreviewMouseRightButtonDown");
            listViewItemRightButtonBook = (s as ListViewItem).Content as IBook;
        }

        /// <summary>
        /// イベント - リストビューアイテム - マウス右アップ
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListViewItem_PreviewMouseRightButtonUp(object s, MouseButtonEventArgs e)
        {
            Log.Info($"ListViewItem_PreviewMouseRightButtonUp");
            var book = (s as ListViewItem).Content as IBook;
            if (book == listViewItemRightButtonBook)
            {
                BookshelfContextMenuWindow.Show(book);
            }
            listViewItemRightButtonBook = null;
        }

        /// <summary>
        /// イベント - リストビューアイテム - マウスダブルクリック
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListViewItem_MouseDoubleClick(object s, MouseEventArgs e)
        {
            Log.Info("ListViewItem_MouseDoubleClick");
            BookReaderDocument.Open((s as ListViewItem)?.DataContext as BookItem);
        }

        /// <summary>
        /// イベント - リストビューアイテム - キー入力
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListViewItem_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.I)
            {
                Log.Info("ListViewItem_KeyDown");
                BookInformationDocument.Show(listView.SelectedItem as IBook, true);
            }
        }

        /// <summary>
        /// イベント - リストビューアイテム選択
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            BookInformationDocument.Show(listView.SelectedItem as IBook, false);
        }

        /// <summary>
        /// イベント - ツリービューアイテム選択
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object s, RoutedPropertyChangedEventArgs<Object> e)
        {
            var selectedItem = treeView.SelectedItem;
        }

        /// <summary>
        /// イベント - トグルボタン - チェック - シリーズON/OFF
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void Series_ToggleButtonClick(object s, RoutedEventArgs e)
        {
            Log.Info($"Series_ToggleButtonClick");
            var toggleButton = s as ToggleButton;
        }

        /// <summary>
        /// イベント - ソート - 名前順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByName_RadioButtonChecked(object s, EventArgs e)
        {
            Log.Info($"SortByName_RadioButtonChecked");
            SetBooksSort(GetSortBooksByTitle, ListSortDirection.Ascending);
        }

        /// <summary>
        /// イベント - ソート - 購入日順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByPurchase_RadioButtonChecked(object s, EventArgs e)
        {
            Log.Info("SortByPurchase_RadioButtonChecked");
            SetBooksSort(GetSortBooksByPurchaseDate, ListSortDirection.Descending);
        }

        /// <summary>
        /// イベント - フィルターテキストボックス - キー入力
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void FilterTextBox_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Log.Info("FilterTextBox_KeyDown");
                UpdateBooksFilter();
            }
        }
    }
}
