using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client.Utils
{
    public static class ObjectToSmthgConverter
    {
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();

            using (var memoryStram = new MemoryStream())
            {
                formatter.Serialize(memoryStram, obj);
                return memoryStram.ToArray();
            }
        }
    }
}
