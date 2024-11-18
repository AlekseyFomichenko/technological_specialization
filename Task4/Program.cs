using System.Text;

namespace Task4
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Реализуйте метод ProcessMemoryStreamAsync таким образом, чтобы он выводил на экран содержимое потока.
            byte[] data = Encoding.UTF8.GetBytes("Hello, this is data for MemoryStream!");
            using (MemoryStream ms = new MemoryStream(data))
            {
                await ProcessMemoryStreamAsync(ms);
            }
        }

        private static async Task ProcessMemoryStreamAsync(MemoryStream memoryStream)
        {
            byte[] buffer = new byte[1024];
            var bytesRead = 0;
            StringBuilder sb = new StringBuilder();
            while ((bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer));
            }
            await Console.Out.WriteLineAsync(sb.ToString());
        }
    }
}
