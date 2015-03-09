@echo off
SETLOCAL

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl


$dump = shift(@ARGV);
$map  = shift(@ARGV);

die "Invalid Args!  Usage: GetFunctsFromProfileDump.cmd  PROFILE_DUMP  MAP_FILE\r\n" if( "$dump" eq "" || "$map" eq "" );

open DUMP, "<$dump" || die "Invalid Args!  Usage: CreateCalldump  PROFILE_DUMP  MAP_FILE\r\n";
open MAP,  "<$map"  || die "Invalid Args!  Usage: CreateCalldump  PROFILE_DUMP  MAP_FILE\r\n";

%sym_map;
$start = 0;
while( <MAP> )
{
    if( $start == 0 )
    {
        if( $_ =~ /Memory Map of the image/ig )
        {
            $start = 1;
        }
    }
    elsif( /LR\$\$Debug/ )
    {
        $start = 0;
    }
    elsif( /^[\s]*(0x[\dabcdefABCDEF]*)[\W\w]+\s+([\S]+)\s+([\w\W]+)/ )
    {
        $name = "$2, $3";
        chomp $name;
        @sys_map{hex($1)} = $name;
        #print( "$name\r\n" );
    }
}
close MAP;

$header = sprintf( "%-11s %-95s %05s", "FunctAddr", "FunctName", "Offset" );
print "\r\n$header\r\n---------------------------------------------------------------------------------------------------------------------\r\n";

while( <DUMP> )
{
    foreach $str (split(/\s+/, $_))
    {
        if( $str =~ /(0*x*[\dabcdef]+)/ig )
        {
	        $strVal = $1;
            #print( "Data: $strVal\r\n" );
            $last_val = 0;
            foreach $val (sort(keys(%sys_map)))
            {
                if( $val > hex($strVal) )
                {
                    if( $last_val != 0 && !($sys_map{$last_val} =~ /^SectionFor/))
                    {
                        $offset = hex($strVal) - $last_val;
                        $strOff = sprintf( "%-11s %-95s 0x%05x", "$strVal", $sys_map{$last_val}, $offset );
                        #print "$strVal: $sys_map{$last_val}, offset: 0x$strOff\r\n";
                        print "$strOff\r\n";
                    }
                    last;
               }
               $last_val = $val;
            }
        }
        else
        {
            #print "No Data: $_\r\n";
        }
    }
}
close DUMP;
