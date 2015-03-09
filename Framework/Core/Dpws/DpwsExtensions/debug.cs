using Microsoft.SPOT;
using System.Diagnostics;

namespace System.Ext
{
    public static class Console
    {
        public static bool Verbose = false;

        //--//

        [Conditional("DEBUG")]
        public static void Write(byte[] message, int index, int count)
        {
            if (Verbose && message != null)
            {
                try
                {
                    byte[] data = new byte[count];

                    Array.Copy(message, index, data, 0, count);

                    Debug.Print(new string(System.Text.Encoding.UTF8.GetChars(data)));
                }
                catch
                {
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Write(byte[] message)
        {
            if (Verbose && message != null)
            {
                Write(message, 0, message.Length);
            }
        }

        [Conditional("DEBUG")]
        public static void Write(string message)
        {
            if (Verbose && message != null)
            {
                Debug.Print(message);
            }
        }
    }
}


