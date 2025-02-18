﻿using System.Collections;
using System.Globalization;
using System.Text;

namespace Feast.Foundation.Core.Utils
{
    /// <summary>
    /// Formatter to convert the named format items like {NamedformatItem} to <see cref="M:string.Format"/> format.
    /// </summary>
    internal class LogValuesFormatter
    {
        private const string NullValue = "(null)";
        private static readonly object[] EmptyArray = Array.Empty<object>();
        private static readonly char[] FormatDelimiters = { ',', ':' };
        private readonly string format;
        private readonly List<string> valueNames = new();

        public LogValuesFormatter(string format)
        {
            OriginalFormat = format;

            var sb = new StringBuilder();
            var scanIndex = 0;
            var endIndex = format.Length;

            while (scanIndex < endIndex)
            {
                var openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                var closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                if (closeBraceIndex == endIndex)
                {
                    sb.Append(format, scanIndex, endIndex - scanIndex);
                    scanIndex = endIndex;
                }
                else
                {
                    // Format item syntax : { index[,alignment][ :formatString] }.
                    var formatDelimiterIndex =
                        FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

                    sb.Append(format, scanIndex, openBraceIndex - scanIndex + 1);
                    sb.Append(valueNames.Count.ToString(CultureInfo.InvariantCulture));
                    valueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                    sb.Append(format, formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1);

                    scanIndex = closeBraceIndex + 1;
                }
            }

            this.format = sb.ToString();
        }

        public string OriginalFormat { get; }
        public List<string> ValueNames => valueNames;

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            var braceIndex = endIndex;
            var scanIndex = startIndex;
            var braceOccurenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurence of '{' or '}'.
                        braceOccurenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurenceCount == 0)
                        {
                            // For '}' pick the first occurence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurence.
                        braceIndex = scanIndex;
                    }

                    braceOccurenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            var findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }

        public string Format(object[]? values)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = FormatArgument(values[i]);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, format, values ?? EmptyArray);
        }

        internal string Format()
        {
            return format;
        }

        internal string Format(object arg0)
        {
            return string.Format(CultureInfo.InvariantCulture,
                format,
                FormatArgument(arg0));
        }

        internal string Format(object arg0, object arg1)
        {
            return string.Format(CultureInfo.InvariantCulture,
                format,
                FormatArgument(arg0),
                FormatArgument(arg1));
        }

        internal string Format(object arg0, object arg1, object arg2)
        {
            return string.Format(CultureInfo.InvariantCulture,
                format,
                FormatArgument(arg0),
                FormatArgument(arg1),
                FormatArgument(arg2));
        }

        public KeyValuePair<string, object> GetValue(object[] values, int index)
        {
            if (index < 0 || index > valueNames.Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            return valueNames.Count > index
                ? new KeyValuePair<string, object>(valueNames[index], values[index])
                : new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues(object[] values)
        {
            var valueArray = new KeyValuePair<string, object>[values.Length + 1];
            for (var index = 0; index != valueNames.Count; ++index)
            {
                valueArray[index] = new KeyValuePair<string, object>(valueNames[index], values[index]);
            }

            valueArray[^1] = new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
            return valueArray;
        }

        private static object FormatArgument(object? value)
        {
            return value switch
            {
                null => NullValue,
                // since 'string' implements IEnumerable, special case it
                string => value,
                // if the value implements IEnumerable, build a comma separated string.
                IEnumerable enumerable => string.Join(", ", enumerable.Cast<object>().Select(o => o ?? NullValue)),
                _ => value
            };
        }

    }
}
