using System;
using System.IO;
using System.Xml;
//using System.Security.Permissions ; 
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections;
using System.Globalization;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

//[assembly:FileIOPermission( SecurityAction.RequestMinimum , Unrestricted =true)]

public class XmlDriver
{
	//XmlDocument		m_xmlDocument;
    XmlReader m_xmlDocument;
	CertificateInfo[]	m_Certificates;
	

	public XmlDriver(byte[] xml)
	{
        MemoryStream ms = new MemoryStream(xml);
        m_xmlDocument = XmlReader.Create(ms); //new XmlDocument();
		//m_xmlDocument.Load(filename);
	}

	//
	//	Read the xml file, fill out and return CertificateInfo objects based on it
	//
	public CertificateInfo[] Certificates
	{
		get{
			if (m_Certificates == null) {
				ArrayList al = new ArrayList();
				while(!m_xmlDocument.EOF)
				{
                    m_xmlDocument.Read();
					CertificateInfo si = new CertificateInfo();
                    si.FileName = m_xmlDocument.GetAttribute("file");
                    si.Encoding = m_xmlDocument.GetAttribute("encoding");

                    m_xmlDocument.ReadEndElement();

                    m_xmlDocument.Read();
					//foreach(XmlNode n in node.ChildNodes)
                    while(m_xmlDocument.Read())
					{
						switch(m_xmlDocument.Name)
						{
							case "PrivateKey": si.PrivateKeyFile = m_xmlDocument.Value; break;
							case "TrustAnchor": si.Anchor = m_xmlDocument.Value; break;
							case "CRL": si.CRL = m_xmlDocument.Value; break;
                            case "Version": si.Version = Int32.Parse(m_xmlDocument.Value); break;
                            case "Subject": si.Subject = m_xmlDocument.Value; break;
                            case "Issuer": si.Issuer = m_xmlDocument.Value; break;
                            case "SerialNumber": si.SerialNumber = m_xmlDocument.Value; break;
                            //case "ValidFrom": si.NotBefore = DateTime.Parse(m_xmlDocument.Value, CultureInfo.InvariantCulture); break;
							//case "ValidTo": si.NotAfter = DateTime.Parse(m_xmlDocument.InnerXml, CultureInfo.InvariantCulture); break;
                            case "Thumbprint": si.Thumbprint = m_xmlDocument.Value; break;
                            case "ThumbprintAlgorithm": si.ThumbprintAlg = m_xmlDocument.Value; break;
							case "PublicKey":
								si.PublicKeyAlg = m_xmlDocument.Attributes["alg"].Value;
                                si.PublicKeyBlob = ToByteArray(m_xmlDocument.Value);
								break;
                            case "SignatureAlgorithm": si.SignatureAlg = m_xmlDocument.Value; break;
                            //case "Extentions":
                            //    ArrayList alExt = new ArrayList();
                            //    foreach(XmlNode extNode in m_xmlDocument.ChildNodes)
                            //    {
                            //        ExtensionInfo ei = new ExtensionInfo(extNode.Name, extNode.InnerXml);
                            //        if (ei.Type== "KeyUsage")
                            //            ei.Data = ei.Data.Replace("-", "");
                            //        alExt.Add(ei);
                            //    }
                            //    si.Extentions = (ExtensionInfo[])alExt.ToArray(typeof(ExtensionInfo));
                            //    break;
                            case "Password": si.Password = m_xmlDocument.Value; break;
                            case "ToStringBaseLine": si.ToStringBaseLine = m_xmlDocument.Value.Trim(); break;
                            case "ToStringVerboseBaseLine": si.ToStringVerboseBaseLine = m_xmlDocument.Value.Trim(); break;
						}
					}
					al.Add(si);
				}
				m_Certificates = (CertificateInfo[])al.ToArray(typeof(CertificateInfo));
			}

			return m_Certificates;
		}
	}


