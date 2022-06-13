using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

namespace KindleViewer
{
    public partial class BookListView : UserControl
    {
        private Kindle kindle = null;

        private bool isInitialized = false;
        private ListView listView = null;
        private ScrollViewer listScrollViewer = null;

        private Queue<Book> loadQueue = new Queue<Book>();
        private AsyncLock controlLock = new AsyncLock();

        private CancellationTokenSource sortCancelSource = null;

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
                    UpdateBooks(GetShowRange());
                };

                // スクロールしたら更新
                listScrollViewer = listView.FindDescendant<ScrollViewer>();
                if (listScrollViewer != null)
                {
                    listScrollViewer.ScrollChanged += (s, e) =>
                    {
                        UpdateBooks(GetShowRange());
                    };
                }

                // ListViewアイテムをロード
                LoadListViewItem();
            };
        }

        /// <summary>
        /// ListViewアイテムをロード
        /// </summary>
        private void LoadListViewItem()
        {
            if (kindle.Books.Length <= 0)
            {
                return;
            }

            // 起動を速くするため、非同期でアイテムをちょっとずつ追加する
            this.Dispatcher.Invoke(async () =>
            {
                GetSortBooksByPurchaseDate(kindle.Books, ListSortDirection.Descending).ForEach(b => loadQueue.Enqueue(b));
                Log.Info($"book add start. {loadQueue.Count}");
                var loadLoop = true;
                while (loadLoop)
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    var enabled = mainWindow != null && mainWindow.ADDockingManager.ActiveContent == this;

                    // 一覧ドキュメントがアクティブなときだけアイテムを追加する
                    if (enabled)
                    {
                        using (await controlLock.LockAsync())
                        {
                            for (var j = 0; j < 10 && loadQueue.Count > 0; j++)
                            {
                                listView.Items.Add(loadQueue.Dequeue());
                            }

                            if (loadQueue.Count <= 0)
                            {
                                loadLoop = false;
                            }
                        }

                        await Task.Delay(1);
                    }
                    // 違ったら長めにスリープさせる
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                Log.Info($"book add end. {listView.Items.Count}");
            });
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
            if (sortCancelSource != null)
            {
                sortCancelSource.Dispose();
                sortCancelSource = null;
            }

            Task.Run(async () =>
            {
                Log.Info("sort proc start");
                using (await controlLock.LockAsync())
                {
                    sortCancelSource = new CancellationTokenSource();
                    await SortListViewTaskProc(field, direction, sortCancelSource.Token);
                }
                Log.Info("sort proc end");
            });
        }

        /// <summary>
        /// ListView ソートタスク処理
        /// </summary>
        /// <param name="field"></param>
        /// <param name="direction"></param>
        /// <param name="cancelToken"></param>
        private async Task SortListViewTaskProc(string field, ListSortDirection direction, CancellationToken cancelToken)
        {
            try
            {
                await this.Dispatcher.Invoke(async () =>
                {
                    var currentListItemCount = listView.Items.Count;

                    // 現在読み込んでいるアイテムまでを、ソートしたアイテムに置き換える
                    var sortQueue = new Queue<Book>();
                    ((field == "Title") ? GetSortBooksByTitle(kindle.Books, direction) : GetSortBooksByPurchaseDate(kindle.Books, direction)).ForEach(b => sortQueue.Enqueue(b));

                    var idx = 0;
                    while (currentListItemCount > idx && !cancelToken.IsCancellationRequested)
                    {
                        // 10個ずつ処理する
                        for (var i = 0; i < 10 && currentListItemCount > idx && !cancelToken.IsCancellationRequested; i++)
                        {
                            // 現在表示中のリストからアイテムを入れ替える
                            listView.Items.RemoveAt(idx);
                            listView.Items.Insert(idx, sortQueue.Dequeue());
                            idx++;
                        }

                        await Task.Delay(1);
                    }

                    Log.Info($"sort. {idx}, {sortQueue.Count}");

                    // 残りは loadQueue に任せる
                    if (!cancelToken.IsCancellationRequested)
                    {
                        loadQueue.Clear();
                        while (sortQueue.Count > 0)
                        {
                            loadQueue.Enqueue(sortQueue.Dequeue());
                        }
                    }

                    // 表示を更新
                    UpdateBooks(GetShowRange());
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
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
