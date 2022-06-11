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

namespace KindleViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Kindle kindle = new Kindle();

        public MainWindow()
        {
            Log.Info("start");

            ImageLoader.Instance.Start();

            kindle.ReadCache();

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                // 本一覧追加
                var layoutDocumentPane = this.FindName("LayoutDocumentPane") as AvalonDock.Layout.LayoutDocumentPane;
                var layoutDocument = new AvalonDock.Layout.LayoutDocument();
                layoutDocument.Title = "  一覧  ";
                layoutDocument.CanClose = false;
                layoutDocument.Content = new BookListView(kindle);
                layoutDocumentPane.Children.Add(layoutDocument);
            };
        }
    }
}
