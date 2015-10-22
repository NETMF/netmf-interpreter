using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using CODESIGN.Submitter;

namespace Microsoft.SPOT.AutomatedBuild.BuildSigner
{
    public class SignInfo
    {
        public string jobDescription;
        public string jobKeywords;
        public string certificate;
        public string[] Approvers;
        public string displayName;
        public string displayURL;
    }

    public class BuildSigner
    {
        static Task m_CallingTask;
        static SignInfo m_Info;

        public static bool SignFiles(string [] files, string stageRoot, SignInfo info, Task callingTask)
        {
            m_CallingTask = callingTask;
            m_Info = info;

            bool retVal = true;

            if (!stageRoot.EndsWith("\\"))
            {
                stageRoot += "\\";
            }

            string[] level1 = { "Unsigned\\" };
            string[] level2 = { "Client", "Server" };

            string unsignedStage = stageRoot + level1[0];
            
            // Set up staging area
            if (Directory.Exists(stageRoot))
                Directory.Delete(stageRoot, true);

            Directory.CreateDirectory(stageRoot);

            foreach (string level in level1)
            {
                string parent = stageRoot + level;

                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                foreach (string nextLevel in level2)
                {
                    string child = parent + nextLevel;

                    if (!Directory.Exists(child))
                    {
                        Directory.CreateDirectory(child);
                    }
                }
            }

            Dictionary<string, string> fileDictionary = new Dictionary<string, string>();

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    callingTask.Log.LogWarning("File not found: {0}", file);
                    continue;
                }

                if ((File.GetAttributes(file) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    callingTask.Log.LogMessage("File \"{0}\" is read only and will not be signed.", file);
                    continue;
                }

                string directory = Path.GetDirectoryName(file).ToLower();
                bool client = directory.Contains("\\client\\");

                string key = (client ? "Client\\" : "Server\\") + Path.GetFileName(file);
                if (!fileDictionary.ContainsKey(key))
                {
                    fileDictionary.Add(key, file);
                    File.Copy(file, unsignedStage + (client ? "Client\\" : "Server\\") + Path.GetFileName(file), true);
                }
            }


            string signedStage = stageRoot + "Signed\\";
            retVal = SubmitJob(unsignedStage, signedStage);

            ReturnFiles(signedStage, fileDictionary, "Client", callingTask);
            ReturnFiles(signedStage, fileDictionary, "Server", callingTask);

            Directory.Delete(unsignedStage, true);
            
            return retVal;
        }

        #region Email From Code Sign
        /*
            Hi Frederic,

            Per approval by Eric Lang, GM, your lab account  MFALA, has been set up.

            Your permissions have been setup as requested. We've granted you smart card 
            bypass access to http://Codesignaoc and permissions to sign with the four most 
            commonly used Authenticode certificates 
                
                #2 Microsoft Corporation (Internal Use Only)  
                #23 Microsoft Corporation (MD5 MS Root) 
                #98 Microsoft Corporation ClickOnce Signing 
                #72 Microsoft Shared Libraries (.NET 2.0).

            On each computer you would submit, approve or search job from, you must follow the 3 steps outlined outline below.

                1) You need to have .NET Framework 2.0 or higher installed on your machine. 
                   (Already installed on for Vista users.)  
                   It can be found at \\products\public\Products\Developers\NET Micro Framework SDK 2.0. 

                2) You will need to install the Codesign.Object which can be found in: 
                   \\CSNEOVLT.dns.corp.microsoft.com\Public\Submitter Tool for Download\SubmitterV2.3_.NET2.0 . 
                   Copy Codesign.Submitter.msi to your local drive and then install it from there. 

                3) SmartCard - The CodeSign process is an important piece of protecting our corporate assets, 
                   therefore use of this site and related activities are secured appropriately. 
                   Users that wish to use code sign's website services must have permissions granted to
                   do so and use a corporate issued SmartCard as a means of verifying their identity.

            SMARTCARDS ARE CREATED AND DISTRIBUTED BY CORPORATE SECURITY - PLEASE GOTO http://csamweb/smartcards/default.asp TO OBTAIN A SMARTCARD. 

            Verify your installations, you can go the http://codesignaoc and click on the  Diagnostics  tab.  
            There you will find 2 buttons:  Verify Installation  and  Verify Smart Card . 
            Please click on both of them.  If you get the "Good to go!" message on both of them, 
            then you are able to fully access the site.  If you get an error, please take a screen 
            shot of the error and e-mail it to relsup@microsoft.com. We will troubleshoot your installation.

            You will need two FTEs with codesign accounts to act as approvers in the codesugn submission stage. 

            We will send any updates about codesign system to SIGNNEWS. Please join this autogroup if you have not already done so.

            http://AutoGroup/JoinGroup.asp?GroupAlias=signnews

            Please let us know if you need any further assistance.

            Thanks,
            DBRICK
            Release Technical Solutions Centre
            +1 (425) 7032736 X32736
            relsup@microsoft.com
            -Helping Our Customers Reach Theirs
        */
        #endregion

