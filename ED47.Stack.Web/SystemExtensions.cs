using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Mvc;


public static class StringExtensions
{
    public const string EmailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}"
            + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\"
            + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

    public const string EmailRegexReplace = @"([A-Z0-9._%-]+)@([A-Z0-9.-]+\.[A-Z]{2,6})\(([^)]*)\)";

    public static string Ellipsis(this string s, int length, string showMoreText = "...")
    {
        if (s == null)
            return null;

        if (length < 0)
            return s;

        return s.Length < length
                   ? s
                   : s.Substring(0, Math.Max(s.IndexOfLastSpace(length),length) ) + showMoreText;
    }

    public static int IndexOfLastSpace(this string s, int pos)
    {
        if (pos < 0)
            return 0;

        if (pos == s.Length)
            pos = s.Length - 1;

        if (s[pos] == ' ')
            return pos;

        return IndexOfLastSpace(s, pos - 1);
    }

    public static bool IsEmail(this string s) {
    
        var re = new Regex(EmailRegex);
        if (re.IsMatch(s))
            return (true);
        
        return (false);
    }


    public static string Format(this string s, params object[] args)
    {
        return String.Format(s, args);
    }


    public static Stream ToStream(this string str) {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(str);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }


    public static bool IsLike(this string s, object text) {
        if (s == null && text == null)
            return true;

        if (s == null && text != null)
            return false;

        if (text == null)
            return false;

        var textStr = text.ToString().Trim();
        var sAux = s.Trim();


        if (!string.IsNullOrEmpty(s) && string.IsNullOrEmpty(textStr))
            return false;

        /* Turn "off" all regular expression related syntax in the pattern string. */
        textStr = Regex.Escape(textStr);

        /* Replace the SQL LIKE wildcard metacharacters with the equivalent regular expression metacharacters. */
        textStr = textStr.Replace("%", ".*?").Replace("_", ".");

        /* The previous call to Regex.Escape actually turned off too many metacharacters, i.e. 
         * those which are recognized by bbth the regular expression engine and the SQL LIKE statement ([...] and [^...]). 
         * Those metacharacters have to be manually unescaped here. */
        textStr = textStr.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

        return Regex.IsMatch(sAux, textStr, RegexOptions.IgnoreCase);
    }

    public static string GetProtectedEmailFromSpam(this string text)
    {
        return text.GetProtectedEmailFromSpam("email");
        //Regex.Replace(text, EmailRegexReplace, "<a class=\"email\" onclick=\"javascript:eml(\\'$2\\',this,\\'$3\\',\\'$1\\');\" target=\"_self\">$3</a>", RegexOptions.IgnoreCase);
    }

    public static string GetProtectedEmailFromSpam(this string text, string cssclass) {
        return Regex.Replace(text, EmailRegexReplace, "<a class=\"" + cssclass + "\" onclick=\"javascript:eml(\\'$2\\',this,\\'$3\\',\\'$1\\');\" target=\"_self\">$3</a>", RegexOptions.IgnoreCase);
    }

    public static string GetProtectedEmailFromSpam(this string text, string cssClass, string replacement) {
        return Regex.Replace(text, EmailRegexReplace, replacement);
    }

    private static Regex _tags = new Regex("<[^>]*(>|$)",RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
    

    public static string Sanitize(this string source, Regex whitelist)
    {
        if (String.IsNullOrEmpty(source)) return source;

        string tagname;
        Match tag;

        // match every HTML tag in the input
        MatchCollection tags = _tags.Matches(source);
        for (int i = tags.Count - 1; i > -1; i--)
        {
            tag = tags[i];
            tagname = tag.Value.ToLowerInvariant();

            if (!(whitelist.IsMatch(tagname)))
            {
                source = source.Remove(tag.Index, tag.Length);
            }
        }

        return source;
    }

}


public static class WebViewPageExtensions
{
    
    public static bool IsPublish(this HtmlHelper htmlHelper)
    {
        #if PUBLISH
            return true;
        #else
            return false;
        #endif
    }

    public static bool IsDebug(this HtmlHelper htmlHelper)
    {
        #if DEBUG
            return true;
        #else
            return false;
        #endif
    }
}

    

