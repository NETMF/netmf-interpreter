@echo off
SETLOCAL

for %%i in (%*) DO (
  if /i "%%i"=="copy_only" (
    @ECHO ***** Copy Only! *****
    goto :NO_BUILD
  )
)

@ECHO setting managed build flavor to RELEASE
set FLAVOR_DAT=Release
set FLAVOR_WIN=Release

call %~dp0\init.cmd

:NO_BUILD

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl


use File::Basename;
use strict;


my @errors;
my @warnings;


my $ARGC = $#ARGV+1;

usage() unless $ARGC >= 1;

if( "$ENV{SPOCLIENT}" eq "" )
{
    die "Environment variable %SPOCLIENT% is not set\n";
}

# Command Line arguments <port_output_dir> [copy_only [build_automation]]
my $port_dir = shift(@ARGV);
chomp($port_dir);

my $spoclient = $ENV{SPOCLIENT};
my $sporoot   = $ENV{SPOROOT};


my $cpy_only   = "";

my $zip = 0;

my $bld_auto   = 0;

my $buildNumber = "0";

my $revisionNumber = "0";

foreach my $arg (@ARGV)
{
   if($arg =~ /copy_only/ig)
   {
       $cpy_only = $arg;
   }
   elsif($arg =~ /build_automation/ig)
   {
       $bld_auto = 1;
   }
   elsif($arg =~ /^buildnumber=(.+)$/ig)
   {
   		$buildNumber = $1;
   }
   elsif($arg =~ /^revisionnumber=(.+)/ig)
   {
   		$revisionNumber = $1;
   }
   elsif($arg =~ /zip/ig)
   {
        $zip = 1;
   }
}

print "\n\n";

if( (-e $port_dir) && $bld_auto == 0 )
{
    RunSystemCmd( "rd /s /q \"$port_dir\"", 1 );
}


Main();

