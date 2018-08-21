﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetLock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:45:07
    /// </summary>
    public class NewtonsoftJsonSerializer
    {
        public static JsonSerializerSettings Settings { get; private set; }

        static NewtonsoftJsonSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(),
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="value">json格式字符串</param>
        /// <param name="type">返回结果类型</param>
        /// <returns>反序列化对象</returns>
        public object Deserialize(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, Settings);
        }

        /// <summary>
        /// 根据json格式字符串获取对象
        /// </summary>
        /// <typeparam name="T">需要获取的对象</typeparam>
        /// <param name="value">json格式的字符串</param>
        /// <returns></returns>
        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }

        /// <summary>
        /// 异步反序列化
        /// </summary>
        /// <param name="value">json格式字符串</param>
        /// <param name="type">返回结果类型</param>
        /// <returns>反序列化对象</returns>
        public Task<object> DeserializeAsync(string value, Type type)
        {
            return Task.FromResult(Deserialize(value, type));
        }

        /// <summary>
        /// 异步反序列化
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="value">json格式字符串</param>
        /// <returns>反序列化对象</returns>
        public Task<T> DeserializeAsync<T>(string value)
        {
            return Task.FromResult(Deserialize<T>(value));
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns>json格式的字符串</returns>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        /// <summary>
        /// 异步序列化
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns>json格式的字符串</returns>
        public Task<string> SerializeAsync(object obj)
        {
            return Task.FromResult(Serialize(obj));
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jsonProperty = base.CreateProperty(member, memberSerialization);
                if (jsonProperty.Writable) return jsonProperty;
                var property = member as PropertyInfo;
                if (property == null) return jsonProperty;
                var hasPrivateSetter = property.GetSetMethod(true) != null;
                jsonProperty.Writable = hasPrivateSetter;
                return jsonProperty;
            }
        }
    }
}