using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace GelbooruTagDownloader
{
    class Program
    {
        static readonly string folderName = "Saves\\";
        static readonly int startId = 0;
        static readonly string searchLink = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&pid=";
        static readonly string tagLink = "https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=";

        static void Main(string[] args)
        {
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            Tuple<Dictionary<string, int>, string>[] allTags = new Tuple<Dictionary<string, int>, string>[] {
                new Tuple<Dictionary<string, int>, string>(new Dictionary<string, int>(), "Trivia"),
                new Tuple<Dictionary<string, int>, string>(new Dictionary<string, int>(), "Artists"),
                null,
                new Tuple<Dictionary<string, int>, string>(new Dictionary<string, int>(), "Copyrights"),
                new Tuple<Dictionary<string, int>, string>(new Dictionary<string, int>(), "Characters"),
                new Tuple<Dictionary<string, int>, string>(new Dictionary<string, int>(), "Metadatas")
            };
            Console.WriteLine("Informations will be saved in " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + folderName);
            Console.Write("Images analysed: 0");
            int id = startId;
            using (WebClient wc = new WebClient())
            {
                string html;
                while (true)
                {
                    html = wc.DownloadString(searchLink + id);
                    html = GetElementJson("tags=\"", html, '"');
                    string[] tags = html.Split(' ');
                    foreach (string s in tags)
                    {
                        if (s == "")
                            continue;
                        int? tagVal = GetTagId(s);
                        if (tagVal == null)
                            continue;
                        var tag = allTags[tagVal.Value];
                        if (tag == null)
                            Console.WriteLine("Invalid tag: " + s + ", value: " + GetTagId(s));
                        else
                        {
                            if (tag.Item1.ContainsKey(s))
                                tag.Item1[s]++;
                            else
                                tag.Item1.Add(s, 1);
                        }
                        foreach (var t in allTags)
                        {
                            if (t != null)
                            {
                                string finalStr = "";
                                foreach (var key in t.Item1)
                                {
                                    finalStr += key.Key + " " + key.Value + Environment.NewLine;
                                }
                                File.WriteAllText(folderName + t.Item2 + ".dat", finalStr);
                            }
                        }
                    }
                    id++;
                    Console.Write("\rImages analysed: " + id);
                }
            }
        }

        private static int? GetTagId(string tag)
        {
            using (WebClient wc = new WebClient())
            {
                string tagStr = wc.DownloadString(tagLink + tag);
                if (GetElementJson("name=\"", tagStr, '"') == tag)
                    return (Convert.ToInt32(GetElementJson("<tag type=\"",  tagStr, '"')[0] - '0'));
                return (null);
            }
        }

        /// <summary>
        /// Get an element in a string
        /// </summary>
        /// <param name="tag">The tag where we begin to take the element</param>
        /// <param name="file">The string to search in</param>
        /// <param name="stopCharac">The character after with we stop looking for</param>
        /// <returns></returns>
        private static string GetElementJson(string tag, string file, char stopCharac)
        {
            string saveString = "";
            int prog = 0;
            char lastChar = ' ';
            foreach (char c in file)
            {
                if (prog == tag.Length)
                {
                    if (c == stopCharac
                        && ((stopCharac == '"' && lastChar != '\\') || stopCharac != '"'))
                        break;
                    saveString += c;
                }
                else
                {
                    if (c == tag[prog])
                        prog++;
                    else
                        prog = 0;
                }
                lastChar = c;
            }
            return (saveString);
        }
    }
}
