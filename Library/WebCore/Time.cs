using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebCore.TimeSpanParserUtil;

namespace WebCore
{
    public class TimeSpanParser
    {
        public static TimeSpan Parse(string text)
        {
            if (TryParse(text, timeSpan: out TimeSpan timeSpan)) return timeSpan;
            throw new ArgumentException("Failed to parse.", nameof(text));
        }

        public static TimeSpan Parse(string text, TimeSpanParserOptions options)
        {
            if (TryParse(text, options, out TimeSpan timeSpan)) return timeSpan;
            throw new ArgumentException("Failed to parse.", nameof(text));
        }

        public static TimeSpan Parse(string text, Units uncolonedDefault, Units colonedDefault)
        {
            if (TryParse(text, uncolonedDefault, colonedDefault, out TimeSpan timeSpan)) return timeSpan;
            throw new ArgumentException("Failed to parse.", nameof(text));
        }


        private static Dictionary<string, Units> _Units;
        protected static Dictionary<string, Units> GetUnitsDict()
        {
            if (_Units == null) _Units = new Dictionary<string, Units> { ["ps"] = Units.Picoseconds, ["picosec"] = Units.Picoseconds, ["picosecs"] = Units.Picoseconds, ["picosecond"] = Units.Picoseconds, ["picoseconds"] = Units.Picoseconds, ["ns"] = Units.Nanoseconds, ["nanosec"] = Units.Nanoseconds, ["nanosecs"] = Units.Nanoseconds, ["nanosecond"] = Units.Nanoseconds, ["nanoseconds"] = Units.Nanoseconds, ["μs"] = Units.Microseconds, ["microsec"] = Units.Microseconds, ["microsecs"] = Units.Microseconds, ["microsecond"] = Units.Microseconds, ["microseconds"] = Units.Microseconds, ["ms"] = Units.Milliseconds, ["millisec"] = Units.Milliseconds, ["millisecs"] = Units.Milliseconds, ["millisecond"] = Units.Milliseconds, ["milliseconds"] = Units.Milliseconds, ["s"] = Units.Seconds, ["sec"] = Units.Seconds, ["secs"] = Units.Seconds, ["second"] = Units.Seconds, ["seconds"] = Units.Seconds, ["m"] = Units.Minutes, ["min"] = Units.Minutes, ["mins"] = Units.Minutes, ["minute"] = Units.Minutes, ["minutes"] = Units.Minutes, ["h"] = Units.Hours, ["hr"] = Units.Hours, ["hrs"] = Units.Hours, ["hour"] = Units.Hours, ["hours"] = Units.Hours, ["d"] = Units.Days, ["day"] = Units.Days, ["days"] = Units.Days, ["w"] = Units.Weeks, ["wk"] = Units.Weeks, ["wks"] = Units.Weeks, ["week"] = Units.Weeks, ["weeks"] = Units.Weeks, ["month"] = Units.Months, ["months"] = Units.Months, ["y"] = Units.Years, ["yr"] = Units.Years, ["yrs"] = Units.Years, ["year"] = Units.Years, ["years"] = Units.Years };
            return _Units;
        }

