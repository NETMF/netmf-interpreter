@echo off
perl -x "%~f0" %* & if ERRORLEVEL 1 exit /b 10
goto :EOF
#!perl

for($i=0; $i<$#ARGV; $i++)
{
    my $tmp = $ARGV[$i];

    $tmp =~ s|\"|\\\"|g;

    $tmp = "\"" . $tmp . "\"" if $tmp =~ /[ \"]/;

    $ARGV[$i] = $tmp;
}

$cmdline = join( " ", @ARGV);

print "$cmdline\n";

$maxWait = 10 * 60;

for($i=0;$i<$maxWait;$i++)
{
    $licenseFailure = 0;
    $skippedWarning = 0;


    open(OUTPUT, qq/$cmdline 2>&1|/);

    while(<OUTPUT>)
    {
        chop;

        # Warning: C2354W: cast from ptr/ref to 'struct CLR_RT_HeapBlock_BinaryBlob' to ptr/ref to 'struct CLR_RT_HeapBlock'; one is undefined, assuming unrelated
        if(/C2354W/) { $skippedWarning++; next; }

        ## Warning: C2262W: Attempt to initialise non-aggregate
        #if(/C2262W/) { $skippedWarning++; next; }

        if(/(.*): (\d+) warnings, (\d+) errors, (\d+) serious errors/)
        {
            $file          = $1;
            $warnings      = $2; $warnings -= $skippedWarning;
            $errors        = $3;
            $seriouserrors = $4;

            if($warnings || $errors || $seriouserrors)
            {
                print "$file: $warnings warnings, $errors errors, $seriouserrors serious errors";
            }

            next;
        }

        $licenseFailure = 1 if /C9932E/gi;
        $licenseFailure = 1 if /C3397E/gi;
        $licenseFailure = 1 if /L6579E/gi;
        $licenseFailure = 1 if /L0589E/gi;
        $licenseFailure = 1 if /Q0594E/gi;
        $licenseFailure = 1 if /Cannot obtain license for/gi;
        $licenseFailure = 1 if /WinSock\: Address already in use/gi;

        # MSBUILD TREATS ANY "Error" TEXT PRINTED TO THE CONSOLE AS A FAILURE, 
        # SO CHANGE "Error" to "err" for license failures.
	$_ =~ s/Error/Err/ig if $licenseFailure == 1;

        if(/^\"(.*)\", line (\d*):(.*)$/)
        {
            printf( "%s(%s) : %s\n", $1, $2, $3 );
        }
        else
        {
            print "$_\n";
        }
    }
    close(OUTPUT);

    if($licenseFailure)
    {
        #clear error conditions
        $? = 0; 
        $! = 0;
        $^E = 0;

        print "License Failure, sleeping 1 second...\n";
 
        sleep( 1 );
    }
    else
    {
        $exit_value = $? >> 8;

        if( $exit_value == 0 )
        {
            exit 0;
        }
        else
        {
            exit 1;
        }
    }
}
