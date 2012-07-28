﻿using System;
using System.Text;
namespace Reddit
{
    using System.Net;
    using System.IO;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.Serialization.Formatters.Binary;

    public class RedditAPI
    {
        CookieContainer redditCookie;
        WebClient jsonGet;

        //A cache of me.json, since we likely don't need to retrieve it every time we use it
        Hashtable me;
        string usr;
        string cookiefn;

        /// <summary>
        /// Class constructor, logs in or checks for previous login from cookie file
        /// </summary>
        /// <param name="user">Reddit account username</param>
        /// <param name="pswd">Reddit Account password</param>
        /// <param name="cookiefilename">File name of the cookie</param>
        public RedditAPI(string user, string pswd, string cookiefilename = "cookie")
        {
            redditCookie = new CookieContainer();
            jsonGet = new WebClient();
            usr = user;
            cookiefn = cookiefilename;
            try
            {
                redditCookie = Loadcookie(cookiefilename);
                CookieCollection tmpc = redditCookie.GetCookies(new System.Uri("http://www.reddit.com/api/login/" + usr));
            }
            catch
            {
                Login(user, pswd);
            }

            GetMe();


        }

        /// <summary>
        /// Logs the user in
        /// </summary>
        /// <param name="user">Reddit account username</param>
        /// <param name="pswd">Reddit account password</param>
        /// <returns>True/False depending on success of login (NYI)</returns>
        private bool Login(string user, string pswd)
        {

            HttpWebRequest login = WebRequest.Create("http://www.reddit.com/api/login/" + user) as HttpWebRequest;
            login.CookieContainer = redditCookie;
            login.Method = "POST";
            login.ContentType = "application/x-www-form-urlencoded";

            string postData = string.Format("api_type=json&user={0}&passwd={1}", user, pswd);
            byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(postData);
            login.ContentLength = dataBytes.Length;
            Stream postStream = login.GetRequestStream();

            postStream.Write(dataBytes, 0, dataBytes.Length);
            postStream.Close();


            //Do the actual login
            WebResponse response = login.GetResponse();
            StreamReader r = new StreamReader(response.GetResponseStream());
            Console.WriteLine(r.ReadToEnd());
            Console.WriteLine(redditCookie.GetCookieHeader(new System.Uri("http://www.reddit.com/api/login/" + usr)));

            Savecookie(cookiefn, redditCookie);
            return true;
        }

        /// <summary>
        /// Gets a fresh copy of me.json and saves it to the cache
        /// </summary>
        /// <returns>True/false depending on success (NYI)</returns>
        private Hashtable GetMe()
        {
            jsonGet.Headers["COOKIE"] = redditCookie.GetCookieHeader(new System.Uri("http://www.reddit.com/api/login/" + usr));
            Stream jsonStream = jsonGet.OpenRead("http://www.reddit.com/api/me.json");
            StreamReader jSR = new StreamReader(jsonStream);
            string metmp = jSR.ReadToEnd();
            Hashtable meData = (Hashtable)JSON.JsonDecode(metmp);
            me = (Hashtable)meData["data"];

            return me;
        }

        /// <summary>
        /// Returns me.json's data as a hashtable
        /// </summary>
        /// <returns>me.json's data as a hashtable</returns>
        /// <remarks>e.g. (string)me["modhash"] would be the user's modhash as a string</remarks>
        public Hashtable GetMeCache()
        {
            return me;
        }

