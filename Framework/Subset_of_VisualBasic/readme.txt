
Step 1 
Compile user runtime using default runtime, then all functions like ToBool are resolved to default runtime
vbc Microsoft.VisualBasic.vb /noconfig /target:library /out:microsoft.visualbasic.dll    
In order for this compilation step to work the /vbrutnime:<path> switch is required

command line from msbuild:
manual: vbc Microsoft.VisualBasic.vb /noconfig /target:library /out:microsoft.visualbasic2.dll /vbruntime:lib\cst\microsoft.visualbasic.dll /sdkpath:E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\dll
build system:C:\Windows\Microsoft.NET\Framework\v4.0.30319\Vbc.exe /noconfig /imports:Microsoft.VisualBasic.CompilerServices.Utils,System,Microsoft.VisualBasic /optionstrict:custom /nowarn:42016 /nostdlib /removeintchecks+ /sdkpath:E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\dll /netcf /define:"CONFIG=\"Debug\",PLATFORM=\"AnyCPU\",SMARTDEVICE_RUNTIME_SUPPORTED=TRUE,DEBUG,TRACE" /reference:E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\dll\mscorlib.dll /debug+ /debug:full /optimize+ /out:E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\obj\Microsoft.VisualBasic\__COMMON\Microsoft.VisualBasic.dll /target:library /warnaserror+ Microsoft.VisualBasic.vb "C:\Users\lorenzte\AppData\Local\Temp\.NETMicroFramework,Version=v4.2.AssemblyAttributes.vb" E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\dll\Microsoft.VisualBasic_assemblyinfo.txt E:\src\client_v4_2_dev\BuildOutput\public\Debug\Client\obj\Microsoft.VisualBasic\__COMMON\_version.vb @NoVBRuntimeRef.rsp 


Step 2 
compile user runtime using previously generated runtime, this step is to make sure that user runtime is self-contained.
vbc Microsoft.VisualBasic.vb /noconfig /target:library /out:microsoft.visualbasic2.dll /vbruntime:microsoft.visualbasic.dll

