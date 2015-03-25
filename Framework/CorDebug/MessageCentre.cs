using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Diagnostics;


namespace Microsoft.SPOT.Debugger
{

    public abstract class MessageCentre
    {
        protected static readonly Guid s_InternalErrorsPaneGuid     = Guid.NewGuid();
        protected static readonly Guid s_DeploymentMessagesPaneGuid = Guid.NewGuid();

        //--//

        public abstract void DebugMsg(string msg);

        public abstract void ClearDeploymentMsgs();

        public abstract void DeploymentMsg(string msg);

        public abstract void DeployDot();

        public abstract void InternalErrorMsg(string msg);
        
        public abstract void InternalErrorMsg(bool assertion, string msg);

        public abstract void InternalErrorMsg(bool assertion, string msg, int skipFrames);

        public abstract void OutputMsgHandler(object sendingProcess, DataReceivedEventArgs outLine);

        public abstract void ErrorMsgHandler(object sendingProcess, DataReceivedEventArgs outLine);

        public abstract void StartProgressMsg(string msg);

        public abstract void StopProgressMsg(string msg);

        public void StopProgressMsg()
        {
            this.StopProgressMsg(null);
        }
    }

//--//

    public class NullMessageCentre : MessageCentre
    {
        public override void DebugMsg(string msg)
        {
        }

        public override void ClearDeploymentMsgs()
        {
        }

        public override void DeploymentMsg(string msg)
        {
        }

        public override void DeployDot()
        {
        }

        public override void InternalErrorMsg(string msg)
        {
        }

        public override void InternalErrorMsg(bool assertion, string msg)
        {
        }

        public override void InternalErrorMsg(bool assertion, string msg, int skipFrames)
        {
        }

        public override void OutputMsgHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
        }

        public override void ErrorMsgHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
        }

        public override void StartProgressMsg(string msg)
        {
        }

        public override void StopProgressMsg(string msg)
        {
        }
    }

//--//

    public class MessageCentreDeployment : MessageCentre
    {
        private IVsOutputWindow     m_outputWindow;
        private IVsOutputWindowPane m_debugPane;
        private IVsOutputWindowPane m_deploymentMessagesPane;
        private IVsStatusbar        m_statusBar;
        private bool                m_fShowInternalErrors;

        //--//

        public MessageCentreDeployment()
        {
            m_outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            if(m_outputWindow == null) throw new Exception( "Package.GetGlobalService(SVsOutputWindow) failed to provide the output window" );

            Guid tempId = VSConstants.GUID_OutWindowDebugPane;
            m_outputWindow.GetPane(ref tempId, out m_debugPane);

            tempId = s_DeploymentMessagesPaneGuid;
            m_outputWindow.CreatePane(ref tempId, "Micro Framework Device Deployment", 0, 1);

            tempId = s_DeploymentMessagesPaneGuid;
            m_outputWindow.GetPane(ref tempId, out m_deploymentMessagesPane);

            m_fShowInternalErrors = false;
            if (RegistryAccess.GetBoolValue(@"\NonVersionSpecific\UserInterface", "showInternalErrors", out m_fShowInternalErrors, false))
            {
                this.Message(m_deploymentMessagesPane, "Micro Framework deployment internal errors will be reported.");
            }

            m_statusBar = Package.GetGlobalService(typeof(SVsStatusbar)) as IVsStatusbar;
        }

        public override void DebugMsg(string msg)
        {
            this.Message(m_debugPane, msg==null?"":msg);
        }

        public override void ClearDeploymentMsgs()
        {
            try
            {
                if (m_deploymentMessagesPane != null)
                    m_deploymentMessagesPane.Clear();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public override void DeploymentMsg(string msg)
        {
            this.Message(m_deploymentMessagesPane, msg);
        }

        public override void DeployDot()
        {
            try
            {
                int fFrozen = 1;
                string msg;

                if (m_statusBar != null
                    && m_statusBar.IsFrozen(out fFrozen) == Utility.COM_HResults.S_OK
                    && fFrozen != 1
                    && m_statusBar.GetText(out msg) == Utility.COM_HResults.S_OK
                    )
                {
                    m_statusBar.SetText(msg + ".");
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        public override void InternalErrorMsg(string msg)
        {
            this.InternalErrorMsg(false, msg);
        }

        public override void InternalErrorMsg(bool assertion, string msg)
        {
            this.InternalErrorMsg(assertion, msg, -1);
        }

        public override void InternalErrorMsg(bool assertion, string msg, int skipFrames)
        {
            if (!assertion && m_fShowInternalErrors)
            {
                msg = String.IsNullOrEmpty(msg) ? "Unknown Error" : msg;
                
                if (skipFrames >= 0)
                {
                    StackTrace st = new StackTrace(skipFrames + 1, true);
                    this.Message(m_deploymentMessagesPane, String.Format("[@ {0}: {1} @]", msg, st.ToString()));
                }
                else
                {
                    this.Message(m_deploymentMessagesPane, "[@ " + msg + " @]");
                }
            }
        }

        public override void OutputMsgHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.DebugMsg(outLine.Data);
        }


        public override void ErrorMsgHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.DebugMsg(outLine.Data);
        }

        //--//

        private void Message(IVsOutputWindowPane pane, String msg)
        {
            if(pane == null)
                return;

            if(msg==null)
                msg = "[no message string provided to MessageCentre.Message()" + new StackTrace().ToString();

            try
            {
                lock (pane)
                {
                    pane.Activate();
                    pane.OutputStringThreadSafe(msg + "\r\n");
                }
            }
            catch( InvalidComObjectException )
            {
            }

        }

        public override void StartProgressMsg(string msg)
        {
            try
            {
                int fFrozen = 1;
                if (m_statusBar != null && m_statusBar.IsFrozen(out fFrozen) == Utility.COM_HResults.S_OK && fFrozen != 1)
                {
                    m_statusBar.SetText(msg);
                }
            }
            catch (InvalidOperationException)
            {
            }

        }

        public override void StopProgressMsg(string msg)
        {
            try
            {
                int fFrozen = 1;
                if (m_statusBar != null && m_statusBar.IsFrozen(out fFrozen) == Utility.COM_HResults.S_OK)
                {
                    if (fFrozen == 1)
                    {
                        m_statusBar.FreezeOutput(0);
                    }

                    if (String.IsNullOrEmpty(msg))
                        m_statusBar.Clear();
                    else
                        m_statusBar.SetText(msg);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}

