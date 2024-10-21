using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChartEditor.Utils.Cache
{
    /// <summary>
    /// 管理封面图片缓存
    /// </summary>
    public class CoverImageCache
    {
        private static string logTag = "[CoverImageCache]";
        /// <summary>
        /// 单例
        /// </summary>
        private static readonly Lazy<CoverImageCache> instance = new Lazy<CoverImageCache>(() => new CoverImageCache());
        public static CoverImageCache Instance => instance.Value;

        private Dictionary<string, BitmapImage> imageCache;

        private static int MaxCacheSize = 50;

        private CoverImageCache()
        {
            this.imageCache = new Dictionary<string, BitmapImage>();
        }

        /// <summary>
        /// 从缓存中获取图片，如果缓存中不存在则加载图片
        /// </summary>
        public BitmapImage GetImage(string imagePath)
        {
            if (imageCache.ContainsKey(imagePath))
            {
                return imageCache[imagePath];
            }
            else
            {
                BitmapImage bitmap = ImageUtil.LoadImage(imagePath);
                if (bitmap != null) AddToCache(imagePath, bitmap);
                return bitmap;
            }
        }

        /// <summary>
        /// 将图片从缓存中移除
        /// </summary>
        public void RemoveImage(string imagePath)
        {
            if (imageCache.ContainsKey(imagePath))
            {
                this.imageCache[imagePath] = null;
                this.imageCache.Remove(imagePath);
                Console.WriteLine(logTag + "图片缓存已移除");
                GC.Collect();
            }
        }

        /// <summary>
        /// 将图片添加到缓存中
        /// </summary>
        private void AddToCache(string imagePath, BitmapImage bitmap)
        {
            if (this.imageCache.Count >= MaxCacheSize)
            {
                ClearOldestCache();
            }
            this.imageCache[imagePath] = bitmap;
            Console.WriteLine(logTag + "图片缓存已添加");
        }

        /// <summary>
        /// 清除最早缓存的图片
        /// </summary>
        private void ClearOldestCache()
        {
            if (this.imageCache.Count > 0)
            {
                var firstKey = new List<string>(this.imageCache.Keys)[0];
                this.imageCache[firstKey] = null;
                this.imageCache.Remove(firstKey);
                GC.Collect();
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearCache()
        {
            foreach (var key in imageCache.Keys)
            {
                this.imageCache[key] = null;
            }
            this.imageCache.Clear();
            GC.Collect();
        }
    }
}
