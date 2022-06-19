using System;
using System.Windows.Controls;

namespace KindleViewer
{
    public partial class BookReaderDoucument : UserControl
    {
        private Book book;

        public Uri ReaderURI => book.ReaderURI;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="book"></param>
        public BookReaderDoucument(Book book)
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

            MainWindow.AddDocument(book.Title, new BookReaderDoucument(book), true, null);
        }
    }
}
