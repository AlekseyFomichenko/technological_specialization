

namespace Homeworks.StreamAndBuff
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            string str = ReaderFrom(args[0]);
            WriteTo(str, args[1]);
        }

        private static void WriteTo(string str, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using StreamWriter writer = new StreamWriter(str);
                writer.Write(str);
            }
        }

        private static string ReaderFrom(string path)
        {
            using StreamReader sr = new StreamReader(path);
            return sr.ReadToEnd();
        }
    }
}
