// AddResource.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

void LogFailure(LPCTSTR pszStep);

int _tmain(int argc, _TCHAR* argv[])
{
	int resID;
	int resType;

	TCHAR szExeFile[1024];
	TCHAR szResFile[1024];

	const TCHAR szUsage[] =  L"\nUsage: AddResource.exe exe_file resource_id resource_type resource_file\n\n"
					   L"    exe_file      = path of file to add resource to\n"
					   L"    resource_id   = resource ID (int)\n"
					   L"    resource_type = resource type (int)\n"
					   L"    resource_filr = path of file containing resource data\n";

	if (argc != 5)
	{
		wprintf(szUsage);
		return -1;
	}

	DWORD dwLen = ExpandEnvironmentStrings(argv[1], szExeFile, ARRAYSIZE(szExeFile));
	if (dwLen == 0 || dwLen > ARRAYSIZE(szExeFile))
	{
		LogFailure(L"expand exe file path");
		return -1;
	}

	resID = _ttoi(argv[2]);
	if (resID <= 0)
	{
		wprintf(L"Invalid resource ID\n");
		return -1;
	}
      
	resType = _ttoi(argv[3]);
	if (resType <= 0)
	{
		wprintf(L"Invalid resource type\n");
		return -1;
	}

	dwLen = ExpandEnvironmentStrings(argv[4], szResFile, ARRAYSIZE(szResFile));
	if (dwLen == 0 || dwLen > ARRAYSIZE(szResFile))
	{
		LogFailure(L"expand resource file path");
		return -1;
	}

	BOOL fSuccess = FALSE;

	HANDLE hUpdate = BeginUpdateResource(szExeFile, false);
	if (hUpdate != NULL)
	{
		HANDLE hResFile = CreateFile(szResFile, GENERIC_READ, 0, NULL, OPEN_EXISTING, 0, NULL);
		if (hResFile != INVALID_HANDLE_VALUE)
		{
			DWORD cbDataSize = GetFileSize(hResFile, NULL);
			if (cbDataSize != INVALID_FILE_SIZE)
			{
				BYTE* pbData = (BYTE*)malloc(cbDataSize);
				if (pbData != NULL)
				{
					DWORD dwRead;
					if (ReadFile(hResFile, pbData, cbDataSize, &dwRead, NULL) && dwRead == cbDataSize)
					{
						if (UpdateResource(hUpdate, MAKEINTRESOURCE(resType), MAKEINTRESOURCE(resID), 
							           MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), pbData, cbDataSize))
						{
							if (EndUpdateResource(hUpdate, false))
							{
								hUpdate = NULL;
								fSuccess = TRUE;
							}
							else
							{
								LogFailure(L"save updated exe file");
							}
						}
						else
						{
							LogFailure(L"add resouce to exe");
						}
					}
					else
					{
						LogFailure(L"read resource data");
					}
					free(pbData);
				}
				else
				{
					wprintf(L"Out of memory\n");
				}
			}
			else
			{
				LogFailure(L"read resource file size");
			}

			CloseHandle(hResFile);
		}
		else
		{
			LogFailure(L"open resource file");
		}

		if (hUpdate != NULL)
		{
			// If update didn't succeed, close handle discarding changes
			EndUpdateResource(hUpdate, true);
		}
	}
	else
	{
		LogFailure(L"open exe file");
	}

	return fSuccess ? 0 : -1;
}

void LogFailure(LPCTSTR pszStep)
{
	wprintf(L"Failed to %s - error = %d\n", pszStep, GetLastError());
}

