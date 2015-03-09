using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Test
{
    public abstract class Log
    {
        #region Member Variables

        private static int m_passCount = 0;
        private static int m_failCount = 0;
        private static int m_skipCount = 0;
        private static int m_knownFailureCount = 0;

        private enum CommentType
        {
            Comment,
            Exception
        }

        #endregion

        #region Public

        /// <summary>
        /// This method is used to log any test comments.
        /// </summary>
        /// <param name="message">A string containing the test comments.</param>
        public static void Comment(string message)
        {            
            LocalComment(message, CommentType.Comment);
        }

        /// <summary>
        /// This method is used to log any test comments.
        /// </summary>
        /// <param name="message">A string containing the test comments.</param>
        public static void FilteredComment(string message)
        {
            SpotTestLog.WriteRaw("\t<Comment type=\"0\">"
                + "<Text>" + FilterUnsafeXml(message) + "</Text>"
                + "<Date>" + GetDate() + "</Date>"
                + "<Time>" + DateTime.Now.TimeOfDay.ToString() + "</Time>"
                + "</Comment>");
        }

        /// <summary>
        /// This method is used to log any exceptions that may arise during the test.
        /// </summary>
        /// <param name="message">A string containing the exception message.</param>
        public static void Exception(string message)
        {
            LocalComment(message, CommentType.Exception);
        }

        /// <summary>
        /// This method is used to log any exceptions that may arise during the test.
        /// </summary>
        /// <param name="message">A string containing the exception message.</param>
        public static void Exception(string message, Exception ex)
        {
            LocalComment(message, CommentType.Exception);
            LocalComment("Message: " + ex.Message, CommentType.Exception);
            LocalComment("Stack: " + ex.StackTrace, CommentType.Exception);
            if (ex.InnerException != null)
            {
                LocalComment("InnerException Message: " + ex.InnerException.Message, CommentType.Exception);
                LocalComment("InnerException Stack: " + ex.InnerException.StackTrace, CommentType.Exception);
            }
        }

        #endregion

        #region Internal

        internal static void ResetCounts()
        {
            m_passCount = 0;
            m_failCount = 0;
            m_skipCount = 0;
            m_knownFailureCount = 0;
        }

        internal static void Initialize(string test)
        {
            m_passCount = 0;
            m_failCount = 0;
            m_skipCount = 0;
            m_knownFailureCount = 0;
            SpotTestLog.StartTestLog(test);
            Log.StartMethod("Initialize");
        }

        internal static void CleanUp(string test)
        {
            Log.EndMethod("CleanUp");
            SpotTestLog.StartNode("Results");
            LogPassCount();
            LogFailCount();
            LogSkipCount();
            LogKnownFailureCount();
            SpotTestLog.EndNode("Results");
            SpotTestLog.EndTestLog(test);
            //System.Threading.Thread.Sleep(2000);
        }

        internal static void TestResult(string message, MFTestResults result)
        {
            if (result == MFTestResults.Pass)
            {
                SpotTestLog.StartResultNode("Pass");
                Pass(message);
            }
            else if (result == MFTestResults.Fail)
            {
                SpotTestLog.StartResultNode("Fail");
                Fail(message);
            }
            else if (result == MFTestResults.Skip)
            {
                SpotTestLog.StartResultNode("Skip");
                Skip(message);
            }
            else if (result == MFTestResults.KnownFailure)
            {
                SpotTestLog.StartResultNode("KnownFailure");
                KnownFailure(message);
            }

            SpotTestLog.EndNode("TestMethodResult");
        }

        internal static void Pass(string message)
        {
            m_passCount++;
            WriteMessage(message);
        }

        internal static void Fail(string message)
        {
            m_failCount++;
            WriteMessage(message);
        }

        internal static void Skip(string message)
        {
            m_skipCount++;
            WriteMessage(message);
        }

        internal static void KnownFailure(string message)
        {
            m_knownFailureCount++;
            WriteMessage(message);
        }

        internal static void WriteMessage(string message)
        {
            SpotTestLog.WriteRaw("\t\t" + "<Text><![CDATA[" + message + "]]></Text>" +
                "<Date>" + GetDate() + "</Date>" +
                "<Time>" + DateTime.Now.TimeOfDay.ToString() + "</Time>");
        }

        internal static void StartTestMethod(string name)
        {
            SpotTestLog.StartTestMethod(name);
        }

        internal static void EndTestMethod()
        {
            SpotTestLog.EndTestMethod();
        }

        internal static void StartMethod(string name)
        {
            SpotTestLog.StartNode(name);
        }

        internal static void EndMethod(string name)
        {
            SpotTestLog.EndNode(name);
        }

        #endregion

        #region Private

        private static void LocalComment(string message, CommentType ct)
        {
            SpotTestLog.WriteRaw("\t<Comment type=\"" + ct + "\">"
                + "<Text><![CDATA[" + message + "]]></Text>"
                + "<Date>" + GetDate() + "</Date>"
                + "<Time>" + DateTime.Now.TimeOfDay.ToString() + "</Time>"
                + "</Comment>");
        }

        private static string FilterUnsafeXml(string message)
        {
            // The filtering code is slow since it goes char by char and 
            // can lead to time outs on very long strings. Hence do not 
            // filter unsafe xml from strings longer than 200 chars.

            string filtered = "";

            // Iterate through each char and replace as we go
            for (int i = 0; i < message.Length; i++)
            {
                switch (message[i])
                {
                    case '&':
                        filtered += "&amp;";
                        break;
                    case '>':
                        filtered += "&gt;";
                        break;
                    case '<':
                        filtered += "&lt;";
                        break;
                    case '"':
                        filtered += "&quot;";
                        break;
                    case '\'':
                        filtered += "&apos;";
                        break;
                    default:
                        int val = (int)message[i];
                        if ((val > 127) || (val < 32))
                        {
                            filtered += "&#" + val + ";";
                        }
                        else
                        {
                            filtered += message[i];
                        }
                        break;
                }
            }
            return filtered;
        }

        private static void LogPassCount()
        {
            SpotTestLog.WriteRaw("\t<PassCount>" + "<Text>" + m_passCount.ToString() + "</Text>"
                + "<Date>" + GetDate() + "</Date>" + "<Time>" + DateTime.Now.TimeOfDay.ToString()
                + "</Time>" + "</PassCount>");
        }

        private static void LogFailCount()
        {
            SpotTestLog.WriteRaw("\t<FailCount>" + "<Text>" + m_failCount.ToString() + "</Text>"
                + "<Date>" + GetDate() + "</Date>" + "<Time>" + DateTime.Now.TimeOfDay.ToString()
                + "</Time>" + "</FailCount>");
        }

        private static void LogSkipCount()
        {
            SpotTestLog.WriteRaw("\t<SkipCount>" + "<Text>" + m_skipCount.ToString() + "</Text>"
                + "<Date>" + GetDate() + "</Date>" + "<Time>" + DateTime.Now.TimeOfDay.ToString()
                + "</Time>" + "</SkipCount>");
        }

        private static void LogKnownFailureCount()
        {
            SpotTestLog.WriteRaw("\t<KnownFailureCount>" + "<Text>" + m_knownFailureCount.ToString() + "</Text>"
                + "<Date>" + GetDate() + "</Date>" + "<Time>" + DateTime.Now.TimeOfDay.ToString()
                + "</Time>" + "</KnownFailureCount>");
        }

        private static string GetDate()
        {
            return DateTime.Today.Month + "/" + DateTime.Today.Day + "/" + DateTime.Today.Year;
        }

        #endregion
    }

    internal abstract class SpotTestLog
    {
        #region Internal

        internal static void Write(string node, string data)
        {
            string value = "<" + RemoveInvalidCharacters(node) + ">" +
                RemoveInvalidCharacters(data) + "</" + RemoveInvalidCharacters(node) + ">";
            Debug.Print(value);
        }

        internal static void StartTestLog(string test)
        {
            string value = "<TestLog Test=\"" + test + "\">";
            Debug.Print(value);
        }

        internal static void EndTestLog(string test)
        {
            string value = "</TestLog>";
            Debug.Print(value);
        }

        internal static void StartResultNode(string result)
        {
            string value = "\t<TestMethodResult Result=\"" + result + "\">";
            Debug.Print(value);
        }

        internal static void EndResultNode(string test)
        {
            string value = "</TestMethodResult>";
            Debug.Print(value);
            Debug.Print(string.Empty);
        }

        internal static void StartTestMethod(string name)
        {
            string value = "<TestMethod name=\"" + name + "\">";            
            Debug.Print(value);
        }

        internal static void EndTestMethod()
        {
            string value = "</TestMethod>";
            Debug.Print(value);
            Debug.Print(string.Empty);
        }

        internal static void StartNode(string node)
        {
            string value;
            if (string.Equals(node.ToLower(), "initialize") ||
                string.Equals(node.ToLower(), "cleanup") ||
                string.Equals(node.ToLower(), "results"))
            {
                if (string.Equals(node.ToLower(), "initialize") ||
                string.Equals(node.ToLower(), "results"))
                {
                    Debug.Print(string.Empty);
                }
                value = "<" + RemoveInvalidCharacters(node) + ">";
            }
            else
            {
                value = "\t<" + RemoveInvalidCharacters(node) + ">";
            }

            Debug.Print(value);            
        }

        internal static void EndNode(string node)
        {
            string value;
            if (string.Equals(node.ToLower(), "initialize") ||
                string.Equals(node.ToLower(), "cleanup") ||
                string.Equals(node.ToLower(), "results"))
            {
                value = "</" + RemoveInvalidCharacters(node) + ">";
            }
            else
            {
                value = "\t</" + RemoveInvalidCharacters(node) + ">";
            }

            Debug.Print(value);

            if (string.Equals(node.ToLower(), "initialize") ||
                string.Equals(node.ToLower(), "results"))
            {
                Debug.Print(string.Empty);
            }
        }

        internal static void WriteDate(string date)
        {
            string value = "<Date>" + date + "</Date>";
            Debug.Print("\t\t" + value);
        }

        internal static void WriteTime(string time)
        {
            string value = "<Time>" + time + "</Time>";
            Debug.Print("\t\t" + value);
        }

        internal static void WriteText(string text)
        {
            string value = "<Text><![CDATA[" + text + "]]></Text>";
            Debug.Print("\t\t" + value);
        }

        internal static void WriteRaw(string text)
        {
            Debug.Print(text);
        }

        #endregion

        #region Private

        internal static string RemoveInvalidCharacters(string input)
        {
            // Remove chars that will cause problems loading the xml file ('<', '>' etc).
            char[] invalidChars = { '<', '>', '&', '\'', '"' };
            char[] array = input.ToCharArray();
            foreach (char c in invalidChars)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == c)
                    {
                        array[i] = ' ';
                    }
                }
            }

            string returnVal = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != ' ')
                {
                    returnVal += array[i];
                }
            }

            return returnVal;
        }

        private static string StripInValidXmlChars(string s)
        {
            string validXML = string.Empty;
            char current;
            char[] charArray = s.ToCharArray();
            if (s.Length == 0) return string.Empty;

            for (int i = 0; i < charArray.Length; i++)
            {
                current = charArray[i];
                if ((current == 0x9) ||
                    (current == 0xA) ||
                    (current == 0xD) ||
                    ((current >= 0x20) && (current <= 0xD7FF)) ||
                    ((current >= 0xE000) && (current <= 0xFFFD)))
                {
                    validXML += current;
                }
            }

            return validXML;
        }

        #endregion
    }
}