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
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private Kindle kindle = new Kindle();

        public ReactiveCommand DocumentCloseCommand { get; } = new ReactiveCommand();

        public AvalonDock.DockingManager ADDockingManager { get; private set; } = null;
        public AvalonDock.Layout.LayoutDocumentPane ADLayoutDocumentPane { get; private set; } = null;

        public MainWindow()
        {
            Log.Info("start");

            ImageLoader.Instance.Start();

            kindle.ReadCache();

            DataContext = this;

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Log.Info($"MainWindow loaded.");

                ADDockingManager = this.FindName("adDockingManager") as AvalonDock.DockingManager;
                ADLayoutDocumentPane = this.FindName("adLayoutDocumentPane") as AvalonDock.Layout.LayoutDocumentPane;

                // ドキュメント閉じる
                DocumentCloseCommand.Subscribe(_ =>
                {
                    if (ADDockingManager.ActiveContent != null)
                    {
                        CloseLayoutDocument(ADDockingManager.ActiveContent);
                    }
                });

                // 本一覧追加
                AddLayoutDocument("一覧", new BookListView(kindle), true, false);
            };
        }

        /// <summary>
        /// LayoutDocument 追加
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="canClose"></param>
        public void AddLayoutDocument(string title, object content, bool actived = true, bool canClose = true)
        {
            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            Log.Info($"add layoutDocument. {content.GetType().Name}");
            var layoutDocument = new AvalonDock.Layout.LayoutDocument();
            layoutDocument.Title = title;
            layoutDocument.Content = content;
            layoutDocument.CanClose = canClose;
            ADLayoutDocumentPane.Children.Add(layoutDocument);

            // 追加と同時にアクティブにする
            if (actived)
            {
                ADDockingManager.ActiveContent = content;
            }
        }

        /// <summary>
        /// LayoutDocument 閉じる
        /// </summary>
        /// <param name="content"></param>
        public void CloseLayoutDocument(object content)
        {
            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            foreach (var doc in ADLayoutDocumentPane.Children)
            {
                // 閉じるボタンが出てないと閉じれない
                if (!doc.CanClose)
                {
                    continue;
                }

                // コンテンツが一致するものを閉じる
                if (doc.Content == content)
                {
                    Log.Info($"close layoutDocument. {doc.Title}");
                    doc.Close();
                    break;
                }
            }
        }
    }
}