	//
	//	Return a CertificateInfo for a certificate file
	//
	public CertificateInfo FindCertificate(string name)
	{
		foreach(CertificateInfo ci in Certificates)
			if (ci.FileName == name) return ci;
		return null;
	}


    byte ParseChar(char c)
    {
        if ('0' <= c && c <= '9')
        {
            return (byte)(c - '0');
        }
        else if ('a' <= c && c <= 'f')
        {
            return (byte)(c - 'a' + 10);
        }
        else if ('A' <= c && c <= 'F')
        {
            return (byte)(c - 'A' + 10);
        }
        return 0;
    }

    byte ParseByte(string str)
    {
        return (byte)(ParseChar(str[0]) << 4 | ParseChar(str[1]));
    }

	private byte[] ToByteArray(string s)
	{
		string[] tokens = s.Split(new char[]{' ', ','});
		byte[] res = new byte[tokens.Length];
		for(int i=0; i<tokens.Length; i++)
            res[i] = ParseByte(tokens[i]);
		return res;
	}

	public static bool Compare(Byte[] rgb1, Byte[] rgb2) { 
		int 	i;
		if (rgb1.Length != rgb2.Length) return false;
		for (i=0; i<rgb1.Length; i++) {
			if (rgb1[i] != rgb2[i]) return false;
		}
		return true;
	}

}


public class CertificateInfo
{
	public string		FileName;
	public string		Encoding;
	public string		PrivateKeyFile;
	public string		Anchor;
	public string		CRL;
	public int	 		Version = -1;
	public string 		Subject;
	public string 		Issuer;
	public string 		SerialNumber;
	public DateTime	NotBefore;
	public DateTime	NotAfter;
	public string 		Thumbprint;
	public string		ThumbprintAlg;
	public string		PublicKeyAlg;
	public byte[]		PublicKeyBlob;
	public string 		SignatureAlg;
	public string		Password;
	public string		ToStringBaseLine;
	public string		ToStringVerboseBaseLine;
	public ExtensionInfo[]	Extentions;

