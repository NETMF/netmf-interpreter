namespace System.Security.Cryptography.X509Certificates 
{
    using System;
    using System.Collections;

    /// <summary>
    /// Specifies the type of value the X509Certificate2Collection.Find method searches for.
    /// </summary>
    public enum X509FindType
    {
        FindByThumbprint = 0,
        FindBySubjectName = 1,
        FindBySubjectDistinguishedName = 2,
        FindByIssuerName = 3,
        FindByIssuerDistinguishedName = 4,
        FindBySerialNumber = 5,
        FindByTimeValid = 6,
        FindByTimeNotYetValid = 7,
        FindByTimeExpired = 8,
        FindByTemplateName = 9,
        FindByApplicationPolicy = 10,
        FindByCertificatePolicy = 11,
        FindByExtension = 12,
        FindByKeyUsage = 13,
        FindBySubjectKeyIdentifier = 14
    }

    /// <summary>
    /// Represents a collection of X509Certificate2 objects.
    /// </summary>
    [Serializable()]
    public class X509Certificate2Collection : ArrayList 
    {
        /// <summary>
        /// Initializes a new instance of the X509Certificate2Collection class without any X509Certificate2 information.
        /// </summary>
        public X509Certificate2Collection() 
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the X509Certificate2Collection class using the specified certificate collection.
        /// </summary>
        /// <param name="value"></param>
        public X509Certificate2Collection(X509Certificate2Collection value) 
        {
            this.AddRange(value);
        }
        
        /// <summary>
        /// Initializes a new instance of the X509Certificate2Collection class using an array of X509Certificate2 objects.
        /// </summary>
        /// <param name="value"></param>
        public X509Certificate2Collection(X509Certificate2[] value) 
        {
            this.AddRange(value);
        }
        
        /// <summary>
        /// Gets or sets the X509Certificate2 at the given index.
        /// </summary>
        /// <param name="index">The numeric index into the array of X509Certificate2 objects</param>
        /// <returns>The X509Certificate2 object at the given index.</returns>
        public new X509Certificate2 this[int index] 
        {
            get 
            {
                return ((X509Certificate2)(base[index]));
            }
            set 
            {
                base[index] = value;
            }
        }
        
        /// <summary>
        /// Adds an object to the end of the X509Certificate2Collection.
        /// </summary>
        /// <param name="value">An X.509 certificate represented as an X509Certificate2 object. </param>
        /// <returns>The X509Certificate2Collection index at which the certificate has been added.</returns>
        public int Add(X509Certificate2 value) 
        {
            return base.Add(value);
        }
        
        /// <summary>
        /// Adds multiple X509Certificate2 objects in an array to the X509Certificate2Collection object.
        /// </summary>
        /// <param name="value">An array of X509Certificate2 objects.</param>
        public void AddRange(X509Certificate2[] value) 
        {
            for (int i = 0; (i < value.Length); i = (i + 1)) 
            {
                base.Add(value[i]);
            }
        }
        
        /// <summary>
        /// Adds multiple X509Certificate2 objects in an X509Certificate2Collection object to another X509Certificate2Collection object.
        /// </summary>
        /// <param name="value">An X509Certificate2Collection object.</param>
        public void AddRange(X509Certificate2Collection value) 
        {
            for (int i = 0; (i < value.Count); i = (i + 1)) 
            {
                base.Add(value[i]);
            }
        }
        
        /// <summary>
        /// Determines whether the X509Certificate2Collection object contains a specific certificate.
        /// </summary>
        /// <param name="value">The X509Certificate2 object to locate in the collection.</param>
        /// <returns>true if the X509Certificate2Collection contains the specified certificate; otherwise, false.</returns>
        public bool Contains(X509Certificate2 value) 
        {
            return base.Contains(value);
        }
        
        /// <summary>
        /// Copies the X509Certificate values in the current X509CertificateCollection to a one-dimensional Array instance at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the values copied from X509CertificateCollection.</param>
        /// <param name="index">The index into array to begin copying.</param>
        public void CopyTo(X509Certificate2[] array, int index) 
        {
            base.CopyTo(array, index);
        }
        
        /// <summary>
        /// Returns the index of the specified X509Certificate in the current X509CertificateCollection.
        /// </summary>
        /// <param name="value">The X509Certificate to locate.</param>
        /// <returns>The index of the X509Certificate specified by the value parameter in the X509CertificateCollection, if found; otherwise, -1.</returns>
        public int IndexOf(X509Certificate2 value) 
        {
            return base.IndexOf(value);
        }
        
        /// <summary>
        /// Inserts an object into the X509Certificate2Collection object at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert certificate.</param>
        /// <param name="value">The X509Certificate2 object to insert.</param>
        public void Insert(int index, X509Certificate2 value) 
        {
            base.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a certificate from the X509Certificate2Collection object.
        /// </summary>
        /// <param name="value">The X509Certificate2 object to be removed from the X509Certificate2Collection object.</param>
        public void Remove(X509Certificate2 value) 
        {
            base.Remove(value);
        }

        /// <summary>
        /// Builds a hash value based on all values contained in the current X509CertificateCollection.
        /// </summary>
        /// <returns>A hash value based on all values contained in the current X509CertificateCollection.</returns>
        public override int GetHashCode() 
        {
            int hashCode = 0;

            foreach (X509Certificate2 cert in this) 
            {                
                hashCode += cert.GetHashCode();  
            }

            return hashCode;
        }

        /// <summary>
        /// Searches an X509Certificate2Collection object using the search criteria specified by the X509FindType enumeration and the findValue object.
        /// </summary>
        /// <param name="findType">One of the X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        /// <param name="validOnly">true to allow only valid certificates to be returned from the search; otherwise, false.</param>
        /// <returns>An X509Certificate2Collection object.</returns>
        public X509Certificate2Collection Find(
            X509FindType findType,
            Object findValue,
            bool validOnly
        )
        {
            throw new NotImplementedException();
        }
    }
}
