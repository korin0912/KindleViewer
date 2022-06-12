using System;
using System.Windows.Controls;

namespace KindleViewer
{
    public partial class ReaderWewbView : UserControl
    {
        private Book book;

        public Uri ReaderURI => book.ReaderURI;

        public ReaderWewbView(Book book)
        {
            this.book = book;

            DataContext = this;

            // Log.Info(book.ReaderURI.ToString());

            InitializeComponent();
        }
    }
}
