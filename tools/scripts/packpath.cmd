@echo off

FOR /F "usebackq" %%i IN (`perl -x "%~f0"`) DO set FILE=%%i

call %FILE%

del/q %FILE%

goto :EOF

#!perl

$file = "$ENV{TEMP}\\TempPath_" . int(rand( 1000000 )) . ".cmd";

die "Cannot create option file" unless open OUTPUT, ">$file";

$got = 0;

foreach $in ( split( /;/, $ENV{PATH} ) )
{
    unless($seen{ $in })
    {
		if($got) { printf( OUTPUT "PATH %PATH%;$in\n" ); }
		else     { printf( OUTPUT "PATH $in\n" 		  ); }
		
		$got = 1;

		$seen{ $in } = 1;
	}
}

close OUTPUT;

printf "%s", $file;
