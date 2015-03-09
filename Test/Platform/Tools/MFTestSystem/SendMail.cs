using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.SPOT.Platform.Test
{
    internal class SendMail
    {
        #region Member variables

        private string m_from;
        private string m_to;
        private string m_subject;
        private string m_body;
        private bool m_isHtml = false;

        #endregion

        #region Internal properties

        internal string From
        {
            get 
            {
                return m_from; 
            }
            
            set 
            {
                m_from = value; 
            }
        }

        internal string To
        {
            get 
            { 
                return m_to; 
            }
            
            set 
            { 
                m_to = value; 
            }
        }

        internal string Subject
        {
            get 
            { 
                return m_subject; 
            }
            
            set 
            { 
                m_subject = value; 
            }
        }

        internal string Body
        {
            get 
            { 
                return m_body; 
            }
            
            set 
            { 
                m_body = value; 
            }
        }

        internal bool IsBodyHtml
        {
            get 
            {
                return m_isHtml; 
            }
            
            set 
            {
                m_isHtml = value; 
            }
        }

        #endregion

        #region Execute

        internal bool Execute()
        {
            try
            {
                // If a file exists on the file system with same name as body
                // then open that and send it.
                // Else assume that the user wants to send whatever body contains.
                if (File.Exists(m_body))
                {
                    StreamReader reader = new StreamReader(m_body);

                    m_body = reader.ReadToEnd();
                }

                SmtpClient client = new SmtpClient("smtphost");

                MailMessage mm = new MailMessage(
                    m_from,
                    m_to,
                    m_subject,
                    m_body);

                mm.IsBodyHtml = m_isHtml;

                client.UseDefaultCredentials = true;

                client.Send(mm);
                return true;
            }
            catch (Exception)
            {                
                return false;
            }
        }

        #endregion
    }
}