        private static Regex _UnitsRegex;
        protected static Regex GetUnitsRegex()
        {
            if (_UnitsRegex == null)
            {
                StringBuilder regex = new StringBuilder();
                // can start with any non-letter characters including underscore, which are all ignored.
                regex.Append(@"^(?:[_\W])*(");
                regex.Append(@"?<units>"); // name group
                regex.Append(string.Join("|", GetUnitsDict().Keys.Select(k => Regex.Escape(k))));
                regex.Append(@")\b");
                _UnitsRegex = new Regex(regex.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            return _UnitsRegex;
        }

        protected static Units ParseSuffix(string suffix)
        {
            if (suffix == null) throw new ArgumentNullException();

            var regex = GetUnitsRegex();
            var match = regex.Match(suffix);
            if (!match.Success) return Units.None;

            var dict = GetUnitsDict();
            var units = Units.None;
            var success = dict.TryGetValue(match.Groups["units"].Value.ToLowerInvariant().Trim(), out units);
            //success = success && units != Units.ErrorAmbiguous && units != Units.Error;

            if (!success) return Units.None;

            return units;
        }

        public static bool TryParse(string text, Units uncolonedDefault, Units colonedDefault, out TimeSpan timeSpan)
        {
            var options = new TimeSpanParserOptions()
            {
                UncolonedDefault = uncolonedDefault,
                ColonedDefault = colonedDefault
            };
            return TryParse(text, options, out timeSpan);
        }

        public static bool TryParse(string text, out TimeSpan timeSpan)
        {
            return TryParse(text, null, out timeSpan);
        }

        public static bool TryParse(string text, TimeSpanParserOptions options, out TimeSpan timeSpan)
        {

            try
            {
                var success = TryParse(text, out TimeSpan[] timeSpans, options, 1);
                if (!success) return false;

                if (timeSpans.Length == 0) return false;

                timeSpan = timeSpans[0];
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine($" - exception:'{e}'");
                return false;
            }
        }

        public static bool TryParse(string text, Units uncolonedDefault, Units colonedDefault, out TimeSpan[] timeSpans, int max = int.MaxValue)
        {
            var options = new TimeSpanParserOptions()
            {
                UncolonedDefault = uncolonedDefault,
                ColonedDefault = colonedDefault
            };

            return TryParse(text, out timeSpans, options, max);

        }
        public static bool TryParse(string text, out TimeSpan[] timeSpans, TimeSpanParserOptions options = null, int max = int.MaxValue)
        {
            try
            {
                return DoParseMutliple(text, out timeSpans, options, max);
            }
            catch (ArgumentException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                timeSpans = null;
                return false;
            }
        }

        private static decimal? ParseNumber(string part, TimeSpanParserOptions options)
        {
            if (decimal.TryParse(part, options.NumberStyles, options.FormatProvider, out decimal dPart))
                return dPart;
            return null;
        }
        protected static bool DoParseMutliple(string text, out TimeSpan[] timeSpans, TimeSpanParserOptions options = null, int max = int.MaxValue)
        {
            if (options == null) options = new TimeSpanParserOptions(); // default options object

            Units[] badDefaults = new Units[] { Units.Error, Units.ErrorTooManyUnits, Units.ErrorAmbiguous };
            if (badDefaults.Any(bad => options.UncolonedDefault == bad) || badDefaults.Any(bad => options.ColonedDefault == bad))
                throw new ArgumentException("Bad default selection.");

            // overly limited: requires groups of 3 numbers or fails: https://social.msdn.microsoft.com/Forums/en-US/431d51f9-8003-4c72-ba1f-e830c6ad75ba/regex-to-match-all-number-formats-used-around-the-world?forum=regexp

            text = text.Normalize(NormalizationForm.FormKC); // fixing any fullwidth characters
            text = text.Replace('_', ' ');

            var numberFormatInfo = (options.FormatProvider == null)
                ? CultureInfo.CurrentCulture.NumberFormat
                : NumberFormatInfo.GetInstance(options.FormatProvider);

            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            bool allowThousands = ((options.NumberStyles & NumberStyles.AllowThousands) > 0);
            string groupSeparator = allowThousands ?
                Regex.Escape(numberFormatInfo.NumberGroupSeparator) : string.Empty;
            string plusMinus = numberFormatInfo.PositiveSign + numberFormatInfo.NegativeSign; // TODO?

            if (options.AllowDotSeparatedDayHours && decimalSeparator != ".") decimalSeparator += "."; // always also need a dot for day.hour separation (unless that's off)

            string zeroRegexStr = @"([+-]?:)?(([-+]?[0" + groupSeparator + "]*[" + Regex.Escape(decimalSeparator) + @"}]?[0]+(?:[eE][-+]?[0-9]+)?)\:?)+"; // 0:00:00 0e100 0.00:00:00:0.000:0e20:00
            string numberRegexStr;
            // +- at start or end depending on culture
            if (allowThousands)
            {
                numberRegexStr = @"([+-]?:)?(([-+]?([0-9]+([" + groupSeparator + "]?)(?=[0-9]))*[" + Regex.Escape(decimalSeparator) + @"}]?[0-9]+(?:[eE][-+]?[0-9]+)?)\:?)+";
            }
            else
            {
                numberRegexStr = @"([+-]?:)?(([-+]?[0-9]*[" + Regex.Escape(decimalSeparator) + @"}]?[0-9]+(?:[eE][-+]?[0-9]+)?)\:?)+";
            }

            // regex notes:
            // - floating point numbers separated by (or ending with) with colon.
            // - matches a number: 30
            // - also matches floating point number: +3e-10
            // - also allows colons: 10:20:21.70
            // - or crazy combo: 10.2e+2:20:21.70 (note: the dot is sometimes a day separator)
            // - regex101.com for testing

            // weird things:
            // - supports mixed formats like "22:11h 10s" (=22:11:10)

            // may change:
            // - starting colon will be ignored, ":30" treated as "30"
            // - but not after: 3: (treated as "3")
            // - in future, starting-colon numbers may get their own option


            var numberRegex = new Regex(numberRegexStr); //  re-use regex + RegexOptions.Compiled
            var zeroRegex = new Regex(zeroRegexStr);

            List<ParserToken> tokens = new List<ParserToken>();

            var matches = numberRegex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            { //  foreach (Match match in matches) {
                Match match = matches[i];

                int numberEnd = match.Index + match.Length;
                int nextMatchIndex = (i + 1 < matches.Count ? matches[i + 1].Index : text.Length);
                int suffixLength = nextMatchIndex - numberEnd;

                //Console.WriteLine($"text:{text}. match[{i}]: suffixLength:{suffixLength}");

                string number = match.Value;
                string suffix = text.Substring(numberEnd, suffixLength);
                bool coloned = number.Contains(':');

                //Console.WriteLine($"part[{i}]: num:'{number}', suffix:'{suffix}', colon:{coloned}");

                Units suffixUnits = ParseSuffix(suffix);

                // ignore initial colon (now) if requested

                if (coloned)
                {
                    var parts = number.Split(':');
                    if (parts.Length <= 1)
                    {
                        timeSpans = null; //  timeSpans = builder.FinalSpans(); // foundTimeSpans.ToArray();
                        return false; // something went wrong. should never happen
                    }

                    var token = new ColonedToken();
                    token.options = options;
                    token.GivenUnit = suffixUnits;

                    // maybe don't do this if parsing a localization that doesn't use a dot separator for days.months ?
                    if (parts != null && parts.Length >= 1 && parts[0].Contains('.'))
                    {
                        token.firstColumnContainsDot = true; //Note: specifically '.' and NOT the regional decimal separator
                        token.firstColumnRightHalf = ParseNumber(parts[0].Split('.')[1], options); // error checking
                    }

                    if (string.IsNullOrWhiteSpace(parts[0]))
                    {
                        // TODO
                        token.startsWithColon = true;
                        parts[0] = null;

                    }
                    else if (parts != null && parts.Length >= 1 && parts[0] != null && parts[0].Trim() == "-")
                    {
                        //don't attempt to parse
                        parts[0] = null;
                        token.negativeColoned = true;
                        token.startsWithColon = true;

                    }
                    else if (parts != null && parts.Length >= 1 && parts[0] != null && parts[0].Trim() == "+")
                    { //TODO tidy
                        parts[0] = null;
                        token.startsWithColon = true;
                    }

                    token.colonedColumns = parts.Select(p => ParseNumber(p, options)).ToArray();
                    tokens.Add(token);

                    //Console.WriteLine($"token: {token}");

                }
                else
                {

                    //decimal parsedNumber;
                    //bool numberSuccess = decimal.TryParse(number, options.NumberStyles, options.FormatProvider, out parsedNumber);

                    var token = new OneUnitToken();
                    token.options = options;
                    token.GivenUnit = suffixUnits;
                    token.uncolonedValue = ParseNumber(number, options);

                    tokens.Add(token);

                    //Console.WriteLine($"token= {token}");
                }
            }

            List<TimeSpan?> timespans = new List<TimeSpan?>();
            ParserToken last = null;
            bool willSucceed = true;
            foreach (ParserToken token in tokens)
            {
                if (token.IsUnitlessFailure() || token.IsOtherFailure())
                {
                    //Console.WriteLine($"wont succeed..." + (!options.FailOnUnitlessNumber ? "or actually it might" : ""));
                    //throw new ArgumentException("failed to parse because of a unitless number.");
                    willSucceed = false;
                    if (last != null)
                        timespans.Add(last.ToTimeSpan());
                    last = null;
                    continue;
                }

                if (last != null)
                {
                    bool success = last.TryMerge(token, out ParserToken newToken);
                    if (!success)
                        throw new ArgumentException("Failed to parse. Probably because of a unitless number.");

                    if (newToken == null)
                    {
                        timespans.Add(last.ToTimeSpan());
                        last = token;

                    }
                    else
                    {
                        last = newToken;
                    }

                }
                else
                {
                    last = token;

                }
            }
            if (last != null)
                timespans.Add(last.ToTimeSpan());

            timeSpans = timespans.Where(t => t.HasValue).Select(t => t.Value).ToArray(); // just the nonnull for now
            return !options.FailOnUnitlessNumber || willSucceed;
        }


        public static bool TryParsePrefixed(string text, string[] prefixes, Units uncolonedDefault, Units colonedDefault, out Dictionary<string, TimeSpan?> matches)
        {
            return TryParsePrefixed(text, prefixes, null, uncolonedDefault, colonedDefault, out matches);
        }

        /// <summary>
        /// Note: a special entries matches["0"] matches["1"] etc are included if `text` starts with timespans.
        /// </summary>
        /// <param name="text"></param>34
        /// <param name="uncolonedDefault"></param>
        /// <param name="colonedDefault"></param>
        /// <param name="prefixes">Prefixes which are (optionally) followed by a timespan</param>
        /// <param name="keywords">Keywords which do not have a timespan variable after them (any timespan after it will be considered a default numbered argument)</param>
        /// <param name="matches"></param>
        /// <returns></returns>
        public static bool TryParsePrefixed(string text, string[] prefixes, string[] keywords, Units uncolonedDefault, Units colonedDefault, out Dictionary<string, TimeSpan?> matches)
        {
            var options = new TimeSpanParserOptions
            {
                UncolonedDefault = uncolonedDefault,
                ColonedDefault = colonedDefault
            };
            return TryParsePrefixed(text, prefixes, keywords, options, out matches);
        }
        public static bool TryParsePrefixed(string text, string[] prefixes, out Dictionary<string, TimeSpan?> matches)
        {
            return TryParsePrefixed(text, prefixes, null, out matches);
        }

        public static bool TryParsePrefixed(string text, string[] prefixes, TimeSpanParserOptions options, out Dictionary<string, TimeSpan?> matches)
        {
            return TryParsePrefixed(text, prefixes, null, options, out matches);
        }

        public static bool TryParsePrefixed(string text, string[] prefixes, string[] keywords, TimeSpanParserOptions options, out Dictionary<string, TimeSpan?> matches)
        {
            //string[] prefixes = new string[] { "for", "in", "delay", "wait" };

            // rename "prefixes" to "parameter names" or "keys" or something

            bool specialColonPrefix = false; // matches[":"] is the first unnamed timespan starting with a ":", used only if prefixes contains ":".

            if (options == null) options = new TimeSpanParserOptions();

            matches = new Dictionary<string, TimeSpan?>();

            //e.g. string pattern = @"\b(for|in|delay|now|wait)\b";
            // replace spaces with any amount of whitespace (currently @"\ ") e.g. "[\s.']*" (spaces dots or ' ) // perhaps do replacements first to make it easier, e.g. replace "

            var wordsList = Enumerable.Empty<string>();

            if (keywords != null)
                wordsList = wordsList.Concat(keywords);

            if (prefixes != null)
            {
                wordsList = wordsList.Concat(prefixes);

                if (prefixes.Contains(":"))
                {
                    specialColonPrefix = true;
                    wordsList = wordsList.Where(w => w != ":");
                }
            }

            // must be in (brackets) to be included in results of regex split
            // @"?<keyword>"; // name group
            //string start = @"(?:\b|_\K|[0-9]\K)"; // was: @"\b" but want to use numbers as boundaries too
            //string end = @"(?=\b|_|[0-9])"; // was: @"\b"
            string start = @"(?<=\b|_|[0-9])"; // was: @"\b" but want to use numbers as boundaries too
            string end = @"(?=\b|_|[0-9])"; // was: @"\b"

            string pattern = start + @"(" + string.Join("|", wordsList.Select(word => Regex.Escape(word))) + @")" + end;

            var regex = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            string[] parts = regex.Split(text);
            //Console.WriteLine("pattern: " + pattern.ToString());
            //Console.WriteLine(string.Join("//", parts));

            int nonkeywordCounter = 0;
            string currentPrefix = null;

            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    var lc = part.ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(lc))
                    {
                        continue;

                    }
                    else if (keywords != null && keywords.Contains(lc))
                    { // keyword with no arguments
                        matches[lc] = null;
                        currentPrefix = null;


                    }
                    else if (prefixes != null && prefixes.Contains(lc))
                    { // keyword (prefix) with possible argument
                        matches[lc] = null;
                        currentPrefix = lc;

                    }
                    else
                    {
                        if (DoParseMutliple(part, out TimeSpan[] timespans, options))
                        {

                            for (int j = 0; j < timespans.Length; j++)
                            {
                                string prefix = currentPrefix
                                    ?? (specialColonPrefix && j == 0 && part.TrimStart().StartsWith(":") && !matches.ContainsKey(":") ? ":" : null) // use ":" as the prefix name under special circumstances .. note/bug: only works for the first timespan (j==0), if there's multiple in a row. TODO
                                    ?? nonkeywordCounter++.ToString();

                                matches[prefix] = timespans[j];
                                currentPrefix = null;
                            }

                        }
                        else
                        {
                            if (options.FailOnUnitlessNumber)
                                return false;
                        }
                    }
                }
            }
            catch (ArgumentException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                if (options.FailOnUnitlessNumber)
                    return false;
            }


