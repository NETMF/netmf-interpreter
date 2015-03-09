# Profile_Map.pl
# perl script to convert a C header file to ASM inc file (#define -> EQU)
#
# Author: Gerald Cermak
# Copyright Microsoft Corporation 2004
#
# 1 May 2004
#

# use strict 'subs';

sub usage 
{
    print "Usage: perl Profile_Map1.pl application.symbdefs profile_address_hits.txt";
}

if ($ARGV[0] eq "") {
    usage();
    die;
}

if ($ARGV[1] eq "") {
    usage();
    die;
}

keys %symbol_type    = 4096;
keys %symbol_address = 4096;

open(SYMDEF_FILE, $ARGV[0] ) || die "ERROR: Can't open file: $ARGV[0]\n";

LINE: while (<SYMDEF_FILE>)
{
    chomp;

    #  #<SYMDEFS># ARM Linker, ADS1.2 [Build 848]: Last Updated: Mon May 03 09:06:14 2004
    #  0x00000020 A UNDEF_SubHandler_Trampoline
    #  0x00000028 A ABORTP_SubHandler_Trampoline
    #  0x00000030 A ABORTD_SubHandler_Trampoline
    #  0x00000038 A IRQ_SubHandler_Trampoline

    if (/#<SYMDEFS>#/) { next LINE; }
    
    if (($address, $type, $symbol) = (/^0x(\S{8}) (\S) (.*)$/)) {
        $symbol_address{$symbol} = hex($address);
        $symbol_type{$symbol}    = $type;
        
        # if ($type eq "D") { next LINE; } # ignore data types
                
        if ($prev_symbol ne "") {
            $symbol_size{$prev_symbol} = hex($address) - $symbol_address{$prev_symbol};
        }
        
        $prev_symbol = $symbol;
        
        next LINE;
    }

    printf("IGNORED:$_\n");

    next LINE;
}

close SYMDEF_FILE;

open(PROF_FILE, $ARGV[1] ) || die "ERROR: Can't open file: $ARGV[1]\n";

PROF_LINE: while (<PROF_FILE>) {
    # Entry Point  = 10300000
    # RAM code end = 00011b2c
    
    if ( /^Entry Point  = (\S{8})$/ ) {
        $FLASH_Base_Address = $1;
        # printf("FLASH start = %08x\n", hex($FLASH_Base_Address));
        next PROF_LINE;
    }
    
    if ( /^RAM code end = (\S{8})$/ ) {
        $RAM_End_Address    = $1;
        # printf("RAM End     = %08x\n", hex($RAM_End_Address));
        next PROF_LINE;
    }
    
    if (($hex_address, $hex_hits) = /^(\S+) (\S+)$/) {
        $address = hex($hex_address);
        $hits    = hex($hex_hits);
        
        if ($address >= hex($RAM_End_Address)) { 
            # printf("Adjust (%08x,%08x) %08x to ", hex($RAM_End_Address), hex($FLASH_Base_Address), $address);
            $address += (hex($FLASH_Base_Address) - hex($RAM_End_Address));
            # printf("%08x\n", $address);
        }

        # find the symbol in the map
        
        foreach $symbol (sort keys %symbol_address) {
            $symbol_end = $symbol_address{$symbol} + $symbol_size{$symbol};
            
            if (($address >= $symbol_address{$symbol}) & ($address < $symbol_end)) {
                $symbol_hits{$symbol} += $hits;
        
                if ($symbol_type{$symbol} eq "D") { die "Cannot Profile Data $symbol\n"; }
                        
                next PROF_LINE;
            }
        }
        
        printf("Unassigned Profile Address %08x %08x\n", $address, $hits);
    }
    else {
        die;
    }
}

close PROF_FILE;

foreach $symbol (sort keys %symbol_address) {
    if ($symbol_type{$symbol} eq "A") {
        if ($symbol_size{$symbol} > 0) {
            printf("%08x %8d %10u %13.2f %-50s\n",        $symbol_address{$symbol}, $symbol_size{$symbol}, $symbol_hits{$symbol}, $symbol_hits{$symbol}/$symbol_size{$symbol}, $symbol);
        }
        else {
            printf("%08x %8d %10u               %-50s\n", $symbol_address{$symbol}, $symbol_size{$symbol}, $symbol_hits{$symbol},                                              $symbol);
        }
    }
}

# return with errorlevel set to 0 (no error)

exit 0;


