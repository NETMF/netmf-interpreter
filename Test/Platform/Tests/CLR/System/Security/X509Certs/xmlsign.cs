/******************************************************************************
'
' THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
' EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
' WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
'
' Copyright (C) 1999-2003.  Microsoft Corporation.  All rights reserved.
'
'******************************************************************************
'
' XmlSign.cs
'
' This sample illustrates how to use the X509Certificate2 class to 
' sign/verify an XML document.
'
'*****************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace XmlSign {
    class XmlSign {
        enum IncludeOptions {
            ExcludeRoot  = 0,
            EndCertOnly  = 1,
            WholeChain   = 2,
            SubjectName  = 3,
            SKI          = 4,
            IssuerSerial = 5
        }

        enum Command {
            Unknown  = 0,
            Sign     = 1,
            Verify   = 2
        }

        static Command              command                = Command.Unknown;
        static StoreLocation        storeLocation          = StoreLocation.CurrentUser;
        static bool                 storeLocationSpecified = false;
        static string               pfxFile                = null;
        static string               password               = null;
        static string               sha1                   = null;
        static ArrayList            subjects               = new ArrayList();
        static ArrayList            issuers                = new ArrayList();
        static bool                 verbose                = false;
        static ArrayList            includeOptions         = new ArrayList();
        static ArrayList            fileNames              = new ArrayList();
        static bool                 detached               = false;
        static bool Result = false ; 

        [STAThread]
        static int Main(string[] args) {
            try {
                string title = string.Empty;

                // Parse the command line.
                ParseCommandLine(args);

                X509Certificate2Collection certificates = new X509Certificate2Collection();
                switch (command)
                {
                    case Command.Sign:
                        // For view, source can come from either store or cert file.
                        if (null == pfxFile) {
                            // Open the source (store and/or pfx file).
                            Console.WriteLine( storeLocation ) ; 
                            X509Store store = new X509Store("MY", storeLocation);

                            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                            certificates = store.Certificates;
                            title = "Viewing " + storeLocation.ToString() + " " + "MY" + " store";
                        } 
                        else {
                            certificates.Import(pfxFile, password, X509KeyStorageFlags.DefaultKeySet);
                            title = "Viewing " + pfxFile;
                        }
                        break;
                    case Command.Verify:
                        break;
                    default:
                        break;
                }

                // Carry out the command.
                switch (command)
                {
                    case Command.Sign:
                        // Perform filter(s) requested.
                        X509Certificate2 certificate = FilterCertificates(certificates);
                        DoSignCommand(title, certificate);
                        break;
                    case Command.Verify:
                        DoVerifyCommand(title);
                        break;
                }
            }
            catch (UsageException) {
                DisplayUsage();
            }
            catch (InternalException e) {
                Console.WriteLine(e.Message);
            }
            catch( Exception e )
            	{
            	Console.WriteLine(e);
            	}

            return Result ? 100 : 101 ;
        }

        #region DoSignCommand
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Carry out the Sign command.
        /// </summary>
        ///
        static void DoSignCommand(string title, X509Certificate2 certificate) {
            Console.WriteLine();
            Console.WriteLine("Signing Xml file \"" + fileNames[0] + "\"...");
            Console.WriteLine();

            // display more details for verbose operation.
            if (verbose) {
                DisplayDetail(null, certificate, detached);
            }

            SignedXml signedXml = new SignedXml();
            ICspAsymmetricAlgorithm csp = (ICspAsymmetricAlgorithm) certificate.PrivateKey;
            if (csp.CspKeyContainerInfo.RandomlyGenerated)
                throw new InternalException("Internal error: This certificate does not have a corresponding private key.");
            signedXml.SigningKey = (AsymmetricAlgorithm) csp;
            Console.WriteLine(signedXml.SigningKey.ToXmlString(false));

            if (detached) {
                Reference reference = new Reference();
                reference.Uri = "file://" + Path.GetFullPath((string) fileNames[0]);
                signedXml.AddReference(reference);
            } else {
                Reference reference = new Reference();
                reference.Uri = "#object-1";

		        // Add an object
		        XmlDocument dataObject = new XmlDocument();
                dataObject.PreserveWhitespace = true;
                XmlElement dataElement = (XmlElement) dataObject.CreateElement("DataObject", SignedXml.XmlDsigNamespaceUrl);
                dataElement.AppendChild(dataObject.CreateTextNode(new UTF8Encoding(false).GetString(ReadFile((string) fileNames[0]))));
                dataObject.AppendChild(dataElement);
		        DataObject obj = new DataObject();
		        obj.Data = dataObject.ChildNodes;
		        obj.Id = "object-1";
		        signedXml.AddObject(obj);
                signedXml.AddReference(reference);
            }

            signedXml.KeyInfo = new KeyInfo();
            if (includeOptions.Count == 0) {
                signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificate, X509IncludeOption.ExcludeRoot));
            } else {
                KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();
                foreach (IncludeOptions includeOption in includeOptions) {
                    switch (includeOption) {
                    case IncludeOptions.ExcludeRoot:
                    case IncludeOptions.EndCertOnly:
                    case IncludeOptions.WholeChain:
                        keyInfoX509Data = new KeyInfoX509Data(certificate, (X509IncludeOption) includeOption);
                        break;
                    case IncludeOptions.SubjectName:
                        keyInfoX509Data.AddSubjectName(certificate.SubjectName.Name);
                        break;
                    case IncludeOptions.SKI:
                        X509ExtensionCollection extensions = certificate.Extensions;
                        foreach (X509Extension extension in extensions) {
                            if (extension.Oid.Value == "2.5.29.14") { // OID for SKI extension
                                X509SubjectKeyIdentifierExtension ski = extension as X509SubjectKeyIdentifierExtension;
                                if (ski != null) {
                                    keyInfoX509Data.AddSubjectKeyId(ski.SubjectKeyIdentifier);
                                    break;
                                }
                            }
                        }
                        break;
                    case IncludeOptions.IssuerSerial:
                        keyInfoX509Data.AddIssuerSerial(certificate.IssuerName.Name, certificate.SerialNumber);
                        break;
                    }

                    signedXml.KeyInfo.AddClause(keyInfoX509Data);
                }
            }

            // compute the signature
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // write it out
            XmlTextWriter xmltw = new XmlTextWriter((string) fileNames[1], new UTF8Encoding(false));
            xmlDigitalSignature.WriteTo(xmltw);
            xmltw.Close();

            Console.WriteLine();
            Console.WriteLine("Signature written to file \"" + fileNames[1] + "\".");
            Console.WriteLine();

            return;
        }
        #endregion

        #region DoVerifyCommand
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Carry out the Verify command.
        /// </summary>
        ///
        static void DoVerifyCommand(string title) {
            Console.WriteLine("Verifying signed Xml file \"" + (string) fileNames[0] + "\", please wait...");
            Console.WriteLine();

            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.Load((string) fileNames[0]);

            SignedXml signedXml = new SignedXml(document);
            XmlNamespaceManager nsm = new XmlNamespaceManager(document.NameTable);
            nsm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlElement signatureNode = document.SelectSingleNode("//ds:Signature", nsm) as XmlElement;
            if (signatureNode == null)
                throw new InternalException("Internal error: Missing signature.");

            signedXml.LoadXml(signatureNode);

            if (signedXml.CheckSignature())
            	{
                Console.WriteLine("=== Valid signature. ===");
                Result = true ; 
            	}
            else
                Console.WriteLine("=== Invalid signature. ===");
        }
        #endregion

        #region DisplayDetail
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Display detail information.
        /// </summary>
        ///
        static void DisplayDetail(string title, X509Certificate2 certificate, bool detached) {
            if (title != null) {
                Console.WriteLine(title);
            }

            Console.WriteLine("Performing " + (detached ? "detached" : "non-detached") + " signature using the following certificate...");
            Console.WriteLine(certificate.ToString(true));
        }
        #endregion

        #region ReadFile
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Reads a file into a byte array.
        /// </summary>
        ///
        private static byte[] ReadFile (string fileName) {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int size = (int) f.Length;
            byte[] data = new byte[size];
            size = f.Read(data, 0, size);
            f.Close();
            return data;
        }
        #endregion

        #region FilterCertificates
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Carry out the filters requested.
        /// </summary>
        ///
        static X509Certificate2 FilterCertificates(X509Certificate2Collection certificates) {
            int index;
            if (0 < certificates.Count && null != sha1) {
                certificates = certificates.Find(X509FindType.FindByThumbprint, sha1, false);
            }

            if (0 < certificates.Count && 0 < subjects.Count) {
                foreach (string subject in subjects) {
                    certificates = certificates.Find(X509FindType.FindBySubjectDistinguishedName, subject, false);
                }
            }

            if (0 < certificates.Count && 0 < issuers.Count) {
                foreach (string issuer in issuers) {
                    certificates = certificates.Find(X509FindType.FindByIssuerDistinguishedName, issuer, false);
                }
            }

            // filter out certificates without a private key.
            if (0 < certificates.Count) {
                X509Certificate2Collection collection = new X509Certificate2Collection();
                for (index = 0; index < certificates.Count; index++) {
                	try
                		{
                    if (certificates[index].PrivateKey != null)
                        collection.Add(certificates[index]);
                		}
                	catch{}
                }
                certificates.Clear();
                certificates = collection;
            }

            // finally, ask the user to select a certificate if more than one is found.
            if (1 < certificates.Count) {
                certificates = X509Certificate2UI.SelectFromCollection( certificates , "Certificates", "Please select a certificate", X509SelectionFlag.SingleSelection);
            }

            if (certificates.Count != 1)
                throw new InternalException("Internal error: No valid certificates were found to sign the document.");
                

            return (certificates.Count == 0 ? null : certificates[0]);
        }
        #endregion

        #region ParseCommandLine
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Parse the command line, and set the options accordingly. Throw
        /// UsageException for fatal argument error or help request. Also will
        /// throw InternalException for fatal internal error.
        /// </summary>
        ///

        enum ArgStates {
            Command      = 0,
            Options      = 1,
            Location     = 2,
            SHA1         = 3,
            Subject      = 4,
            Issuer       = 5,
            PFX          = 6,
            Password     = 7,
         // Name         = 8,	Not used
            Include      = 9,
            FileName     = 10
        }
  
        static void ParseCommandLine (string[] args) {
            ArgStates argState = ArgStates.Command;
            for (int index = 0; index < args.Length; index++) {
                string arg = args[index];
                switch (argState)
                {
                    case ArgStates.Command:
                        switch (arg.ToUpperInvariant()) {
                            case "SIGN":
                                command = Command.Sign;
                                break;
                            case "VERIFY":
                                command = Command.Verify;
                                break;
                            default:
                                throw new UsageException();
                        }

                        argState = ArgStates.Options;
                        break;

                    case ArgStates.Options:
                        if (arg.Substring(0, 1) == "-" || arg.Substring(0, 1) == "/")
                        {
                            switch (arg.Substring(1).ToUpperInvariant())
                            {
                                case "L":
                                    storeLocationSpecified = true;
                                    argState = ArgStates.Location;
                                    break;
                                case "SHA1":
                                    argState = ArgStates.SHA1;
                                    break;
                                case "PWD":
                                    argState = ArgStates.Password;
                                    break;
                                case "SUBJECT":
                                    argState = ArgStates.Subject;
                                    break;
                                case "ISSUER":
                                    argState = ArgStates.Issuer;
                                    break;
                                case "PFX":
                                    argState = ArgStates.PFX;
                                    break;
                                case "INCLUDE":
                                    argState = ArgStates.Include;
                                    break;
                                case "V":
                                    verbose = true;
                                    break;
                                case "DETACHED":
                                    detached = true;
                                    break;
                                case "?":
                                    goto default;
                                default:
                                    throw new UsageException();
                            } 
                        }
                        else {
                            fileNames.Clear();
                            fileNames.Add(arg);
                            argState = ArgStates.FileName;
                        }
                        break;

                    case ArgStates.Location:
                        switch (arg.ToUpperInvariant()) {
                            case "CU":
                                storeLocation = StoreLocation.CurrentUser;
                                break;
                            case "LM":
                                storeLocation = StoreLocation.LocalMachine;
                                break;
                            default:
                                throw new UsageException();
                        }
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.Password:
                        password = arg;
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.SHA1:
                        sha1 = arg;
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.Subject:
                        subjects.Add(arg);
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.Issuer:
                        issuers.Add(arg);
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.PFX:
                        pfxFile = arg;
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.Include:
                        try {
                            includeOptions.Add((IncludeOptions) Convert.ToInt32(arg));
                        }
                        catch (FormatException) {
                            throw new UsageException();
                        }
                        argState = ArgStates.Options;
                        break;

                    case ArgStates.FileName:
                        if (arg.Substring(0, 1) == "-" || arg.Substring(0, 1) == "/")
                            throw new UsageException();
                        fileNames.Add(arg);
                        break;

                    default:
                        throw new InternalException("Internal error: Unknown argument state (argState = " + argState.ToString() + ").");
                }
            }

            // Make sure we are in good state.
            if (argState != ArgStates.FileName)
                throw new UsageException();

            // Make sure all required options are valid.
            // Note: As stated in the help screen, non-fatal invalid options for
            //       the specific command is ignore. You can add the logic here
            //       to further handle these invalid options if desired.
            switch (command)
            {
                case Command.Sign:
                   // -l and -pfxFile are exclusive. 
                   if (null != pfxFile) {
                      if (storeLocationSpecified)
                         throw new UsageException();
                   }
                   break;

                case Command.Verify:
                   break;

                default:
                   throw new InternalException("Internal error: Unknown command state (Command = " + command.ToString() + ").");
            }
        }
        #endregion

        #region DisplayUsage
        ///////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Display the usage screen.
        /// </summary>
        ///
        static void DisplayUsage () {
             switch (command)
             {
                case Command.Unknown:
                    Console.WriteLine("Usage: XmlSign Command [Options] File1 [File2]");
                    Console.WriteLine();
                    Console.WriteLine("Command:");
                    Console.WriteLine();
                    Console.WriteLine("  Sign                   -- Sign an Xml file");
                    Console.WriteLine("  Verify                 -- Verify a signed Xml file");
                    Console.WriteLine();
                    Console.WriteLine("For help on a specific command, enter \"XmlSign Command -?\"");
                    break;

                case Command.Sign:
                    Console.WriteLine("Usage: XmlSign Sign [Options] InputFile SignedFile");
                    Console.WriteLine();
                    Console.WriteLine("The Sign command is used to sign an Xml file. Signing protects a file from");
                    Console.WriteLine("tampering, and allows users to verify the signer based on signing certificate.");
                    Console.WriteLine();
                    Console.WriteLine("For non-detached signing, both the content and signature will be saved to");
                    Console.WriteLine("SignedFile. For detached signing, only the signature is saved to SignedFile.");
                    Console.WriteLine();
                    Console.WriteLine("Options:");
                    Console.WriteLine();
                    Console.WriteLine("  -l       <location>    -- CU or LM (default to CU)");
                    Console.WriteLine("  -pfx     <filename>    -- Use certificate in the PFX (exclusive with -l)");
                    Console.WriteLine("  -pwd     <password>    -- Password for the PFX file (require -pfx).");
                    Console.WriteLine("  -sha1    <hash>        -- SHA1 hash of the signing certificate");
                    Console.WriteLine("  -subject <name>        ** Subject Name of the signing certificate must");
                    Console.WriteLine("                            contain this name");
                    Console.WriteLine("  -issuer  <name>        ** Issuer Name of the signing certificate must");
                    Console.WriteLine("                            contain this name");
                    Console.WriteLine("  -include <0 | 1 | 2 | 3 | 4 | 5>");
                    Console.WriteLine("                         ** Include option: 0 for chain minus root, 1 for end");
                    Console.WriteLine("                            certificate only, 2 for whole chain, 3 for subject");
                    Console.WriteLine("                            name, 4 for SKI or 5 for Issuer Serial (default to 0)");
                    Console.WriteLine("  -detached              -- Detached signing");
                    Console.WriteLine("  -v                     -- Verbose operation");
                    Console.WriteLine("  -?                     -- This help screen");
                    Console.WriteLine();
                    Console.WriteLine("  InputFile              -- Xml file to be signed");
                    Console.WriteLine();
                    Console.WriteLine("  SignedFile             -- Signed file (contains signature only if detached)");
                    Console.WriteLine();
                    Console.WriteLine("Note: All non-fatal invalid options for this specific command will be ignored,");
                    Console.WriteLine("      and the ** symbol indicates option can be listed multiple times.");
                    Console.WriteLine();
                    Console.WriteLine("      If there is only one certificate found in the MY store or PFX that");
                    Console.WriteLine("      matches the requirement, that particular certificate will be used.");
                    Console.WriteLine("      However, if there is more than one certificate matching the requirement,");
                    Console.WriteLine("      a dialog will be displayed to allow selection of the signing certificate.");
                    Console.WriteLine();
                    break;

                case Command.Verify:
                    Console.WriteLine("Usage: XmlSign Verify [Options] SignedFile OutputFile");
                    Console.WriteLine();
                    Console.WriteLine("The Verify command is used to verify signed Xml files. Verification checks");
                    Console.WriteLine("integrity of the signed file and determines if the signing certificate is");
                    Console.WriteLine("valid and issued by a trusted party.");
                    Console.WriteLine(); 
                    Console.WriteLine("For non-detached signed file, the content will be extracted and saved to");
                    Console.WriteLine("ContentFile. For detached signed file, the ContentFile is not modified.");
                    Console.WriteLine();
                    Console.WriteLine("Options:");
                    Console.WriteLine();
                    Console.WriteLine("  -v                     -- Verbose operation");
                    Console.WriteLine("  -?                     -- This help screen");
                    Console.WriteLine();
                    Console.WriteLine("  SignedFile             -- Signed file (contains signature only if detached)");
                    Console.WriteLine();
                    Console.WriteLine("Note: All non-fatal invalid options for this specific command will be ignored.");
                    Console.WriteLine();
                    break;

                default:
                    throw new InternalException("Internal error: Unknown help state (Command = " + command.ToString() + ").");
            }

            if (Command.Unknown != command) {
                Console.WriteLine();
                Console.WriteLine("Note: All non-fatal invalid options for this specific command will be ignored,");
                Console.WriteLine("      and the ++ symbol indicates option can be listed multiple times.");
                Console.WriteLine();
            }
            return;
        }
        #endregion

        class UsageException : ApplicationException {
            public UsageException () : base() {
            }
        }

        class InternalException : ApplicationException {
            public InternalException (string message) : base(message) {
            }
        }
    }
}
