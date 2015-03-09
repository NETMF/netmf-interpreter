#region Using

using System;
using System.IO;
using System.Xml;
using System.Text;

#endregion

namespace Microsoft.SPOT.Platform.Test
{
    internal class XmlLog
    {
        #region Member Variables

        private string m_logFile;
        private XmlTextWriter m_xmlWriter;

        #endregion

        #region Internal Methods

        internal void StartLog(string logFile, string xslPath)
        {
            m_logFile = logFile;
            m_xmlWriter = new XmlTextWriter(logFile, Encoding.UTF8);
            m_xmlWriter.Formatting = Formatting.Indented;
            m_xmlWriter.WriteStartDocument();
            m_xmlWriter.WriteRaw("<?xml-stylesheet type=\"text/xsl\" href=\"" + xslPath + "\"?>");
            m_xmlWriter.WriteStartElement("SPOT_Platform_Test");
        }

        internal void CloseLog(string value)
        {
            try
            {
                m_xmlWriter.WriteRaw(value);
            }
            catch (InvalidOperationException ex)
            {
                Utils.WriteToEventLog(string.Format("An exception was thrown in XmlLog: {0}", ex.ToString()));
            }

            m_xmlWriter.WriteEndElement();
            m_xmlWriter.WriteEndDocument();
            m_xmlWriter.Flush();
            m_xmlWriter.Close();
            ((IDisposable)m_xmlWriter).Dispose();
        }

        internal void AddDeviceStatusToLog(string comment)
        {
            string frag = "<DeviceStatus><Text>"
                    + ConvertUnsafeXmlStrings(comment)
                    + "</Text><Date></Date><Time></Time></DeviceStatus>";

            XmlDocument doc = new XmlDocument();
            doc.Load(m_logFile);
            XmlNode node = doc.SelectSingleNode("/SPOT_Platform_Test");
            XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
            docFrag.InnerXml = frag;
            node.AppendChild(docFrag);
            doc.Save(m_logFile);
        }

        internal void WriteElementString(string localName, string value)
        {
            m_xmlWriter.WriteElementString(localName, value);
        }

        internal void SynchronizeLogTime(string file, DateTime initialTime)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(m_logFile);

            try
            {
                SynchronizeLogTime(m_logFile, doc, initialTime);
                doc.Save(m_logFile);                

                // Add info strings to the log.
                InsertInfoStringsInLog(doc);

            }
            catch(XmlException)
            {
            }

            try
            {
                // Add node.
                AddXmlNodeToLog(doc, file);
                XmlDocumentFragment docFrag = doc.CreateDocumentFragment();                
                doc.Save(m_logFile);
            }
            catch (XmlException)
            {
            }
        }

        internal string LogFile
        {
            get
            {
                return m_logFile;
            }
        }

        internal string ConvertUnsafeXmlStrings(string output)
        {
            // fix strings that come from Debugger
            output = output.Replace("<no name>", "&lt;no name&gt;");
            output = output.Replace("<No Name>", "&lt;No Name&gt;");
            output = output.Replace("<>", "&lt;&gt;");
            output = output.Replace("<.ctor>", "&lt;.ctor&gt;");
            output = output.Replace("z&iss", "z&amp;iss");

            return output;
        }

        internal void AddCommentToLog(string comment)
        {
            m_xmlWriter.WriteRaw("<Comment type=\"99\"><Text>"
                    + ConvertUnsafeXmlStrings(comment)
                    + "</Text><Date></Date><Time></Time></Comment>");
        }        

        #endregion

        #region Private Methods

        private void AddXmlNodeToLog(XmlDocument doc, string file)
        {
            XmlNode node = doc.SelectSingleNode("/SPOT_Platform_Test");
            XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
            docFrag.InnerXml = "<SourceFile>" + ConvertUnsafeXmlStrings(file) + "</SourceFile>";

            node.AppendChild(docFrag);
            doc.Save(m_logFile);
        }

        private void SynchronizeLogTime(string fileName, XmlDocument doc, DateTime initialTime)
        {
            XmlNodeList timeNodes = doc.GetElementsByTagName("Time");
            foreach (XmlNode timeNode in timeNodes)
            {
                string time = timeNode.InnerText;
                DateTime dt = Convert.ToDateTime(time);
                dt = dt.AddMilliseconds((double)initialTime.Millisecond);
                dt = dt.AddSeconds((double)initialTime.Second);
                dt = dt.AddMinutes((double)initialTime.Minute);
                dt = dt.AddHours((double)initialTime.Hour);
                timeNode.InnerText = string.Format("{0}:{1}:{2}:{3}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            }

            XmlNodeList dateNodes = doc.GetElementsByTagName("Date");
            foreach (XmlNode dateNode in dateNodes)
            {
                string date = dateNode.InnerText;
                DateTime dt = Convert.ToDateTime(date);
                dt = new DateTime(initialTime.Year, initialTime.Month, initialTime.Day);
                dateNode.InnerText = string.Format("{0}/{1}/{2}", dt.Month, dt.Day, dt.Year);
            }
        }

        private void ConvertTextToXmlNode(XmlNode childNode, XmlNode refNode, XmlDocument doc)
        {
            string nodeText = childNode.InnerText;
            childNode.InnerText = string.Empty;
            XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
            docFrag.InnerXml = "<Comment type=\"98\"><Text>"
                    + ConvertUnsafeXmlStrings(nodeText)
                    + "</Text><Date></Date><Time></Time></Comment>";

            XmlNode sibling = childNode.NextSibling;
            bool doesNextSiblingExist = true;
            if (null == sibling)
            {
                sibling = childNode.PreviousSibling;
                doesNextSiblingExist = false;
            }
            if (doesNextSiblingExist)
            {
                refNode.InsertBefore(docFrag, sibling);
            }
            else
            {
                refNode.InsertAfter(docFrag, sibling);
            }

            // Save the doc.
            doc.Save(m_logFile);
        }

        private void InsertInfoStringsInLog(XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("/SPOT_Platform_Test");

            foreach (XmlNode firstLevelChildNode in node.ChildNodes)
            {
                if (firstLevelChildNode.NodeType == XmlNodeType.Text)
                {
                    ConvertTextToXmlNode(firstLevelChildNode, node, doc);
                }

                foreach (XmlNode secondLevelChildNode in firstLevelChildNode.ChildNodes)
                {
                    if (secondLevelChildNode.NodeType == XmlNodeType.Text)
                    {
                        if ((firstLevelChildNode.Attributes.Count == 1) &&
                            (!string.Equals(firstLevelChildNode.Attributes[0].Value, "99",
                            StringComparison.InvariantCultureIgnoreCase)))
                        {
                            ConvertTextToXmlNode(secondLevelChildNode, firstLevelChildNode, doc);
                        }
                    }

                    foreach (XmlNode thirdLevelChildNode in secondLevelChildNode.ChildNodes)
                    {
                        if (thirdLevelChildNode.NodeType == XmlNodeType.Text)
                        {
                            if ((secondLevelChildNode.Attributes.Count == 1) &&
                                (!string.Equals(secondLevelChildNode.Attributes[0].Value, "99",
                            StringComparison.InvariantCultureIgnoreCase)))
                            {
                                ConvertTextToXmlNode(thirdLevelChildNode, secondLevelChildNode, doc);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}