using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WebCore.Json
{
    public class JsonDeserializationBase
    {
        private static readonly Type[] EmptyTypes = new Type[0];
        private static readonly Dictionary<Type, object> DeserializedTypes = new Dictionary<Type, object>();

        protected internal static Func<BlittableJsonReaderObject, T> GenerateJsonDeserializationRoutine<T>()
        {
            try
            {
                var type = typeof(T);
                var json = Expression.Parameter(typeof(BlittableJsonReaderObject), "json");

                var vars = new Dictionary<Type, ParameterExpression>();

                if (type == typeof(BlittableJsonReaderArray))
                {
                    return null;
                }

                var ctor = type
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.GetParameters().Length == 0);
                var instance = ctor != null
                    ? Expression.New(ctor)
                    : Expression.New(type);

                if (type.GetInterfaces().Contains(typeof(IFillFromBlittableJson)))
                {
                    var obj = Expression.Parameter(type, "obj");
                    var methodToCall = typeof(IFillFromBlittableJson).GetMethod(nameof(IFillFromBlittableJson.FillFromBlittableJson), BindingFlags.Public | BindingFlags.Instance);

                    var returnTarget = Expression.Label(type);

                    var block = Expression.Block(
                        new[] { obj },
                        Expression.Assign(obj, Expression.MemberInit(instance)),
                        Expression.Call(obj, methodToCall, json),
                        Expression.Return(returnTarget, obj, type),
                        Expression.Label(returnTarget, Expression.Default(type))
                    );

                    var l = Expression.Lambda<Func<BlittableJsonReaderObject, T>>(block, json);
                    return l.Compile();
                }

                var propInit = new List<MemberBinding>();
                foreach (var fieldInfo in typeof(T).GetFields())
                {
                    if (fieldInfo.IsStatic || fieldInfo.IsDefined(typeof(JsonDeserializationIgnoreAttribute)))
                        continue;

                    if (fieldInfo.IsPublic && fieldInfo.IsInitOnly)
                        ThrowDeserializationError(type, fieldInfo);

                    propInit.Add(Expression.Bind(fieldInfo, GetValue(fieldInfo.Name, fieldInfo.FieldType, json, vars)));
                }

                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    if (propertyInfo.CanWrite == false || propertyInfo.IsDefined(typeof(JsonDeserializationIgnoreAttribute)))
                        continue;

                    propInit.Add(Expression.Bind(propertyInfo, GetValue(propertyInfo.Name, propertyInfo.PropertyType, json, vars)));
                }

                var lambda = Expression.Lambda<Func<BlittableJsonReaderObject, T>>(Expression.Block(vars.Values, Expression.MemberInit(instance, propInit)), json);
                return lambda.Compile();
            }
            catch (Exception e)
            {
                return FailureBuildingJsonParser<T>(e);
            }
        }

        private static Func<BlittableJsonReaderObject, T> FailureBuildingJsonParser<T>(Exception e)
        {
            return o => throw new InvalidOperationException($"Could not build json parser for {typeof(T).FullName}", e);
        }

        private static void ThrowDeserializationError(Type type, FieldInfo fieldInfo)
        {
            throw new InvalidOperationException($"Cannot create deserialization routine for '{type.FullName}' because '{fieldInfo.Name}' is readonly field");
        }

        private static Expression GetValue(string propertyName, Type propertyType, ParameterExpression json, Dictionary<Type, ParameterExpression> vars)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (type == typeof(string) ||
                type == typeof(bool) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal) ||
                type.GetTypeInfo().IsEnum ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(BlittableJsonReaderArray) ||
                type == typeof(BlittableJsonReaderObject))
            {
                var value = GetParameter(propertyType, vars);

                Type[] genericTypes;
                if (type == typeof(string) || type == typeof(double)) // we support direct conversion to these types
                    genericTypes = EmptyTypes;
                else
                    genericTypes = new[] { propertyType };

                var tryGet = Expression.Call(json, nameof(BlittableJsonReaderObject.TryGet), genericTypes, Expression.Constant(propertyName), value);
                return Expression.Condition(tryGet, value, Expression.Default(propertyType));
            }

            if (type == typeof(TimeSpan))
            {
                var value = GetParameter(propertyType, vars);
                var methodToCall = typeof(JsonDeserializationBase)
                    .GetMethod(propertyType == typeof(TimeSpan) ? nameof(TryGetTimeSpan) : nameof(TryGetNullableTimeSpan), BindingFlags.NonPublic | BindingFlags.Static);

                var tryGet = Expression.Call(methodToCall, json, Expression.Constant(propertyName), value);
                return Expression.Condition(tryGet, value, Expression.Default(propertyType));
            }

            if (propertyType.GetTypeInfo().IsGenericType)
            {
                var genericTypeDefinition = propertyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var valueType = propertyType.GenericTypeArguments[1];
                    if (valueType == typeof(string))
                    {
                        var keyType = propertyType.GenericTypeArguments[0];
                        if (keyType == typeof(string))
                        {
                            var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfString), BindingFlags.NonPublic | BindingFlags.Static);
                            return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                        }
                        if (keyType.GetTypeInfo().IsEnum)
                        {
                            var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfEnumKeys), BindingFlags.NonPublic | BindingFlags.Static)
                                .MakeGenericMethod(keyType);
                            return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                        }
                        throw new NotSupportedException(propertyType.FullName + " is not supported by the deserializer, please add support to it");
                    }
                    if (valueType == typeof(Dictionary<string, string[]>))
                    {
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfDictionaryOfStringArray), BindingFlags.NonPublic | BindingFlags.Static);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                    }
                    if (valueType == typeof(string[]))
                    {
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfStringArray), BindingFlags.NonPublic | BindingFlags.Static);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                    }
                    if (valueType == typeof(List<string>))
                    {
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfStringList), BindingFlags.NonPublic | BindingFlags.Static);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                    }
                    if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var listType = valueType.GenericTypeArguments[0];
                        var converterExpression = Expression.Constant(GetConverterFromCache(listType));
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(listType);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName), converterExpression);
                    }
                    if (valueType.GetTypeInfo().IsEnum)
                    {
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfEnum), BindingFlags.NonPublic | BindingFlags.Static);
                        methodToCall = methodToCall.MakeGenericMethod(valueType);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                    }
                    if (valueType == typeof(long) ||
                        valueType == typeof(double))
                    {
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionaryOfPrimitive), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(valueType);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
                    }
                    else
                    {
                        var converterExpression = Expression.Constant(GetConverterFromCache(valueType));
                        var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToDictionary), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(propertyType.GenericTypeArguments[0], valueType);
                        return Expression.Call(methodToCall, json, Expression.Constant(propertyName), converterExpression);
                    }
                }

                if (propertyType == typeof(List<string>) || propertyType == typeof(HashSet<string>))
                {
                    var method = typeof(JsonDeserializationBase).GetMethod(nameof(ToCollectionOfString), BindingFlags.NonPublic | BindingFlags.Static);
                    method = method.MakeGenericMethod(propertyType);
                    return Expression.Call(method, json, Expression.Constant(propertyName));
                }

                if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(IReadOnlyList<>))
                {
                    var valueType = propertyType.GenericTypeArguments[0];
                    var converterExpression = Expression.Constant(GetConverterFromCache(valueType));
                    var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(valueType);
                    return Expression.Call(methodToCall, json, Expression.Constant(propertyName), converterExpression);
                }
            }

            // Ignore types
            /*if (type == typeof(IDisposable))
            {
                return Expression.Default(type);
            }*/
            if (propertyType == typeof(string[]))
            {
                var method = typeof(JsonDeserializationBase).GetMethod(nameof(ToArrayOfString), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(method, json, Expression.Constant(propertyName));
            }
            if (propertyType.IsArray)
            {
                var valueType = propertyType.GetElementType();
                var converterExpression = Expression.Constant(GetConverterFromCache(valueType));
                var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToArray), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(valueType);
                return Expression.Call(methodToCall, json, Expression.Constant(propertyName), converterExpression);
            }

            // extract proper value from blittable if we have relevant type
            if (propertyType == typeof(object) || propertyType.GetTypeInfo().IsPrimitive)
            {
                var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(GetPrimitiveProperty), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(propertyType);
                return Expression.Call(methodToCall, json, Expression.Constant(propertyName));
            }

            // ToObject
            {
                var converterExpression = Expression.Constant(GetConverterFromCache(propertyType));
                var methodToCall = typeof(JsonDeserializationBase).GetMethod(nameof(ToObject), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(propertyType);
                return Expression.Call(methodToCall, json, Expression.Constant(propertyName), converterExpression);
            }

            // throw new InvalidOperationException($"We weren't able to convert the property '{propertyName}' of type '{type}'.");
        }

        private static object GetConverterFromCache(Type propertyType)
        {
            object converter;
            if (DeserializedTypes.TryGetValue(propertyType, out converter) == false)
            {
                DeserializedTypes[propertyType] = converter = typeof(JsonDeserializationBase)
                    .GetMethod(nameof(GenerateJsonDeserializationRoutine), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(propertyType)
                    .Invoke(null, null);
            }
            return converter;
        }

        private static ParameterExpression GetParameter(Type type, Dictionary<Type, ParameterExpression> vars)
        {
            ParameterExpression value;
            if (vars.TryGetValue(type, out value) == false)
            {
                value = Expression.Variable(type, type.Name);
                vars[type] = value;
            }
            return value;
        }

        private static Dictionary<string, T> ToDictionaryOfPrimitive<T>(BlittableJsonReaderObject json, string name)
            where T : struct
        {
            var dic = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                object val;
                if (obj.TryGetMember(propertyName, out val))
                {
                    dic[propertyName] = (T)val;
                }
            }
            return dic;
        }

        private static Dictionary<TK, TV> ToDictionary<TK, TV>(BlittableJsonReaderObject json, string name, Func<BlittableJsonReaderObject, TV> converter)
        {
            var isStringKey = typeof(TK) == typeof(string);
            var dictionary = new Dictionary<TK, TV>((IEqualityComparer<TK>)StringComparer.Ordinal); // we need to deserialize it as we got it, keys could be case sensitive - DB-8713

            BlittableJsonReaderObject obj;
            if (json.TryGet(name, out obj) == false || obj == null)
                return dictionary;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                object val;
                if (obj.TryGetMember(propertyName, out val) == false)
                    continue;

                dynamic key;
                if (isStringKey)
                    key = propertyName;
                else
                    key = (TK)Convert.ChangeType(propertyName, typeof(TK));

                var typeOfValue = typeof(TV);
                if (typeOfValue.IsConstructedGenericType && typeOfValue.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = typeOfValue.GenericTypeArguments[0];
                    var valueType = typeOfValue.GenericTypeArguments[1];
                    var newConverter = GetConverterFromCache(valueType);
                    var methodInfo = typeof(JsonDeserializationBase)
                        .GetMethod("ToDictionary", BindingFlags.NonPublic | BindingFlags.Static);
                    var method = methodInfo.MakeGenericMethod(keyType, valueType);
                    var result = method.Invoke(null, new[] { obj, key, newConverter });
                    dictionary[key] = (TV)Convert.ChangeType(result, typeOfValue);
                }
                else
                {
                    if (typeOfValue != typeof(object) &&
                        val is BlittableJsonReaderObject blittableJsonReaderObject)
                    {
                        dictionary[key] = converter(blittableJsonReaderObject);
                    }
                    else
                    {
                        if (val is BlittableJsonReaderArray)
                            ThrowNotSupportedBlittableArray(propertyName);

                        obj.TryGet(propertyName, out TV value);
                        dictionary[key] = value;
                    }
                }
            }
            return dictionary;
        }

        private static void ThrowNotSupportedBlittableArray(string propertyName)
        {
            throw new ArgumentException($"Not supported BlittableJsonReaderArray, property name: {propertyName}");
        }

        private static Dictionary<string, TEnum> ToDictionaryOfEnum<TEnum>(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                string val;
                if (obj.TryGet(propertyName, out val))
                {
                    dic[propertyName] = (TEnum)Enum.Parse(typeof(TEnum), val, true);
                }
            }
            return dic;
        }
        private static Dictionary<TEnum, string> ToDictionaryOfEnumKeys<TEnum>(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<TEnum, string>();

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                object val;
                if (obj.TryGet(propertyName, out val))
                {
                    dic[(TEnum)Enum.Parse(typeof(TEnum), propertyName, true)] = val?.ToString();
                }
            }
            return dic;
        }
        private static Dictionary<string, string> ToDictionaryOfString(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                object val;
                if (obj.TryGet(propertyName, out val))
                {
                    dic[propertyName] = val?.ToString();
                }
            }
            return dic;
        }

        private static Dictionary<string, List<T>> ToDictionaryOfList<T>(BlittableJsonReaderObject json, string name, Func<BlittableJsonReaderObject, T> converter)
        {
            var dic = new Dictionary<string, List<T>>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                BlittableJsonReaderArray array;
                if (obj.TryGet(propertyName, out array))
                {
                    var list = new List<T>(array.Length);
                    foreach (BlittableJsonReaderObject item in array)
                    {
                        list.Add(converter(item));
                    }
                    dic[propertyName] = list;
                }
            }
            return dic;
        }

        private static Dictionary<string, List<string>> ToDictionaryOfStringList(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                BlittableJsonReaderArray array;
                if (obj.TryGet(propertyName, out array))
                {
                    var list = new List<string>(array.Length);
                    foreach (object item in array)
                    {
                        list.Add(item?.ToString());
                    }
                    dic[propertyName] = list;
                }
            }
            return dic;
        }

        private static Dictionary<string, string[]> ToDictionaryOfStringArray(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                BlittableJsonReaderArray val;
                if (obj.TryGet(propertyName, out val))
                {
                    var array = new string[val.Length];
                    for (int i = 0; i < val.Length; i++)
                    {
                        array[i] = val[i]?.ToString();
                    }
                    dic[propertyName] = array;
                }
            }
            return dic;
        }

        private static Dictionary<string, Dictionary<string, string[]>> ToDictionaryOfDictionaryOfStringArray(BlittableJsonReaderObject json, string name)
        {
            var dic = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.OrdinalIgnoreCase);

            BlittableJsonReaderObject obj;
            //should a "null" exist in json? -> not sure that "null" can exist there
            if (json.TryGet(name, out obj) == false || obj == null)
                return dic;

            foreach (var propertyName in obj.GetPropertyNames())
            {
                BlittableJsonReaderObject result;
                if (obj.TryGet(propertyName, out result))
                {
                    var prop = new Dictionary<string, string[]>();
                    dic[propertyName] = prop;
                    foreach (var innerPropName in result.GetPropertyNames())
                    {
                        BlittableJsonReaderArray val;
                        if (result.TryGet(innerPropName, out val))
                        {
                            var array = new string[val.Length];
                            for (int i = 0; i < val.Length; i++)
                            {
                                array[i] = val[i]?.ToString();
                            }
                            prop[innerPropName] = array;
                        }
                    }

                }

            }
            return dic;
        }

        private static TCollection ToCollectionOfString<TCollection>(BlittableJsonReaderObject json, string name)
            where TCollection : ICollection<string>, new()
        {
            var collection = new TCollection();

            BlittableJsonReaderArray jsonArray;
            if (json.TryGet(name, out jsonArray) == false || jsonArray == null)
                return collection;

            foreach (var value in jsonArray)
                collection.Add(value.ToString());

            return collection;
        }

        private static string[] ToArrayOfString(BlittableJsonReaderObject json, string name)
        {
            var collection = new List<string>();

            BlittableJsonReaderArray jsonArray;
            if (json.TryGet(name, out jsonArray) == false || jsonArray == null)
                return collection.ToArray();

            foreach (var value in jsonArray)
                collection.Add(value.ToString());

            return collection.ToArray();
        }

        private static T GetPrimitiveProperty<T>(BlittableJsonReaderObject json, string prop)
        {
            if (json.TryGet(prop, out T val) == false)
                ThrowInvalidPrimitiveCastException(prop, typeof(T).Name, json);

            return val;
        }

        private static void ThrowInvalidPrimitiveCastException(string prop, string type, BlittableJsonReaderObject json)
        {
            throw new InvalidCastException($"Failed to fetch property name = {prop} of type {type} from json with value : [{json}]");
        }

        private static T ToObject<T>(BlittableJsonReaderObject json, string name, Func<BlittableJsonReaderObject, T> converter) where T : new()
        {
            if (json.TryGet(name, out BlittableJsonReaderObject obj) == false || obj == null)
            {
                return default;
            }

            return converter(obj);
        }

        private static List<T> ToList<T>(BlittableJsonReaderObject json, string name, Func<BlittableJsonReaderObject, T> converter)
        {
            var list = new List<T>();

            BlittableJsonReaderArray array;
            if (json.TryGet(name, out array) == false || array == null)
                return list;

            foreach (BlittableJsonReaderObject item in array.Items)
                list.Add(converter(item));

            return list;
        }

        private static T[] ToArray<T>(BlittableJsonReaderObject json, string name, Func<BlittableJsonReaderObject, T> converter)
        {
            var list = new List<T>();

            BlittableJsonReaderArray array;
            if (json.TryGet(name, out array) == false || array == null)
                return list.ToArray();

            foreach (BlittableJsonReaderObject item in array.Items)
                list.Add(converter(item));

            return list.ToArray();
        }

        private static bool TryGetTimeSpan(BlittableJsonReaderObject json, string propertyName, out TimeSpan timeSpan)
        {
            if (TryGetNullableTimeSpan(json, propertyName, out var nullableTimeSpan) == false)
            {
                timeSpan = default;
                return false;
            }

            if (nullableTimeSpan.HasValue == false)
                throw new FormatException($"Could not convert 'null' to {typeof(TimeSpan).Name}");

            timeSpan = nullableTimeSpan.Value;
            return true;
        }

        private static bool TryGetNullableTimeSpan(BlittableJsonReaderObject json, string propertyName, out TimeSpan? timeSpan)
        {
            if (json.TryGetMember(propertyName, out var value) == false || value == null)
            {
                timeSpan = null;
                return false;
            }

            if (value is LazyStringValue || value is LazyCompressedStringValue || value is string)
            {
                BlittableJsonReaderObject.ConvertType(value, out timeSpan);
                return true;
            }

            if (value is long l)
            {
                timeSpan = TimeSpan.FromMilliseconds(l);
                return true;
            }

            if (value is LazyNumberValue lnv)
            {
                timeSpan = TimeSpan.FromMilliseconds(lnv);
                return true;
            }

            throw new FormatException($"Could not convert {value.GetType().Name} ('{value}') to {typeof(TimeSpan).Name}");
        }


    }
}
