////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Text;

using NativeIO = Microsoft.SPOT.IO.NativeIO;

namespace System.IO
{
    /*
     * Provides methods for processing directory strings in an ideally
     * cross-platform manner.  Most of the methods don't do a complete
     * full parsing (such as examining a UNC hostname), but they will
     * handle most string operations.
     *
     * <p>File names cannot contain backslash (\), slash (/), colon (:),
     * asterick (*), question mark (?), quote ("), less than (&lt;),
     * greater than (&gt;), or pipe (|).  The first three are used as directory
     * separators on various platforms.  Asterick and question mark are treated
     * as wild cards.  Less than, Greater than, and pipe all redirect input
     * or output from a program to a file or some combination thereof.  Quotes
     * are special.
     *
     * <p>We are guaranteeing that <var>Path.SeparatorChar</var> is the correct
     * directory separator on all platforms, and we will support
     * <var>Path.AltSeparatorChar</var> as well.  To write cross platform
     * code with minimal pain, you can use slash (/) as a directory separator in
     * your strings.
     */
    // Class contains only static data, no need to serialize
    public sealed class Path
    {

        private Path()
        {
        }

        /*
         * Platform specific directory separator character.  This is backslash
         * ('\') on Windows, slash ('/') on Unix, and colon (':') on Mac.
         *
         * Make this platform specific when we port.
         */
        public static readonly char DirectorySeparatorChar = '\\';
        /*
         * Platform specific invalid list of characters in a path.
         *
         * Make this platform specific when we port.
         */
        public static readonly char[] InvalidPathChars = { '/', '\"', '<', '>', '|', ':', '\0', (Char)1, (Char)2, (Char)3, (Char)4, (Char)5, (Char)6, (Char)7, (Char)8, (Char)9, (Char)10, (Char)11, (Char)12, (Char)13, (Char)14, (Char)15, (Char)16, (Char)17, (Char)18, (Char)19, (Char)20, (Char)21, (Char)22, (Char)23, (Char)24, (Char)25, (Char)26, (Char)27, (Char)28, (Char)29, (Char)30, (Char)31 };

        /*
         * Changes the extension of a file path. The <code>path</code> parameter
         * specifies a file path, and the <code>extension</code> parameter
         * specifies a file extension (with a leading period, such as
         * <code>".exe"</code> or <code>".cool"</code>).<p>
         *
         * The function returns a file path with the same root, directory, and base
         * name parts as <code>path</code>, but with the file extension changed to
         * the specified extension. If <code>path</code> is null, the function
         * returns null. If <code>path</code> does not contain a file extension,
         * the new file extension is appended to the path. If <code>extension</code>
         * is null, any exsiting extension is removed from <code>path</code>.
         *
         * @param path The path for which to change file extension.
         * @param extension The new file extension (with a leading period), or
         * null to remove the extension.
         * @return Path with the new file extension.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #getExtension
         * @see #hasExtension
         */
        public static String ChangeExtension(String path, String extension)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                String s = path;
                for (int i = path.Length; --i >= 0; )
                {
                    char ch = path[i];
                    if (ch == '.')
                    {
                        s = path.Substring(0, i);
                        break;
                    }

                    if (ch == DirectorySeparatorChar) break;
                }

                if (extension != null && path.Length != 0)
                {
                    if (extension.Length == 0 || extension[0] != '.')
                    {
                        s = s + ".";
                    }

                    s = s + extension;
                }

                return s;
            }

