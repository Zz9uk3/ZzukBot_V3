#include "stdafx.h"


#include <windows.h>
#include <tchar.h>
#include <tlhelp32.h>
#include <iostream>
#include <vector>
#include <algorithm>
#include <string>
#include <iostream>
#include <ctime>
#include <fstream>
using namespace std;

#include "io.h"
#include "inject.h"

#import <msxml6.dll> rename_namespace(_T("MSXML"))

std::tstring
GetPath(const std::tstring &ModuleName)
{
	HMODULE Self = GetModuleHandle(NULL);
	std::vector<TCHAR> LoaderPath(MAX_PATH);

	if (!GetModuleFileName(Self, &LoaderPath[0], static_cast<DWORD>(LoaderPath.size())) ||
		GetLastError() == ERROR_INSUFFICIENT_BUFFER)
	{
		std::cout << "Could not get path to loader." << std::endl;
		exit(-1);
	}

	std::tstring ModulePath(&LoaderPath[0]);
	ModulePath = ModulePath.substr(0, ModulePath.rfind(_T("\\")) + 1);
	ModulePath.append(ModuleName);

	if (GetFileAttributes(ModulePath.c_str()) == INVALID_FILE_ATTRIBUTES)
	{
		std::cout << "Could not find module.  Path: '" << ModulePath.c_str() << "'." << std::endl;
		exit(-1);
	}

	return ModulePath;
}

int
Inject()
{
	DWORD pid;
	const std::tstring ModuleName(_T("Internal\\Loader.Dll"));
	std::tstring ModulePath;
	STARTUPINFO wowsi;
	PROCESS_INFORMATION wowpi;
	HKEY wowkey;
	std::vector<char> path(MAX_PATH);
	DWORD size = path.size();

	std::string wowpath("");

#if 1
	MSXML::IXMLDOMDocument2Ptr xmlDoc;
	HRESULT hr = xmlDoc.CreateInstance(__uuidof(MSXML::DOMDocument60), NULL, CLSCTX_INPROC_SERVER);
	// TODO: if (FAILED(hr))...

	if (xmlDoc->load(_T("Settings\\Settings.xml")) != VARIANT_TRUE)
	{
		MessageBox(NULL, L"Cant find XML file. Run ThadHack.exe inside the Internal folder to recreate", NULL, MB_OK);
		printf("Unable to load input.xml\n");
	}
	else
	{
		xmlDoc->setProperty("SelectionLanguage", "XPath");
		MSXML::IXMLDOMNodePtr path = xmlDoc->selectSingleNode("/Settings/Path");
		wowpath = path->text;
	}
	//wowpath.append("\"C:\\Users\\Ano\\Desktop\\Tools\\WoW\\1.12.1 HQ WoW\\WoW.exe\" -console");
#else
	if ((ret = RegOpenKeyEx(HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Blizzard Entertainment\\World of Warcraft\\"), 0, KEY_READ, &wowkey)) != ERROR_SUCCESS)
		wowpath.append("C:\\Portable Apps\\World of Warcraft 1.12\\WoW.exe");
	else if (RegQueryValueExA(wowkey, "InstallPath", NULL, NULL, (LPBYTE)&path[0], (LPDWORD)&size))
	{
		std::cout << "Error opening InstallPath" << std::endl;
		return -1;
	}
	else
	{
		wowpath.append(&path[0]);
		wowpath.append("WoW.exe -console");
	}
#endif

	memset(&wowsi, 0, sizeof(wowsi));
	wowsi.cb = sizeof(wowsi);
	memset(&wowpi, 0, sizeof(wowpi));

	if (!CreateProcessA(NULL, (LPSTR)wowpath.c_str(), NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, NULL, (LPSTARTUPINFOA)&wowsi, &wowpi))
	{
		std::cout << "Could not start WoW.exe.  Start it manually and specify a pid." << std::endl;
		return -1;
	}

	pid = wowpi.dwProcessId;

	std::cout << "Spawned " << wowpath << " with pid: " << pid << std::endl;

	if (!pid)
	{
		std::tcout << _T("Invalid PID.");
		return -1;
	}

	if (!SeDebugPrivilege())
		return -1;

	ModulePath = GetPath(ModuleName);

	if (!InjectLib(pid, ModulePath))
	{
		TerminateProcess(wowpi.hProcess, 0);
		return -1;
	}

	ResumeThread(wowpi.hThread);

	CloseHandle(wowpi.hThread);
	CloseHandle(wowpi.hProcess);

	return 0;
}