            return true; //return (matches.Count > 0);
        }
    }

    namespace TimeSpanParserUtil
    {
        public enum Units { None, Error, ErrorAmbiguous, Years, Months, Weeks, Days, Hours, Minutes, Seconds, Milliseconds, Microseconds, Nanoseconds, Picoseconds, ErrorTooManyUnits, ZeroOnly }

        public static class UnitsExtensions
        {
            public static bool IsTimeUnit(this Units unit)
            {
                return (unit >= Units.Years && unit <= Units.Picoseconds);
            }
        }

        //See also:
        //https://github.com/ploeh/Numsense

        //based on: https://stackoverflow.com/a/11278252/443019
        public class EnglishNumberParser
        {
            static string[] ones = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            static string[] teens = { "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            static string[] tens = { "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            static int ParseEnglish(string number)
            {
                string[] words = number.ToLower().Split(new char[] { ' ', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, int> modifiers = new Dictionary<string, int>() {
                    {"billion", 1000000000},
                    {"million", 1000000},
                    {"thousand", 1000},
                    {"hundred", 100}
             };

                //if (number == "eleventy billion")
                //    return int.MaxValue; // 110,000,000,000 is out of range for an int!

                int result = 0;
                int currentResult = 0;
                int lastModifier = 1;

                foreach (string word in words)
                {
                    if (modifiers.ContainsKey(word))
                    {
                        lastModifier *= modifiers[word];
                    }
                    else
                    {
                        int n;

                        if (lastModifier > 1)
                        {
                            result += currentResult * lastModifier;
                            lastModifier = 1;
                            currentResult = 0;
                        }

                        if ((n = Array.IndexOf(ones, word) + 1) > 0)
                        {
                            currentResult += n;
                        }
                        else if ((n = Array.IndexOf(teens, word) + 1) > 0)
                        {
                            currentResult += n + 10;
                        }
                        else if ((n = Array.IndexOf(tens, word) + 1) > 0)
                        {
                            currentResult += n * 10;
                        }
                        else if (word != "and" && word != "a")
                        {
                            throw new ArgumentException("Unrecognized word: " + word);
                        }
                    }
                }

                return result + currentResult * lastModifier;
            }
        }

        public class TimeSpanParserOptions
        {
            // How to treat the first number, if it has no units (and no colon)
            // e.g. Units.Minutes will mean "4" gets treated as "4 minutes"
            public Units UncolonedDefault = Units.None;

            // How to treat the first number, if it has no units and does have a colon.
            // e.g. Units.Minutes will mean "4:30" gets treated as a 4m30s
            // valid things are: Days, Hours, Minutes
            public Units ColonedDefault = Units.Hours;

            //NYI TODO and make true by default
            public bool FailIfMoreTimeSpansFoundThanRequested = false;

            // e.g. if true and ColonedDefault = Units.Minutes, parse "05:10:30" as "05h10m30s" rather than failing
            // change into "Segments must match units" or something
            public bool AutoUnitsIfTooManyColons = true;

            // "1.12:13" becomes "1d 12h 13m" (regardless of ColonedDefault). Only works for coloned numbers.
            // But won't if already has four parts, e.g. 1.2:20:40:50 (move this to testing docs)
            // if AutoUnitsIfTooManyColons == false, then must specify "days" as unit (text or ColonedDefault)
            // (or what about None?)
            public bool AllowDotSeparatedDayHours = true;

            // implement
            // If true, treat :30 the same as 30, and :10:30 the same as 10:30. Especially useful if you're parsing input like "time:30" (where "time:" is to be ignored)
            // If false (NYI), treat :30 like 0:30, and :10:30 like 00:10:30
            public readonly bool IgnoreStartingColon = true;

            // handling of empty colons, e.g. 10::30 // treat as 10:30 or 10:00:30 (or error) ?

            // If true then allow "3 hours 5 seconds" but not "5 seconds 3 hours"
            // (causes AddUnit() to return false, signally an error or to divides into multiple TimeSpans)
            public bool StrictBigToSmall = true;

            // [remove as redundant to StrictBigToSmall]
            // If false, allow "1h 10m 5m" to equal 1h15m;
            // If true, cause an error (or divide into multiple TimeSpans) when a unit is repeated.
            // If StrictBigToSmall is true then it basically overrides this as if it were true anyway.
            // public bool DisallowRepeatedUnit = true;

            // If true and StrictBigToSmall, disallow a timespan with both milliseconds and seconds with decimal point.
            // if true and StrictBigToSmall, disallow "10.5 seconds 200 milliseconds", otherwise treat it like "10.7 seconds"
            //  something for all units with decimals, e.g. to force "10.5min 40s" to fail
            public bool DecimalSecondsCountsAsMilliseconds = false;

            // Apart AllowUnitlessZero, covered below,
            // should unitless numbers with no default unit cause parsing to fail?
            // If false, just ignore them
            public bool FailOnUnitlessNumber = true;

            // If true, a 0 or 0:00 by itself doesn't need any units, even if FailOnUnitlessNumber is true. (todo: just say "see GuideTests for details")
            // If true, "0" will be parsed as TimeSpan.Zero even if UncolonedDefault = Units.None.
            // Likewise "0:00" or "0:0:0:0" etc will be parsed even if and ColonedDefault is set to Units.None.
            // if !StrictBigToSmall, then multiple zeros can be parsed (and ignored) in the same timespan.
            // Otherwise it will parse a unitless "0" only for the first unit, and not allow further units
            public bool AllowUnitlessZero = true;

            // if true, units which are too small will cause an OverflowException. E.g. 1 nanosecond (which is 100x smaller than the smallest unit, a tick).
            //public bool ErrorOnTooSmallOutOfRange = true;
            // options: Overflow if any element is too smaller; Overflow if entire TimeSpan is too small; Round to zero
            // same for overflow of too large elements

            // options as binary flags?

            // default to very permissive, but do not: AllowTrailingSign, AllowParentheses, AllowCurrencySymbol
            public NumberStyles NumberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowThousands | NumberStyles.AllowTrailingWhite;

            // normal initial negative handling. Should -30h 30m mean -30:30h or -30h +30m (-29.5h).. argh.
            //public bool WeirdNegativeHandling = false;

            // For parsing numbers, etc
            // Defaulting to CultureInfo.InvariantCulture for now until support is improved.
            // In future, will default to CultureInfo.CurrentCulture
            // Not fully supported yet. Does support changing the units decimal separator but then might fail on a US-style coloned number where .net's TimeSpan.Parser would not
            public IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        }


        abstract class ParserToken
        {

            public TimeSpanParserOptions options;
            public Units GivenUnit = Units.None;

            public override string ToString()
            {
                return ToTimeSpan().ToString() ?? "null";
            }

            public abstract bool IsNull();

            public abstract bool IsZero();

            public abstract bool IsInitialNegative();

            public abstract bool UsesColonedDefault();

            public virtual bool IsUnitlessFailure()
            {
                var units = BestGuessUnits();
                bool unitless = !(units.IsTimeUnit() || units == Units.ZeroOnly);
                bool ambiguous = !IsZero() && (units == Units.Months || units == Units.Years);

                return unitless || ambiguous;

                // what about IsNull() ?
            }

            public virtual bool IsOtherFailure()
            {
                var smallest = SmallestUnit();

                return (!smallest.IsTimeUnit() && smallest != Units.ZeroOnly);
            }

            protected virtual Units GivenOrDefaultOrZeroUnits()
            {
                if (GivenUnit != Units.None)
                    return GivenUnit;

                Units def = Units.None;
                if (UsesColonedDefault())
                {
                    def = options.ColonedDefault;

                }
                else
                {
                    def = options.UncolonedDefault;
                }

                if (def == Units.None && IsZero() && options.AllowUnitlessZero)
                    return Units.ZeroOnly;

                return def; //  check is valid (don't give error values?)
            }


            public virtual Units BestGuessUnits()
            { // GivenOrDefaultOrSplitUnits
                return GivenOrDefaultOrZeroUnits();
            }


            /// <summary>
            ///
            /// </summary>
            /// <param name="otherNext"></param>
            /// <param name="merged"></param>
            /// <returns>Success:
            ///     False: there was a failure and parsing should fail.
            ///     True && merged == null: did not merge, but continue on.
            ///     True && merged != null: did merge, continue on.</returns>
            public bool TryMerge(ParserToken otherNext, out ParserToken merged)
            {
                if (otherNext == null)
                {
                    merged = null;
                    return false; // shouldn't happen?
                }

                var otherUnits = otherNext.BestGuessUnits();

                if (otherUnits == Units.None || otherUnits == Units.ErrorTooManyUnits)
                { // && options.FailOnUnitlessNumber
                  //Console.WriteLine("error thing");
                    merged = null;
                    return false;
                }

                var otherTimeSpan = otherNext.ToTimeSpan();
                if (IsInitialNegative())
                    otherTimeSpan = -otherNext.ToTimeSpan();

                if (options.StrictBigToSmall)
                {
                    var smallestUnit = SmallestUnit();

                    //Console.WriteLine($"combining: {ToTimeSpan()?.ToString() ?? "null"} ({smallestUnit}) & {otherNext.ToTimeSpan()?.ToString() ?? "null"} ({otherUnits})");
                    if (otherUnits.IsTimeUnit() && smallestUnit.IsTimeUnit() && otherUnits > smallestUnit)
                    {

                        var newTokenWithSmallest = new TimeSpanToken();
                        newTokenWithSmallest.options = options;
                        newTokenWithSmallest.timespan = this.ToTimeSpan() + otherTimeSpan;
                        newTokenWithSmallest.smallest = otherNext.SmallestUnit();
                        newTokenWithSmallest.GivenUnit = BestGuessUnits(); // shouldn't matter
                        newTokenWithSmallest.initialNegative = IsInitialNegative();

                        merged = newTokenWithSmallest;
                        return true;
                    }

                    merged = null;
                    return true;
                }

                var newToken = new TimeSpanToken();
                newToken.options = options;
                newToken.timespan = this.ToTimeSpan() + otherTimeSpan;
                newToken.initialNegative = IsInitialNegative();

                //newToken.smallest = otherNext.SmallestUnit(); // shouldn't matter because not StrictBigToSmall
                //newToken.GivenUnit = BestGuessUnits(); // shouldn't matter
                merged = newToken;
                return true;
            }

            public abstract TimeSpan? ToTimeSpan();

            protected virtual Units SmallestUnit()
            {
                return BestGuessUnits();
            }

            protected Units NextSmallestUnit(Units unit, int next = 1)
            {
                if (unit == Units.None)
                    return Units.None;

                if (unit == Units.ZeroOnly)
                {
                    return Units.ZeroOnly; // return Units.None;
                }

                if (unit.IsTimeUnit())
                {
                    var nextSmallest = unit + next;
                    if (nextSmallest.IsTimeUnit())
                        return nextSmallest;

                    return Units.ErrorTooManyUnits;
                }

                return Units.None; // or return unit? (may be an error)
            }

            protected static TimeSpan? GetValue(decimal? time, Units unit)
            {
                if (time == null || unit == Units.None)
                    return null;

                if (unit == Units.Years)
                {
                    if (time == 0)
                        return TimeSpan.Zero;
                    return null; // Ambiguous / error (support in future maybe)

                }
                else if (unit == Units.Months)
                {
                    if (time == 0)
                        return TimeSpan.Zero;
                    return null; // Ambiguous / errror (support in future maybe)

                }
                else if (unit == Units.Weeks)
                {
                    return TimeSpan.FromDays((double)time * 7);

                }
                else if (unit == Units.Days)
                {
                    return TimeSpan.FromDays((double)time);

                }
                else if (unit == Units.Hours)
                {
                    return TimeSpan.FromHours((double)time);

                }
                else if (unit == Units.Minutes)
                {
                    return TimeSpan.FromMinutes((double)time);

                }
                else if (unit == Units.Seconds)
                {
                    //timeSpan += TimeSpan.FromSeconds(time); // not very accurate, use ticks instead.

                    long ticks = (long)(time * 10_000_000);
                    return TimeSpan.FromTicks(ticks);

                    /*
                    if (Options.DecimalSecondsCountsAsMilliseconds) {
                        var timeTrunc = Math.Truncate(time);
                        if (time != timeTrunc) {
                            DoneUnits.Add(Units.Milliseconds);
                        }
                    }
                    */

                }
                else if (unit == Units.Milliseconds)
                {
                    //return TimeSpan.FromMilliseconds((double)time);
                    //TODO overflow checking
                    long ticks = (long)(time * 10_000);
                    return TimeSpan.FromTicks(ticks);

                }
                else if (unit == Units.Microseconds)
                {
                    var absTime = Math.Abs(time.Value);
                    if (absTime > 0 && absTime < new decimal(0.1))
                    {
                        throw new OverflowException("A component of the timespan was out of range (too small).");
                    }

                    long ticks = (long)(time * 10);
                    return TimeSpan.FromTicks(ticks);

                }
                else if (unit == Units.Nanoseconds)
                {
                    var absTime = Math.Abs(time.Value);
                    if (absTime > 0 && absTime < 100)
                    {
                        throw new OverflowException("A component of the timespan was out of range (too small).");
                    }

                    long ticks = (long)(time / 100);
                    return TimeSpan.FromTicks(ticks);

                }
                else if (unit == Units.Picoseconds)
                {
                    var absTime = Math.Abs(time.Value);
                    if (absTime > 0 && absTime < 100_000)
                    {
                        throw new OverflowException("A component of the timespan was out of range (too small).");
                    }

                    long ticks = (long)(time / 100_000);
                    return TimeSpan.FromTicks(ticks);

                }
                else if (unit == Units.ZeroOnly)
                {
                    // do nothing
                    return TimeSpan.Zero;

                }

                return TimeSpan.Zero; //  error?
            }

        }

        class ColonedToken : ParserToken
        {

            //public bool coloned; // use colonedColumns --  make a separate subclass
            public bool negativeColoned; // started with a negative sign, not included in numbers
                                         //public bool zeroOnly; // == IsZero()
            public bool startsWithColon;

            Units Autounit = Units.None; // e.g. if ColonedDefault = Units.Minutes, parse "05:10:30" as "05h10m30s" rather than failing
            Units SplitUnits = Units.None; // if we had to split the day column into days.hours, then SplitUnits will be Units.Days.

            public bool firstColumnContainsDot;  //Note: specifically '.' and NOT the regional decimal separator
            public decimal? firstColumnRightHalf = null;

            // units apply to the first column, i.e. colonedColumns[0]
            // a null value means an empty column, e.g. 10::30
            // if [0] == null then number started with a colon.
            // [0] may be later separated into days.hours at the decimal point.
            public decimal?[] colonedColumns;
            decimal?[] startingColonRemovedColumns; // same as above but the first entry removed because it was empty, e.g. [null]:30
            decimal?[] splitColonedColumns; // same as above but days.hours have been split if needed
            bool calcDone = false;

            public override bool UsesColonedDefault()
            {
                return true;
            }

            public override bool IsNull()
            {
                return colonedColumns == null || colonedColumns.All(c => c == null);
            }

            public override bool IsZero()
            {
                if (IsNull()) return false; // technically not zero

                return colonedColumns.Any(c => c == 0) && colonedColumns.All(c => c == 0 || c == null);
            }

            private bool IsFirstColNegative()
            {
                var cols = Columns();
                if (cols.Length >= 1 && cols[0].HasValue && cols[0].Value < 0)
                    return true;

                return false;

            }
            public override bool IsInitialNegative()
            {
                return (negativeColoned || IsFirstColNegative());
            }

            protected decimal?[] Columns()
            {
                CalcSplitDays();

                return startingColonRemovedColumns ?? splitColonedColumns ?? colonedColumns;

            }
            public override TimeSpan? ToTimeSpan()
            {
                if (IsNull())
                    return null;

                var columns = Columns();
                if (columns == null)
                    return null;

                Units units = BestGuessUnits();
                bool flip = IsFirstColNegative() && !negativeColoned; // inverse all numbers, not just the first. But if negativeColoned, flip whole thing at the end instead.

                bool first = true;
                TimeSpan sum;
                foreach (var c in columns)
                {
                    if (c != null)
                    {
                        if (!first && c != 0 && units > Units.Seconds)
                        {
                            throw new FormatException("Too many units or colons");
                        }

                        if (!units.IsTimeUnit() && units != Units.ZeroOnly && c != 0)
                        {
                            throw new FormatException("Bad units: " + units);
                        }

                        if (!first && flip)
                        {
                            sum += GetValue(-c, units).Value;
                        }
                        else
                        {
                            sum += GetValue(c, units).Value;
                        }
                    }

                    units++;
                    first = false;

                }
                if (negativeColoned) sum = -sum;
                return sum;

            }


            protected bool ShouldSplitDaysHours()
            {
                //TODO future: throw new FormatException("Multiple dots. Don't know where to cut days and hour."); // e.g. "1.2.3" (1.2 days + 3 hours, or 1 day, 2.3 hours) // may require a special custom token or something / guessing rules... but these wouldn't be found by the initial regex anyway

                // maybe should also split OneUnitToken. e.g. 1.7 days ? maybe require another option

                var expected = GivenOrDefaultOrZeroUnits();
                return (options.AllowDotSeparatedDayHours && firstColumnContainsDot && firstColumnRightHalf != null && colonedColumns[0].HasValue) // though presumably it has a value because firstColumnContainedPeriod
                    && (expected == Units.Days
                     || ((colonedColumns.Length == 2 | colonedColumns.Length == 3)
                        && (expected == Units.Hours || expected == Units.None)));
            }

            protected bool ShouldIgnoreStartingColon()
            {
                return options.IgnoreStartingColon && colonedColumns.Length >= 2 && colonedColumns[0] == null;
            }

            protected void CalcSplitDays()
            {
                if (calcDone)
                    return;

                if (ShouldIgnoreStartingColon())
                {
                    //not truly ignoring it. //TODO
                    startingColonRemovedColumns = colonedColumns.Skip(1).ToArray();

                    Autounit = AutoUnits();

                }
                else if (ShouldSplitDaysHours())
                {
                    List<decimal?> splitDays = new List<decimal?>();
                    var daysHours = colonedColumns[0].Value;
                    var days = decimal.Truncate(colonedColumns[0].Value);
                    var hours = firstColumnRightHalf; // daysHours - days * Math.Pow(10, places));
                    splitDays.Add(days);
                    splitDays.Add(hours);
                    splitDays.AddRange(colonedColumns.Skip(1));
                    splitColonedColumns = splitDays.ToArray();

                    Autounit = Units.Days;
                    calcDone = true;
                    return;

                }

                Autounit = AutoUnits();
                calcDone = true;

            }

            protected Units AutoUnits()
            {
                if (!options.AutoUnitsIfTooManyColons)
                {
                    return Units.None;
                }

                if (splitColonedColumns != null)
                    return Units.Days;

                var columns = startingColonRemovedColumns ?? colonedColumns;
                var parts = columns.Length;
                var probableUnits = GivenOrDefaultOrZeroUnits();

                // check for bad units
                if (parts == 4 && probableUnits != Units.Days)
                { // partUnit >= Units.Hours && partUnit < Units.Milliseconds || partUnit == Units.None
                  // unit too small, auto adjust
                    return Units.Days; // largest unit we'll AutoAdjust to (i.e. Don't do weeks unless explicit)

                }
                else if (parts == 3 && (probableUnits == Units.Minutes || probableUnits == Units.Seconds))
                {
                    // unit too small, auto adjust
                    return Units.Hours;

                }
                else if (parts == 2 && probableUnits == Units.Seconds)
                {
                    // unit too small, auto adjust
                    return Units.Minutes;
                }

                return Units.None;
            }

            public override Units BestGuessUnits()
            { // GivenOrDefaultOrSplitUnits

                CalcSplitDays();

                if (Autounit != Units.None)
                    return Autounit;

                if (SplitUnits != Units.None)
                    return SplitUnits;

                return GivenOrDefaultOrZeroUnits();
            }

            protected override Units SmallestUnit()
            {
                var start = BestGuessUnits();

                if (start == Units.None)
                {
                    if (IsZero())
                        return Units.ZeroOnly;
                    return Units.None;
                }

                if (start == Units.ZeroOnly)
                    return Units.ZeroOnly;

                var columns = Columns();
                if (columns == null)
                    return Units.None;

                var smallestColumn = NextSmallestUnit(start, columns.Length - 1);

                if (smallestColumn == Units.ErrorTooManyUnits && IsZero())
                    return Units.ZeroOnly;

                return smallestColumn;
            }

        }

        class OneUnitToken : ParserToken
        {
            public decimal? uncolonedValue;

            public override bool UsesColonedDefault()
            {
                return false;
            }

            public override bool IsNull()
            {
                return uncolonedValue == null;
            }

            public override bool IsZero()
            {
                if (IsNull()) return false; // technically not zero

                return (uncolonedValue == 0); // already null checked
            }

            public override bool IsInitialNegative()
            {
                return uncolonedValue.HasValue && uncolonedValue < 0;
            }

            public override TimeSpan? ToTimeSpan()
            {
                if (IsNull())
                    return null;

                return GetValue(uncolonedValue, GivenOrDefaultOrZeroUnits());
            }

        }

        class TimeSpanToken : ParserToken
        {
            public TimeSpan? timespan;
            public Units smallest = Units.None;
            public bool initialNegative = false;

            public override bool IsInitialNegative()
            {
                //return timespan < TimeSpan.Zero;
                return initialNegative;
            }

            public override bool IsNull()
            {
                return !timespan.HasValue;
            }

            public override bool IsZero()
            {
                return timespan == TimeSpan.Zero;
            }

            public override TimeSpan? ToTimeSpan()
            {
                return timespan;
            }

            public override bool UsesColonedDefault()
            {
                return false;
            }

            protected override Units SmallestUnit()
            {
                return smallest;
            }

            //delete me
            protected Units SmallestUnitv2()
            {
                if (!timespan.HasValue)
                    return Units.None;

                TimeSpan t = (TimeSpan)timespan.Value;

                if (t.Ticks != 0 || t.Milliseconds != 0)
                    return Units.Milliseconds;

                if (t.Seconds != 0)
                    return Units.Seconds;

                if (t.Minutes != 0)
                    return Units.Minutes;

                if (t.Hours != 0)
                    return Units.Hours;

                if (t.Days != 0)
                    return Units.Days;

                return Units.Years; // uh not quite right
            }
        }
    }
}
