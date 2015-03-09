@echo off
SETLOCAL

call "%VS110COMNTOOLS%\vsvars32.bat"
call %~dp0\init.cmd

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF

#!perl
################################################################################

foreach ( @ARGV )
{
    if( /^depends$/ig)
    {
		$full = 1;
		next;
	}

    if (/^rebuild$/ig)
    {
        $clean = 1;
        next;
    }

    if (/^(options|[\/-][h?])$/ig)
    {
        &usage();
        exit 0;
    }
}



################################################################################

chdir "$ENV{CLRROOT}";

if ($full)
{
	if ($clean)
	{
		checkedsystem("msbuild Support/FrameworkBase/dotnetmf.proj /t:Clean");
		checkedsystem("msbuild Framework/Subset_of_CorLib/SpotCorLib.csproj /t:Clean");
		checkedsystem("msbuild Framework/CryptoWrapper/CryptoWrapper.csproj /t:Clean");
		checkedsystem("msbuild Framework/Core/build.dirproj /t:Clean");
		checkedsystem("msbuild Framework/TinyCore/TinyCore.csproj /t:Clean");
		checkedsystem("msbuild Framework/TinyCore/Ink.csproj /t:Clean");
		checkedsystem("msbuild Framework/Tools/MFDeploy/build.dirproj /t:Clean");
		checkedsystem("msbuild Framework/Tools/MFProfiler/MFProfiler.csproj /t:Clean");
		checkedsystem("msbuild Framework/Tools/MFSvcUtil/build.dirproj /t:Clean");
	}
	
	checkedsystem("msbuild Support/FrameworkBase/dotnetmf.proj /t:Build");
	checkedsystem("msbuild Framework/Subset_of_CorLib/SpotCorLib.csproj /t:Build");
	checkedsystem("msbuild Framework/CryptoWrapper/CryptoWrapper.csproj /t:Build");
	checkedsystem("msbuild Framework/Core/build.dirproj /t:Build");
	checkedsystem("msbuild Framework/TinyCore/TinyCore.csproj /t:Build");
	checkedsystem("msbuild Framework/TinyCore/Ink.csproj /t:Build");
	checkedsystem("msbuild Framework/Tools/MFDeploy/build.dirproj /t:Build");
	checkedsystem("msbuild Framework/Tools/MFProfiler/MFProfiler.csproj /t:Build");
	checkedsystem("msbuild Framework/Tools/MFSvcUtil/build.dirproj /t:Build");
}

chdir "$ENV{CLRROOT}/Test/Platform";

if ($clean)
{
     checkedsystem("msbuild build.dirproj /t:Clean");
}
checkedsystem("msbuild build.dirproj /t:Build");

################################################################################
sub checkedsystem()
{
    my ($cmd) = @_;

    printf( "\nBUILDING: %s\n", $cmd );

    return if system( $cmd ) == 0;

    printf( "\nFATAL ERROR AT: %s\n", $cmd );

    exit 1;
}

################################################################################
sub usage()
{
    printf( "\nBuild modifiers:\n\n" );
    printf( "    %-20s : %s\n", "rebuild"              , "Cleans and rebuilds the platform tests and generates the test installer."  );
    printf( "    %-20s : %s\n", "depends"              , "Builds the parts of the Micro Framework that the tests depend on."  );
}

################################################################################