        /// <summary>
        /// Checks if the user has mail based on a fresh polling of me.json
        /// </summary>
        /// <returns>True/false depending on if the user has an orangered</returns>
        public bool HasMail()
        {
            //Get a fresh copy of me.json
            GetMe();
            if ((string)me["has_mail"] == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sends data in POST to the specified URI
        /// </summary>
        /// <param name="data">POST data</param>
        /// <param name="URI">URI to POST data to</param>
        /// <returns>True/false based on success (NYI)</returns>
        private bool SendPOST(string data, string URI)
        {
            HttpWebRequest connect = WebRequest.Create(new System.Uri(URI)) as HttpWebRequest;
            connect.Headers["COOKIE"] = redditCookie.GetCookieHeader(new System.Uri(URI));
            connect.CookieContainer = redditCookie;
            connect.Method = "POST";
            connect.ContentType = "application/x-www-form-urlencoded";

            byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(data);
            connect.ContentLength = dataBytes.Length;
            Stream postStream = connect.GetRequestStream();

            postStream.Write(dataBytes, 0, dataBytes.Length);
            postStream.Close();

            //Do the actual connection
            connect.GetResponse();
            return true;
        }

        /// <summary>
        /// Posts a comment to the specified "thing"
        /// </summary>
        /// <param name="id">"thing" ID code</param>
        /// <param name="content">Comment contents</param>
        /// <returns>True/false based on success (NYI)</returns>
        /// <remarks>See Glossary here for more info on "things" https://github.com/reddit/reddit/wiki/API </remarks>
        public bool PostComment(string id, string content)
        {
            string modhash = (string)me["modhash"];
            SendPOST(string.Format("thing_id={0}&text={1}&uh={2}", id, content, modhash),
                    "http://www.reddit.com/api/comment");
            return true;
        }
        /// <summary>
        /// Casts a vote on the specified "thing"
        /// </summary>
        /// <param name="post">"thing" ID</param>
        /// <param name="type">Vote type, 1, 0 or -1</param>
        /// <returns>True/false based on success (NYI)</returns>
        private bool Vote(string post, int type)
        {
            string modhash = (string)me["modhash"];
            SendPOST(string.Format("id={0}&dir={1}&uh={2}", post, type, modhash),
                    "http://www.reddit.com/api/vote");

            return true;
        }

        /// <summary>
        /// Upvotes a "thing"
        /// </summary>
        /// <param name="postID">"thing" ID</param>
        /// <returns></returns>
        /// <remarks>See Glossary here for more info on "things" https://github.com/reddit/reddit/wiki/API </remarks>
        public bool Upvote(string postID)
        {
            Vote(postID, 1);
            return true;
        }

        /// <summary>
        /// Rescinds vote from "thing"
        /// </summary>
        /// <param name="postID">"thing" ID</param>
        /// <returns></returns>
        /// <remarks>See Glossary here for more info on "things" https://github.com/reddit/reddit/wiki/API </remarks>
        public bool UnVote(string postID)
        {
            Vote(postID, 0);
            return true;
        }

        /// <summary>
        /// Downvotes a "thing"
        /// </summary>
        /// <param name="postID">"thing" ID</param>
        /// <returns></returns>
        /// <remarks>See Glossary here for more info on "things" https://github.com/reddit/reddit/wiki/API </remarks>
        public bool Downvote(string postID)
        {
            Vote(postID, -1);
            return true;
        }

        /// <summary>
        /// Posts a link/self post
        /// </summary>
        /// <param name="kind">"self" or "link"</param>
        /// <param name="url">URL or Self Post content</param>
        /// <param name="sr">subreddit</param>
        /// <param name="title">Title of post</param>
        /// <returns></returns>
        private bool Post(string kind, string url, string sr, string title)
        {
            string modhash = (string)me["modhash"];
            SendPOST(string.Format("uh={0}&kind={1}&url={2}&sr={3}&title={4}&r={3}&renderstyle=html", (string)me["modhash"], kind, url, sr, title),
                    "http://www.reddit.com/api/submit");
            return true;
        }

        /// <summary>
        /// Posts a self post
        /// </summary>
        /// <param name="link">Text of the self post</param>
        /// <param name="title">Title of submission</param>
        /// <param name="sr">Subreddit to post to</param>
        public void PostSelf(string text, string title, string sr)
        {

            Post("self", text, sr, title);
        }

        /// <summary>
        /// Posts a link
        /// </summary>
        /// <param name="link">URI of post</param>
        /// <param name="title">Title of submission</param>
        /// <param name="sr">Subreddit to post to</param>
        public void PostLink(string link, string title, string sr)
        {
            Post("self", link, sr, title);
        }

        /// <summary>
        /// Saves cookie to file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="rcookie"></param>
        private void Savecookie(string filename, CookieContainer rcookie)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, rcookie);
            stream.Close();
        }

        /// <summary>
        /// Loads cookie from file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private CookieContainer Loadcookie(string filename)
        {
            CookieContainer rcookie;
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            rcookie = (CookieContainer)bFormatter.Deserialize(stream);
            stream.Close();
            return rcookie;
        }
    }


