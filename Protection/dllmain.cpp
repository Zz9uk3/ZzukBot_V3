#include "stdafx.h"
#include "BlackBone/src/BlackBone/Process/Process.h"
#include "BlackBone/src/BlackBone/LocalHook/LocalHook.hpp"
#include <ctime>

using namespace blackbone;
using namespace std;

map<ptr_t, ptr_t> protectionMap;
map<ptr_t, pe::PEImage> imgMap;
HMODULE protectionHandle;
Process thisProc;

typedef NTSTATUS(NTAPI* tNtQueryInformationProcess)(
	HANDLE ProcessHandle,
	PROCESSINFOCLASS ProcessInformationClass,
	PVOID ProcessInformation, SIZE_T Length, PSIZE_T ResultLength
	);

typedef NTSTATUS(NTAPI* tNtQueryVirtualMemory)(
	HANDLE ProcessHandle,
	PVOID BaseAddress, MEMORY_INFORMATION_CLASS MemoryInformationClass,
	PVOID Buffer, SIZE_T Length, PSIZE_T ResultLength
	);

void* hookPtr;

Detour<tNtQueryVirtualMemory> detour;
tNtQueryVirtualMemory orig;

Detour<tNtQueryInformationProcess> detourProcess;
tNtQueryInformationProcess origProcess;

static bool HookInit = false;

NTSTATUS NTAPI HookNtQueryInformationProcess(
	HANDLE& ProcessHandle,
	PROCESSINFOCLASS& MemoryInformationClass,
	PVOID& ProcessInformation, SIZE_T& Length, PSIZE_T& ResultLength
)
{
	auto Result = detourProcess.CallOriginal((HANDLE&&)ProcessHandle,
		(PROCESSINFOCLASS&&)MemoryInformationClass,
		(PVOID&&)ProcessInformation, (SIZE_T&&)Length, (PSIZE_T&&)ResultLength);

	if (FAILED(Result))
	{
		return Result;
	}
	if (MemoryInformationClass == 0)
	{
		PPROCESS_BASIC_INFORMATION pbi = static_cast<PPROCESS_BASIC_INFORMATION>(ProcessInformation);
		pbi->PebBaseAddress = NULL;
	}
	return Result;
}


