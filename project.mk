NSS_INC_PATH=$(PROJECT_SOURCE_ROOT)\AdvancedScriptCompiler\scripts_X2;$(PROJECT_SOURCE_ROOT)\AdvancedScriptCompiler\Scripts_X1;$(PROJECT_SOURCE_ROOT)\AdvancedScriptCompiler\Scripts\Scripts
NSS_COMPILER=$(PROJECT_SOURCE_ROOT)\AdvancedScriptCompiler\NWNScriptCompiler.exe
ERF_UTIL=$(PROJECT_SOURCE_ROOT)\ErfUtil\ErfUtil.exe

#CLR_VERSION=v2.0.50727
CLR_VERSION=v4.0.30319
MSBUILD_TOOLS_PATH=$(SYSTEMROOT)\Microsoft.NET\Framework\$(CLR_VERSION)
MSBUILD=$(MSBUILD_TOOLS_PATH)\msbuild.exe

OUTPUTDIR=$(PROJECT_SOURCE_ROOT)\release\ 

ERFUTIL_RSP=ErfUtil.rsp

