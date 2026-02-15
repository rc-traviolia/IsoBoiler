using System.Text;

namespace IsoBoiler.Http
{
    public static class StreamExtensions
    {
        public static async Task<string> ReadStreamAsync(this Stream body)
        {
            using (StreamReader reader = new StreamReader(body, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
