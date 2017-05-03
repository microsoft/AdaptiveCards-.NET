﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace AdaptiveCards.Rendering
{
    /// <summary>
    ///     Utility methods for dealing with body and binding
    /// </summary>
    public static class RendererUtilities
    {
        private static readonly Regex TextFunctionRegex =
            new Regex(@"\{\{(?<func>DATE|TIME){1}\((?<date>.+?){1}(?:,\s*(?<hint>Short|Long){1}\s*)??\)\}\}",
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        private static readonly Regex _regexBinding = new Regex(@"(?<property>\{\{\w+?\}\})+?",
            RegexOptions.ExplicitCapture);

        /// <summary>
        ///     This funct will return modified text replacing {{DATE|TIME()}} style functions as the formatted text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ApplyTextFunctions(string text)
        {
            if (text != null)
            {
                foreach (Match match in TextFunctionRegex.Matches(text))
                {
                    Functions function;
                    if (Enum.TryParse(match.Groups[1].Value.ToUpper(), out function))
                    {
                        DateTime date;
                        if (DateTime.TryParse(match.Groups[2].Value, out date))
                        {
                            TimeHints timeHint;
                            if (!Enum.TryParse(match.Groups[3].Value.ToUpper(), out timeHint))
                                timeHint = TimeHints.LONG;

                            var dateTimeFormat = "D";
                            if (function == Functions.DATE)
                                dateTimeFormat = timeHint == TimeHints.LONG ? "D" : "d";
                            else if (function == Functions.TIME)
                                dateTimeFormat = timeHint == TimeHints.LONG ? "T" : "t";
                            text = text.Replace(match.Value, date.ToString(dateTimeFormat));
                        }
                    }
                }
            }
            return text ?? String.Empty;
        }

        /// <summary>
        ///     This function will parse text for {{binding}} statements and replace with appropriate values from the data object
        ///     passed in
        /// </summary>
        /// <param name="data">data to bind fromt</param>
        /// <param name="text"></param>
        /// <param name="url">true if url text is a url (escaping will be applied to the vaue of the binding)</param>
        /// <returns></returns>
        public static string BindData(dynamic data, string text, bool url = false)
        {
            foreach (Match match in _regexBinding.Matches(text))
            {
                var key = match.Value.Trim('{', '}');
                var val = data[key]?.ToString() ?? string.Empty;
                if (url)
                    val = Uri.EscapeDataString(val);
                text = text.Replace(match.Value, val);
            }

            return text;
        }

        private enum Functions
        {
            DATE,
            TIME
        }

        private enum TimeHints
        {
            LONG,
            SHORT
        }

        public static string JoinString(IList<string> choices, string sep, string last)
        {
            var sb = new StringBuilder();
            var s = string.Empty;
            for (var i = 0; i < choices.Count - 1; i++)
            {
                sb.Append(s);
                sb.Append(choices[i]);
                s = sep;
            }
            if (choices.Count > 1)
                sb.Append(last);
            sb.Append(choices.Last());
            return sb.ToString();
        }

        public static T TryGetValue<T>(this IDictionary dictionary, string key)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.Contains(key))
            {
                return (T)dictionary[key];
            }

            return default(T);
        }

        public static T TryGetValue<T>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
                return default(T);

            if (dictionary.ContainsKey(key))
            {
                return (T)dictionary[key];
            }

            return default(T);
        }


    }
}