    //I didn't want to write my own JSON parser, so I used the free one available here:
    // http://techblog.procurios.nl/k/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
    // Included in the same source file because I wanted it to be in 1 cs file

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    public class JSON
    {
        public const int TOKEN_NONE = 0;
        public const int TOKEN_CURLY_OPEN = 1;
        public const int TOKEN_CURLY_CLOSE = 2;
        public const int TOKEN_SQUARED_OPEN = 3;
        public const int TOKEN_SQUARED_CLOSE = 4;
        public const int TOKEN_COLON = 5;
        public const int TOKEN_COMMA = 6;
        public const int TOKEN_STRING = 7;
        public const int TOKEN_NUMBER = 8;
        public const int TOKEN_TRUE = 9;
        public const int TOKEN_FALSE = 10;
        public const int TOKEN_NULL = 11;

        private const int BUILDER_CAPACITY = 2000;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json)
        {
            bool success = true;

            return JsonDecode(json, ref success);
        }

        /// <summary>
        /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="success">Successful parse?</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json, ref bool success)
        {
            success = true;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                object value = ParseValue(charArray, ref index, ref success);
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a Hashtable / ArrayList object into a JSON string
        /// </summary>
        /// <param name="json">A Hashtable / ArrayList</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string JsonEncode(object json)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = SerializeValue(json, builder);
            return (success ? builder.ToString() : null);
        }

        protected static Hashtable ParseObject(char[] json, ref int index, ref bool success)
        {
            Hashtable table = new Hashtable();
            int token;

            // {
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {

                    // name
                    string name = ParseString(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);
                    if (token != JSON.TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
        {
            ArrayList array = new ArrayList();

            // [
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                int token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    array.Add(value);
                }
            }

            return array;
        }

        protected static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case JSON.TOKEN_STRING:
                    return ParseString(json, ref index, ref success);
                case JSON.TOKEN_NUMBER:
                    return ParseNumber(json, ref index, ref success);
                case JSON.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);
                case JSON.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case JSON.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;
                case JSON.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;
                case JSON.TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case JSON.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        protected static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {

                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {

                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        s.Append('"');
                    }
                    else if (c == '\\')
                    {
                        s.Append('\\');
                    }
                    else if (c == '/')
                    {
                        s.Append('/');
                    }
                    else if (c == 'b')
                    {
                        s.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        s.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        s.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        s.Append('\r');
                    }
                    else if (c == 't')
                    {
                        s.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else
                {
                    s.Append(c);
                }

            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static double ParseNumber(char[] json, ref int index, ref bool success)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;

            double number;
            success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        protected static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        protected static void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        protected static int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        protected static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return JSON.TOKEN_NONE;
            }

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return JSON.TOKEN_CURLY_OPEN;
                case '}':
                    return JSON.TOKEN_CURLY_CLOSE;
                case '[':
                    return JSON.TOKEN_SQUARED_OPEN;
                case ']':
                    return JSON.TOKEN_SQUARED_CLOSE;
                case ',':
                    return JSON.TOKEN_COMMA;
                case '"':
                    return JSON.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JSON.TOKEN_NUMBER;
                case ':':
                    return JSON.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return JSON.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return JSON.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return JSON.TOKEN_NULL;
                }
            }

            return JSON.TOKEN_NONE;
        }

        protected static bool SerializeValue(object value, StringBuilder builder)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string)value, builder);
            }
            else if (value is Hashtable)
            {
                success = SerializeObject((Hashtable)value, builder);
            }
            else if (value is ArrayList)
            {
                success = SerializeArray((ArrayList)value, builder);
            }
            else if (IsNumeric(value))
            {
                success = SerializeNumber(Convert.ToDouble(value), builder);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                builder.Append("false");
            }
            else if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                success = false;
            }
            return success;
        }

        protected static bool SerializeObject(Hashtable anObject, StringBuilder builder)
        {
            builder.Append("{");

            IDictionaryEnumerator e = anObject.GetEnumerator();
            bool first = true;
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    builder.Append(", ");
                }

                SerializeString(key, builder);
                builder.Append(":");
                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        protected static bool SerializeArray(ArrayList anArray, StringBuilder builder)
        {
            builder.Append("[");

            bool first = true;
            for (int i = 0; i < anArray.Count; i++)
            {
                object value = anArray[i];

                if (!first)
                {
                    builder.Append(", ");
                }

                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("]");
            return true;
        }

        protected static bool SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
            return true;
        }

        protected static bool SerializeNumber(double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, null, etc). 
        /// 
        /// Thanks to mtighe for pointing out Double.TryParse to me.
        /// </summary>
        protected static bool IsNumeric(object o)
        {
            double result;

            return (o == null) ? false : Double.TryParse(o.ToString(), out result);
        }
    }


}