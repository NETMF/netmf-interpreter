# unique.pl
# perl script to print the sorted unique output of an input file
#
# Author: Gerald Cermak
# Copyright Microsoft Corporation 2002
#
# 25 Sept 2002
#

open( LOGFILE, $ARGV[0] ) || die "ERROR: Could not open $ARGV[0]\n";

while (<LOGFILE>)
{
	chop;

	$unique{$_} += 1;
}

close(LOGFILE);

foreach $key (sort keys %unique)
{
   printf("%s\n", $key);
}
