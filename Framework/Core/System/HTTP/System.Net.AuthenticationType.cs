//------------------------------------------------------------------------------
// <copyright from='1997' to='2001' company='Microsoft Corporation'>           
//    Copyright (c) Microsoft Corporation. All Rights Reserved.                
//    Information Contained Herein is Proprietary and Confidential.            
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;

    /// <summary>
    /// Network authentication type.
    /// Currently supports:
    /// Basic Authentication
    /// Microsoft Live Id Delegate Authentication
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// 
        /// </summary>
        Basic,
        /// <summary>
        /// 
        /// </summary>
        WindowsLive
    };
}
