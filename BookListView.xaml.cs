using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading.Tasks;
using Reactive.Bindings;

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

        private List<Book> listViewItemsSource => listView.ItemsSource as List<Book>;

        private List<Book> showBooks = new List<Book>();

        public ReactiveProperty<Book> SelectedBook { get; private set; } = new ReactiveProperty<Book>(new Book());

        public BookListView(Kindle kindle)
        {
            Log.Info("bookList start");

            DataContext = this;

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

                // ListView 取得
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

                // ListViewアイテムを設定
                var books = GetSortBooksByPurchaseDate(kindle.Books, ListSortDirection.Descending);
                Log.Info($"listView set books {books.Count}");
                listView.ItemsSource = books;

                // フィルタ設定
                showBooks.Clear();
                var textBoxFilter = this.FindName("BookListView_TextBox_Filter") as TextBox;
                CollectionViewSource.GetDefaultView(listView.ItemsSource).Filter = (item) =>
                {
                    var book = item as Book;

                    var filterText = textBoxFilter.Text.Trim();
                    // Log.Info(filterText);

                    var ret =
                            filterText.Length <= 0 ||
                            book.Title.IndexOf(filterText) != -1 ||
                            book.TitlePronunciation.IndexOf(filterText) != -1 ||
                            book.Authors.Any(a => a.Author.IndexOf(filterText) != -1 || a.Pronunciation.IndexOf(filterText) != -1) ||
                            book.Publishers.Any(p => p.Publisher.IndexOf(filterText) != -1)
                        ;

                    if (ret)
                    {
                        // Log.Info($"filter: {book.Title}");
                        showBooks.Add(book);
                        book.Hide();
                    }

                    return ret;
                };
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
                            var book = showBooks[i] as Book;
                            if (!book.IsShow)
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

                                book.Show();
                            }
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
        private void SetBooksSort(Func<IEnumerable<Book>, ListSortDirection, List<Book>> sorter, ListSortDirection direction)
        {
            Log.Info($"set books sort. {direction.ToString()}");

            this.Dispatcher.Invoke(() =>
            {
                // 一旦 StackPanel の中身を全部消す
                var wrapPanel = listView.FindDescendant<WrapPanel>();
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
        private List<Book> GetSortBooksByTitle(IEnumerable<Book> books, ListSortDirection direction)
            => GetSortBooksOrder<string>(books, direction).Invoke(b => b.TitlePronunciation).ToList();

        /// <summary>
        /// 購入日でソートした本リスト取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private List<Book> GetSortBooksByPurchaseDate(IEnumerable<Book> books, ListSortDirection direction)
            => GetSortBooksOrder<DateTime>(books, direction).Invoke(b => b.PurchaseDate).ToList();

        /// <summary>
        /// 本リストソーター取得
        /// </summary>
        /// <param name="books"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Func<Func<Book, T>, IOrderedEnumerable<Book>> GetSortBooksOrder<T>(IEnumerable<Book> books, ListSortDirection direction)
            => (direction == ListSortDirection.Ascending) ? books.OrderBy : books.OrderByDescending;

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

            (Application.Current.MainWindow as MainWindow)?.AddLayoutDocument(book.Title, new ReaderWewbView(book));
        }

        /// <summary>
        /// イベント - リストビューアイテム選択
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            var book = listView.SelectedItem as Book;
            if (book != null)
            {
                SelectedBook.Value = book;
            }
            else
            {
                SelectedBook.Value = new Book();
            }
        }

        /// <summary>
        /// イベント - ソート - 名前順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByName_ButtonClick(object s, EventArgs e)
        {
            Log.Info("SortByName_ButtonClick");
            SetBooksSort(GetSortBooksByTitle, ListSortDirection.Ascending);
        }

        /// <summary>
        /// イベント - ソート - 購入日順
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void SortByPurchase_ButtonClick(object s, EventArgs e)
        {
            Log.Info("SortByPurchase_ButtonClick");
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
