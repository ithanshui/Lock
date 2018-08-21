namespace DotNetLock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:44:17
    /// </summary>
    public static class JsonSerializeExtention
    {
        /// <summary>
        /// 将对象转换为json格式的字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj"></param>
        /// <returns>json格式的字符串</returns>
        public static string ToJson<T>(this T obj)
        {
            return new NewtonsoftJsonSerializer().Serialize(obj);
        }

        /// <summary>
        /// 将json格式的字符串转为指定对象
        /// 如果json格式字符串格式不对则抛异常
        /// </summary>
        /// <typeparam name="T">要转换的对象类型</typeparam>
        /// <param name="json">json格式字符串</param>
        /// <returns>指定对象的实例</returns>
        public static T ToObjInfo<T>(this string json)
        {
            return new NewtonsoftJsonSerializer().Deserialize<T>(json);
        }
    }
}