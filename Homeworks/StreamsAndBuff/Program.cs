
using System.Globalization;

namespace Homeworks.StreamAndBuff
{
    internal class Program
    {
        const string path = "Program.cs";
        const string word = "List";
        static void Main(string[] args)
        {
            var text = ReadFrom(path);
            var filter = Filter(word, text);
            Console.WriteLine(string.Join("\n", filter));
        }

        static List<string> ReadFrom(string path)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream) 
                {
                    var line = sr.ReadLine();
                    list.Add(sr.ReadLine()!);
                    Console.WriteLine(line);
                }
            }
            return list;
        }

        static List<string> Filter(string word, List<string> text)
        {
            return text.Where(x => x.ToLower().Contains(word.ToLower())).
                        Select(a => a.ToLower().Replace(word.ToLower(), word.ToUpper())).ToList();
        }
    }
}