	//
	//	Returns true is all available data in CertificateInfo matches the corresponding data in X509Certificate2 object
	//
	public bool Matches(X509Certificate2 cert)
	{
		bool bRes = true;
		
		if (PrivateKeyFile != null)
		{
			// TODO: implement
		}

		// get the public key
		PublicKey pk = cert.PublicKey;

		// compare the algorithm if available
		if ( !CompareStrings( pk.Oid.Value , PublicKeyAlg ) )
		{
			Log.Comment("Public key algorithm mismatch (1): " + pk.Oid.Value + " != " + PublicKeyAlg);
			bRes = false;
		}
		if ( !CompareStrings( cert.GetKeyAlgorithm() , PublicKeyAlg) )
		{
			Log.Comment("Public key algorithm mismatch (2): " + cert.GetKeyAlgorithm() + " != " + PublicKeyAlg);
			bRes = false;
		}

		// compare the public key blob
		if (PublicKeyBlob != null)
			if (!XmlDriver.Compare(pk.EncodedKeyValue.RawData, PublicKeyBlob)) 
			{
				Log.Comment("Public key blob mismatch: " + "\n" +
								BitConverter.ToString(pk.EncodedKeyValue.RawData) + " != " +
								BitConverter.ToString(PublicKeyBlob));
				bRes = false;								
			}

		// compare the private key
		// TODO: enhance, compare the actual value
		// The idea for now is that if there is a password that means there is a private key
		if (Password != null)
		{
			if (cert.PrivateKey == null)
			{
				Log.Comment("BAD: PrivateKey is null");
				bRes = false;
			} else
			{
				Log.Comment("Private Key is there - " + cert.PrivateKey);
			}
		}
		

		// compare the version
		if (Version != -1)
			if (cert.Version != Version)
			{
				Log.Comment("Version mismatch: " + cert.Version + " != " + Version);
				bRes = false;
			}

		// compare the subject
		if (Subject != null)
			if ( !CompareStrings( cert.SubjectName.Name , Subject) )
			{
				Log.Comment("Subject mismatch: " + cert.SubjectName.Name + " != " + Subject);
				bRes = false;
			}

		// compare the Issuer
		if (Issuer != null)
			if ( !CompareStrings( cert.IssuerName.Name , Issuer) )
			{
				Log.Comment("Issuer mismatch: " + cert.IssuerName.Name + " != " + Issuer);
				bRes = false;
			}

		// compare the serial number
		if (SerialNumber != null) 
			if ( !CompareStrings( cert.SerialNumber , SerialNumber) )
			{
				Log.Comment("Serial number mismatch: " + cert.SerialNumber + " != " + SerialNumber);
				bRes = false;
			}

		// compare the NotBefore
		if (NotBefore != DateTime.MinValue)
			if (cert.NotBefore.CompareTo(NotBefore) != 0)
			{
				Log.Comment("NotBefore mismatch: " + cert.NotBefore + " != " + NotBefore);
				bRes = false;
			}

		// compare the NotAfter				
		if (NotAfter != DateTime.MinValue)
			if (cert.NotAfter.CompareTo(NotAfter) != 0)
			{
				Log.Comment("NotAfter mismatch: " + cert.NotAfter + " != " + NotAfter);
				bRes = false;
			}

		// compare the signature algorithm
		if (SignatureAlg != null)
			if ( !CompareStrings( cert.SignatureAlgorithm.FriendlyName , SignatureAlg))
			{
				Log.Comment("Signature algorithm mismatch: " + cert.SignatureAlgorithm.FriendlyName + " != " + SignatureAlg);
				bRes = false;
			}

		// compare the thumbprint
		if (Thumbprint != null)
			if ( !CompareStrings( cert.Thumbprint.ToLower() , Thumbprint.ToLower()) )
			{
				Log.Comment("Thumbprint mismatch: " + cert.Thumbprint + " != " + Thumbprint);
				bRes = false;
			}

		// compare the ToString baselines
/*		if (ToStringBaseLine != null)
	//		if ( (cert.ToString().Replace("\0", "").Replace( " " , "" ).Replace("\n", "").Trim()
//				!= ToStringBaseLine.Replace( " " , "" ) )) // || (cert.ToString(false).Trim() != ToStringBaseLine) )
			if( !CompareToStrings( cert.ToString() , ToStringBaseLine ) )
			{
				Log.Comment("ToString() baseline mismatch: " + "\n" + 
								cert.ToString().Replace("\0", "").Replace("\n", "").Trim() + "\n" + 
								" != " + "\n" + 
								ToStringBaseLine);
				bRes = false;
			}*/
			
		if (ToStringVerboseBaseLine != null)
//			if (cert.ToString(true).Replace("\0", "").Replace("\n\n", "\n").Replace("\n", "").Trim()
	//			!= ToStringVerboseBaseLine)
			if( !CompareToStrings( ToStringVerboseBaseLine , cert.ToString(true) ) )
			{
				Log.Comment("ToString(true) baseline mismatch: " + "\n" + 
								cert.ToString(true).Replace("\0", "").Replace("\n", "").Trim() + "\n" + 
								" != " + "\n" + 
								ToStringVerboseBaseLine);
				bRes = false;
			}
				

		// TODO: Thumbprint Alg comparions if we get a property for it

		// compare extensions		
		ArrayList alCertExt = new ArrayList();
		foreach(X509Extension ext in cert.Extensions)
		{
			ExtensionInfo ei = null;
			if (ext is X509KeyUsageExtension)
			{
				ei = new ExtensionInfo("KeyUsage", DumpFlags(typeof(X509KeyUsageFlags), ((X509KeyUsageExtension)ext).KeyUsages));
				ei.Data += "(" + (((byte)((X509KeyUsageExtension)ext).KeyUsages)).ToString("X").ToLower() + ")";
			} else
			if (ext is X509BasicConstraintsExtension)
			{
				X509BasicConstraintsExtension bce = (X509BasicConstraintsExtension)ext;
				string data = bce.CertificateAuthority.ToString() + ", " + 
							bce.HasPathLengthConstraint + ", " +
							bce.PathLengthConstraint;
				ei = new ExtensionInfo("BasicConstraints", data);	
			} else
			if (ext is X509EnhancedKeyUsageExtension)
			{
				OidCollection oids = ((X509EnhancedKeyUsageExtension)ext).EnhancedKeyUsages;
				string data = "";
				foreach(Oid oid in oids)
					data += oid.FriendlyName + " (" + oid.Value +"), ";
				data = data.Substring(0, data.Length-1); //.Replace("-", "");
				ei = new ExtensionInfo("EnhancedKeyUsage", data);
			} else
			if (ext is X509SubjectKeyIdentifierExtension)
			{
				ei = new ExtensionInfo("SubjectKeyIdentifier", ((X509SubjectKeyIdentifierExtension)ext).SubjectKeyIdentifier.ToLower());
			} else
			{
				ei = new ExtensionInfo(ext.Oid.FriendlyName, ext.Format(false));
			}
			alCertExt.Add(ei);
		}

		if (false == CompareKeyExtensions(Extentions, (ExtensionInfo[])alCertExt.ToArray(typeof(ExtensionInfo))))
		{
			Log.Comment("Key extensions mismatch.");
			bRes = false;
		}
		
		return bRes;		
	}

