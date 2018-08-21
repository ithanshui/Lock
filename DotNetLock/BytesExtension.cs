using System;
using System.Text;

namespace DotNetLock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:43:21
    /// </summary>
    public static partial class BytesExtension
    {
        /// <summary>
        /// 将字节数组转为字符串类型（UTF8编码转换）
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>转换后的字符串</returns>
        public static string ToStr(this byte[] bytes)
        {
            return ToStr(bytes, Encoding.UTF8);
        }

        /// <summary>
        /// 将字节数组转为字符串类型
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="encoder">编码格式</param>
        /// <returns>转换后的字符串</returns>
        public static string ToStr(this byte[] bytes, Encoding encoder)
        {
            if (encoder == null)
            {
                throw new ArgumentNullException("Encoding");
            }
            if (bytes == null)
            {
                return null;
            }
            return encoder.GetString(bytes);
        }

        /// <summary>
        /// 将字符串转为字节数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>转换后的字节数组</returns>
        public static byte[] ToBytes(this string str)
        {
            return ToBytes(str, Encoding.UTF8);
        }

        /// <summary>
        /// 将字符串转为字节数组
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="encodeName">转换编码名字</param>
        /// <returns>字节数组</returns>
        public static byte[] ToBytes(this string str, string encodeName)
        {
            return ToBytes(str, Encoding.GetEncoding(encodeName));
        }

        /// <summary>
        /// 将字符串转为字节数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoder">编码格式</param>
        /// <returns>转换后的字节数组</returns>
        public static byte[] ToBytes(this string str, Encoding encoder)
        {
            if (encoder == null)
            {
                throw new Exception("编码信息不能为空");
            }
            if (str == null)
            {
                return null;
            }
            return encoder.GetBytes(str);
        }
    }
}