////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Windows specific headers
#define _WIN32_WINNT 0x0501
#define WIN32_LEAN_AND_MEAN     // Exclude rarely-used stuff from Windows headers

#include <windows.h>
#include <wininet.h>
#include <process.h>
#include <tchar.h>

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Standard C++ headers
#include <cstdio>
#include <cstdarg>
#include <cstdlib>

#include <vector>
#include <list>
#include <queue>
#include <mutex>
#include <condition_variable>
#include <functional>
#include <memory>
#include <iostream>
#include <sstream>
#include <cstdint>
#include <locale>
#include <codecvt>
#include <regex>
#include <iomanip>

////////////////////////////////////////////////////////////////////////////////////////////////////
// NETMF Specific

#include <TinyCLR_Application.h>
#include <TinyCLR_Win32.h>
#include <TinyCLR_ErrorCodes.h>
#include <TinyCLR_Interop.h>
#include <TinyCLR_ParseOptions.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

template<typename T>
inline bool bool_cast( typename std::enable_if< std::is_integral<T>::value, T>::type val)
{ 
    return !!val;
}

template<typename T>
struct bool_cast_traits;

template<>
struct bool_cast_traits<BOOL>
{
    const BOOL TrueValue = TRUE;
    const BOOL FalseValue = FALSE;
};

template<typename T>
inline T bool_cast( bool val )
{ 
    return val ? bool_cast_traits<T>::TrueValue : bool_cast_traits<T>::FalseValue;
}

