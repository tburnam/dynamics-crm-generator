@echo off

@echo.
echo Configuring your XRM Solutions development machine..
==========================================================

@echo.
echo Setting workspace variable..
==========================================================

if not defined DevAgentTag (
	REM set default value of development agent tag
	set DevAgentTag=true
	echo Development agent tag is not defined - defaulting to true..
)

if not defined TestAgentTag (
	REM set default value of development agent tag
	set TestAgentTag=true
	echo Test agent tag is not defined - defaulting to true..
)

if not defined AltWSRoot (
	REM set default value of alternate workspace root
	set AltWSRoot=
	echo Alternate workspace is not defined..
)

if [%AltWSRoot%]==[] (
	REM configure when there is no alternative workspace root
	set AltWSRoot=%cd%
)

REM display the value of the workspace root
echo AltWSRoot is %AltWSRoot%

set WSRoot=%AltWSRoot%
echo WSRoot is %WSRoot%

@echo.
echo Restoring nuget packages..
==========================================================

set PATH=%PATH%;%ProgramFiles(x86)%\MSBuild\14.0\Bin\;%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\;%ProgramFiles%\Git\usr\bin
powershell -ExecutionPolicy Bypass -Command %WSRoot%\init.ps1
set PATH=%WSRoot%\.tools;%WSRoot%\.tools\VSS.NuGet.AuthHelper;%PATH%

echo Package directory is: %WSRoot%\packages
nuget restore %WSRoot%\build\config\packages.config -ConfigFile %WSRoot%\build\config\nuget.config -PackagesDirectory %WSRoot%\packages

@echo.
echo Setting user variables..
==========================================================

REM Set environment variables for package PKG_XRMAPP_TOOLS
set "getPackage=%WSRoot%\build\agent\AgentUtilities.exe /command:getpkgfolder /config:%WSRoot%\build\config\packages.config /packageroot:%WSRoot%\packages /package:PKG_XRMAPP_TOOLS"
for /f "tokens=*" %%a in ('%getPackage%') do (set PKG_XRMAPP_TOOLS=%%a)
powershell -ExecutionPolicy Bypass -Command "%PKG_XRMAPP_TOOLS%\build\agent\agent_setProcessVariable.ps1 PKG_XRMAPP_TOOLS %PKG_XRMAPP_TOOLS%
echo PKG_XRMAPP_TOOLS is: %PKG_XRMAPP_TOOLS%


REM Restore build variables from available artifacts if they exist
echo TestAgentTag is %TestAgentTag%
echo BuildVariables path is %WSRoot%\target\buildVariables.txt

if [%TestAgentTag%] == [true] if exist %WSRoot%\target\buildVariables.txt (
	echo Enabling existing build variables..
	
	for /f %%a in (%WSRoot%\target\buildVariables.txt) do (
		set "%%a"
		echo %%a has been set
		
		for /f "delims== tokens=1,2" %%G IN ("%%a") do echo powershell -ExecutionPolicy Bypass -Command "%PKG_XRMAPP_TOOLS%\build\agent\agent_setProcessVariable.ps1 %%G %%H
	)
)


@echo.
REM Set Build Configuration environment variable
set BuildConfigurationDefault=debug
if not [%1] == [] (
	set BuildConfiguration=%1
)

if not defined BuildConfiguration (
	set BuildConfiguration=%BuildConfigurationDefault%
)

if [%BuildConfiguration%]==[] (
	set BuildConfiguration=%BuildConfigurationDefault%
)

echo BuildConfiguration=%BuildConfiguration%

REM Set Build Platform environment variable
set BuildPlatformDefault=AnyCPU
if not [%2] == [] (
	set BuildPlatform=%2
)

if not defined BuildPlatform (
	set BuildPlatform=%BuildPlatformDefault%
)

if [%BuildPlatform%]==[] (
	set BuildPlatform=%BuildPlatformDefault%
)

echo BuildPlatform=%BuildPlatform%

REM Set DropInConfigurationFolder
set DropInConfigurationFolderDefault=true

if not defined DropInConfigurationFolder (
	set DropInConfigurationFolder=%DropInConfigurationFolderDefault%
)

if ("%DropInConfigurationFolder%"=="") (
	set DropInConfigurationFolder=%DropInConfigurationFolderDefault%
)

echo DropInConfigurationFolder=%DropInConfigurationFolderDefault%

REM Set environment variables for remainder of packages
mkdir %WSRoot%\target\%BuildConfiguration%\%BuildPlatform%
%PKG_XRMAPP_TOOLS%\build\agent\AgentUtilities.exe /command:listpkgvars /config:%WSRoot%\build\config\packages.config /output:%WSRoot%\target\%BuildConfiguration%\%BuildPlatform%\processVariables.txt /packageroot:%WSRoot%\packages
for /f %%a in (%WSRoot%\target\%BuildConfiguration%\%BuildPlatform%\processVariables.txt) do (
	set "%%a"
	echo %%a has been set
)

REM Set Build Architecture environment variable
if [%BuildPlatform%] == [amd64] (
	set XSBuildArchitecture=x64
)

