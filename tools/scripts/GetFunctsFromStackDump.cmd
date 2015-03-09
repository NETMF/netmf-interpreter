@echo off
SETLOCAL

perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl


$dump = shift(@ARGV);
$map  = shift(@ARGV);

die "Invalid Args!  Usage: GetFunctsFromStackDump.cmd  CRASH_DUMP  MAP_FILE\r\n" if( "$dump" eq "" || "$map" eq "" );

open DUMP, "<$dump" || die "Invalid Args!  Usage: CreateCallstack  CRASH_DUMP  MAP_FILE\r\n";
open MAP,  "<$map"  || die "Invalid Args!  Usage: CreateCallstack  CRASH_DUMP  MAP_FILE\r\n";

%sym_map;
%size_map;
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
    elsif( /^[\s]*(0x[\dabcdefABCDEF]+)\s+(0x[\dabcdefABCDEF]+)[\W\w]+\s+([\S]+)\s+([\w\W]+)/ )
    {
        $name = "$3, $4";
        chomp $name;
        $sys_map{hex($1)} = $name;
        $size_map{$name} = $2;
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
        if( $str =~ /(0x[\dabcdef]+)/ig )
        {
	        $strVal = hex($1);
            #print( "Data: $strVal\r\n" );
            $last_val = 0;
            foreach $val (sort {$a <=> $b} keys(%sys_map))
            {
                if( $val > $strVal )
                {
                    if( $last_val != 0 && !($sys_map{$last_val} =~ /^\./) && !($sys_map{$last_val} =~ /^SectionFor/) && !($sys_map{$last_val} =~ /^Veneer\$\$/))
                    {
                        $offset = $strVal - $last_val;

                        if($offset < hex($size_map{$sys_map{$last_val}}))
                        {
                            $strOff = sprintf( "%-11s %-95s 0x%05x", $1, $sys_map{$last_val}, $offset );
                            #print "$strVal: $sys_map{$last_val}, offset: 0x$strOff\r\n";
                            print "$strOff\r\n";
                        }
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
