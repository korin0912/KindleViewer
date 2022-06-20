using System;
using System.Windows.Controls;

namespace KindleViewer
{
    public partial class BookReaderDocument : UserControl
    {
        private Book book;

        public Uri ReaderURI => book.ReaderURI;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="book"></param>
        private BookReaderDocument(Book book)
        {
            this.book = book;

            DataContext = this;

            // Log.Info(book.ReaderURI.ToString());

            InitializeComponent();
        }

        /// <summary>
        /// 本開く
        /// </summary>
        /// <param name="book"></param>
        public static void Open(Book book)
        {
            if (book == null)
            {
                return;
            }

            MainWindow.AddDocument(book.Title, new BookReaderDocument(book), true, layoutDocument =>
            {
                MainWindow.Instance.ADLayoutDocumentPane.Children.Add(layoutDocument);
            });
        }
    }
}
