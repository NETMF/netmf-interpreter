/******************************************************************************
'
' THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
' EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
' WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
'
' Copyright (C) 1999 - 2003.  Microsoft Corporation.  All rights reserved.
'
'******************************************************************************
'
' CStore.cs
'
' This is a sample c# program to illustrate how to use the X509Store and 
' X509Certificate2 classes to manage X509 certificate(s) in a specified store 
' with filtering options.
'
' Note: For simplicity, this program does not handle exception.
'
'*****************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class CStore : IMFTestInterface
    {
        bool m_isEmulator;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // Add your functionality here.                
            try
            {
                m_isEmulator = (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3);
            }
            catch
            {
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        enum Command
        {
            Unknown = 0,
            View = 1,
            Import = 2,
            Export = 3,
            Delete = 4,
            Archive = 5,
            Activate = 6,
        }

        enum Verbose
        {
            Normal = 0,
            Detail = 1,
            UI = 2,
        }

        static Command command = Command.Unknown;
        //static StoreLocation        storeLocation             = StoreLocation.CurrentUser;
        //static X509KeyStorageFlags  keyStorageFlag            = X509KeyStorageFlags.DefaultKeySet;
        //static X509ContentType      saveAs                    = X509ContentType.SerializedStore;
        static bool delKey = false;
        static bool noPrompt = false;
        static bool validOnly = false;
        static bool loadFromCertFile = false;
        static StoreName storeName = StoreName.My;
        static string certFile = null;
        static byte[] certData = null;
        static string password = null;
        static string sha1 = null;
        static ArrayList subjects = new ArrayList();
        static ArrayList issuers = new ArrayList();
        static ArrayList serials = new ArrayList();
        static ArrayList templates = new ArrayList();
        static ArrayList extensions = new ArrayList();
        static ArrayList ekus = new ArrayList();
        static ArrayList policies = new ArrayList();
        static ArrayList keyUsages = new ArrayList();
        static bool performTimeValidityCheck = false;
        static DateTime time = DateTime.Now;
        static X509FindType expirationType = X509FindType.FindByTimeValid;
        static Verbose verbose = Verbose.Normal;
        static OpenFlags openMode = OpenFlags.ReadOnly;

        #region Main
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        [TestMethod]
        public MFTestResults CStore_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session(""))
                {
                    X509Store store = new X509Store(sess, StoreName.My);
                    X509Certificate2 cert = new X509Certificate2(sess, Properties.Resources.GetBytes(Properties.Resources.BinaryResources.cacert), "");

                    bool fInit = (store.Certificates.Count == 0);

                    if(!fInit)
                    {
                        ArrayList rem = new ArrayList();
                        for (int i = 0; i < store.Certificates.Count; i++)
                        {
                            rem.Add(store.Certificates[i]);
                        }

                        foreach (X509Certificate2 certx in rem)
                        {
                            store.Remove(certx);
                        }
                    }

                    store.Add(cert, "CACert");

                    bRes &= Test(sess, "VIEW", "-v", "1", "My");

                    //store.Remove(cert);

                    //X509Certificate2 cert2 = new X509Certificate2(sess, Properties.Resources.GetBytes(Properties.Resources.BinaryResources.cacert), "");

                    for (int i=0; i<100; i++)
                    {
                        store.Add(cert, "CACert2");
                        store.Remove(cert);
                    }

                    store.Add(cert, "CACert3");

                    bRes &= Test(sess, "VIEW", "-v", "1", "My");

                    // cleanup
                    ArrayList rem2 = new ArrayList();
                    for (int i = 0; i < store.Certificates.Count; i++)
                    {
                        rem2.Add(store.Certificates[i]);
                    }

                    foreach (X509Certificate2 certx in rem2)
                    {
                        store.Remove(certx);
                    }
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA_1_HMAC))
                    {
                        X509Store store = new X509Store(sess, StoreName.My);

                        X509Certificate2 cert = new X509Certificate2(sess, Properties.Resources.GetBytes(Properties.Resources.BinaryResources.cacert), "");
                        store.Add(cert, "CACert");
                        
                        bRes &= Test(sess, "VIEW", "-v", "1", "My");

                        store.Remove(cert);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("", e);
                bRes = false;
            }
            return bRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        static bool Test(Session session, params string[] args)
        {
            try
            {
                string title = string.Empty;

                // Parse the command line.
                ParseCommandLine(args);

                // Open the source (store and/or certfile).
                X509Store store = new X509Store(session, storeName); //, storeLocation);
                X509Certificate2Collection certificates; // = store.Certificates; //  = new X509Certificate2Collection();
                switch (command)
                {
                    case Command.View:
                        {
                            // For view, source can come from either store or cert file.
                            if (loadFromCertFile)
                            {
                                delKey = true;
                                //certificates.Import(certFile, password); //, X509KeyStorageFlags.DefaultKeySet);
                                X509Certificate2 cert = new X509Certificate2(session, certData, password);
                                store.Add(cert, certFile);
                                title = "Viewing " + certFile;
                            }
                            else
                            {
                                store.Open(openMode | OpenFlags.OpenExistingOnly);
                                title = "Viewing " + /*storeLocation.ToString() +*/ " " + storeName + " store";
                            }

                            certificates = (X509Certificate2Collection)store.Certificates;
                            // Perform filter(s) requested.
                            certificates = FilterCertificates(certificates);

                            // Carry out the command.
                            DoViewCommand(title, certificates);
                            break;
                        }
                    case Command.Import:
                        {
                            // For import, the source to filter comes from the cert file,
                            // and the destination is the store.
                            store.Open(OpenFlags.ReadWrite);
                            //certificates.Import(certFile, password); //, keyStorageFlag | X509KeyStorageFlags.PersistKeySet);
                            store.Add(new X509Certificate2(session, certData, password), certFile);
                            
                            title = "Importing certificates from " + certFile + " to " /*+ storeLocation.ToString()*/ + storeName;

                            certificates = (X509Certificate2Collection)store.Certificates;

                            // Perform filter(s) requested.
                            certificates = FilterCertificates(certificates);

                            // Carry out the command.
                            DoImportCommand(title, store, certificates);
                            break;
                        }
                    case Command.Export:
                        {
                            // For export, open existing store for read access.
                            store.Open(openMode | OpenFlags.OpenExistingOnly);
                            certificates = (X509Certificate2Collection)store.Certificates;
                            title = "Exporting certificates from " + /*storeLocation.ToString() +*/ storeName + " to " + certFile;

                            // Perform filter(s) requested.
                            certificates = FilterCertificates(certificates);

                            // Carry out the command.
                            DoExportCommand(title, certificates);
                            break;
                        }
                    case Command.Delete:
                        {
                            // For delete, open existing store for write access.
                            store.Open(openMode | OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                            certificates = (X509Certificate2Collection)store.Certificates;
                            title = "Deleting certificates from " + /*storeLocation.ToString() +*/ storeName;

                            // Perform filter(s) requested.
                            certificates = FilterCertificates(certificates);

                            // Carry out the command.
                            DoDeleteCommand(title, store, certificates);
                            break;
                        }
                    //case Command.Archive:
                    //    {
                    //        // For archive, open existing store for write access.
                    //        store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                    //        certificates = (X509Certificate2Collection)store.Certificates;
                    //        title = "Archiving certificates for " + /*storeLocation.ToString() +*/ storeName;

                    //        // Perform filter(s) requested.
                    //        certificates = FilterCertificates(certificates);

                    //        // Carry out the command.
                    //        DoArchiveCommand(title, certificates);
                    //        break;
                    //    }
                    //case Command.Activate:
                    //    {
                    //        // For activate, open existing store, including archived certificates, for write access.
                    //        store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly | OpenFlags.IncludeArchived);
                    //        certificates = (X509Certificate2Collection)store.Certificates;
                    //        title = "Activating archived certificates in " + /*storeLocation.ToString() +*/ storeName;

                    //        // Perform filter(s) requested.
                    //        certificates = FilterCertificates(certificates);

                    //        // Carry out the command.
                    //        DoActivateCommand(title, certificates);
                    //        break;
                    //    }
                    default:
                        {
                            // We should never get here!
                            throw new InternalException("Internal error: Unknown command (command = " + command.ToString() + ").");
                        }
                }
            }
            catch (UsageException e)
            {
                DisplayUsage(e.Message);
                return false;
            }
            catch (AbortException e)
            {
                Log.Comment(e.Message);
                return false;
            }
            catch (InternalException e)
            {
                Log.Comment(e.Message);
                return false;
            }

            return true;
        }
        #endregion

        #region DoViewCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the View command.
        /// </summary>
        static void DoViewCommand(string title, X509Certificate2Collection certificates)
        {
            Log.Comment(title + " - " + certificates.Count.ToString() + " certificate(s)");
            Log.Comment("");
            for (int index = 0; index < certificates.Count; index++)
            {
                DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificates[index]);
            }
            return;
        }
        #endregion

        #region DoImportCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the Import command.
        /// </summary>
        static void DoImportCommand(string title, X509Store store, X509Certificate2Collection certificates)
        {
            Log.Comment(title + " - " + certificates.Count.ToString() + " certificate(s)");
            Log.Comment("");
            //store.AddRange(certificates);

            for (int i = 0; i < certificates.Count; i++)
            {
                store.Add(certificates[i]);
            }
            for (int index = 0; index < certificates.Count; index++)
            {
                DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificates[index]);
            }
            return;
        }
        #endregion

        #region DoExportCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the Export command.
        /// </summary>
        static void DoExportCommand(string title, X509Certificate2Collection certificates)
        {
            //Log.Comment(title + " - " + certificates.Count.ToString() + " certificate(s)");
            //Log.Comment("");
            //FileStream file = File.Create(certFile);
            //byte[] bytes = certificates.Export(saveAs, password);
            //file.Write(bytes, 0, bytes.Length);
            //for (int index = 0; index < certificates.Count; index++)
            //{
            //    DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificates[index]);
            //}
            //return;
        }
        #endregion

        #region DoDeleteCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the Delete command.
        /// </summary>
        static void DoDeleteCommand(string title, X509Store store, X509Certificate2Collection certificates)
        {
            int index = 0;
            int deleted = 0;
            string prompt = delKey ? "Delete this certificate and its key container (No/Yes/All)? " :
                                     "Delete this certificate (No/Yes/All)? ";

            Log.Comment(title + " - " + certificates.Count.ToString() + " certificate(s)");
            Log.Comment("");

            foreach (X509Certificate2 certificate in certificates)
            {
                DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificate);

                if (UserAgreed(prompt))
                {
                    store.Remove(certificate);
                    deleted++;
                }
                index++;
            }

            Log.Comment(deleted + " certificate(s) successfully deleted.");
            return;
        }
        #endregion

        #region DoArchiveCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the Archive command.
        /// </summary>
        //static void DoArchiveCommand(string title, X509Certificate2Collection certificates)
        //{
        //    int archived = 0;

        //    Log.Comment(title + "  " + "Please wait...");
        //    Log.Comment("");

        //    for (int index = 0; index < certificates.Count; index++)
        //    {
        //        if (!certificates[index].Archived)
        //        {
        //            DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificates[index]);

        //            if (UserAgreed("Archive this certificate (No/Yes/All)? "))
        //            {
        //                certificates[index].Archived = true;
        //                archived++;
        //            }
        //        }
        //    }

        //    Log.Comment(archived + " certificate(s) successfully archived.");
        //    return;
        //}
        #endregion

        #region DoActivateCommand
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the Activate command.
        /// </summary>
        //static void DoActivateCommand(string title, X509Certificate2Collection certificates)
        //{
        //    int index = 0;
        //    int activated = 0;

        //    Log.Comment(title + "  " + "Please wait...");
        //    Log.Comment("");

        //    foreach (X509Certificate2 certificate in certificates)
        //    {
        //        if (certificate.Archived)
        //        {
        //            DisplayCertificate("=== Certificate " + (index + 1).ToString() + " of " + certificates.Count.ToString() + " ===", certificate);

        //            if (UserAgreed("Active this certificate (No/Yes/All)? "))
        //            {
        //                certificate.Archived = false;
        //                activated++;
        //            }
        //        }
        //        index++;
        //    }

        //    Log.Comment(activated + " certificate(s) successfully activated.");
        //    return;
        //}
        #endregion

        #region UserAgreed
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Prompt user for permission to carry out a command.
        /// </summary>
        static bool UserAgreed(string message)
        {
            bool answer = noPrompt;

            // Prompt the user if not disabled.
            //if (!answer)
            //{
            //    int tries;

            //    for (tries = 0; tries < 3; tries++)
            //    {
            //        Console.Write(message);
            //        string response = Console.ReadLine().ToUpper();
            //        Log.Comment("");

            //        if ("Y" == response || "YES" == response)
            //        {
            //            answer = true;
            //            break;
            //        }
            //        else if ("N" == response || "NO" == response)
            //        {
            //            answer = false;
            //            break;
            //        }
            //        else if ("A" == response || "ALL" == response)
            //        {
            //            noPrompt = true;
            //            answer = true;
            //            break;
            //        }
            //        else
            //        {
            //            Log.Comment("Valid answers are No, Yes, or All. Please try again.");
            //        }
            //    }

            //    if (3 <= tries)
            //    {
            //        throw new AbortException("Too many tries. Programming is exiting.");
            //    }
            //}

            return (answer);
        }
        #endregion

        #region DisplayCertificate
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Display the certificate to console or using the UI.
        /// </summary>
        static void DisplayCertificate(string title, X509Certificate2 certificate)
        {
            Log.Comment(title);
            if (Verbose.UI == verbose)
            {
                //X509Certificate2UI.DisplayCertificate(certificate);
            }
            else if (Verbose.Detail == verbose)
            {
                Log.Comment(certificate.ToString(true));
            }
            else
            {
                // Normal.
                Log.Comment(certificate.ToString(false));
            }
        }
        #endregion

        #region FilterCertificates
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Carry out the filters requested.
        /// </summary>
        static X509Certificate2Collection FilterCertificates(X509Certificate2Collection certificates)
        {
            int index;
            if (0 < certificates.Count && null != sha1)
            {
                certificates = certificates.Find(X509FindType.FindByThumbprint, sha1, false);
            }

            if (0 < certificates.Count && 0 < subjects.Count)
            {
                foreach (string subject in subjects)
                {
                    certificates = certificates.Find(X509FindType.FindBySubjectName, subject, false);
                }
            }
            if (0 < certificates.Count && 0 < issuers.Count)
            {
                foreach (string issuer in issuers)
                {
                    certificates = certificates.Find(X509FindType.FindByIssuerName, issuer, false);
                }
            }
            if (0 < certificates.Count && 0 < serials.Count)
            {
                foreach (string serial in serials)
                {
                    certificates = certificates.Find(X509FindType.FindBySerialNumber, serial, false);
                }
            }
            if (0 < certificates.Count && 0 < templates.Count)
            {
                foreach (string template in templates)
                {
                    certificates = certificates.Find(X509FindType.FindByTemplateName, template, false);
                }
            }
            if (0 < certificates.Count && 0 < extensions.Count)
            {
                foreach (string extension in extensions)
                {
                    certificates = certificates.Find(X509FindType.FindByExtension, extension, false);
                }
            }
            if (0 < certificates.Count && 0 < ekus.Count)
            {
                foreach (string eku in ekus)
                {
                    certificates = certificates.Find(X509FindType.FindByApplicationPolicy, eku, false);
                }
            }
            if (0 < certificates.Count && 0 < policies.Count)
            {
                foreach (string policy in policies)
                {
                    certificates = certificates.Find(X509FindType.FindByCertificatePolicy, policy, false);
                }
            }
            if (0 < certificates.Count && 0 < keyUsages.Count)
            {
                for (index = 0; index < keyUsages.Count; index++)
                {
                    certificates = certificates.Find(X509FindType.FindByKeyUsage, keyUsages[index], false);
                }
            }
            if (0 < certificates.Count && performTimeValidityCheck)
            {
                certificates = certificates.Find(expirationType, time, false);
            }
            if (0 < certificates.Count && validOnly)
            {
                // Use empty subject name to perform null filtering.
                certificates = certificates.Find(X509FindType.FindBySubjectName, string.Empty, true);
            }
            return certificates;
        }
        #endregion

        #region ParseCommandLine
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Parse the command line, and set the options accordingly. Throw
        /// UsageException for fatal argument error or help request. Also will
        /// throw InternalException for fatal internal error.
        /// </summary>

        enum ArgStates
        {
            Command = 0,
            Options = 1,
            Location = 2,
            StoreName = 3,
            SHA1 = 4,
            Subject = 5,
            Issuer = 6,
            Serial = 7,
            Template = 8,
            Extension = 9,
            EKU = 10,
            Policy = 11,
            KeyUsage = 12,
            Time = 13,
            Expiration = 14,
            SaveAs = 15,
            Verbose = 16,
            Password = 17,
            End = 18,
        }

        static void ParseCommandLine(string[] args)
        {
            ArgStates argState = ArgStates.Command;
            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                switch (argState)
                {
                    case ArgStates.Command:
                        {
                            switch (arg.ToUpper())
                            {
                                case "VIEW":
                                    {
                                        command = Command.View;
                                        break;
                                    }
                                case "IMPORT":
                                    {
                                        command = Command.Import;
                                        break;
                                    }
                                case "EXPORT":
                                    {
                                        command = Command.Export;
                                        break;
                                    }
                                case "DELETE":
                                    {
                                        command = Command.Delete;
                                        break;
                                    }
                                //case "ARCHIVE":
                                //    {
                                //        command = Command.Archive;
                                //        break;
                                //    }
                                //case "ACTIVATE":
                                //    {
                                //        command = Command.Activate;
                                //        break;
                                //    }
                                default:
                                    {
                                        throw new UsageException("Error: " + arg + " is not a valid command.");
                                    }
                            }

                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Options:
                        {
                            if (arg.Substring(0, 1) == "-" || arg.Substring(0, 1) == "/")
                            {
                                switch (arg.Substring(1).ToUpper())
                                {
                                    case "L":
                                        {
                                            argState = ArgStates.Location;
                                            break;
                                        }
                                    case "S":
                                        {
                                            argState = ArgStates.StoreName;
                                            break;
                                        }
                                    case "A":
                                        {
                                            openMode = openMode | OpenFlags.IncludeArchived;
                                            break;
                                        }
                                    case "SHA1":
                                        {
                                            argState = ArgStates.SHA1;
                                            break;
                                        }
                                    case "SUBJECT":
                                        {
                                            argState = ArgStates.Subject;
                                            break;
                                        }
                                    case "ISSUER":
                                        {
                                            argState = ArgStates.Issuer;
                                            break;
                                        }
                                    case "SERIAL":
                                        {
                                            argState = ArgStates.Serial;
                                            break;
                                        }
                                    case "TEMPLATE":
                                        {
                                            argState = ArgStates.Template;
                                            break;
                                        }
                                    case "EXTENSION":
                                        {
                                            argState = ArgStates.Extension;
                                            break;
                                        }
                                    case "EKU":
                                        {
                                            argState = ArgStates.EKU;
                                            break;
                                        }
                                    case "POLICY":
                                        {
                                            argState = ArgStates.Policy;
                                            break;
                                        }
                                    case "KEYUSAGE":
                                        {
                                            argState = ArgStates.KeyUsage;
                                            break;
                                        }
                                    case "TIME":
                                        {
                                            argState = ArgStates.Time;
                                            break;
                                        }
                                    case "EXPIRATION":
                                        {
                                            argState = ArgStates.Expiration;
                                            break;
                                        }
                                    case "VALIDONLY":
                                        {
                                            validOnly = true;
                                            break;
                                        }
                                    case "SAVEAS":
                                        {
                                            argState = ArgStates.SaveAs;
                                            break;
                                        }
                                    case "DELKEY":
                                        {
                                            delKey = true;
                                            break;
                                        }
                                    case "NOPROMPT":
                                        {
                                            noPrompt = true;
                                            break;
                                        }
                                    //case "E":
                                    //    {
                                    //        keyStorageFlag |= X509KeyStorageFlags.Exportable;
                                    //        break;
                                    //    }
                                    //case "P":
                                    //    {
                                    //        keyStorageFlag |= X509KeyStorageFlags.UserProtected;
                                    //        break;
                                    //    }
                                    case "V":
                                        {
                                            argState = ArgStates.Verbose;
                                            break;
                                        }
                                    case "?":
                                        {
                                            throw new UsageException(string.Empty);
                                        }
                                    default:
                                        {
                                            throw new UsageException("Error: " + arg + " is not a valid option.");
                                        }
                                }
                            }
                            else
                            {
                                switch (command)
                                {
                                    case Command.View:
                                        {
                                            if (loadFromCertFile)
                                            {
                                                goto case Command.Export;
                                            }
                                            goto default;
                                        }
                                    case Command.Import:
                                    case Command.Export:
                                        {
                                            certFile = arg;
                                            argState = ArgStates.Password;
                                            break;
                                        }
                                    default: // Delete, Archive, Activate.
                                        {
                                            switch (arg)
                                            {
                                                case "My":
                                                    storeName = StoreName.My;
                                                    break;

                                                case "Disallowed":
                                                    storeName = StoreName.Disallowed;
                                                    break;

                                                case "CA":
                                                    storeName = StoreName.CA;
                                                    break;
                                            }
                                            argState = ArgStates.End;
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    //case ArgStates.Location:
                    //    {
                    //        switch (arg.ToUpper())
                    //        {
                    //            case "CU":
                    //                {
                    //                    storeLocation = StoreLocation.CurrentUser;
                    //                    break;
                    //                }
                    //            case "LM":
                    //                {
                    //                    storeLocation = StoreLocation.LocalMachine;
                    //                    break;
                    //                }
                    //            case "FILE":
                    //                {
                    //                    loadFromCertFile = true;
                    //                    break;
                    //                }
                    //            default:
                    //                {
                    //                    throw new UsageException("Error: " + arg + " is not a valid location.");
                    //                }
                    //        }
                    //        argState = ArgStates.Options;
                    //        break;
                    //    }
                    case ArgStates.StoreName:
                        {
                            switch (arg)
                            {
                                case "Disallowed":
                                    storeName = StoreName.Disallowed;
                                    break;

                                case "CA":
                                    storeName = StoreName.CA;
                                    break;

                                case "My":
                                default:
                                    storeName = StoreName.My;
                                    break;
                            }
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.SHA1:
                        {
                            sha1 = arg;
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Subject:
                        {
                            subjects.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Issuer:
                        {
                            issuers.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Serial:
                        {
                            serials.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Template:
                        {
                            templates.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Extension:
                        {
                            extensions.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.EKU:
                        {
                            ekus.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Policy:
                        {
                            policies.Add(arg);
                            argState = ArgStates.Options;
                            break;
                        }
                    //case ArgStates.KeyUsage:
                    //{
                    //   try
                    //   {
                    //      X509KeyUsageFlags keyUsageFlags = (X509KeyUsageFlags) Convert.ToInt32(arg);
                    //      keyUsages.Add(keyUsageFlags);
                    //   }
                    //   catch (FormatException)
                    //   {
                    //      keyUsages.Add(arg);
                    //   }
                    //   argState = ArgStates.Options;
                    //   break;
                    //}
                    //case ArgStates.Time:
                    //{
                    //   try
                    //   {
                    //      time = Convert.ToDateTime(arg);
                    //   }
                    //   catch (FormatException)
                    //   {
                    //      throw new UsageException("Error: " + arg + " is not a valid time.");
                    //   }
                    //   performTimeValidityCheck = true;
                    //   argState = ArgStates.Options;
                    //   break;
                    //}
                    case ArgStates.Expiration:
                        {
                            int expiration;
                            try
                            {
                                expiration = Convert.ToInt32(arg);
                            }
                            catch //(FormatException)
                            {
                                throw new UsageException("Error: " + arg + " is not a valid expiration option.");
                            }
                            switch (expiration)
                            {
                                case 0:
                                    {
                                        expirationType = X509FindType.FindByTimeNotYetValid;
                                        break;
                                    }
                                case 1:
                                    {
                                        expirationType = X509FindType.FindByTimeValid;
                                        break;
                                    }
                                case 2:
                                    {
                                        expirationType = X509FindType.FindByTimeExpired;
                                        break;
                                    }
                                default:
                                    {
                                        throw new UsageException("Error: " + arg + " is not a valid expiration option.");
                                    }
                            }
                            performTimeValidityCheck = true;
                            argState = ArgStates.Options;
                            break;
                        }
                    //case ArgStates.SaveAs:
                    //    {
                    //        switch (arg.ToUpper())
                    //        {
                    //            case "SST":
                    //                {
                    //                    saveAs = X509ContentType.SerializedStore;
                    //                    break;
                    //                }
                    //            case "PKCS7":
                    //                {
                    //                    saveAs = X509ContentType.Pkcs7;
                    //                    break;
                    //                }
                    //            case "PFX":
                    //                {
                    //                    saveAs = X509ContentType.Pfx;
                    //                    break;
                    //                }
                    //            default:
                    //                {
                    //                    throw new UsageException("Error: " + arg + " is not a valid SaveAs type.");
                    //                }
                    //        }
                    //        argState = ArgStates.Options;
                    //        break;
                    //    }
                    case ArgStates.Verbose:
                        {
                            int level;
                            try
                            {
                                level = Convert.ToInt32(arg);
                            }
                            catch //(FormatException)
                            {
                                throw new UsageException("Error: " + arg + " is not a valid verbose level.");
                            }
                            if (0 > level || 2 < level)
                            {
                                throw new UsageException("Error: " + arg + " is not a valid verbose level.");
                            }
                            verbose = (Verbose)level;
                            argState = ArgStates.Options;
                            break;
                        }
                    case ArgStates.Password:
                        {
                            password = arg;
                            argState = ArgStates.End;
                            break;
                        }
                    case ArgStates.End:
                        {
                            throw new UsageException(string.Empty);
                        }

                    default:
                        {
                            throw new InternalException("Internal error: Unknown argument state (argState = " + argState.ToString() + ").");
                        }
                }
            }

            // Make sure we are in good state.
            if (argState != ArgStates.Options && argState != ArgStates.Password && argState != ArgStates.End)
            {
                throw new UsageException(string.Empty);
            }

            // Make sure all required options are valid.
            // Note: As stated in the help screen, non-fatal invalid options for
            //       the specific command is ignore. You can add the logic here
            //       to further handle these invalid options if desired.
            switch (command)
            {
                case Command.View:
                    {
                        // If -l FILE specified, then we need to have a CertFile.
                        if (loadFromCertFile && null == certFile)
                        {
                            throw new UsageException("Error: CertFile is not specified.");
                        }
                        break;
                    }
                case Command.Import:
                case Command.Export:
                    {
                        // Make sure we do have a certificate file name. 
                        if (null == certFile)
                        {
                            throw new UsageException("Error: CertFile is not specified.");
                        }
                        goto case Command.Activate;
                    }
                case Command.Delete:
                case Command.Archive:
                case Command.Activate:
                    {
                        // -l FILE is not allowed.
                        if (loadFromCertFile)
                        {
                            throw new UsageException("Error: -l FILE is not a valid option.");
                        }
                        break;
                    }

                default:
                    {
                        throw new InternalException("Internal error: Unknown command state (Command = " + command.ToString() + ").");
                    }
            }
        }
        #endregion

        #region DisplayUsage
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Display the usage screen.
        /// </summary>
        static void DisplayUsage(string message)
        {
            if (string.Empty != message)
            {
                Log.Comment(message);
                Log.Comment("");
            }

            switch (command)
            {
                case Command.Unknown:
                    {
                        Log.Comment("Usage: CStore Command [Options] <[Store] | CertFile [Password]>");
                        Log.Comment("");
                        Log.Comment("Command:");
                        Log.Comment("");
                        Log.Comment("  View      -- View certificate(s) of store or file");
                        Log.Comment("  Import    -- Import certificate(s) from file to store");
                        Log.Comment("  Export    -- Export certificate(s) from store to file");
                        Log.Comment("  Delete    -- Delete certificate(s) from store");
                        //Log.Comment("  Archive   -- Archive certificate(s) in store");
                        //Log.Comment("  Activate  -- Activate (de-archive) certificate(s) in store");
                        Log.Comment("");
                        Log.Comment("For help on a specific command, enter \"CStore Command -?\"");
                        break;
                    }
                case Command.View:
                    {
                        Log.Comment("Usage: CStore View [Options] [Store | CertFile [Password]]");
                        Log.Comment("");
                        Log.Comment("The View command is used to view certificate(s) of a certificate store or file.");
                        Log.Comment("You can use the filtering option(s) to narrow down the set of certificate(s) to");
                        Log.Comment("be displayed.");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU, LM, FILE (default to CU). If FILE, then");
                        Log.Comment("                                CertFile, optionally Password, must be provided");
                        Log.Comment("  -a                         -- Include archived certificates");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Display valid certificates only.");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  Store                      -- My, CA, AddressBook, etc. (default to My)");
                        Log.Comment("");
                        Log.Comment("  CertFile                   -- Certificate file - CER, SST, P7B, PFX, etc.");
                        Log.Comment("");
                        Log.Comment("  Password                   -- Password for PFX file");
                        break;
                    }
                case Command.Import:
                    {
                        Log.Comment("Usage: CStore Import [Options] CertFile [Password]");
                        Log.Comment("");
                        Log.Comment("The Import command is used to import certificate(s) from a certificate file");
                        Log.Comment("(.CER, .SST, .P7B, .PFX, etc.) to a store. You can use the filtering option(s)");
                        Log.Comment("to narrow down the set of certificate(s) to be imported.");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU or LM (default to CU)");
                        Log.Comment("  -s           <store>       -- My, CA, AddressBook, etc. (default to My)");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("                                this name");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Import valid certificates only.");
                        Log.Comment("  -e                         -- Mark private key as exportable (PFX only)");
                        Log.Comment("  -p                         -- Mark private key as user protected (PFX only)");
                        Log.Comment("                                Note: The DPAPI dialog will be displayed");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  CertFile                   -- Certificate file to be imported");
                        Log.Comment("");
                        Log.Comment("  Password                   -- Password for PFX file");
                        break;
                    }
                case Command.Export:
                    {
                        Log.Comment("Usage: CStore Export [Options] CertFile [Password]");
                        Log.Comment("");
                        Log.Comment("The Export command is used to export certificate(s) from a certificate store to");
                        Log.Comment("file (.SST, .P7B, or .PFX). You can use the filtering option(s) to narrow down");
                        Log.Comment("the set of certificate(s) to be exported.");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU or LM (default to CU)");
                        Log.Comment("  -s           <store>       -- My, CA, AddressBook, etc. (default to My)");
                        Log.Comment("  -a                         -- Include archived certificates");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("                                this name");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Export valid certificates only.");
                        Log.Comment("  -saveas      <type>        -- SST, PKCS7, or PFX (default to SST)");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  CertFile                   -- File to save the exported certificate(s)");
                        Log.Comment("");
                        Log.Comment("  Password                   -- Password for PFX file");
                        break;
                    }
                case Command.Delete:
                    {
                        Log.Comment("Usage: CStore Delete [Options] [Store]");
                        Log.Comment("");
                        Log.Comment("The Delete command is used to delete certificate(s) from a certificate store.");
                        Log.Comment("You can use the filtering option(s) to narrow down the set of certificate(s) to");
                        Log.Comment("be deleted.");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU or LM (default to CU)");
                        Log.Comment("  -a                         -- Include archived certificates");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("                                this name");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Delete valid certificates only.");
                        Log.Comment("  -delkey                    -- Delete key container if exists");
                        Log.Comment("  -noprompt                  -- Do not prompt (always delete)");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  Store                      -- My, CA, AddressBook, etc. (default to My)");
                        break;
                    }
                case Command.Archive:
                    {
                        Log.Comment("Usage: CStore Archive [Options] [Store]");
                        Log.Comment("");
                        Log.Comment("The Archive command is used to archive certificate(s) in a certificate store.");
                        Log.Comment("You can use the filtering option(s) to narrow down the set of certificate(s) to");
                        Log.Comment("be archived.");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU or LM (default to CU)");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the signing certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("                                this name");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Archive valid certificates only.");
                        Log.Comment("  -noprompt                  -- Do not prompt (always archive)");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  Store                      -- My, CA, AddressBook, etc. (default to My)");
                        break;
                    }
                case Command.Activate:
                    {
                        Log.Comment("Usage: CStore Activate [Options] [Store]");
                        Log.Comment("");
                        Log.Comment("The Activate command is used to activate archived certificate(s) in a");
                        Log.Comment("certificate store. You can use the filtering option(s) to narrow down the set");
                        Log.Comment("of certificate(s) to be activated (de-archived).");
                        Log.Comment("");
                        Log.Comment("Options:");
                        Log.Comment("");
                        Log.Comment("  -l           <location>    -- CU or LM (default to CU)");
                        Log.Comment("  -sha1        <hash>        -- SHA1 hash of the certificate");
                        Log.Comment("  -subject     <name>        ++ Subject name of the certificate must contain");
                        Log.Comment("                                this name");
                        Log.Comment("  -issuer      <name>        ++ Issuer name of the certificate must contain");
                        Log.Comment("  -serial      <number>      ++ Serial number of the certificate");
                        Log.Comment("                                this name");
                        Log.Comment("  -template    <name | oid>  ++ Template name or OID");
                        Log.Comment("  -extension   <name | oid>  ++ Extension name or OID");
                        Log.Comment("  -eku         <name | oid>  ++ EKU name or OID");
                        Log.Comment("  -policy      <name | oid>  ++ Certificate policy name or OID");
                        Log.Comment("  -keyusage    <key usage>   ++ Key usage bit flag or name");
                        Log.Comment("  -time        <time>        -- Time used for validation (default to now)");
                        Log.Comment("  -expiration  <0 | 1 | 2>   -- Time validity, 0 for not yet valid, 1 for");
                        Log.Comment("                                valid, 2 for expired (default to 1)");
                        Log.Comment("  -validonly                 -- Activate valid certificates only");
                        Log.Comment("  -noprompt                  -- Do not prompt (always activate)");
                        Log.Comment("  -v           <level>       -- Verbose level, 0 for normal, 1 for detail,");
                        Log.Comment("                                2 for UI mode (default to level 0)");
                        Log.Comment("  -?                         -- This help screen");
                        Log.Comment("");
                        Log.Comment("  Store                      -- My, CA, AddressBook, etc. (default to My)");
                        break;
                    }
                default:
                    {
                        throw new InternalException("Internal error: Unknown help state (Command = " + command.ToString() + ").");
                    }
            }

            if (Command.Unknown != command)
            {
                Log.Comment("");
                Log.Comment("Note: All non-fatal invalid options for this specific command will be ignored,");
                Log.Comment("      and the ++ symbol indicates option can be listed multiple times.");
                Log.Comment("");
            }
            return;
        }
        #endregion
    }

    #region Exception classes
    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Application defined exception classes.
    /// </summary>
    class UsageException : ApplicationException
    {
        public UsageException(string message)
            : base(message)
        {
        }
    }

    class AbortException : ApplicationException
    {
        public AbortException(string message)
            : base(message)
        {
        }
    }

    class InternalException : ApplicationException
    {
        public InternalException(string message)
            : base(message)
        {
        }
    }
    #endregion
}
