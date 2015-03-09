@echo off
perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl

#
# Get the current directory
#
open CWD, 'cd 2>&1|';
$ScorchDir = <CWD>;
close CWD;
chomp $ScorchDir;

remove_empty_dirs( $ScorchDir );

sub remove_empty_dirs
{
    my ($directory)  = @_;
	my $nextdir;

    $directory =~ s:/$::;

    opendir DIR, $directory;
    foreach (readdir DIR)
	{
		next if ($_ eq '.' or $_ eq '..');

		$nextdir = "$directory\\$_";

        if (-d $nextdir) { remove_empty_dirs( $nextdir ); }
	}
    closedir DIR;

    opendir DIR, $directory;
    foreach (readdir DIR)
	{
		next if ($_ eq '.' or $_ eq '..');

		return;
	}
    closedir DIR;

	print "Removing empty directory $directory...\n";
	system( "rd/q $directory" );
}