            return null;
        }

        /*
       * Returns the directory path of a file path. This method effectively
       * removes the last element of the given file path, i.e. it returns a
       * string consisting of all characters up to but not including the last
       * backslash ("\") in the file path. The returned value is null if the file
       * path is null or if the file path denotes a root (such as "\", "C:", or
       * "\\server\share").
       *
       * @param path The path of a file or directory.
       * @return The directory path of the given path, or null if the given path
       * denotes a root.
       * @exception ArgumentException if <var>path</var> contains invalid characters.
       * @see #GetExtension
       * @see #GetFileNameFromPath
       * @see #GetRoot
       * @see #IsRooted
       */
        public static String GetDirectoryName(String path)
        {
            if (path != null)
            {
                NormalizePath(path, false);

                int root = GetRootLength(path);

                int i = path.Length;
                if (i > root)
                {
                    i = path.Length;
                    if (i == root) return null;
                    while (i > root && path[--i] != DirectorySeparatorChar) ;
                    return path.Substring(0, i);
                }
            }

            return null;
        }

        /*
         * Gets the length of the root DirectoryInfo or whatever DirectoryInfo markers
         * are specified for the first part of the DirectoryInfo name.
         *
         * @internalonly
         */
        internal static int GetRootLength(String path)
        {
            CheckInvalidPathChars(path);

            int i = 0;
            int length = path.Length;
            if (length >= 1 && (IsDirectorySeparator(path[0])))
            {
                // handles UNC names and directories off current drive's root.
                i = 1;
                if (length >= 2 && (IsDirectorySeparator(path[1])))
                {
                    i = 2;
                    int n = 2;
                    while (i < length && ((path[i] != DirectorySeparatorChar || --n > 0))) i++;
                }
            }

            return i;
        }

        internal static bool IsDirectorySeparator(char c)
        {
            return c == DirectorySeparatorChar;
        }

        public static char[] GetInvalidPathChars()
        {
            return (char[])InvalidPathChars.Clone();
        }

        public static String GetFullPath(String path)
        {
            ValidateNullOrEmpty(path);

            if (!Path.IsPathRooted(path))
            {
                string currDir = Directory.GetCurrentDirectory();
                path = Path.Combine(currDir, path);
            }

            return NormalizePath(path, false);
        }

        /*
         * Returns the extension of the given path. The returned value includes the
         * period (".") character of the extension except when you have a terminal period when you get String.Empty, such as <code>".exe"</code> or
         * <code>".cpp"</code>. The returned value is null if the given path is
         * null or if the given path does not include an extension.
         *
         * @param path The path of a file or directory.
         * @return The extension of the given path, or null if the given path does
         * not include an extension.
         * @exception ArgumentException if <var>path</var> contains invalid
         * characters.
         * @see #GetDirectoryNameFromPath
         * @see #GetFileNameFromPath
         * @see #GetRoot
         * @see #HasExtension
         */
        public static String GetExtension(String path)
        {
            if (path == null)
                return null;

            CheckInvalidPathChars(path);
            int length = path.Length;
            for (int i = length; --i >= 0; )
            {
                char ch = path[i];
                if (ch == '.')
                {
                    if (i != length - 1)
                        return path.Substring(i, length - i);
                    else
                        return String.Empty;
                }

                if (ch == DirectorySeparatorChar)
                    break;
            }

            return String.Empty;
        }

        /*
         * Returns the name and extension parts of the given path. The resulting
         * string contains the characters of <code>path</code> that follow the last
         * backslash ("\"), slash ("/"), or colon (":") character in
         * <code>path</code>. The resulting string is the entire path if <code>path</code>
         * contains no backslash after removing trailing slashes, slash, or colon characters. The resulting
         * string is null if <code>path</code> is null.
         *
         * @param path The path of a file or directory.
         * @return The name and extension parts of the given path.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetDirectoryNameFromPath
         * @see #GetExtension
         * @see #GetRoot
         */
        public static String GetFileName(String path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                int length = path.Length;
                for (int i = length; --i >= 0; )
                {
                    char ch = path[i];
                    if (ch == DirectorySeparatorChar)
                        return path.Substring(i + 1, length - i - 1);

                }
            }

            return path;
        }

        public static String GetFileNameWithoutExtension(String path)
        {
            path = GetFileName(path);
            if (path != null)
            {
                int i;
                if ((i = path.LastIndexOf('.')) == -1)
                    return path; // No path extension found
                else
                    return path.Substring(0, i);
            }

            return null;
        }

        /*
         * Returns the root portion of the given path. The resulting string
         * consists of those rightmost characters of the path that constitute the
         * root of the path. Possible patterns for the resulting string are: An
         * empty string (a relative path on the current drive), "\" (an absolute
         * path on the current drive), "X:" (a relative path on a given drive,
         * where X is the drive letter), "X:\" (an absolute path on a given drive),
         * and "\\server\share" (a UNC path for a given server and share name).
         * The resulting string is null if <code>path</code> is null.
         *
         * @param path The path of a file or directory.
         * @return The root portion of the given path.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetDirectory
         * @see #GetExtension
         * @see #GetName
         * @see #IsRooted
         */
        public static String GetPathRoot(String path)
        {
            if (path == null) return null;
            return path.Substring(0, GetRootLength(path));
        }

        /*
        * Tests if a path includes a file extension. The result is
        * <code>true</code> if the characters that follow the last directory
        * separator ('\\' or '/') or volume separator (':') in the path include
        * a period (".") other than a terminal period. The result is <code>false</code> otherwise.
        *
        * @param path The path to test.
        * @return Boolean indicating whether the path includes a file extension.
        * @exception ArgumentException if <var>path</var> contains invalid characters.
        * @see #ChangeExtension
        * @see #GetExtension
        */
        public static bool HasExtension(String path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                for (int i = path.Length; --i >= 0; )
                {
                    char ch = path[i];
                    if (ch == '.')
                    {
                        if (i != path.Length - 1)
                            return true;
                        else
                            return false;
                    }

                    if (ch == DirectorySeparatorChar) break;
                }
            }

            return false;
        }

        /*
         * Tests if the given path contains a root. A path is considered rooted
         * if it starts with a backslash ("\") or a drive letter and a colon (":").
         *
         * @param path The path to test.
         * @return Boolean indicating whether the path is rooted.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetRoot
         */
        public static bool IsPathRooted(String path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                int length = path.Length;
                if (length >= 1 && (path[0] == DirectorySeparatorChar))
                    return true;
            }

            return false;
        }

        public static String Combine(String path1, String path2)
        {
            if (path1 == null || path2 == null)
                throw new ArgumentNullException(/*(path1==null) ? "path1" : "path2"*/);
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);

            if (path2.Length == 0)
                return path1;

            if (path1.Length == 0)
                return path2;

            if (IsPathRooted(path2))
                return path2;

            char ch = path1[path1.Length - 1];
            if (ch != DirectorySeparatorChar)
                return path1 + DirectorySeparatorChar + path2;
            return path1 + path2;
        }

        //--//

        internal static void CheckInvalidPathChars(String path)
        {
            if (-1 != path.IndexOfAny(InvalidPathChars))
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidPathChars")*/);
        }

        // Windows API definitions
        internal const int MAX_PATH = 260;  // From WinDef.h

        internal static char[] m_illegalCharacters = { '?', '*' };

        internal static void ValidateNullOrEmpty(string str)
        {
            if (str == null)
                throw new ArgumentNullException();

            if (str.Length == 0)
                throw new ArgumentException();
        }

        internal static string NormalizePath(string path, bool pattern)
        {
            ValidateNullOrEmpty(path);

            int pathLength = path.Length;

            int i = 0;
            for (i = 0; i < pathLength; i++)
            {
                if (path[i] != '\\')
                    break;
            }

            bool rootedPath = false;
            bool serverPath = false;
            /// Handle some of the special cases.
            /// 1. Root (\)
            /// 2. Server (\\server).
            /// 3. InvalidPath (\\\, \\\\, etc).
            if (i == 1)
            {
                rootedPath = true;
            }
            else if ((i == 2) && (pathLength > 2))
            {
                serverPath = true;
            }
            else if (i > 2)
            {
                throw new ArgumentException();
            }

            if (rootedPath)
            {
                int limit = i + NativeIO.FSNameMaxLength;
                for (; i < limit && i < pathLength; i++)
                {
                    if (path[i] == '\\')
                    {
                        break;
                    }
                }

                if (i == limit) // if the namespace is too long
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.VolumeNotFound);
                }
                else if (pathLength - i >= NativeIO.FSMaxPathLength) // if the "relative" path exceeds the limit
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }
            }
            else // For non-rooted paths (i.e. server paths or relative paths), we follow the MAX_PATH (260) limit from desktop
            {
                if (pathLength >= MAX_PATH)
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }
            }

            string[] pathParts = path.Split(DirectorySeparatorChar);
            if (pattern && (pathParts.Length > 1))
                throw new ArgumentException();

            ArrayList finalPathSegments = new ArrayList();
            int pathPartLen;
            for (int e = 0; e < pathParts.Length; e++)
            {
                pathPartLen = pathParts[e].Length;
                if (pathPartLen == 0)
                {
                    /// Do nothing. Apparently paths like c:\\folder\\\file.txt works fine in Windows.
                    continue;
                }
                else if (pathPartLen >= NativeIO.FSMaxFilenameLength)
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }

                if (pathParts[e].IndexOfAny(InvalidPathChars) != -1)
                    throw new ArgumentException();

                if (!pattern)
                {
                    if (pathParts[e].IndexOfAny(m_illegalCharacters) != -1)
                        throw new ArgumentException();
                }

                /// verify whether pathParts[e] is all '.'s. If it is
                /// we have some special cases. Also path with both dots
                /// and spaces only are invalid.
                int length = pathParts[e].Length;
                bool spaceFound = false;

                for (i = 0; i < length; i++)
                {
                    if (pathParts[e][i] == '.')
                        continue;
                    if (pathParts[e][i] == ' ')
                    {
                        spaceFound = true;
                        continue;
                    }

                    break;
                }

                if (i >= length)
                {
                    if (!spaceFound)
                    {
                        /// Dots only.
                        if (i == 1)
                        {
                            /// Stay in same directory.
                        }
                        else if (i == 2)
                        {
                            if (finalPathSegments.Count == 0)
                                throw new ArgumentException();

                            finalPathSegments.RemoveAt(finalPathSegments.Count - 1);
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }
                    else
                    {
                        /// Just dots and spaces doesn't make the cut.
                        throw new ArgumentException();
                    }
                }
                else
                {
                    int trim = length - 1;
                    while (pathParts[e][trim] == ' ' || pathParts[e][trim] == '.')
                    {
                        trim--;
                    }

                    finalPathSegments.Add(pathParts[e].Substring(0, trim + 1));
                }
            }

            string normalizedPath = "";

            if (rootedPath)
            {
                normalizedPath += @"\";
            }
            else if (serverPath)
            {
                normalizedPath += @"\\";

                /// btw, server path must specify server name.
                if (finalPathSegments.Count == 0)
                    throw new ArgumentException();
            }

            bool firstSegment = true;
            for (int e = 0; e < finalPathSegments.Count; e++)
            {
                if (!firstSegment)
                {
                    normalizedPath += "\\";
                }
                else
                {
                    firstSegment = false;
                }

                normalizedPath += (string)finalPathSegments[e];
            }

            return normalizedPath;
        }
    }
}


