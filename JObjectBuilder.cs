using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FoxyTools
{
    internal class JObjectBuilder
    {
        public static JObject ProcessDataClass<U>(U data)
        {
            return ProcessDataClass(typeof(U), data);
        }

        public static JObject ProcessDataClass(Type type, object data)
        {
            var result = new JObject();

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).OrderBy(m => m.Name);
            foreach (var member in members)
            {
                dynamic value;
                if (member is FieldInfo field)
                {
                    value = field.GetValue(data);
                }
                else if (member is PropertyInfo property && property.CanRead)
                {
                    value = property.GetValue(data);
                }
                else
                {
                    continue;
                }

                try
                {
                    result.Add(member.Name, new JValue(value));
                }
                catch
                {
                    FoxyToolsMain.Warning($"Can't convert {type.Name}.{member.Name} to simple value");
                }
            }

            return result;
        }
    }

    internal class JObjectBuilder<T> : JObjectBuilder
    {
        public readonly JObject Result;
        private readonly T _source;

        public JObjectBuilder(T source)
        {
            Result = new JObject();
            _source = source;
        }

        public JObjectBuilder<T> With<TVal>(Expression<Func<T, TVal>> expr, Func<TVal, JToken> processor = null)
        {
            TVal value = GetValue(expr, out string propName);
            JToken token = GetToken(value, processor);

            Result.Add(propName, token);
            return this;
        }

        public JObjectBuilder<T> WithEach<TVal>(Expression<Func<T, IEnumerable<TVal>>> expr, Func<TVal, JToken> processor = null)
        {
            IEnumerable<TVal> values = GetValue(expr, out string propName);

            var array = new JArray();
            foreach (TVal value in values)
            {
                JToken token = GetToken(value, processor);
                array.Add(token);
            }
            Result.Add(propName, array);

            return this;
        }

        public JObjectBuilder<T> WithDataClass<U>(Expression<Func<T, U>> expr)
        {
            return With(expr, ProcessDataClass);
        }

        public JObject AsDataClass()
        {
            return ProcessDataClass(_source);
        }

        private TVal GetValue<TVal>(Expression<Func<T, TVal>> expr, out string memberName)
        {
            if (expr.Body is MemberExpression body)
            {
                memberName = body.Member.Name;

                switch (body.Member.MemberType)
                {
                    case MemberTypes.Field:
                        return (TVal)((FieldInfo)body.Member).GetValue(_source);

                    case MemberTypes.Property:
                        return (TVal)((PropertyInfo)body.Member).GetValue(_source);

                    default:
                        throw new ArgumentException("Can't include non field or property member in JSON");
                }
            }
            else
            {
                throw new ArgumentException("Can't include non field or property member in JSON");
            }
        }

        private JToken GetToken<TVal>(TVal value, Func<TVal, JToken> processor)
        {
            if (processor != null)
            {
                return processor(value);
            }
            else
            {
                if (value == null)
                {
                    return JValue.CreateNull();
                }

                try
                {
                    dynamic nonObjVal = value;
                    return new JValue(nonObjVal);
                }
                catch (RuntimeBinderException)
                {
                    if (typeof(TVal).IsValueType)
                    {
                        return new JValue(value.ToString());
                    }

                    return GetToken(value, ProcessDataClass);
                }
            }
        }
    }
}
