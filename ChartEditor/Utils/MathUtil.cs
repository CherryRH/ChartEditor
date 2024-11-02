using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 提供数学相关函数
    /// </summary>
    public class MathUtil
    {
        /// <summary>
        /// 计算最大公约数
        /// </summary>
        public static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Math.Abs(a);
        }
    }
}
