using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 实现音乐相关方法
    /// </summary>
    public class MusicUtil
    {
        private static string logTag = "[MusicUtil]";

        /// <summary>
        /// 将MP3文件转换为OGG格式
        /// </summary>
        public static async Task ConvertMp3ToOgg(string inputFilePath, string outputFilePath)
        {
            string ffmpegPath = Common.GetFFmpegPath();

            if (!File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException("FFmpeg.exe未找到");
            }

            // 设置 FFmpeg 命令参数
            var arguments = $"-i \"{inputFilePath}\" -c:a libvorbis -b:a 192k \"{outputFilePath}\"";

            // 创建一个新的进程来运行 FFmpeg
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // 启动进程并等待完成
            process.Start();
            string output = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine(logTag + "mp3到ogg转换失败：" + output);
                throw new Exception(output);
            }
            else
            {
                Console.WriteLine(logTag + $"mp3到ogg转换完成: {outputFilePath}");
            }
        }
    }
}
