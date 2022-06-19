using System.Windows.Controls;
using Reactive.Bindings;

namespace KindleViewer
{
    public partial class BookInformationDocument : UserControl
    {
        private static BookInformationDocument Current = null;

        public ReactiveProperty<Book> ShowBook { get; private set; } = new ReactiveProperty<Book>(new Book());

        private bool isResize = false;
        private AvalonDock.Layout.LayoutDocumentPane resizeLayoutDocumentPane = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BookInformationDocument(Book book)
        {
            ShowBook.Value = book;

            DataContext = this;

            this.Loaded += (s, e) =>
            {
                Log.Info($"load book information.");
                Current = this;

                if (isResize && resizeLayoutDocumentPane != null)
                {
                    isResize = false;
                    resizeLayoutDocumentPane.DockWidth = new System.Windows.GridLength(300);
                }
            };

            this.Unloaded += (s, e) =>
            {
                if (Current == this)
                {
                    Log.Info($"unload book information.");
                    Current = null;
                }
            };

            InitializeComponent();
        }

        /// <summary>
        /// 本設定
        /// </summary>
        /// <param name="book"></param>
        public void SetBook(Book book)
        {
            Log.Info($"set book information. {book.Title}");
            ShowBook.Value = book;
        }

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="book"></param>
        public static void Show(Book book, bool isDocumentOpen)
        {
            if (book == null)
            {
                return;
            }

            if (Current == null)
            {
                if (isDocumentOpen)
                {
                    var doc = new BookInformationDocument(book);
                    MainWindow.AddDocument("本情報", doc, false, layoutDocument =>
                    {
                        var layoutDocumentPane = new AvalonDock.Layout.LayoutDocumentPane(layoutDocument);
                        layoutDocumentPane.Children.CollectionChanged += (s, e) =>
                        {
                            // 子が一個もなくなったら消す
                            if (layoutDocumentPane.Children.Count <= 0)
                            {
                                MainWindow.Instance.ADLayoutaPanelHorizontal.Children.Remove(layoutDocumentPane);
                            }
                        };
                        MainWindow.Instance.ADLayoutaPanelHorizontal.Children.Add(layoutDocumentPane);

                        doc.isResize = true;
                        doc.resizeLayoutDocumentPane = layoutDocumentPane;
                    });
                }
            }
            else
            {
                Current.SetBook(book);
            }
        }
    }
}
