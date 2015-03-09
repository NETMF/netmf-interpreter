# h2inc.pl
# perl script to convert a C header file to ASM inc file (#define -> EQU)
#
# Author: Gerald Cermak
# Copyright Microsoft Corporation 2002
#
# 24 Sept 2002
#

use strict 'subs';

sub usage 
{
    print "Usage: perl h2inc.pl headerfile.h";
}

if ($ARGV[0] eq "") {
    usage();
    die;
}

open(HFILE, $ARGV[0] ) || die "ERROR: Can't open file: $ARGV[0]\n";
open(INCFILE, ">$ARGV[1]" ) || die "ERROR: Can't open file: $ARGV[1]\n";

print INCFILE "; DO NOT MODIFY - this is an automatically generated file from $ARGV[0]\n";
print INCFILE "\n";

# this is a really dumb script to extract #defines and convert to equates

LINE: while (<HFILE>)
{
  chomp;

  # asm files can't deal with parenthesis
  /[()]/  && next LINE;

  # asm includes can't deal with COM1/COM2 STDIO (stupid ARM assembler!)
  /COM1/  && next LINE;
  /COM2/  && next LINE;
  /STDIO/ && next LINE;

  # this is a placeholder we don't want in the output
  /reserved/ && next LINE;

  /#define/ && do { ($define, $label, $value) = split(' ', $_, 3) ; print INCFILE "$label\tEQU\t$value\n" }
}

print INCFILE "\tEND\n";

close INCFILE;
close HFILE;

# return with errorlevel set to 0 (no error)

exit 0;


