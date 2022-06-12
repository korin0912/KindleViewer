using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;

namespace KindleViewer
{
    public sealed class ImageLoader
    {
        private static ImageLoader _singleton = new ImageLoader();

        public static ImageLoader Instance => _singleton;

        private class LoadData
        {
            public string filePath;
            public Action<BitmapImage> action;
        }

        private Queue<LoadData> loadDatas = new Queue<LoadData>();
        private Dictionary<string, BitmapImage> caches = new Dictionary<string, BitmapImage>();

        private object loaderLockObject = new object();
        private CancellationTokenSource loaderCancelSource = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ImageLoader()
        {
            loadDatas.Clear();
            caches.Clear();
        }

        /// <summary>
        /// 開始
        /// </summary>
        public void Start()
        {
            if (loaderCancelSource != null)
            {
                return;
            }

            loaderCancelSource = new CancellationTokenSource();
            Task.Run(async () => await LoadTaskProc(loaderCancelSource.Token));
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            loaderCancelSource.Dispose();
            loaderCancelSource = null;
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="filePath"></param>
        public void Load(string filePath, Action<BitmapImage> action)
        {
            lock (loaderLockObject)
            {
                if (caches.ContainsKey(filePath))
                {
                    action?.Invoke(caches[filePath]);
                    return;
                }

                loadDatas.Enqueue(new LoadData()
                {
                    filePath = filePath,
                    action = action,
                });
                // Log.Info($"image load enque {filePath}. count={loadDatas.Count}");
            }
        }

        /// <summary>
        /// 画像ロード処理
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        private async Task LoadTaskProc(CancellationToken cancelToken)
        {
            var loads = new List<LoadData>();

            while (!cancelToken.IsCancellationRequested)
            {
                bool loaded = false;

                lock (loaderLockObject)
                {
                    if (loadDatas.Count > 0)
                    {
                        loaded = true;

                        // 10個単位で読みだす
                        loads.Clear();
                        for (var i = 0; i < 10 && loadDatas.Count > 0; i++)
                        {
                            loads.Add(loadDatas.Dequeue());
                        }

                        foreach (var data in loads)
                        {
                            var image = default(BitmapImage);

                            // 画像ロード
                            if (File.Exists(data.filePath))
                            {
                                try
                                {
                                    image = new BitmapImage();
                                    image.BeginInit();
                                    image.UriSource = new Uri(data.filePath);
                                    image.EndInit();

                                    image.Freeze();

                                    if (!caches.ContainsKey(data.filePath))
                                    {
                                        caches.Add(data.filePath, image);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"image load fail. {ex.Message}");
                                }
                            }

                            // 画像を渡す
                            try
                            {
                                data.action?.Invoke(image);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"invoke action fail. {ex.Message}");
                            }
                        }
                    }
                }

                // ロードしてれば 1ms してなければ 100ms 寝る
                await Task.Delay(loaded ? 1 : 100);
            }
        }
    }
}
