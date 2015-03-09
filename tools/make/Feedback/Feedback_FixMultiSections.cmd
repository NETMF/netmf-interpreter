@ECHO OFF

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl

foreach $file (glob("*.feedback"))
{
   $tmp = "$file.tmp";
   unlink $tmp;

   open IN, "<$file" || die "Unable to open $file\n";
   open OUT, ">$tmp" || die "Unable to make temp file\n";

   while(<IN>)
   {
      if( /Sleep_uSec/ )
      {
          $_ = ";$_";
      }
      if( /Action_/ )
      {
          $_ = ";$_" if !/XAction_/;
      }
      if( /Prepare_Copy/  || /Prepare_Zero/    || /PrepareImageRegions/ || /BootstrapCode/ || 
          /Debug_EmitHEX/ || /Bootstrap/ )
      {
          $_ = ";$_";
      }

      $_ =~ s/;;/;/g;
      print OUT $_;
   }
   close IN;
   close OUT;

   rename $file, "$file.old";
   rename $tmp, $file;

   unlink "$file.old";

}