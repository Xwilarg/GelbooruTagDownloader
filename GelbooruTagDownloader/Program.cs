using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace GelbooruTagDownloader
{
    class Program
    {
        static readonly string folderName = "Saves\\";

        static void Main(string[] args)
        {
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            Console.WriteLine("Informations will be saved in " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + folderName);
            Console.Write("Images analysed: 0");
            int id = 0;
            using (WebClient wc = new WebClient())
            {
                string html;
                while (true)
                {
                    html = wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&pid=" + id);
                    html = GetElementJson("tags=\"", html, '"');
                    string[] tags = html.Split(' ');
                    foreach (string s in tags)
                    {
                        switch (GetTagId(s))
                        {
                            case '0':
                                if (File.Exists(folderName + "trivia.dat"))
                                    File.AppendAllText(folderName + "trivia.dat", s + Environment.NewLine);
                                else
                                    File.WriteAllText(folderName + "trivia.dat", s + Environment.NewLine);
                                break;
                        }
                    }
                    id++;
                    Console.Write("\rImages analysed: " + id);
                }
            }
        }

        private static char GetTagId(string tag)
        {
            using (WebClient wc = new WebClient())
            {
                return (GetElementJson("<tag type=\"",  wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + tag), '"')[0]);
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
