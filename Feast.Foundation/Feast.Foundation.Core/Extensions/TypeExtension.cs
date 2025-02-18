﻿using Feast.Foundation.Core.Implements.Converters;
using Feast.Foundation.Core.Interface.Converting;
using System.Runtime.Serialization;

namespace Feast.Foundation.Core.Extensions
{
    public static class TypeExtension
    {
        /// <summary>
        /// 创建未初始化的对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="SerializationException"></exception>
        public static object RawInstance(this Type type) => FormatterServices.GetUninitializedObject(type);
        /// <summary>
        /// 创建未初始化的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RawInstance<T>() => (T)FormatterServices.GetUninitializedObject(typeof(T));
        /// <summary>
        /// 创建一个对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? NewInstance(this Type type) => Activator.CreateInstance(type);
        /// <summary>
        /// 创建一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? NewInstance<T>() => Activator.CreateInstance<T>();

        public static IValueConverter Converter(this Type type, Type toType) =>
            type == toType
                ? NoneConverter.Instance
                : type == typeof(string)
                    ? StringConverterBase.ConvertTo(toType)
                    : throw new NotSupportedException("Not support this type yet");
    }
}
