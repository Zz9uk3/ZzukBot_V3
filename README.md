**How it worked back then**

 - BotLauncher checked if all required files exists and if there are
   updates available
 - It then proceeds to launch ZzukBot.exe which will do more checks and
   create a few required files which are embedded in the application
   inside the internal folder like the Navigation, Loader and
   FastCallDll
 - After creating the files it will launch the WoW process and inject
   itself into the running process using the Loader.dll
 - ZzukBot.exe will check if it is run injected or standalone. In
   this case it will detect that it is running inside the WoW.exe and
   proceed to spawn the GUI

**How to compile stuff**

 - Make sure you got VS2017 and latest VC++ redistributable
 - Open Solution and select debug or release in the configuration
   manager
 - Build the projects in the following order:
	 - Loader
	 - Navigation
	 - BotLauncher
	 - FastCallDll
	 - ZzukBot
 - Loader, Navigation and FastCall will drop the binaries inside the Resource folder of the ZzukBot Project after compilg. Every of those 3 files is a registered file resource and is required to be there to compile ZzukBot.exe
 - You should be fine to compile ZzukBot.exe and launch it
 - Select the 1.12.1 WoW.exe on the first launch and wait for WoW to open and the login GUI to spawn
 - Incase you build as debug you should be able to attach to the WoW process