if not [%BuildPlatform%] == [amd64] (
	set XSBuildArchitecture=x86
)

echo XSBuildArchitecture is %XSBuildArchitecture%

powershell -ExecutionPolicy Bypass -Command "%PKG_XRMAPP_TOOLS%\build\agent\agent_setProcessVariable.ps1 XSBuildArchitecture %XSBuildArchitecture%"


REM Set EnableSigning environment variable
if [%DevAgentTag%] == [true] (
	set EnableSigning=false
	if "%BuildConfiguration%"=="debug" if "%SignDebugBuilds%"=="true" (
			set EnableSigning=true
			powershell -ExecutionPolicy Bypass -Command "%PKG_XRMAPP_TOOLS%\build\agent\agent_setProcessVariable.ps1 EnableSigning true"
	)
)

if [%DevAgentTag%] == [true] if [%BuildConfiguration%]==[retail] if [%SignRetailBuilds%]==[true] (
		set EnableSigning=true
		powershell -ExecutionPolicy Bypass -Command "%PKG_XRMAPP_TOOLS%\build\agent\agent_setProcessVariable.ps1 EnableSigning true"
)

echo Assembly signing is: %EnableSigning%


:initializeEnvironmentPath
@echo.
echo Initializing environment path variable..
==========================================================

:printExistingPath
set listPath=%PATH%
set listItemCount=0
@echo.
echo List of existing PATH entries:

:nextExistingPathItem
if "%listPath%" == "" (
	echo Existing PATH entries count: %listItemCount%
	goto setNewPath
)

set /a listItemCount+=1
for /f "tokens=1* delims=;" %%a in ("%listPath%") do (
	echo %%a
	set "listPath=%%b"
)
goto nextExistingPathItem
	
:setNewPath
set "getCleanedPathCommand=powershell -ExecutionPolicy Unrestricted -Command "%PKG_XRMAPP_TOOLS%\build\agent\AgentUtilities.exe /command:cleanpath /path:'%PATH%' /excludefile:%WSRoot%\build\config\pathExclude.txt""
for /f "delims=" %%I in ('%getCleanedPathCommand%') do set "PATH=%%I;%PKG_XRMAPP_TOOLS%\tools\commands;%PKG_XRMAPP_TOOLS%\tools\ImportSolution;%PKG_XRMAPP_TOOLS%\tools\ImportPackage"
set getCleanedPathCommand=

:printNewPath
set listPath=%PATH%
set listItemCount=0
@echo.
echo List of new PATH entries:

:nextNewPathItem
if "%listPath%" == "" (
	echo New PATH entries count: %listItemCount%
	goto initializeTAConfig
)

set /a listItemCount+=1
for /f "tokens=1* delims=;" %%a in ("%listPath%") do (
	echo %%a
	set "listPath=%%b"
)
goto nextNewPathItem


:initializeTAConfig
@echo.
echo Initializing test agent configuration..
==========================================================

set config_path=%WSRoot%\target\%BuildConfiguration%\%BuildPlatform%\TAConfig.config
echo CONFIG_PATH=%config_path%

REM alter WSRoot when TestAgentPath is available from VSO
if not defined TestAgentPath (
  set TestAgentPath=%WSRoot%
)
echo TestAgentPath=%TestAgentPath%


:initializeBuildCommand
@echo.
echo Initializing build command..
==========================================================

doskey root=cd %WSRoot%
doskey build=msbuild $*

@echo.
echo Generating environment configuration..
==========================================================

set buildVariables=%WSRoot%\target\buildVariables.txt
if exist %buildVariables% (
	del %buildVariables%
)

if [%DevAgentTag%] == [true] (
	%PKG_XRMAPP_TOOLS%\tools\GenerateEnvironmentConfiguration\GenerateEnvironmentConfiguration.exe %WSRoot% %WSRoot%\build\include %BuildConfiguration% %BuildPlatform%
	echo Flushing build variables for post build operations..
	
  echo BuildConfiguration=%BuildConfiguration% >> %buildVariables%
	echo BuildPlatform=%BuildPlatform% >> %buildVariables%
)

set testVariables=%WSRoot%\drop\%BuildConfiguration%\%BuildPlatform%\Tests\testVariables.txt
mkdir %WSRoot%\drop\%BuildConfiguration%\%BuildPlatform%\Tests
if exist %testVariables% (
	del %testVariables%
)

REM alter config_path when TestAgentTAConfigPath is available from VSO
if defined TestAgentTAConfigPath (
  echo TestAgentTAConfigPath=%TestAgentTAConfigPath%\TAConfig.config
  set config_path=%TestAgentTAConfigPath%\TAConfig.config
)

if [%TestAgentTag%] == [true] (
  echo Flushing test variables for post build operations..
  
  echo TestAgentPath=%TestAgentPath%>>%testVariables%
  echo BuildConfiguration=%BuildConfiguration%>>%testVariables%
  echo BuildPlatform=%BuildPlatform%>>%testVariables%
  echo config_path=%config_path%>>%testVariables%
)

==========================================================

:eof
echo Done!
@echo on