NTSTATUS NTAPI HookNtQueryVirtualMemory(
	HANDLE& ProcessHandle, PVOID& BaseAddress,
	MEMORY_INFORMATION_CLASS& MemoryInformationClass,
	PVOID& Buffer, SIZE_T& Length, PSIZE_T& ResultLength
)
{
	std::vector<BYTE> TempBuffer;
	try
	{
		std::copy(reinterpret_cast<PBYTE>(Buffer),
			reinterpret_cast<PBYTE>(Buffer) + Length,
			std::back_inserter(TempBuffer));
	}
	catch (...)
	{
		return detour.CallOriginal((HANDLE&&)ProcessHandle, (PVOID&&)BaseAddress, (MEMORY_INFORMATION_CLASS&&)MemoryInformationClass, (PVOID&&)Buffer, (SIZE_T&&)Length, (PSIZE_T&&)ResultLength);
	}
	NTSTATUS Result = detour.CallOriginal((HANDLE&&)ProcessHandle, (PVOID&&)BaseAddress, (MEMORY_INFORMATION_CLASS&&)MemoryInformationClass, (PVOID&&)Buffer, (SIZE_T&&)Length, (PSIZE_T&&)ResultLength);
	try
	{
		if (FAILED(Result) || GetProcessId(ProcessHandle) != GetCurrentProcessId())
		{
			return Result;
		}
		if (MemoryInformationClass != MemoryBasicInformation &&
			MemoryInformationClass != MemorySectionName)
		{
			return Result;
		}
		bool ShouldHide = false;
		for (auto const& entry : protectionMap)
		{
			PBYTE ModBase = reinterpret_cast<PBYTE>(entry.first);
			PBYTE ModEnd = ModBase + entry.second;
			if (BaseAddress >= ModBase && BaseAddress <= ModEnd)
				ShouldHide = true;
		}
		if (!ShouldHide)
		{
			return Result;
		}
		if (MemoryInformationClass == MemorySectionName)
		{
			std::copy(TempBuffer.begin(), TempBuffer.end(),
				reinterpret_cast<PBYTE>(Buffer));

			return detour.CallOriginal((HANDLE&&)ProcessHandle, (PVOID&&)NULL, (MEMORY_INFORMATION_CLASS&&)MemoryInformationClass, (PVOID&&)Buffer, (SIZE_T&&)Length, (PSIZE_T&&)ResultLength);
		}
		PMEMORY_BASIC_INFORMATION pMBI = static_cast<PMEMORY_BASIC_INFORMATION>(
			Buffer);
		PVOID NextBase = reinterpret_cast<PBYTE>(pMBI->BaseAddress) +
			pMBI->RegionSize;
		MEMORY_BASIC_INFORMATION NextBlock;
		ULONG NextLength = 0;
		Result = detour.CallOriginal((HANDLE&&)ProcessHandle, (PVOID&&)BaseAddress, (MEMORY_INFORMATION_CLASS&&)MemoryInformationClass, (PVOID&&)Buffer, (SIZE_T&&)Length, (PSIZE_T&&)ResultLength);
		pMBI->AllocationBase = 0;
		pMBI->AllocationProtect = 0;
		pMBI->State = MEM_FREE;
		pMBI->Protect = PAGE_NOACCESS;
		pMBI->Type = 0;
		if (NextBlock.State == MEM_FREE)
			pMBI->RegionSize += NextBlock.RegionSize;

		return Result;
	}
	catch (...)
	{
		return Result;
	}
}

int HookSetup(DWORD ProcessId)
{
	Process thisProc;
	thisProc.Attach(ProcessId);
	auto ntdll = GetModuleHandle(L"ntdll.dll");
	ptr_t ntqvm = (ptr_t)GetProcAddress(ntdll, "NtQueryVirtualMemory");
	orig = (tNtQueryVirtualMemory)(ntqvm);
	if (detour.Hook(orig, &HookNtQueryVirtualMemory, HookType::Inline, CallOrder::NoOriginal, ReturnMethod::UseNew))
	{
		HookInit = true;
		return 1;
	}
	return 0;
}

extern "C" __declspec(dllexport) int AddProtection(ptr_t address, ptr_t size)
{
	try
	{
		protectionMap.insert(std::make_pair(address, size));
		return 1;
	}
	catch (...)
	{
		return 0;
	}
}

extern "C" __declspec(dllexport) int HideAdditionalModules() 
{
	protectionMap.clear();
	for (auto m : thisProc.modules().GetAllModules())
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	for (auto m : thisProc.modules().GetAllModules(Sections))
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	for (auto m : thisProc.modules().GetAllModules(PEHeaders))
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	return 0;
}

extern "C" __declspec(dllexport) int SetupHideModules(DWORD ProcessId)
{
	thisProc.Attach(ProcessId);
	HookSetup(ProcessId);
	for (auto m : thisProc.modules().GetAllModules())
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	for (auto m : thisProc.modules().GetAllModules(Sections))
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	for (auto m : thisProc.modules().GetAllModules(PEHeaders))
	{
		if (m.second.get()->fullPath.find(L"internal") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"botbases") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"plugins") != std::wstring::npos
			|| m.second.get()->fullPath.find(L"customClasses") != std::wstring::npos)
		{
			AddProtection(m.second.get()->baseAddress, m.second.get()->size);
		}
	}
	return 0;
}


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:

	case DLL_THREAD_ATTACH:

	case DLL_THREAD_DETACH:

	case DLL_PROCESS_DETACH:

		break;
	}
	return TRUE;
}