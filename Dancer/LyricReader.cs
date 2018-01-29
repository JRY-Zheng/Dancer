using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Dancer
{
    static class LyricReader
    {
        private static string music_name, singer, list_name, music_path;
        public static List<MainWindow.Lyric> lyrics = new List<MainWindow.Lyric>();
        public static void init(string _music_name, string _singer, string _list_name, string _music_path)
        {
            music_name = _music_name;
            singer = _singer;
            list_name = _list_name;
            music_path = _music_path;
        }
        
        public static List<MainWindow.Lyric> load_lyric()
        {
            lyrics.Clear();
            string lyrics_path = String.Format(@"{0}//{1}//{3}.lrc", music_path, list_name, singer, music_name);
            if (File.Exists(lyrics_path))
            {
                string[] lyrics_text;
                lyrics_text = File.ReadAllLines(lyrics_path, Encoding.UTF8);
                foreach(string lyric_text in lyrics_text)
                {
                    Match match = Regex.Match(lyric_text, @"(\[\d+:[\d\.]+\])+(.*)");
                    if (!match.Success) continue;
                    string content = match.Groups[2].ToString();
                    String time = match.Groups[1].ToString();
                    foreach(Match time_match in Regex.Matches(time, @"\[(\d+):([\d\.]+)\]"))
                    {
                        double m, s;
                        m = Convert.ToDouble(time_match.Groups[1].ToString());
                        s = Convert.ToDouble(time_match.Groups[2].ToString());
                        s = m * 60 + s;
                        MainWindow.Lyric lyric;
                        lyric.position = s;
                        lyric.lyric_content = content;
                        lyrics.Add(lyric);
                    }
                }
            }
            return lyrics;
        }
    }
}