	public override string ToString()
	{
		return "Version: " + Version +
				", Subject: " + Subject + 
				", Issuer: " + Issuer + 
				", SerialNumber: " + SerialNumber+
				", NotBefore: " + NotBefore +
				", NotAfter: " + NotAfter +
				", Thumbprint: " + Thumbprint +
				", PublicKeyAlg: " + PublicKeyAlg +
				", PubliKeyBlob: " + BitConverter.ToString(PublicKeyBlob) +
				", SignatureAlg: " + SignatureAlg;
	}

	public string ToString(bool verbose)
	{
		if (!verbose) return ToString();
		return "FileName: " + FileName +
				", Encoding: " + Encoding +
				", PrivateKeyFile: " + PrivateKeyFile +
				", Anchor: " + Anchor +
				", CRL: " + CRL + ", " + ToString();
	}

	public bool CompareKeyExtensions(ExtensionInfo[] aei1, ExtensionInfo[] aei2)
	{
		bool bRes = true;

		if (aei1.Length != aei2.Length)
		{
			Log.Comment("Key extensions lenghts are different");
			return false;
		}

		for(int i = 0; i<aei1.Length; i++)
		{
			bool bSubRes = false;
			for(int j = 0; j<aei2.Length; j++)	// we will remove spaces for now
				if (	CompareStrings( aei1[i].Type.Replace(" ", "").Replace("-","").Trim() , aei2[j].Type.Replace(" ", "").Replace("-","").Replace("\0","").Trim() ) &&
					CompareStrings( aei1[i].Data.Replace(" ", "").Replace("-","").Trim() , aei2[j].Data.Replace(" ", "").Replace("-","").Replace("\0","").Trim() ) )
					{
					bSubRes = true;		// found a match
					}
			bRes = bSubRes && bRes;
			if (!bSubRes) {
				Log.Comment("Could not find a match for extension:\n" + aei1[i].Type.Replace(" ", "").Trim() + " : " + aei1[i].Data.Replace(" ", "").Trim());
				Log.Comment("It didn't match any of:");
				foreach(ExtensionInfo xi in aei2)
					Log.Comment(xi.Type.Replace(" ", "") + " : " + xi.Data.Replace(" ", ""));
			}
				
		}

		if (bRes == true)
			Log.Comment("Key extensions compared EQUAL");

		return bRes;	
	}

