#include "StdAfx.h"
#include "ToolsMsmCA.h"

#include <string>

void _Log(LogSeverity severity, LogMethod method, MSIHANDLE hInstall, const wchar_t* szBuffer)
{
    const wchar_t* szSeverity;
    switch (severity)
    {
        case LogVerbose:
            szSeverity = L"(Debug): ";
            break;

        case LogInfo:
        default:
            szSeverity = L"(Info ): ";
            break;

        case LogWarn:
            szSeverity = L"(Warn ): ";
            break;

        case LogError:
            szSeverity = L"(Error): ";
            break;
    }

    if (method & MethodMsiLog)
    {
        PMSIHANDLE hRec = MsiCreateRecord(3);
        if (hRec)
        {
            MsiRecordSetStringW(hRec, 0, L"Micro Framework SDK/PK installer [1][2]");
            MsiRecordSetStringW(hRec, 1, szSeverity);
            MsiRecordSetStringW(hRec, 2, szBuffer);
            MsiProcessMessage(hInstall, severity == LogError ? INSTALLMESSAGE_FATALEXIT : INSTALLMESSAGE_INFO, hRec);
        }
        else
        {
            OutputDebugStringW(L"ToolsMsmCA: Unable to write message to MSI log.");
            method = MethodOutputDebugString;
        }
    }

    if (method & MethodOutputDebugString)
    {
        OutputDebugStringW(L"ToolsMsmCA");
        OutputDebugStringW(szSeverity);
	    OutputDebugStringW(szBuffer);
        OutputDebugStringW(L"\n");
    }
}

void Log(LogSeverity severity, MSIHANDLE hInstall, const wchar_t* szFormat, ...)
{
    va_list args;
    va_start(args, szFormat);
    const int cchBuffer = 1024;
    wchar_t szBuffer[cchBuffer + 1];

    wvsprintfW(szBuffer, szFormat, args);
    szBuffer[cchBuffer] = 0;

    _Log(severity, MethodMsiLog, hInstall, szBuffer);

    va_end(args);
}

void Log(LogSeverity severity, LogMethod method, MSIHANDLE hInstall, const wchar_t* szFormat, ...)
{
    va_list args;
    va_start(args, szFormat);
    const int cchBuffer = 1024;
    wchar_t szBuffer[cchBuffer + 1];

    wvsprintfW(szBuffer, szFormat, args);
    szBuffer[cchBuffer] = 0;

    _Log(severity, method, hInstall, szBuffer);

    va_end(args);
}

LPWSTR GetProperty(MSIHANDLE hInstall, bool bSuppressErrors, LPCWSTR szPropertyId)
{
    UINT errCode;
    DWORD cch;
    wchar_t chDummy;
    cch = 1;
    LPWSTR pszBuffer = NULL;

    errCode = MsiGetProperty(hInstall, szPropertyId, &chDummy, &cch);
    if (errCode != ERROR_MORE_DATA && errCode != ERROR_SUCCESS)
    {
        Log(
            bSuppressErrors ? LogWarn : LogError,
            hInstall,
            L"Setup package issue: MsiGetProperty(\"%s\", 1) failed (%d).",
            szPropertyId,
            errCode);
        return NULL;
    }

    // Add one for NULL termination, then allocate a buffer.
    cch++;
    pszBuffer = new wchar_t[cch];

    if (!pszBuffer)
    {
        Log(
            bSuppressErrors ? LogWarn : LogError,
            hInstall,
            L"Out of memory allocating buffer for property \"%s\" (%d chars).",
            szPropertyId,
            cch);
        return NULL;
    }

    errCode = MsiGetProperty(hInstall, szPropertyId, pszBuffer, &cch);
    if (errCode != ERROR_SUCCESS)
    {
        Log(
            bSuppressErrors ? LogWarn : LogError,
            hInstall,
            L"Setup package issue: MsiGetProperty(\"%s\", %d) failed (%d).",
            szPropertyId,
            cch,
            errCode);
        return NULL;
    }

    Log(
        LogInfo,
        hInstall,
        L"MsiGetProperty(\"%s\")=%s",
        szPropertyId,
        pszBuffer);

    return pszBuffer;
}

bool SetProperty(MSIHANDLE hInstall, LPCWSTR pszPropertyId, LPCWSTR szPropertyValue)
{
    return ERROR_SUCCESS == MsiSetPropertyW(hInstall, pszPropertyId, szPropertyValue);
}

bool SplitProperty(LPWSTR szJoined, wchar_t splitter, LPWSTR* ppszOut1, ...)
{
    bool ok = true;
    LPWSTR* ppszOutNext;
    LPWSTR pszPos = szJoined;

    if (szJoined == NULL || splitter == 0 || ppszOut1 == NULL)
    {
        return false;
    }

    va_list args;
    va_start(args, ppszOut1);

    *ppszOut1 = pszPos;
    while (ppszOutNext = va_arg(args, LPWSTR*))
    {
        // Scan for next instance of splitter or null.
        while (*pszPos && *pszPos != splitter) pszPos++;

        // If we did not hit null, change splitter into null and skip it.
        if (*pszPos)
        {
            *pszPos = '\0';
            pszPos++;
        }
        else
        {
            // We hit null while scanning for a splitter.  That is bad.
            ok = false;
        }

        // pszPos points to either null or the start of the next string.
        *ppszOutNext = pszPos;
    }

    va_end(args);
    return ok;
}


