#pragma once

enum LogSeverity
{
    LogError,
    LogWarn,
    LogInfo,
    LogVerbose,
};

enum LogMethod
{
    MethodNone = 0,
    MethodOutputDebugString = 1,
    MethodMsiLog = 2
};

struct CustomActionException
{
    wchar_t* m_errmsg;  // pointer to a static string: use with caution
    					// and don't pass just any old thing

	CustomActionException(wchar_t* pwszMsg)
		: m_errmsg(pwszMsg)
		{
		}
};

// Utility functions

// Send message to MSI Log.  Errors also go to message box.
// Format using sprintf formatting.
void Log(LogSeverity severity, MSIHANDLE hInstall, const wchar_t* szFormat, ...);

void Log(LogSeverity severity, LogMethod method, MSIHANDLE hInstall, const wchar_t* szFormat, ...);

// Gets an MSI property.  Allocates memory for it using new[] (caller must free).
// Returns NULL on error.
LPWSTR GetProperty(MSIHANDLE hInstall, bool bSuppressErrors, LPCWSTR szPropertyId);

bool SetProperty(MSIHANDLE hInstall, LPCWSTR pszPropertyId, LPCWSTR szPropertyValue);

// Splits a CustomActionData string using a delimiter.
// Somewhat like strtok.  Replaces the delimiter with null in szJoined.
// For out parameters, pass one for each section of the CustomActionData,
// followed by a NULL to indicate the last parameter.
// For example, if szJoined is "Param1|Param2|Param3" you would call
// SplitProperty(szJoined, '|', &szParam1, &szParam2, &szParam3, NULL).
// On return, szJoined = "Param1\0Param2\0Param3", szParam1 = "Param1",
// szParam2 = "Param2", and szParam3 = "Param3".  Always sets all
// parameters to valid strings within szJoined.  If szJoined does not have
// sufficient sections, the rest all point to the final null.
// Returns true if enough sections were found for all parameters.
bool SplitProperty(LPWSTR szJoined, wchar_t splitter, LPWSTR* ppszOut1, ...);

// Custom action implementations

// Install a VSD Device SDK.  Deferred/Rollback. Uses property
// "CustomActionData", format "SdkGuid|SdkRootDirectory|SdkDefinitionFile".
UINT DeviceSdk(MSIHANDLE hInstall, bool bInstall, bool bSuppressErrors);

// Install or uninstall a help 2.0 filter using HxRegisterSession_IHxFilters.
// Deferred/Rollback. Uses property "CustomActionData", format
// "Namespace|FilterQuery|FilterName".
UINT HelpFilter(MSIHANDLE hInstall, bool bInstall, bool bSuppressErrors);

// Halts install until no processes named "devenv" are running.
// while (devenv.exe is running)
// {
//    Pop up a Retry/Cancel dialog.
//    If Cancel, return failure (triggering installation rollback).
// }
// Return success.
extern "C" UINT __stdcall DevEnvNotRunning(MSIHANDLE hInstall);

// Sets Property [VSDSupportInfo] to "False" or "True", depending on whether
// support for PB 6.00 platforms is detected.
UINT PlatformSupported(MSIHANDLE hInstall);

// Reads properties "DOC_FEATURE", "NATIVE_FEATURE", and "MANAGED_FEATURE"
// from registry.  The values should be feature IDs.  For each feature ID,
// get the feature's "installed" and "action" states.  Put the corresponding
// states into the properties "DOC_FEATURE_Installed", "DOC_FEATURE_Action",
// "MANAGED_FEATURE_Installed", etc.
UINT SdkFeatureStates(MSIHANDLE hInstall);

