﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Easy.Common
{
    public static class TranslateListWithElements
    {
        private static Dictionary<Type, ConvertInstanceDelegate> TranslateICollectionCache
            = new Dictionary<Type, ConvertInstanceDelegate>();

        public static object TranslateToGenericICollectionCache(object from, Type toInstanceOfType, Type elementType)
        {
            ConvertInstanceDelegate translateToFn;
            if (TranslateICollectionCache.TryGetValue(toInstanceOfType, out translateToFn))
                return translateToFn(from, toInstanceOfType);

            var genericType = typeof(TranslateListWithElements<>).MakeGenericType(elementType);
            var mi = genericType.GetStaticMethod("LateBoundTranslateToGenericICollection");
            translateToFn = (ConvertInstanceDelegate)mi.MakeDelegate(typeof(ConvertInstanceDelegate));

            Dictionary<Type, ConvertInstanceDelegate> snapshot, newCache;
            do
            {
                snapshot = TranslateICollectionCache;
                newCache = new Dictionary<Type, ConvertInstanceDelegate>(TranslateICollectionCache);
                newCache[elementType] = translateToFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref TranslateICollectionCache, newCache, snapshot), snapshot));

            return translateToFn(from, toInstanceOfType);
        }

        private static Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate> TranslateConvertibleICollectionCache
            = new Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate>();

        public static object TranslateToConvertibleGenericICollectionCache(
            object from, Type toInstanceOfType, Type fromElementType)
        {
            var typeKey = new ConvertibleTypeKey(toInstanceOfType, fromElementType);
            ConvertInstanceDelegate translateToFn;
            if (TranslateConvertibleICollectionCache.TryGetValue(typeKey, out translateToFn)) return translateToFn(from, toInstanceOfType);

            var toElementType = toInstanceOfType.FirstGenericType().GenericTypeArguments()[0];
            var genericType = typeof(TranslateListWithConvertibleElements<,>).MakeGenericType(fromElementType, toElementType);
            var mi = genericType.GetStaticMethod("LateBoundTranslateToGenericICollection");
            translateToFn = (ConvertInstanceDelegate)mi.MakeDelegate(typeof(ConvertInstanceDelegate));

            Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate> snapshot, newCache;
            do
            {
                snapshot = TranslateConvertibleICollectionCache;
                newCache = new Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate>(TranslateConvertibleICollectionCache);
                newCache[typeKey] = translateToFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref TranslateConvertibleICollectionCache, newCache, snapshot), snapshot));

            return translateToFn(from, toInstanceOfType);
        }

        public static object TryTranslateCollections(Type fromPropertyType, Type toPropertyType, object fromValue)
        {
            var args = typeof(IEnumerable<>).GetGenericArgumentsIfBothHaveSameGenericDefinitionTypeAndArguments(
                fromPropertyType, toPropertyType);

            if (args != null)
            {
                return TranslateToGenericICollectionCache(
                    fromValue, toPropertyType, args[0]);
            }

            var varArgs = typeof(IEnumerable<>).GetGenericArgumentsIfBothHaveConvertibleGenericDefinitionTypeAndArguments(
                fromPropertyType, toPropertyType);

            if (varArgs != null)
            {
                return TranslateToConvertibleGenericICollectionCache(
                    fromValue, toPropertyType, varArgs.Args1[0]);
            }

            return null;
        }

    }

    public class ConvertibleTypeKey
    {
        public Type ToInstanceType { get; set; }
        public Type FromElemenetType { get; set; }

        public ConvertibleTypeKey()
        {
        }

        public ConvertibleTypeKey(Type toInstanceType, Type fromElemenetType)
        {
            ToInstanceType = toInstanceType;
            FromElemenetType = fromElemenetType;
        }

        public bool Equals(ConvertibleTypeKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ToInstanceType, ToInstanceType) && Equals(other.FromElemenetType, FromElemenetType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ConvertibleTypeKey)) return false;
            return Equals((ConvertibleTypeKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ToInstanceType != null ? ToInstanceType.GetHashCode() : 0) * 397)
                    ^ (FromElemenetType != null ? FromElemenetType.GetHashCode() : 0);
            }
        }
    }

    public class TranslateListWithElements<T>
    {
        public static object CreateInstance(Type toInstanceOfType)
        {
            if (toInstanceOfType.IsGeneric())
            {
                if (toInstanceOfType.HasAnyTypeDefinitionsOf(
                    typeof(ICollection<>), typeof(IList<>)))
                {
                    return typeof(List<T>).CreateInstance();
                }
            }

            return toInstanceOfType.CreateInstance();
        }

        public static IList TranslateToIList(IList fromList, Type toInstanceOfType)
        {
            var to = (IList)toInstanceOfType.CreateInstance();
            foreach (var item in fromList)
            {
                to.Add(item);
            }
            return to;
        }

        public static object LateBoundTranslateToGenericICollection(
            object fromList, Type toInstanceOfType)
        {
            if (fromList == null)
                return null; //AOT

            if (toInstanceOfType.IsArray)
            {
                var result = TranslateToGenericICollection(
                    (ICollection<T>)fromList, typeof(List<T>));
                return result.ToArray();
            }

            return TranslateToGenericICollection(
                (ICollection<T>)fromList, toInstanceOfType);
        }

        public static ICollection<T> TranslateToGenericICollection(
            ICollection<T> fromList, Type toInstanceOfType)
        {
            var to = (ICollection<T>)CreateInstance(toInstanceOfType);
            foreach (var item in fromList)
            {
                to.Add(item);
            }
            return to;
        }
    }

    public class TranslateListWithConvertibleElements<TFrom, TTo>
    {
        private static readonly Func<TFrom, TTo> ConvertFn;

        static TranslateListWithConvertibleElements()
        {
            ConvertFn = GetConvertFn();
        }

        public static object LateBoundTranslateToGenericICollection(
            object fromList, Type toInstanceOfType)
        {
            return TranslateToGenericICollection(
                (ICollection<TFrom>)fromList, toInstanceOfType);
        }

        public static ICollection<TTo> TranslateToGenericICollection(
            ICollection<TFrom> fromList, Type toInstanceOfType)
        {
            if (fromList == null) return null; //AOT

            var to = (ICollection<TTo>)TranslateListWithElements<TTo>.CreateInstance(toInstanceOfType);

            foreach (var item in fromList)
            {
                var toItem = ConvertFn(item);
                to.Add(toItem);
            }
            return to;
        }

        private static Func<TFrom, TTo> GetConvertFn()
        {
            if (typeof(TTo) == typeof(string))
            {
                return x => (TTo)(object)TypeSerializer.SerializeToString(x);
            }
            if (typeof(TFrom) == typeof(string))
            {
                return x => TypeSerializer.DeserializeFromString<TTo>((string)(object)x);
            }
            return x => TypeSerializer.DeserializeFromString<TTo>(TypeSerializer.SerializeToString(x));
        }
    }
}
