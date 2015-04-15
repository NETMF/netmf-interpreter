
# Copyright (c) 2010 - 2014, AllSeen Alliance. All rights reserved.
#
#    Permission to use, copy, modify, and/or distribute this software for any
#    purpose with or without fee is hereby granted, provided that the above
#    copyright notice and this permission notice appear in all copies.
#
#    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
#    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
#    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
#    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
#    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
#    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
#    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
#

import getopt
import os
import re
import sys
import subprocess

def usage():
    print >> sys.stderr, """
Usage:
    python test_harness.py [ -c config_file ] [ -t gtestfile ] [ -p path_to_gtestfile ]
where:
    config_file:    test_harness config file;       default: test_harness.conf
    gtestfile:      name of gtest executable file;  default: ajtcltest
    path_to_gtestfile:   optional path to directory containing gtestfile
"""

def main(argv=None):

    # get commandline options

    conffile='test_harness.conf'
    testpath=''
    gtestfileopt=''

    if argv is None:
        argv=[]
    if len(argv) > 0:
        try:
            opts, junk = getopt.getopt(argv, 'c:p:t:')
            if junk:
                print >> sys.stderr, 'error, unrecognized arguments ' + str(junk)
                usage()
                return 2
            for opt, val in opts:
                if opt == '-c':
                    conffile = val
                elif opt == '-p':
                    testpath = val
                elif opt == '-t':
                    gtestfileopt= val
        except getopt.GetoptError, err:
            print >> sys.stderr, 'error, ' + str(err)
            usage()
            return 2

    # initialize

    dict = []
    filter = ''
    negfilter = ''
    gtestfile = ''
    part = ''

    re_comment = re.compile( r'\s*#.*$' )
    re_equals  = re.compile( r'\s*=\s*' )
    re_lastcolon   = re.compile( r':$' )
    re_TestCases   = re.compile( r'^\[\s*Test\s*Cases\s*\]', re.I )
    re_Environment = re.compile( r'^\[\s*Environment\s*\]', re.I )
    re_GTestFile   = re.compile( r'^\[\s*GTest\s*File\s*\]', re.I )

    # read config file one line at a time

    try:
        with open( conffile, 'r' ) as fileptr:

            for line in fileptr:
                # strip leading and trailing whitespace
                line = line.strip()
                line = line.strip('\n')
                # strip trailing comment (and preceding whitespace), if any
                line = re_comment.sub( '', line )

                # search line for part header
                if re_TestCases.search( line ):
                    # line ~= [ TestCases ]
                    part = 'TestCases'
                    continue

                elif re_Environment.search( line ):
                    # line ~= [ Environment ]
                    part = 'Environment'
                    print '[Environment]'
                    continue

                elif re_GTestFile.search( line ):
                    # line ~= [ GTestFile ]
                    part = 'GTestFile'
                    continue

                else:
                    # line is none of the above

                    # split line around equals sign (and surrounding whitespace), if any
                    dict = re_equals.split( line, 1 )

                    if (len(dict) > 1):
                        # line ~= something = something

                        if part == 'TestCases':
                            # Can select individual tests as well as groups.
                            # That is, TestCase selection can look like Foo.Bar=YES, not just Foo=YES.
                            # You can also used negative selection, like *=YES followed by Foo.Bar=NO.

                            d0 = dict[0].split('.',1)
                            if (dict[1].upper() == 'YES' or dict[1].upper() == 'Y'):
                                if (len(d0) > 1):
                                    filter = filter + dict[0] + ':'
                                else:
                                    filter = filter + dict[0] + '.*' + ':'
                            elif (dict[1].upper() == 'NO' or dict[1].upper() == 'N'):
                                if (len(d0) > 1):
                                    negfilter = negfilter + dict[0] + ':'
                                else:
                                    negfilter = negfilter + dict[0] + '.*' + ':'

                        elif part == 'Environment':
                            os.putenv(dict[0],dict[1])
                            print '\t%s="%s"' % ( dict[0], dict[1] )

                        elif part == 'GTestFile':
                            # the file name might contain = character
                            gtestfile = line

                    elif part == 'GTestFile' and line != '':
                        gtestfile = line

                    else:
                        # line is unusable
                        continue

    except IOError:
        print >> sys.stderr, 'error opening config file "%s"' % conffile
        return 2

    # assemble the path to gtestfile to execute

    command = gtestfile
    if gtestfileopt != '':
        command = gtestfileopt
    if command == '':
        command = 'ajtcltest'
    if testpath != '':
        command = os.path.join( testpath, command )

    print '[GTestFile]\n\t%s' % command

    if not ( os.path.exists( command ) or os.path.exists( command + '.exe' ) ):
        print >> sys.stderr, 'error, GTestFile="%s" not found' % command
        return 2
    command=[command]

    # assemble the gtest filter, if any

    if filter == '' and negfilter == '':
        pass
    elif filter != '' and negfilter == '':
        filter = re_lastcolon.sub( '', filter )
    elif filter == '' and negfilter != '':
        filter = '*' + '-' + re_lastcolon.sub( '', negfilter )
    else:
        filter = re_lastcolon.sub( '', filter ) + '-' + re_lastcolon.sub( '', negfilter)

    if filter != '':
        print '[TestCases]\n\t%s' % filter
        command.append('--gtest_filter=' + filter)

    # execute the gtestfile with filter argument, if any
    # exit status 0 if no errors, 1 if any tests failed, 2 if system error

    if subprocess.call(command) == 0:
        return 0
    else:
        return 1

if __name__ == '__main__':
    if len(sys.argv) > 1:
        sys.exit(main(sys.argv[1:]))
    else:
        sys.exit(main())
