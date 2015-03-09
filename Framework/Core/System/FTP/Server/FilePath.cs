using System.Collections;
using System.IO;
using System;

namespace Microsoft.SPOT.Net.Ftp
{
    /// <summary>
    /// Store the path information and provide functions to check paths 
    /// and transform them between network path (using '/' as divider) 
    /// and file system path (using '\' as divider)
    /// </summary>
    internal class FilePath
    {
        // Fields
        private ArrayList m_Names = null;               // store a list of file or directory names
        private bool m_IsAbsolutePath = true;           // the path starts with a divider, i.e. "/abso_path"
        private bool m_IsDirectory = false;             // the path ends with a divider, i.e. "directory/"

        // Methods
        private FilePath()
        {
            m_Names = new ArrayList();
        }

        /// <summary>
        /// Construct from a network path
        /// </summary>
        /// <param name="path"></param>
        public FilePath(string path)
            : this()
        {
            ConstructNewPath(path);
        }

        /// <summary>
        /// Combine the current path with another input path of type string
        /// </summary>
        /// <param name="path">the path to be added into</param>
        /// <returns>resulting file path</returns>
        public FilePath Combine(string path)
        {
            FilePath result = null;
            if (path == null || path == "")
            {
                result = new FilePath(this.GetNetPath());
            }
            else if (path.ToCharArray()[0] == '/')    // absolute path
            {
                result = new FilePath(path);
            }
            else
            {
                result = new FilePath(this.GetNetPath());
                result.AddPath(path);
            }
            return result;
        }

        /// <summary>
        /// Replace the path info with the input value
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void ConstructNewPath(string path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Input path is a null reference.");
            }
            else if (path == "")
            {
                return;
            }
            string[] array = path.Split(new char[] { '/' });
            int length = array.Length;
            if (m_Names == null)
            {
                m_Names = new ArrayList();
            }
            else if (m_Names.Count > 0)
            {
                m_Names.Clear();
            }
            if (array[0] == "")
            {
                m_IsAbsolutePath = true;
            }
            else
            {
                m_IsAbsolutePath = false;
                m_Names.Add(array[0]);
            }
            for (int i = 1; i < length - 1; i++)
            {
                if (array[i] == ".")
                {
                    continue;
                }
                else if (array[i] == "..")
                {
                    if (m_Names.Count == 0)
                    {
                        throw new IOException("Invalid path.");
                    }
                    else
                    {
                        m_Names.RemoveAt(m_Names.Count - 1);
                    }
                }
                else
                {
                    m_Names.Add(array[i]);
                }
            }
            if (array[length - 1] == "")
            {
                m_IsDirectory = true;
            }
            else
            {
                m_Names.Add(array[length - 1]);
            }
            return;
        }

        /// <summary>
        /// Generate a file system path from the class
        /// </summary>
        /// <returns></returns>
        public string GetDirectory()
        {
            if (m_Names == null)
            {
                throw new NullReferenceException("Internal array is null.");
            }
            string result = "";
            if (m_IsAbsolutePath)
            {
                result += "\\";
            }
            for (int i = 0; i < m_Names.Count - 1; i++)
            {
                result += m_Names[i] as string + "\\";
            }
            if (m_Names.Count > 0)
            {
                result += m_Names[m_Names.Count - 1];
                if (m_IsDirectory)
                {
                    result += "\\";
                }
            }
            return result;
        }

        /// <summary>
        /// Generate a network path from the class
        /// </summary>
        /// <returns></returns>
        public string GetNetPath()
        {
            if (m_Names == null)
            {
                throw new NullReferenceException("Internal array is null.");
            }
            string result = "";
            if (m_IsAbsolutePath)
            {
                result += "/";
            }
            for (int i = 0; i < m_Names.Count - 1; i++)
            {
                result += m_Names[i] as string + "/";
            }
            if (m_Names.Count > 0)
            {
                result += m_Names[m_Names.Count - 1];
                if (m_IsDirectory)
                {
                    result += "/";
                }
            }
            return result;
        }

        /// <summary>
        /// Return to the parent directory
        /// </summary>
        /// <returns>current length of the path</returns>
        public int ReturnToParent()
        {
            if (m_Names == null)
            {
                throw new NullReferenceException("Internal array is null.");
            }
            else if (m_Names.Count == 0 || !m_IsDirectory)
            {
                throw new IOException("Invalid path.");
            }
            else
            {
                m_Names.RemoveAt(m_Names.Count - 1);
                return m_Names.Count;
            }
        }

        /// <summary>
        /// Add a relative path to the current path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool AddRelativePath(string path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Input path is a null reference.");
            }
            else if (m_Names == null)
            {
                throw new NullReferenceException("Internal array is null.");
            }
            int lastIdx;
            string[] array = path.Trim().Split(new char[] { '/' });
            int length = array.Length;
            if (length > 0 && array[0] == "")   // the path is a absolute path
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {      
                if (array[i] == ".")            // ignore current directory
                {
                    continue;
                }
                else if (array[i] == "")        // ignore redundent "/"
                {
                    continue;
                }
                else if (array[i] == "..")      // return to the father directory
                {
                    lastIdx = m_Names.Count - 1;
                    if (lastIdx < 1)            // nothing to remove
                    {
                        throw new IOException("Invalid path.");
                    }
                    else
                    {
                        m_Names.RemoveAt(lastIdx);
                    }
                }
                else
                {
                    m_Names.Add(array[i]);
                }
            }
            if (array[length - 1] == "")
            {
                m_IsDirectory = true;
            }
            else
            {
                m_IsDirectory = false;
            }
            return true;
        }

        /// <summary>
        /// Attach a path to the current path
        /// </summary>
        /// <param name="pathName">path as a string</param>
        public void AddPath(string path)
        {
            if (path == null)
                return;
            else if (path == "")
                return;
            if (!AddRelativePath(path))
            {
                ConstructNewPath(path);
            }
            return;
        }

        // Properties
        /// <summary>
        /// Detect whether the path is an absolute path or a relative path
        /// </summary>
        public bool IsAbsolutePath
        {
            get
            {
                return m_IsAbsolutePath;
            }
        }

        /// <summary>
        /// Detect whether the path represents a directory or a file
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return m_IsDirectory;
            }
        }

        /// <summary>
        /// Return the length of this path object
        /// </summary>
        public int PathLength
        {
            get
            {
                if (m_Names != null)
                {
                    return m_Names.Count;
                }
                else
                {
                    return -1;
                }
            }

        }
    }
}
