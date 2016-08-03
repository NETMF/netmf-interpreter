using System.Collections.Generic;
using System.IO;
using XsdInventoryFormatObject;

namespace ComponentObjectModel
{
    public class BuildScriptWrapper
    {
        InventoryHelper m_invHelper;

        public BuildScriptWrapper(List<Inventory> inventories)
        {
            m_invHelper = new InventoryHelper(inventories);
        }

        public void ProduceBuildScript(string path, string scriptFileName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (TextWriter tw = File.CreateText(path + @"\tools\scripts\" + scriptFileName))
            {
                tw.WriteLine("@ECHO OFF");
                tw.WriteLine("SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION");
                tw.WriteLine();
                tw.WriteLine("if \"%1\" == \"\" goto :USAGE");
                tw.WriteLine("if \"%2\" == \"\" goto :USAGE");
                tw.WriteLine();
                tw.WriteLine("set SPO_SDK=%SPOCLIENT%");
                tw.WriteLine();
                tw.WriteLine("call %~dp0\\init.cmd");
                tw.WriteLine();
                tw.WriteLine("if \"%SPO_BUILD%\" == \"\" set SPO_BUILD=%SPO_SDK%_BUILD");
                tw.WriteLine();
                tw.WriteLine("set TARGETLOCATION=%1");
                tw.WriteLine("set TARGETFLAVOR=%2");
                tw.WriteLine();

                tw.Write("set ALL_ProjectS=");
                foreach (MFSolution p in m_invHelper.Solutions)
                {
                    tw.Write(p.Name + " ");
                }
                tw.WriteLine();
                tw.WriteLine("set OPTIONS=");
                tw.WriteLine("set ProjectLIST=");
                tw.WriteLine("set Project_NEXT=");
                tw.WriteLine();
                tw.WriteLine("for %%i IN (%*) do (");
                tw.WriteLine("    SET PROP_OLD=!OPTIONS!");
                tw.WriteLine("    SET VALID_PARAM=");
                tw.WriteLine();
                tw.WriteLine("    if DEFINED Project_NEXT (");
                tw.WriteLine("        SET ProjectLIST=!ProjectLIST!%%i ");
                tw.WriteLine("        SET VALID_PARAM=1");
                tw.WriteLine("        SET Project_NEXT=");
                tw.WriteLine("    )");

                foreach (BuildParameter bp in m_invHelper.BuildParameters)
                {
                    tw.WriteLine(string.Format("    if /i \"%%i\" == {0,-16} {1}", "\"" + bp.Parameter + "\"", "SET OPTIONS=!OPTIONS! " + bp.Action));
                }
                tw.WriteLine("    if /i \"%%i\" == \"Projects\"   (");
                tw.WriteLine("        SET Project_NEXT=1");
                tw.WriteLine("        SET VALID_PARAM=1");
                tw.WriteLine("    )");
                tw.WriteLine("    if /i \"!PROP_OLD!\"==\"!OPTIONS!\"  (");
                tw.WriteLine("        if NOT DEFINED VALID_PARAM (");
                tw.WriteLine("            @ECHO.");
                tw.WriteLine("            @ECHO Error: Invalid argument '%%i'!");
                tw.WriteLine("            goto :USAGE");
                tw.WriteLine("        )");
                tw.WriteLine("     )");
                tw.WriteLine(")");

                tw.WriteLine("if \"!ProjectLIST!\" == \"\" (");
                tw.WriteLine("    if DEFINED SINGLEProject (");
                tw.WriteLine("        SET ProjectLIST=\"%SINGLEProject%\"");
                tw.WriteLine("    ) else (");
                tw.WriteLine("        SET ProjectLIST=!FLASH_ProjectS!");
                tw.WriteLine("        if /i \"!TARGETLOCATION!\" == \"RAM\" set ProjectLIST=!RAM_ProjectS!");
                tw.WriteLine("    )");
                tw.WriteLine(")");
                tw.WriteLine();
                tw.WriteLine("for %%d in (!ProjectLIST!) do (");
                tw.WriteLine("    set PLATS=!ALL_ProjectS!");
                tw.WriteLine();
                tw.WriteLine("    set FOUND=");
                tw.WriteLine("    for %%r in (!PLATS!) do (");
                tw.WriteLine("        if /i \"%%d\"==\"%%r\" (");
                tw.WriteLine("            set FOUND=1");
                tw.WriteLine("            set TARGETProject=%%d");
                tw.WriteLine("            @ECHO msbuild dotNetMF.proj !OPTIONS! /p:Project=%%d /v:n");
                tw.WriteLine("            %MSBUILD_EXE% dotNetMF.proj !OPTIONS! /p:Project=%%d /v:n");
                tw.WriteLine("        )");
                tw.WriteLine("    )");
                tw.WriteLine("    if NOT DEFINED FOUND (");
                tw.WriteLine("        @ECHO.");
                tw.WriteLine("        @ECHO Error: Invalid Project '%%d' for target location '!TARGETLOCATION!'!");
                tw.WriteLine("        goto :USAGE");
                tw.WriteLine("    )");
                tw.WriteLine(")");
                tw.WriteLine();
                tw.WriteLine("ENDLOCAL");
                tw.WriteLine();
                tw.WriteLine("goto :EOF");
                tw.WriteLine();
                tw.WriteLine(
@":USAGE
      @ECHO.");
                tw.WriteLine(
"      @ECHO.Usage: msbuild_dotNetMF.cmd ^<TARGET^> ^<FLAVOR^> ^[^<options^>...^] ^[Projects \"<list>\"^]"
                );
                tw.WriteLine(
@"      @ECHO.
      @ECHO.^<LOCATION^>:
      @ECHO   FLASHROM      - Targets the image to the base of FLASH
      @ECHO   FLASH         - Targets the image to an offset of FLASH (determined by scatterfile)
      @ECHO   RAM           - Targets RAM 
      @ECHO   ROM           - Targets ROM (obsolete)
      @ECHO.
      @ECHO.^<FLAVOR^>:
      @ECHO   debug         - debug build
      @ECHO   release       - release build
      @ECHO   rtm           - release to manufacture build (highly optimized)
      @ECHO.
      @ECHO.^<options^>:");
                foreach (BuildParameter bp in m_invHelper.BuildParameters)
                {
                    tw.WriteLine(string.Format("      @ECHO   {0,-14}- {1}", bp.Parameter, bp.Description));
                }
                tw.WriteLine(
@"      @ECHO.
      @ECHO.Projects:
      @ECHO   ^<list^>      - Space separated list of valid Projects in double quotes
      @ECHO.
      @ECHO.valid Projects:

for %%p in (%ALL_ProjectS%) do @ECHO   %%p

goto :EOF
");

            }
        }

    }
}