	public string DumpFlags(Type t, object o)
	{
		string s = "";
		int val = (int)o;
		foreach(int i in Enum.GetValues(t))
		{
			if ((val & i) != 0)
			s = Enum.GetName(t, i) + ", " + s;
		}
		if (s.Length > 1) s = s.Substring(0, s.Length-2);
		return s;
	}

	public bool CompareToStrings( string s1 , string s2 )
		{
		//first let's remove the date
		string s1NotBefore , s1NotAfter , s2NotBefore , s2NotAfter ; 
		bool rv = false ; 
		string notBefore = "[Not Before]"  , notAfter = "[Not After]" , thumbPrint = "[Thumbprint]" ; 
		
		s1NotBefore = s1.Substring( s1.IndexOf( notBefore ) + notBefore.Length , s1.IndexOf( notAfter ) - s1.IndexOf( notBefore ) - notBefore.Length ) ; 
		s2NotBefore = s2.Substring( s2.IndexOf( notBefore ) + notBefore.Length , s2.IndexOf( notAfter ) - s2.IndexOf( notBefore ) - notBefore.Length ) ; 
		s1NotAfter = s1.Substring( s1.IndexOf( notAfter ) + notAfter.Length , s1.IndexOf( thumbPrint ) - s1.IndexOf( notAfter ) - notAfter.Length ) ; 
		s2NotAfter = s2.Substring( s2.IndexOf( notAfter ) + notAfter.Length , s2.IndexOf( thumbPrint ) - s2.IndexOf( notAfter ) - notAfter.Length ) ; 

		s1 = s1.Replace( s1NotBefore , "" ).Replace( s1NotAfter , "" ) .Replace( " " , "" ).ToLower() ; 
		s2 = s2.Replace( s2NotBefore , "" ).Replace( s2NotAfter , "" ).Replace( " " , "" ).Replace( "\0" , "" ).Replace("\n", "").Trim().ToLower()  ; 

		rv = String.Compare( s1, s2 ) == 0 ; 
		Log.Comment( s1 ) ; 
		Log.Comment( s2 ) ; 
	/*	Actual values are compare
	DateTime dt1 = Convert.ToDateTime( s1NotBefore ) ; 
		DateTime dt2 = Convert.ToDateTime( s2NotBefore ) ; 
		DateTime na1 = Convert.ToDateTime( s1NotAfter ) ;
		DateTime na2 = Convert.ToDateTime( s2NotAfter ) ;		
		PrintOutDate( dt1 ) ; 
		PrintOutDate( dt2 ) ; 
		PrintOutDate( na1 ) ; 
		PrintOutDate( na2 ) ; 
		rv = rv && dt1 == dt2 ; 
		rv = rv && na1 == na2 ; */
	
		
		return rv ; 
		}
	void PrintOutDate( DateTime d )
		{
		Log.Comment( d.Day.ToString() ) ; 
		Log.Comment( " , " ) ; 
		Log.Comment( d.Month.ToString() ) ; 
		Log.Comment( " , " ) ; 
		Log.Comment( d.Year.ToString() ) ; 
		Log.Comment( " , " ) ; 
		Log.Comment( d.Hour.ToString() ) ; 
		Log.Comment( " , " ) ; 
		Log.Comment( d.Minute.ToString() ) ; 
		Log.Comment( " , " ) ; 
		Log.Comment( d.Second.ToString() ) ; 
		Log.Comment( "" ) ;
		}
	public bool CompareStrings( string s1 , string s2 )
		{
		//Remove spaces and set ToLower
		string s1Result = s1.Replace( " " , "" ).ToLower() ; 
		string s2Result = s2.Replace( " " , "" ).ToLower() ; 
		return s1Result == s2Result ; 
		}
	
}

public class ExtensionInfo
{
	public string	Type;
	public string	Data;

	public ExtensionInfo(string type, string data)
	{
		Type = type;
		Data = data;
	}
}
