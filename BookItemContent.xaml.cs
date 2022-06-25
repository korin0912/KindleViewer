using System.Windows.Controls;

namespace KindleViewer
{
    public partial class BookItemContent : UserControl
    {
        public BookItem Book { get; private set; } = null;

        public BookItemContent(BookItem bookItem)
        {
            this.Book = bookItem;

            DataContext = this;

            InitializeComponent();
        }
    }
}
