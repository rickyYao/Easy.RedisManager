﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easy.Common
{
    public static class TextExtensions
    {
        public static string ToCsvField(this string text)
        {
            return string.IsNullOrEmpty(text) || !CsvWriter.HasAnyEscapeChars(text)
                ? text
                : string.Concat
                        (
                            CsvConfig.ItemDelimiterString,
                            text.Replace(CsvConfig.ItemDelimiterString, CsvConfig.EscapedItemDelimiterString),
                            CsvConfig.ItemDelimiterString
                        );
        }

        public static object ToCsvField(this object text)
        {
            return text == null || !CsvWriter.HasAnyEscapeChars(text.ToString())
                ? text
                : string.Concat
                        (
                            CsvConfig.ItemDelimiterString,
                            text.ToString().Replace(CsvConfig.ItemDelimiterString, CsvConfig.EscapedItemDelimiterString),
                            CsvConfig.ItemDelimiterString
                        );
        }

        public static string FromCsvField(this string text)
        {
            return string.IsNullOrEmpty(text) || !text.StartsWith(CsvConfig.ItemDelimiterString, StringComparison.Ordinal)
                    ? text
                    : text.Substring(CsvConfig.ItemDelimiterString.Length, text.Length - CsvConfig.EscapedItemDelimiterString.Length)
                        .Replace(CsvConfig.EscapedItemDelimiterString, CsvConfig.ItemDelimiterString);
        }

        public static List<string> FromCsvFields(this IEnumerable<string> texts)
        {
            var safeTexts = new List<string>();
            foreach (var text in texts)
            {
                safeTexts.Add(FromCsvField(text));
            }
            return safeTexts;
        }

        public static string[] FromCsvFields(params string[] texts)
        {
            var textsLen = texts.Length;
            var safeTexts = new string[textsLen];
            for (var i = 0; i < textsLen; i++)
            {
                safeTexts[i] = FromCsvField(texts[i]);
            }
            return safeTexts;
        }

        public static string SerializeToString<T>(this T value)
        {
            return JsonSerializer.SerializeToString(value);
        }
    }
}
