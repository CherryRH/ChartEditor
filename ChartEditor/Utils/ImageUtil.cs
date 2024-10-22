using ChartEditor.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        /// <summary>
        /// 打开文件选择窗口并选择一个图片文件，输出文件路径和位图。如果用户关闭了选择窗口，文件路径返回非空值
        /// </summary>
        public static BitmapImage SelectImage(out string imageFileName)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "图片文件 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (openFileDialog.ShowDialog() == true)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    imageFileName = openFileDialog.FileName;
                    return bitmap;
                }
                else
                {
                    imageFileName = "**";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                imageFileName = "";
                return null;
            }
        }

        /// <summary>
        /// 生成曲目封面图片文件路径
        /// </summary>
        public static string GenerateMusicCoverPath(string folderPath)
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderName = $"cover_{timeStamp}.png";
            return Path.Combine(folderPath, folderName);
        }
    }
}
