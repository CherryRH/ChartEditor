using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 图片相关工具类
    /// </summary>
    public class ImageUtil
    {
        private static string logTag = "[ImageUtil]";

        /// <summary>
        /// 将指定路径的图像文件转换为PNG格式并保存到目标路径
        /// </summary>
        public static void ConvertToPng(string sourcePath, string destinationPath)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(sourcePath))
                {
                    bitmap.Save(destinationPath, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + "图片转换到Png失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 加载图片为BitMapImage
        /// </summary>
        public static BitmapImage LoadImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + "加载图片失败: " + ex.Message);
                return null;
            }
        }
    }
}