        //"Codesign" represents the server - static variable do not change
        //9556 represents the port - constant value do not change
        const string RelayServer = "codesign.gtm.microsoft.com";
        const int RelayPort = 9556;

        private static bool SubmitJob(string unsignedDirectory, string signedDirectory)
        {
            // use env var so it is dynamic for the machine running this build to help prevent
            // accidentally leaving this setting enabled in a code check-in.
            var fakeSign = Environment.GetEnvironmentVariable("DEVFAKESIGN") != null;

            CODESIGN.Submitter.Job job = null;
            try
            {
                //Initialize the Codesign.Submitter object
                job = CODESIGN.Submitter.Job.Initialize(RelayServer, RelayPort, true);

                // Sets the Partial return flag option.
                // False - If any files fail signing you will not get any files back.
                // True - Only retrieve successfully signed files.
                job.IsAllowReturnPartial = false;		// default is false

                // Set this flag true to ensure that the file types you are submitting can be signed
                // with the selected certificates, false to let the system try (bypass filetype 
                // validation). This is particularly useful if your file type is new and unknown to
                // the system.
                job.IsRequireVerifyCerts = false;		// default is false 

                // Set this flag true to enforce hash checking during copies as well as staging; this
                // isn’t normally necessary but is provided for network integrity. It does slow copy
                // performance significantly
                job.IsRequireHash = false;				// default is false

                // This is reference information that can be displayed or used in searches
                job.Description = m_Info.jobDescription;
                job.Keywords = m_Info.jobKeywords;

                // This call selects a certificate from the ones allowed for this user
                // You must have permissions to the requested cert or this call throws an exception
                if(!fakeSign)
                    job.SelectCertificate(m_Info.certificate);	

                // These calls add notification subscriptions to the job. A number of others are 
                // available, these are the standard ones.
                job.SetNotification(
                    job.Submitter, 
                    new CODESIGN.NotificationEventTypeEnum[] 
                        { 
                            CODESIGN.NotificationEventTypeEnum.JobCompletionFailure, 
                            CODESIGN.NotificationEventTypeEnum.JobCompletionSuccess, 
                            CODESIGN.NotificationEventTypeEnum.JobVirusScanFailure 
                        });

                foreach (string approver in m_Info.Approvers)
                {
                    job.AddApprover(approver);
                }
                
                //This will remove all notifications
                job.ClearTargets();  //Clears notification for all but submitter

                // This call adds an entire directory tree to the job, duplicating its structure in
                // the submission share and making all metadata the same for each file.
                job.AddFile(
                    unsignedDirectory + "Client", 
                    m_Info.displayName, 
                    m_Info.displayURL, 
                    CODESIGN.JavaPermissionsTypeEnum.None);
                
                job.AddFile(
                    unsignedDirectory + "Server",
                    m_Info.displayName,
                    m_Info.displayURL,
                    CODESIGN.JavaPermissionsTypeEnum.None);

                // This call sends the job to the back end for processing
                if (!fakeSign)
                    job.Send();
                else
                {
                    m_CallingTask.Log.LogMessage("!!!!**** DEVFAKESIGN is set, skipping submit and just copying files for development testing ****!!!!");
                    var userTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                    var fakeCompletionPath = Path.Combine(userTemp, Path.GetRandomFileName());
                    Directory.CreateDirectory(fakeCompletionPath);
                    job.JobCompletionPath = fakeCompletionPath;
                    job.JobNumber = "DEVFAKESIGN";
                    CopyTree(unsignedDirectory, fakeCompletionPath );
                }

                // This call displays the job number, assigned during the send process
                m_CallingTask.Log.LogMessage("Job Number is: {0}", job.JobNumber);
                m_CallingTask.Log.LogMessage("Job Completion Path is: {0}", job.JobCompletionPath);

                JobWatcher jw = new JobWatcher();
                if (!fakeSign)
                {
                    try
                    {
                        jw.Watch(job.JobNumber, RelayServer, RelayPort, true);
                    }
                    catch(Exception ex )
                    { // I hate general exceptions but the complete lack of documentation on the submitter leaves little choice and we need to ship... 
                        m_CallingTask.Log.LogWarning("Exception waiting for job - assuming job completed, subsequent steps may fail.\nExeption: {0}", ex);
                    }
                }

                // Now we're done, so display any errors or warnings (in case we are in non-event mode)
                bool retVal = true;
                m_CallingTask.Log.LogMessage(
                    "Job is finished, Success={0}  Signed={1}  BytesSigned={2}", 
                    fakeSign ? true : jw.IsSuccess, 
                    fakeSign ? 0 : jw.TotalSigned, 
                    fakeSign ? 0 : jw.TotalByteSize);

                if (!fakeSign)
                {
                    if (jw.IsPartial)
                    {
                        m_CallingTask.Log.LogError("Partial Success: {0}", jw.IsPartial);
                        retVal = false;
                    }

                    foreach (JobError je in job.ErrorList.Values)
                    {
                        m_CallingTask.Log.LogError(je.Number + ":" + je.Description + " {" + je.Explanation + "}");
                        retVal = false;
                    }
                    foreach (JobFile jf in jw.FailedFileList.Values)
                    {
                        m_CallingTask.Log.LogError("Failed -> " + jf.FileFullPath);
                        retVal = false;
                    }
                }
                m_CallingTask.Log.LogMessage("Copying files from codesign server");
                var completionPath = fakeSign ? job.JobCompletionPath : jw.CompletionPath;
                CopyTree( completionPath, signedDirectory);

                return retVal;
            }
            catch (Exception exc)
            {
                m_CallingTask.Log.LogError("Job submission failed: {0}", CODESIGN.EventLogProxy.GetMessage(exc));
                foreach (JobError je in job.ErrorList.Values)
                {
                    m_CallingTask.Log.LogError(je.Number + ":" + je.Description + " {" + je.Explanation + "}");
                }

                return false;
            }

        }

