////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Resources;

namespace Microsoft.SPOT
{
    public static class ResourceUtility
    {
        private static ExtendedWeakReference s_ewr;
        private const int s_idSelectorCultureInfo = 0;

        static ResourceUtility()
        {
            try
            {
                s_ewr = ExtendedWeakReference.RecoverOrCreate(typeof(ResourceUtility), s_idSelectorCultureInfo, ExtendedWeakReference.c_SurviveBoot | ExtendedWeakReference.c_SurvivePowerdown);
                s_ewr.Priority = (int)ExtendedWeakReference.PriorityLevel.System;

                string ciName = (string)s_ewr.Target;

                if (ciName != null)
                {
                    CultureInfo ci = new CultureInfo(ciName);

                    CurrentUICultureInternal = ci;
                }
            }
            catch
            {
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static object GetObject(System.Resources.ResourceManager rm, Enum id);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static object GetObject(System.Resources.ResourceManager rm, Enum id, int offset, int length);

        private extern static CultureInfo CurrentUICultureInternal
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        static public void SetCurrentUICulture(CultureInfo culture)
        {
            CurrentUICultureInternal = culture;

            if(s_ewr != null)
            {
                s_ewr.Target = culture.Name;
            }
        }

        static public string[] GetDelimitedStringResources(ResourceManager rm, Enum resource)
        {
            string resources = (string)GetObject(rm, resource);

            return resources.Split('|');
        }

        static public string GetDelimitedStringResource(ResourceManager rm, Enum resource, int i)
        {
            string[] resources = GetDelimitedStringResources(rm, resource);

            return resources[i];
        }
    }
}


