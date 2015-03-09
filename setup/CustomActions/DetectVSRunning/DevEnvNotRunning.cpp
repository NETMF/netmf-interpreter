#include "StdAfx.h"
#include "ToolsMsmCA.h"

const int messageIdCloseDevEnv = 25000;
const wchar_t* const DevEnvExe = L"devenv.exe";
const wchar_t* const DexploreExe = L"dexplore.exe";
const wchar_t* const WDExpressExe = L"WDExpress.exe";

#pragma comment(linker, "/EXPORT:DevEnvNotRunning=_DevEnvNotRunning@4")

enum DevEnvStatus
{
    StatusNotRunning,
    StatusRunning,
    StatusError,
};

DevEnvStatus GetDevEnvStatus(MSIHANDLE hInstall)
{
    DevEnvStatus status = StatusNotRunning;

    const DWORD MaxProcessIds = 8192;
    DWORD* processIds = new DWORD[MaxProcessIds];
    const DWORD cbProcessIdsAllocated = MaxProcessIds * sizeof(*processIds);
    DWORD cbProcessIdsUsed;
    DWORD cProcesses;

    const int cchProcessName = MAX_PATH;
    wchar_t szProcessName[MAX_PATH + 1];

    if (!EnumProcesses(processIds, cbProcessIdsAllocated, &cbProcessIdsUsed))
    {
        status = StatusError;
        Log(LogError, hInstall, L"EnumProcesses failed, GetLastError() = %u.", GetLastError());
        goto Done;
    }

    cProcesses = cbProcessIdsUsed / sizeof(*processIds);

    for (DWORD i = 0; i < cProcesses; i++)
    {
        HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, processIds[i]);
        if (hProcess)
        {
            HMODULE hMod;
            DWORD cbNeeded;
            if (EnumProcessModules(hProcess, &hMod, sizeof(hMod), &cbNeeded))
            {
                if (GetModuleBaseNameW(hProcess, hMod, szProcessName, cchProcessName))
                {
                    szProcessName[cchProcessName] = 0;
                    if (0 == _wcsicmp(szProcessName, DevEnvExe))
                    {
                        Log(LogInfo, hInstall, L"DevEnv.exe found (PID %u).", processIds[i]);
                        status = StatusRunning;
                        break;
                    }

                    if (0 == _wcsicmp(szProcessName, DexploreExe))
                    {
                        Log(LogInfo, hInstall, L"dexplore.exe found (PID %u).", processIds[i]);
                        status = StatusRunning;
                        break;
                    }

                    if (0 == _wcsicmp(szProcessName, WDExpressExe))
                    {
                        Log(LogInfo, hInstall, L"WDExpress.exe found (PID %u).", processIds[i]);
                        status = StatusRunning;
                        break;
                    }
                }
                else
                {
                    Log(LogInfo, hInstall, L"GetModuleBaseNameW failed for PID %u, GetLastError() = %u.", processIds[i], GetLastError());
                }
            }
            else
            {
                Log(LogInfo, hInstall, L"EnumProcessModules failed for PID %u, GetLastError() = %u.", processIds[i], GetLastError());
            }
        }
        else
        {
            Log(LogInfo, hInstall, L"OpenProcess failed for PID %u, GetLastError() = %u.", processIds[i], GetLastError());
        }
    }

Done:
    delete[] processIds;
    return status;
}

extern "C" UINT __stdcall DevEnvNotRunning(MSIHANDLE hInstall)
{
    UINT errCode;
    DevEnvStatus status;
    bool cancelled = false;
    MSIHANDLE hMessage = NULL;

    status = GetDevEnvStatus(hInstall);
    while (!cancelled && status == StatusRunning)
    {
        if (hMessage == NULL)
        {
            hMessage = MsiCreateRecord(2);
        }

        MsiRecordClearData(hMessage);
        MsiRecordSetInteger(hMessage, 1, messageIdCloseDevEnv);

        int result = MsiProcessMessage(
            hInstall,
            static_cast<INSTALLMESSAGE>(INSTALLMESSAGE_WARNING | MB_RETRYCANCEL),
            hMessage);
        cancelled = (result == IDCANCEL);

        if (!cancelled)
        {
            status = GetDevEnvStatus(hInstall);
        }
    }

    switch (status)
    {
    case StatusNotRunning:
        errCode = ERROR_SUCCESS;
        break;

    case StatusRunning:
        errCode = ERROR_INSTALL_USEREXIT;
        break;

    case StatusError:
    default:
        Log(LogError, hInstall, L"Failed to search for running DevEnv.exe process.");
        errCode = ERROR_INSTALL_FAILURE;
        break;
    }

    if (hMessage != NULL) MsiCloseHandle(hMessage);

    return errCode;
}
