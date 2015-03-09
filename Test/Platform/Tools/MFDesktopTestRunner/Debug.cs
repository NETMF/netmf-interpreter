using System;
using System.Text;
using System.IO;

namespace Microsoft.SPOT.Platform.Test
{
    public class Debug
    {
        public static StringBuilder m_builder = new StringBuilder();

        internal static void Print(string message)
        {
            m_builder = m_builder.Append(message);
        }

        internal static void SaveLog()
        {
            File.WriteAllText(
                Environment.GetEnvironmentVariable("temp") 
                + @"\DesktopTestRunner.log", m_builder.ToString());
        }
    }
}