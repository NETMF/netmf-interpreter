@echo off
SETLOCAL

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl

$file = shift(@ARGV);
$tmp = $file.tmp;

open IN, "<$file" || die "Unable to open $file\n";
open TMP, ">$tmp" || die "Unable to create $tmp\n";

while(<IN>)
{
  $_ =~ s/﻿//ig;

  if(/\s+([\w\d_]+)\[\]/)
  {
     $_ =~ s/($1)\[\]/System.Collections.Generic.List\<$1\>/ig;
  }
  print TMP $_;
}

close IN;
close TMP;

unlink $file;
rename $tmp, $file;