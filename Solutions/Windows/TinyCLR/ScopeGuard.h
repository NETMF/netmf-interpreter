#pragma once
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
// Description:
//   Provides standardized RAII pattern implementation for calling
//   clean up code on scope exit.
//
#include <functional>
#include <cassert>

namespace Microsoft
{
    namespace Patterns
    {
        ///<summary>Provides generalized RAII pattern clean support</summary>
        ///<remarks>
        ///<para>This class provides for a standardized implementation
        ///   of the classic RAII pattern. It stores a function
        ///   object that is called from the destructor when the
        ///   containing scope exits.</para>
        ///<para>The stored function can be any callable object that
        ///   takes no parameters and has no return (void) - including
        ///   classic pointers to functions, std::function and
        ///   C++11 Lambda expressions.</para>
        ///
        ///<para>Inspiration for this class comes from an article in
        ///   the Dec. 2000 Issue of Dr Dobb's journal by 
        ///   Andrei Alexandrescu and Petru Marginean. Their
        ///   implementation is more complex due to the lack of
        ///   functional programming paradigms in the C++ language
        ///   and standard libraries at the time they wrote it.
        ///   This version of the concept of ScopeGuard takes full
        ///   advantage of the latest features of C++ 11 and related
        ///   standard libraries resulting in a much simpler
        ///   implementation that is even more flexible and powerful
        ///   than the original.</para>
        ///</remarks>
        class ScopeGuard
        {
            std::function<void ()> UndoAction;

            // block copy/move
            ScopeGuard();
            ScopeGuard( const ScopeGuard& );
            ScopeGuard& operator=( const ScopeGuard& );

        public: 
            ///<summary>Creates a new ScopeGuard Instance</summary>
            ///<param name="undoAction>action to perform whenever code execution leaves the current scope</param>
            ScopeGuard( std::function<void ()> undoAction)
                : UndoAction( undoAction )
            {
            }

            // calls the guard function (if it isn't nullptr) to
            // clean up resources on scope exit.
            ~ScopeGuard()
            {
                if( UndoAction == nullptr)
                    return;

                try
                {
                    UndoAction();
                }
                catch(...)
                {
// assert is useful for early development, but causes issues with unit testing
// where tests deliberately cause exceptions to test proper handling of this
// catch block (destructors MUST NOT allow exceptions to escape out!)
#if SCOPEGUARD_ASSERT_ON_UNDOACTION_FAIL
                    // destructors *MUST NOT* throw an exception!
                    // if UndoAction fails, there is nothing that
                    // can be done about it. Move on...
                    assert(false);
#endif
                }
            }

            ///<summary>Release the guard so it does not clean up on exit</summary>
            ///<remarks>This is primarily useful for cases where the resource
            /// protected by this guard is allocated early in the scope where
            /// subsequent actions might fail but the resource is returned
            /// on success. In such a case the code for the containing scope
            /// should call Release before returning to prevent the automatic
            /// cleanup of the resource that is being returned.
            ///</remarks>
            void Release()
            {
                UndoAction = nullptr;
            }
        };
    }
}

// internal macros for building up a unique local scope instance name
#define SG_CONCAT(x,l) SG_CONCAT2(x,l) // use *VALUE* of X and l in SA_CONCAT2
#define SG_CONCAT2(x,l) x##l           // bind them to make a single name
#define SG_VARIABLE(str) SG_CONCAT(str, __LINE__)

///////////////////////////////////////////
///<summary>Provides generalized RAII pattern implementation for the current scope</summary>
///<param name="action">Action to perform on scope exit</param> 
///<remarks>
///<para>This macro creates and initializes an instance
/// of the ScopeGuard class with a name that is 
/// determined by the file line number in the source
/// code to ensure it is a unique name in the scope.</para> 
///
///<para><paramref href="action">action</paramref> can be any callable object that takes no parameters and
///   has no return (void). Including classic pointers to 
///   functions, std::function, and C++0x Lambda expressions.</para>
///
///<example>
///<code>
///      HANDLE hFile = ::CreateFile(path, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
///      if(INVALID_HANDLE_VALUE == hFile)
///          throw exception(...);
///    
///      // Now that handle is open - make sure it is always closed on exit
///      // via ScopeGuard and a C++11 Lambda.
///      // NOTE:
///      //   Lambda syntax allows capturing data (hFile) by value or by
///      //   reference depending on the needs of the situation. 
///      ON_SCOPE_EXIT( [hFile]() { ::CloseHandle( hFile ); } );
///
///      HANDLE hFile2 = ::CreateFile(path2, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
///      if(INVALID_HANDLE_VALUE == hFile2)
///          throw exception(...); // *only* hFile1 is cleaned up before exiting, since ON_SCOPE_EXIT for hFile2 isn't called yet
///    
///      // now that it is known to be valid ensure it is closed
///      ON_SCOPE_EXIT( [&hFile2]() { ::CloseHandle( hFile2 ); });
///
///      throw exception(...) // *both* hFile1 and hFile2 are cleaned up before exiting!
///<code>
///</example>
#define ON_SCOPE_EXIT( f ) Microsoft::Patterns::ScopeGuard SG_VARIABLE(ScopeGuard_)( f )