        public static void CopyTree(string sourceTree, string destTree, bool overWrite = false)
        {
            if(!Directory.Exists(destTree))
            {
                Directory.CreateDirectory(destTree);
            }

            foreach (string subdir in Directory.GetDirectories(sourceTree))
            {
                CopyTree(subdir, destTree + "\\" + Path.GetFileName(subdir));
            }

            foreach (string file in Directory.GetFiles(sourceTree))
            {
                File.Copy(file, destTree + "\\" + Path.GetFileName(file), overWrite);
            }
        }

        private static void ReturnFiles(string signedStage, Dictionary<string, string> fileDictionary, string buildType, Task callingTask)
        {
            if(!Directory.Exists(signedStage + buildType))
            {   //Empty folders don't get created; such as when there are no client files in the job.
                return;
            }

            string[] files = Directory.GetFiles(signedStage + buildType);

            foreach (string file in files)
            {
                string key = buildType + "\\" + Path.GetFileName(file);

                if (!fileDictionary.ContainsKey(key))
                {
                    callingTask.Log.LogWarning("Extra file found in signed stage directory: {0}", file);
                    continue;
                }
                callingTask.Log.LogMessage("Replacing file {0} with signed file {1}", fileDictionary[key], file);
                try
                {
                    File.Copy( file, fileDictionary[ key ], true );
                }
                catch(Exception ex)
                {
                    callingTask.Log.LogErrorFromException( ex, true, true, file );
                    throw ex;
                }
            }
        }
    }
}
