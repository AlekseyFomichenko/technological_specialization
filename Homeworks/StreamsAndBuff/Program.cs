namespace Homework8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            SearchFile(current, args[0], args[1]);
        }

        static void SearchFile(DirectoryInfo dir, string extension, string text, int indent = 0)
        {
            foreach (var folder in dir.EnumerateDirectories())
            {
                Console.WriteLine("{0}[{1}]", new string(' ', indent), Path.GetFileName(folder.FullName));
                SearchFile(folder, extension, text, indent + 2);
                SearchText(folder, extension, text);
            }
            SearchText(dir, extension, text, -2);
        }

        static private void SearchText(DirectoryInfo directory, string extension, string text, int indent = 0)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                if (extension.Contains(file.Extension))
                {
                    using (StreamReader sr = new StreamReader(file.FullName))
                    {
                        string line;
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            if (line.Contains(text))
                            {
                                Console.WriteLine("{0}{1}", new string(' ', indent + 2), Path.GetFileName(file.FullName));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
