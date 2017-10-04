#include "stdafx.h"

#include <Windows.h>
#include <TlHelp32.h>
#include <tchar.h>
#include <iostream>
#include <algorithm>

#include "inject.h"

bool
SeDebugPrivilege()
{
	// Open current process token with adjust rights
	HANDLE Token;
	BOOL RetVal;

	// set t
	if (!(RetVal = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &Token)))
	{
		std::cout << "Could not open process token." << std::endl;
		return false;
	}

	// Get the LUID for SE_DEBUG_NAME 
	LUID Luid = { NULL }; // Locally unique identifier

	if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &Luid))
	{
		std::cout << "Could not look up privilege value for SE_DEBUG_NAME." << std::endl;
		return false;
	}
	if (Luid.LowPart == NULL && Luid.HighPart == NULL)
	{
		std::cout << "Could not get LUID for SE_DEBUG_NAME." << std::endl;
		return false;
	}

	// Process privileges
	TOKEN_PRIVILEGES Privileges = { NULL };

	// Set the privileges we need
	Privileges.PrivilegeCount = 1;
	Privileges.Privileges[0].Luid = Luid;
	Privileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

	// Apply the adjusted privileges
	if (!AdjustTokenPrivileges(Token, FALSE, &Privileges, sizeof(Privileges), NULL, NULL))
	{
		std::cout << "Could not adjust token privileges." << std::endl;
		return false;
	}

	return true;
}

bool
InjectLib(DWORD pid, const std::wstring &path)
{
	HANDLE Process;
	size_t Size;
	LPVOID DllRemote;

	if (!(Process = OpenProcess(PROCESS_QUERY_INFORMATION |
								PROCESS_CREATE_THREAD |
								PROCESS_VM_OPERATION |
								PROCESS_VM_WRITE, FALSE, pid)))
	{
		std::cout << "Could not get handle to process." << std::endl;
		return false;
	}

	Size = (path.length() + 1) * sizeof(wchar_t);

	if (!(DllRemote = VirtualAllocEx(Process, NULL, Size, MEM_COMMIT, PAGE_READWRITE)))
	{
		std::cout << "Could not allocate memory in remote process." << std::endl;
		return false;
	}

	if (!WriteProcessMemory(Process, DllRemote, path.c_str(), Size, NULL))
	{
		std::cout << "Could not write to memory in remote process." << std::endl;
		return false;
	}

	HMODULE hKernel32;

	if (!(hKernel32 = GetModuleHandle(TEXT("kernel32.dll"))))
	{
		std::cout << "Could not get handle to Kernel32." << std::endl;
		return false;
	}

	PTHREAD_START_ROUTINE pfnThreadRtn;

	if (!(pfnThreadRtn = reinterpret_cast<PTHREAD_START_ROUTINE>(GetProcAddress(hKernel32, "LoadLibraryW"))))
	{
		std::cout << "Could not get pointer to LoadLibraryW." << std::endl;
		return false;
	}

	HANDLE Thread;

	if (!(Thread = CreateRemoteThread(Process, NULL, 0, pfnThreadRtn, DllRemote, 0, NULL)))
	{
		std::cout << "Could not create thread in remote process." << std::endl;
		return false;
	}

	WaitForSingleObject(Thread, INFINITE);

	DWORD ExitCode;

	if (!(GetExitCodeThread(Thread, &ExitCode)))
	{
		std::cout << "Could not get thread exit code." << std::endl;
		return false;
	}

	if (!ExitCode)
	{
		std::cout << "Call to LoadLibraryW in remote process failed.  Dll must have exited non-gracefully." << std::endl;
		return false;
	}

	return true;
}