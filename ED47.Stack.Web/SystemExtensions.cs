using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Mvc;


public static class StringExtensions
{
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
        var strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}"
            + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\"
            + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        var re = new Regex(strRegex);
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

    

