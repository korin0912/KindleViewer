using System;
using System.Windows.Controls;

namespace KindleViewer
{
    public partial class BookReaderDocument : UserControl
    {
        private BookItem bookItem;

        public Uri ReaderURI => bookItem.ReaderURI;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="book"></param>
        private BookReaderDocument(BookItem bookItem)
        {
            this.bookItem = bookItem;

            DataContext = this;

            // Log.Info(book.ReaderURI.ToString());

            InitializeComponent();
        }

        /// <summary>
        /// 本開く
        /// </summary>
        /// <param name="book"></param>
        public static void Open(BookItem bookItem)
        {
            if (bookItem == null)
            {
                return;
            }

            MainWindow.AddDocument(bookItem.Title, new BookReaderDocument(bookItem), true, layoutDocument =>
            {
                MainWindow.Instance.ADLayoutDocumentPane.Children.Add(layoutDocument);
            });
        }
    }
}
