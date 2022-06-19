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
        public ReactiveCommand DocumentCloseCommand { get; } = new ReactiveCommand();

        public AvalonDock.DockingManager ADDockingManager { get; private set; } = null;
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
                ADLayoutDocumentPane = this.FindName("adLayoutDocumentPane") as AvalonDock.Layout.LayoutDocumentPane;

                // ドキュメント閉じる
                DocumentCloseCommand.Subscribe(_ =>
                {
                    if (ADDockingManager.ActiveContent != null)
                    {
                        CloseDocument(ADDockingManager.ActiveContent);
                    }
                });

                // 本一覧ドキュメント追加
                AddDocument("一覧", new BookshelfDocument(), true, false);
            };
        }

        /// <summary>
        /// ドキュメント追加
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="canClose"></param>
        public static void AddDocument(string title, object content, bool actived = true, bool canClose = true, Action<AvalonDock.Layout.LayoutDocument> initAction = null)
        {
            var mainWindow = (Application.Current.MainWindow as MainWindow);
            if (mainWindow == null)
            {
                Log.Warning("MainWindow is null.");
                return;
            }

            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            Log.Info($"add layoutDocument. {title}, {content.GetType().Name}, {actived}, {canClose}");
            var layoutDocument = new AvalonDock.Layout.LayoutDocument();
            layoutDocument.Title = title;
            layoutDocument.Content = content;
            layoutDocument.CanClose = canClose;
            mainWindow.ADLayoutDocumentPane.Children.Add(layoutDocument);

            initAction?.Invoke(layoutDocument);

            // 追加と同時にアクティブ
            if (actived)
            {
                mainWindow.ADDockingManager.ActiveContent = content;
            }
        }

        /// <summary>
        /// ドキュメント閉じる
        /// </summary>
        /// <param name="content"></param>
        public static void CloseDocument(object content)
        {
            var mainWindow = (Application.Current.MainWindow as MainWindow);
            if (mainWindow == null)
            {
                Log.Warning("MainWindow is null.");
                return;
            }

            if (content == null)
            {
                Log.Warning("content is null.");
                return;
            }

            foreach (var doc in mainWindow.ADLayoutDocumentPane.Children)
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
            var mainWindow = (Application.Current.MainWindow as MainWindow);
            if (mainWindow == null)
            {
                Log.Warning("MainWindow is null.");
                return null;
            }

            foreach (var doc in mainWindow.ADLayoutDocumentPane.Children)
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
