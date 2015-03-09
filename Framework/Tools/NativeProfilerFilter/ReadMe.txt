//--------------------------------------------------------------------------------------------------//
//   NativeProfileFilter.exe                                                                        //
//--------------------------------------------------------------------------------------------------//

I. Description:
This application crawls the source code tree and insert or remove native code profiling macros based
on the input file.  

II. Input File:
a) Description:
   The input file indicates to NativeProfileFilter which macros to add or remove from the given
   file, directory or method.

b) Input File Format: (see framework\tools\nativeprofilefilter\nativeprofilemap.txt for an example)
   The input file is a simple text file in which each line should either be empty or contain the 
   following format:


<+|-><F|D|M>, <NATIVE_LEVEL>, <PATH>[, METHOD]


+		- Macro ADD action 
-		- Macro REMOVE action

F		- Indicates that the macro action should be applied to a file (or set of files)
D		- Indicates that the macro action should be applied to a directory and its subdirectories
          	  (you can use 'F' with file type '*' to apply the macro to a single level directory)
M		- Indicates that the macro action should be applied to a particular method of a given file

NATIVE_LEVEL  	- Indicates the level to apply the macro to (CLR, HAL, PAL, or USER)
PATH		- The target for the given action type
		  For type F, this parameter should be the full file path (wild card '*' supported)
                  For type D, this parameter should be the full directory path to process (including subdirs)
		  For type M, this parameter should be the full file path (no wild cards supported)
METHOD		- This parameter is only valid for 'M' type actions.  The value should be the function name 
                  for which the macro action should be applied



Example:

+F, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Core\*
-F, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Core\Jitter*.cpp
+D, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Core
-D, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Libraries\CorLib
+M, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Libraries\SPOT\spot_native_Microsoft_SPOT_Debug.cpp,  Print___STATIC__VOID__STRING
-M, CLR, NATIVE_PROFILE_CLR_CORE, CLR\Libraries\SPOT\spot_native_Microsoft_SPOT_Debug.cpp,  DumpHeap___STATIC__VOID