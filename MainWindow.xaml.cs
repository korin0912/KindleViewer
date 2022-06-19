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
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null;

        public ReactiveCommand DocumentCloseCommand { get; } = new ReactiveCommand();

        public AvalonDock.DockingManager ADDockingManager { get; private set; } = null;
        public AvalonDock.Layout.LayoutPanel ADLayoutaPanelVertical { get; private set; } = null;
        public AvalonDock.Layout.LayoutPanel ADLayoutaPanelHorizontal { get; private set; } = null;
        public AvalonDock.Layout.LayoutDocumentPane ADLayoutDocumentPane { get; private set; } = null;

        public MainWindow()
        {
            Log.Info("start");

            ImageLoader.Instance.Start();

            Kindle.Instance.ReadCache();

            DataContext = this;

            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Log.Info($"MainWindow loaded.");

                ADDockingManager = this.FindName("adDockingManager") as AvalonDock.DockingManager;
                ADLayoutaPanelVertical = this.FindName("adLayoutPanelVertical") as AvalonDock.Layout.LayoutPanel;
                ADLayoutaPanelHorizontal = this.FindName("adLayoutPanelHorizontal") as AvalonDock.Layout.LayoutPanel;
                ADLayoutDocumentPane = this.FindName("adLayoutDocumentPane") as AvalonDock.Layout.LayoutDocumentPane;

                Instance = this;

                // ドキュメント閉じる
                DocumentCloseCommand.Subscribe(_ =>
                {
                    if (ADDockingManager.ActiveContent != null)
                    {
                        CloseDocument(ADDockingManager.ActiveContent);
                    }
                });

                // 本一覧ドキュメント追加
                AddDocument("一覧", new BookshelfDocument(), true, layoutDocument =>
                {
                    layoutDocument.CanClose = false;
                    MainWindow.Instance.ADLayoutDocumentPane.Children.Add(layoutDocument);
                });
            };
        }

        /// <summary>
        /// ドキュメント追加
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="actived"></param>
        /// <param name="initAction"></param>
        public static void AddDocument(string title, object content, bool actived, Action<AvalonDock.Layout.LayoutDocument> initAction)
        {
            if (MainWindow.Instance == null)
            {
                Log.Warning("MainWindow is null.");
                return;
            }

            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            Log.Info($"add LayoutDocument. {title}, {content.GetType().Name}, {actived}");
            var layoutDocument = new AvalonDock.Layout.LayoutDocument();
            layoutDocument.Title = title;
            layoutDocument.Content = content;

            initAction?.Invoke(layoutDocument);

            // 追加と同時にアクティブ
            if (actived)
            {
                MainWindow.Instance.ADDockingManager.ActiveContent = content;
            }
        }

        /// <summary>
        /// ドキュメント閉じる
        /// </summary>
        /// <param name="content"></param>
        public static void CloseDocument(object content)
        {
            if (MainWindow.Instance == null)
            {
                Log.Warning("MainWindow is null.");
                return;
            }

            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            foreach (var doc in MainWindow.Instance.ADLayoutDocumentPane.Children)
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

        /// <summary>
        /// ドキュメント検索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T FindDocument<T>() where T : class
        {
            if (MainWindow.Instance == null)
            {
                Log.Warning("MainWindow is null.");
                return null;
            }

            foreach (var doc in MainWindow.Instance.ADLayoutDocumentPane.Children)
            {
                if (doc.Content is T)
                {
                    return doc.Content as T;
                }
            }

            return null;
        }

        /// <summary>
        /// アプリ終了
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void MenuButton_Quit(object s, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
