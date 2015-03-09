using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Tasks.Internal
{
    public delegate void PropertyMatch(ref string source, Capture capture);

    class BuildTaskUtility
    {

        public static string ExpandEnvironmentVariables(string source)
        {
            RegexProperties(ref source, ExpandEnvironmentMatch);

            return source;
        }

        private static void ExpandEnvironmentMatch(ref string source, Capture capture)
        {
            string envVar = PropertyNameFromCapture(capture);

            string expandedVar = Environment.ExpandEnvironmentVariables("%" + envVar + "%");

            if (expandedVar != null)
            {
                source = source.Replace(capture.Value, expandedVar);
            }

        }

        public static string PropertyNameFromCapture(Capture capture)
        {
            return capture.Value.Substring(2, capture.Value.Length - 3);
        }

        public static void RegexProperties(ref string source, PropertyMatch matchMethod)
        {
            Regex regex = new Regex("\\$\\([a-zA-Z_:][a-zA-Z0-9.\\-_:]*\\)");

            Match match = regex.Match(source);

            foreach (Capture capture in match.Captures)
            {
                matchMethod(ref source, capture);
            }
        }

        public static string GetRelativePath(string pathRelative, string absolutePath)
        {
            if (!absolutePath.ToLower().StartsWith(pathRelative.ToLower()))
            {
                return null;
            }

            string relativePart = absolutePath.Remove(0, pathRelative.Length);

            if (relativePart.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                return relativePart.Remove(0, 1);
            }

            return relativePart;
        }
    }
}
