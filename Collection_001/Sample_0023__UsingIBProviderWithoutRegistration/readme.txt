==========================================================================
Summary

Using private binaries of IBProvider without registration in system.

==========================================================================
Instruction

0. Supposed that you use vc12xp build of "IBProvider Professional Edition".

1. Install IBProvider 32bit and 64bit
   - check "LCPI.IBProvider.3"
   - check "Visual Studio CRT files"
   - check installation of "Tools" components

2. Create "temp" folder

3. Copy to "temp" folder the following files:
    - "c:\Program Files (x86)\LCPI\IBProvider.3\bin\_IBProvider_v3_vc12xp_i.dll"
    - "c:\Program Files\LCPI\IBProvider.3\bin\_IBProvider_v3_vc12xp_w64_i.dll"
    - "c:\Program Files (x86)\LCPI\IBProvider.3\Tools\_Win32ResUpdater.exe"
    - <from this project folder> "IBProvider.32bit.txt"
    - <from this project folder> "IBProvider.64bit.txt"

4. Rename copy of files:
   _IBProvider_v3_vc12xp_i.dll -> _IBProvider_v3_32bit_private.dll
   _IBProvider_v3_vc12xp_w64_i.dll -> _IBProvider_v3_64bit_private.dll

5. Run console "cmd.exe"

6. Make a "temp" folder the current directory

7. Execute:
   _Win32ResUpdater.exe -write -file IBProvider.32bit.txt -module _IBProvider_v3_32bit_private.dll -id 1 -lang 1033 -type PROG_REG_PARAMS

8. Execute:
   _Win32ResUpdater.exe -write -file IBProvider.64bit.txt -module _IBProvider_v3_64bit_private.dll -id 1 -lang 1033 -type PROG_REG_PARAMS

9. Copy to "<this project folder>\private\32bit" folder the following files:

    - temp\_IBProvider_v3_32bit_private.dll
    - "c:\Program Files (x86)\LCPI\IBProvider.3\bin\msvcp120.dll"
    - "c:\Program Files (x86)\LCPI\IBProvider.3\bin\msvcr120.dll"

10. Copy to "<this project folder>\private\64bit" folder the following files:

    - temp\_IBProvider_v3_64bit_private.dll
    - "c:\Program Files\LCPI\IBProvider.3\bin\msvcp120.dll" 
    - "c:\Program Files\LCPI\IBProvider.3\bin\msvcr120.dll" 

11. Build this project.
