using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reactive.Bindings;

namespace KindleViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BookshelfContextMenuWindow : Window
    {
        private Book book = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner"></param>
        private BookshelfContextMenuWindow(Book book)
        {
            this.book = book;

            var mousePos = MainWindow.Instance.PointToScreen(Mouse.GetPosition(MainWindow.Instance));
            this.Top = mousePos.Y;
            this.Left = mousePos.X;
            this.Owner = MainWindow.Instance;
            this.ShowInTaskbar = false;
            this.SizeToContent = SizeToContent.WidthAndHeight;

            InitializeComponent();

            this.Deactivated += (s, e) =>
            {
                Hide();
            };
        }

        /// <summary>
        /// 非表示
        /// </summary>
        private new void Hide()
        {
            if (this.Owner != null)
            {
                this.Owner = null;
                this.Close();
            }
        }

        /// <summary>
        /// イベント - 本情報 - メニューアイテム - クリック
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void BookInformation_MenuItem_Click(object s, RoutedEventArgs e)
        {
            BookInformationDocument.Show(book, true);
            Hide();
        }

        /// <summary>
        /// イベント - 本を読む - メニューアイテム - クリック
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void BookReader_MenuItem_Click(object s, RoutedEventArgs e)
        {
            BookReaderDocument.Open(book);
            Hide();
        }

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="book"></param>
        public static void Show(Book book)
        {
            var window = new BookshelfContextMenuWindow(book);
            window.Show();
        }
    }
}