if($#warnings > 0)
{
    my $i = 1;
    print "\n-------Porting Warnings-------\n";
    foreach my $warn (@warnings)
    {
        print "$i: $warn\n\n";
        $i++;
    }
}

if($#errors > 0)
{
    my $i = 1;
    print "\n-------Porting Errors-------\n";
    foreach my $err (@errors)
    {
        print "$i: $err\n\n";
        $i++;
    }
    exit 1;
}
else
{
    print "\n-------Porting Successful--------\n";
}
exit;


sub RunSystemCmd
{
    my $cmd         = shift(@_);
    my $exitOnError = shift(@_);

    print "$cmd\n";
    system( "$cmd" );

    if( $exitOnError != -1 )
    {
        my $exit_value = $? >> 8; # system return value is shifted up 8 bits

        if( $exit_value != 0 )
        {
            push @errors, $cmd;

            if( $exitOnError == 1 )
            {
               print "Error: $cmd\r\n";
               exit 1;
            }
        }
    }
}

sub Main()
{
    my $light = ""; #light";

    if($bld_auto == 0)
    {
       RunSystemCmd( "mkdir \"$port_dir\"", 1 );
    }

    chdir $ENV{SPOCLIENT};

    if( !($cpy_only =~ /^copy_only$/i) )
    {
        RunSystemCmd( "msbuild sdk.dirproj /t:Build /p:BuildNumber=$buildNumber /p:RevisionNumber=$revisionNumber", 1 );
    }

    ### copy common binary and source files for all platforms, copy the dat files.

    open COPYBIN, "<$spoclient\\tools\\scripts\\port\\BinaryCopyList.txt" || die "Unable to open binary copy list (tools\\scripts\\port\\BinaryCopyList.txt)\n";
    open COPYSRC, "<$spoclient\\tools\\scripts\\port\\SourceCopyList.txt" || die "Unable to open source copy list (tools\\scripts\\port\SourceCopyList.txt)\n";

    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\nCopying binary files ...\n\n";
    print "\n\nvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv\n\n";
    while( <COPYBIN> )
    {
        chomp($_);
        if( $_ ne "" )
        {
            CopyFile($_);
        }
    }
    close COPYBIN;


    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\nCopying source files ...\n\n";
    print "\n\nvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv\n\n";
    while( <COPYSRC> )
    {
        chomp($_);
        if( $_ ne "" )
        {
            CopyFile($_);
        }
    }
    close COPYSRC;

    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
    print "\n\nGenerate zip file ...\n\n";
    print "\n\nvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv\n\n";
    if( $zip == 1)
    {
        my($zipdirectory, $zipfilename) = $port_dir =~ m/(.*\\)(.*)$/;
		RunSystemCmd( "pushd \"$zipdirectory\" && zip.exe -q -r \"$zipfilename.zip\" \"$zipfilename\" && popd", 0 );
    }
}

#---------------------------------------------------------------------------------
#-  This method attempts to remove non-target OEM files
#---------------------------------------------------------------------------------
sub DeleteNonOEMFiles()
{
    my $plat = uc( shift(@_) );
    my %del_list;

    my @suffixes = split( /;/, "cfg;cmd;bmp;dll;pe;pe_downloads;strings;xml;exe"  );

    # delete the files...
    foreach my $key (keys(%del_list))
    {
        if( $del_list{$key} == 1 )
        {
            foreach my $suffix (@suffixes)
            {
                RunSystemCmd( "del /f /s /q \"$port_dir\\*_$key\_*.$suffix\"", -1 );

                RunSystemCmd( "del /f /s /q \"$port_dir\\*_$key.$suffix\""   , -1 );

                RunSystemCmd( "del /f /s /q \"$port_dir\\*$key.*.$suffix\""  , -1 );

                RunSystemCmd( "del /f /s /q \"$port_dir\\*.$key.*.$suffix\"" , -1 );
            }
        }
    }
}

sub GetRelativePath
{
    my $path      = shift(@_);
    my $buildroot = $ENV{BUILD_ROOT_BASE};

    my $tmp = $buildroot."\\";

    $tmp =~ s/([\w\W]+\\)[\w\W]+\\BuildOutput\\/$1/ig;   # leave <client>\buildoutput\
    $tmp =~ s/\\/\\\\/ig;
    $tmp =~ s/\$/\\\$/ig;
    $path =~ s/$tmp//ig;   # remove base build path


    $tmp = $sporoot."\\";
    $tmp =~ s/\\/\\\\/ig;
    $tmp =~ s/\$/\\\$/ig;
    $path =~ s/$tmp//ig;   # remove base source

    $path = "$port_dir\\$path";

    return $path;
}

#----------------------------------------------------------------------------------------------
#   This method copies/deletes the given file(s) in the following format:
#       [+/-], <path>\<file(s)>;    where files can use the wildcard * or ... for all files
#                                   and subdirectories
#----------------------------------------------------------------------------------------------
sub CopyFile
{
    my $arg = shift(@_);

    chomp($arg);

    my $buildroot   = $ENV{BUILD_ROOT_BASE};
    my $sporoot     = $ENV{SPOROOT};

    $arg =~ s/\%SPOCLIENT\%/$spoclient/gi;

    $arg =~ s/\%([\d\w_]+)\%/$ENV{$1}/ig;

    my ($cmd, $source, $targ_name) = split( /\,\s*/, $arg, 3 );

    my $source_path = "";

    my ($name,$source_path,$suff) = fileparse( $source, '\..*');

    my $path = $source_path;

    my $file = $name.$suff;

    $path = GetRelativePath($path);

    if( "$targ_name" eq "" )
    {
        $targ_name = $file;
    }

    my $isRecursive = 0;
    $isRecursive = 1 if $source =~ /\.\.\./g;

    $targ_name =~ s/\.\.\./\*/g;
    $targ_name =~ s/\*\*/\*/g;

    # only apply path if the target is not an absolute path, otherwise
    # get the relative path for the target
    if(!($targ_name =~ /:/g || $targ_name =~ /\\\\/g)) 
    {
        $targ_name = $path.$targ_name;
    }
    else
    {
        $targ_name = GetRelativePath($targ_name);
    }

    my ($targ_name, $path, $ext) = fileparse( $targ_name, '\..*' );

    $targ_name = $targ_name.$ext;

    if( "$cmd" eq "\+" )
    {
        if( !(-e $source_path) )
        {
            my $w = "Error: File doesn't exist - $source\n";
            push @errors, $w;
            print $w;
            return;
        }
        if( !(-e "$path") )
        {
            RunSystemCmd( "mkdir \"$path\"", 0 );
        }

        if( "$file" =~ /\.\.\./ )
        {
            if( "$file" eq "..." )
            {
              $source =~ s/\.\.\./\*/g;
              $path   =~ s/\.\.\./\*/g;
            }
            else
            {
              $source =~ s/\.\.\.//g;
              $path   =~ s/\.\.\.//g;
            }
            RunSystemCmd( "xcopy /Y /S \"$source\" \"$path$targ_name\"", 0 );
        }
        else
        {
            RunSystemCmd( "copy /Y \"$source\" \"$path$targ_name\"", 0 );
        }
    }
    elsif( "$cmd" eq "-" )
    {
        my $recurseArg = "";

        $recurseArg = "/s" if $isRecursive == 1;

        if(-d "$path$targ_name")
        {
           RunSystemCmd( "rd $recurseArg /q \"$path$\\targ_name\"", 0 );
        }
        else
        {
           RunSystemCmd( "del $recurseArg /q \"$path\\$targ_name\"", 0 );
        }
    }
    else
    {
        die "Invalid BinaryCopyList.txt file: Line must start with + or - indicating add/remove ($cmd)\n";
    }
}

sub usage()
{
    print( "\nOptions for generate_port:\n\n" );
    print( " generate_port <port_directory> <platform> [<flavor>[;<flavor>]*] [copy_only] [build_automation] [zip] [endianess]\n\n" );
    print( "     <port_directory>         is the drop point for the port\n\n" );
    print( "     <platform>               is one of the supported SPOT platforms\n\n" );
    print( "     <flavor>                 [release|debug|rtm] flavor of the bits\n\n" );
    print( "     copy_only                when this option is specified bianries are just copied from the previous build\n\n" );
    print( "     build_automation         Do not delete output directory\n\n" );
    print( "     buildnumber=<digits>     Set the build number (0..65534, default 0)\n\n" );
    print( "     revisionnumber=<digits>  Set the revision number (0..65534, default 0)\n\n" );
    print( "     zip                      Creates a compressed archive of the port\n\n" );
    print( "     <endianess>              [BE|LE] specified the big/little endian  \n\n" );
    exit;
}

