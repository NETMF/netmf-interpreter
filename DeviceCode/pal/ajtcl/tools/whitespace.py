#!/usr/bin/python

# Copyright (c) 2011 - 2013 AllSeen Alliance. All rights reserved.
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

# This is a copy of the standard client's whitespace.py
# The original is located in 'alljoyn/build_core/tools/bin'
# Changes to this file must be done here (ajtcl) and in build_core 

import sys, os, fnmatch, re, filecmp, difflib, textwrap
import hashlib, pickle, time
from subprocess import Popen, STDOUT, PIPE

def main(argv=None):
    start_time = time.clock()
    dir_ignore = ["stlport", "build", ".git", ".repo", "alljoyn_objc", "external"]
    file_ignore_patterns = ['\.#.*', 'alljoyn_java\.h', 'Status\.h', 'Status_CPP0x\.h', 'Internal\.h']
    file_patterns = ['*.c', '*.h', '*.cpp', '*.cc']
    valid_commands = ["check", "detail", "fix", "off"]
    uncrustify_config = None
    version_min = 0.57
    unc_suffix = ".uncrustify"
    wscfg = None
    xit=0
    sha_hash = hashlib.sha1()
    whitespace_db = {}
    whitespace_db_updated = False
    run_ws_check = True

    # try an load the whitespace.db file.  The file is dictionary of key:value pairs
    # where the key is the name of a file that has been checked by the WS checker
    # and the value is a sha1 blob calculated for the file.  specified for the key
    # if the key is not found (i.e. a new file or first time this script has been
    # run) then the key will be added.  If the  file fails the WS check it will be
    # removed from the dictionary.  If the file is new or the calculated hash has
    # changed the WS checker will check the file to see if it complies with the WS
    # rules.
    try:
        f = open('whitespace.db', 'r')
        try:
            whitespace_db = pickle.load(f)
        except pickle.UnpicklingError:
            os.remove('whitespace.db')
        finally:
            f.close()
    except IOError:
        print 'whitespace.db not found a new one will be created.'

    if argv is None:
        argv=[]
    if len(argv) > 0:
        wscmd = argv[0]
    else:
        wscmd = valid_commands[0]

    if len(argv) > 1:
        wscfg = os.path.normpath(argv[1])

    if wscmd not in valid_commands:
        print "\'" + wscmd + "\'" + " is not a valid command"
        print_help()
        sys.exit(2)

    '''If config specified in CL then use that, otherwise search for it'''
    if wscfg:
        uncrustify_config = wscfg
    else:
        uncrustify_config = find_config()

    '''Cannot find config file'''
    if not uncrustify_config:
        print "Unable to find a config file"
        print_help()
        sys.exit(2)

    '''Specified config file is invalid'''
    if not os.path.isfile(uncrustify_config):
        print uncrustify_config + " does not exist or is not a file"
        print_help()
        sys.exit(2)

    '''Verify uncrustify install and version'''
    version = uncrustify_version()
    if version < version_min:
        print ("You are using uncrustify v" + str(version) +
            ". You must be using uncrustify v" + str(version_min) +
            " or later.")
        sys.exit(2)

    print "whitespace %s %s" % (wscmd,uncrustify_config)
    print "cwd=%s" % (os.getcwd())
    if wscmd == 'off':
        return 0

    '''Get a list of source files and apply uncrustify to them'''
    for srcfile in locate(file_patterns, file_ignore_patterns, dir_ignore):
        f = open(srcfile, 'rb')
        filesize = os.path.getsize(srcfile)
        sha_digest = None
        try:
            # Compute the sha tag for the file the same way as git computes sha
            # tags this prevents whitespace changes being ignored.
            sha_hash.update("blob %u\0" % filesize)
            sha_hash.update(f.read())
            sha_digest = sha_hash.hexdigest()
        finally:
            f.close()

        if sha_digest != None:
            try:
                if whitespace_db[srcfile] == sha_digest:
                    run_ws_check = False
                else:
                    whitespace_db[srcfile] = sha_digest
                    run_ws_check = True
                    whitespace_db_updated = True
            except KeyError:
                whitespace_db[srcfile] = sha_digest
                run_ws_check = True
                whitespace_db_updated = True

        if run_ws_check:
            uncfile = srcfile + unc_suffix

            '''Run uncrustify and generate uncrustify output file'''
            Popen(["uncrustify", "-q", "-c",
                uncrustify_config, srcfile], stdout=PIPE).communicate()[0]

            '''check command'''
            if wscmd == valid_commands[0]:

                '''If the src file and the uncrustify file are different
                then print the filename'''
                if not filecmp.cmp(srcfile, uncfile, False):
                    print srcfile
                    del whitespace_db[srcfile]
                    xit=1

            '''detail command'''
            if wscmd == valid_commands[1]:

                '''If the src file and the uncrustify file are different
                then diff the files'''
                if not filecmp.cmp(srcfile, uncfile, False):
                    print ''
                    print '******** FILE: ' + srcfile
                    print ''
                    print '******** BEGIN DIFF ********'

                    fromlines = open(srcfile, 'U').readlines()
                    tolines = open(uncfile, 'U').readlines()
                    diff = difflib.unified_diff(fromlines, tolines, n=0)
                    sys.stdout.writelines(diff)

                    print ''
                    print '********* END DIFF *********'
                    print ''
                    del whitespace_db[srcfile]
                    xit=1

            '''fix command'''
            if wscmd == valid_commands[2]:

                '''If the src file and the uncrustify file are different
                then print the filename so that the user can see what will
                be fixed'''
                if not filecmp.cmp(srcfile, uncfile, False):
                    print srcfile
                    del whitespace_db[srcfile]

                '''run uncrustify again and overwrite the non-compliant file with
                the uncrustify output'''
                Popen(["uncrustify", "-q", "-c",
                        uncrustify_config, "--no-backup",
                        srcfile], stdout=PIPE).communicate()[0]

            '''remove the uncrustify output file'''
            if os.path.exists(uncfile):
                try:
                    os.remove(uncfile)
                except OSError:
                    print "Unable to remove uncrustify output file: " + uncfile

    # write the whitespace_db to a file so it is avalible next time the whitespace
    # checker is run.
    if whitespace_db_updated:
        try:
            f = open('whitespace.db', 'w')
            try:
                pickle.dump(whitespace_db, f)
            finally:
                f.close()
        except IOError:
            print 'Unable to create whitespace.db file.'
    print 'WS total run time: {0:.2f} seconds'.format(time.clock() - start_time)
    return xit

'''Return the uncrustify version number'''
def uncrustify_version( ):
    version = None

    try:
        '''run the uncrustify version command'''
        output = Popen(["uncrustify", "-v"], stdout=PIPE).communicate()[0]

    except OSError:
        '''OSError probably indicates that uncrustify is not installed,
         so bail after printing helpful message'''
        print ("It appears that \'uncrustify\' is not installed or is not " +
            "on your PATH. Please check your system and try again.")
        sys.exit(2)

    else:
        '''if no error, then extract version from output string and convert
        to floating point'''
        p = re.compile('^uncrustify (\d.\d{2})')
        m = re.search(p, output)
        version = float(m.group(1))

    return version

'''Command line argument help'''
def print_help( ):
        prog = 'whitespace.py'
        print textwrap.dedent('''\
        usage: %(prog)s [-h] [command] [uncrustify config]

        Apply uncrustify to C++ source files (.c, .h, .cc, .cpp),
        recursively, from the present working directory.  Skips
        'stlport', 'build', '.git', and '.repo' directories.

        Note:  present working directory is presumed to be within,
        or the parent of, one of the AllJoyn archives.

        Script will automatically locate the uncrustify config file
        in build_core, or alternatively, the user may specify one.

        Enables users to see which source files are not in compliance
        with the AllJoyn whitespace policy and fix them as follows:

            check     - prints list of non-compliant files (default)
            detail    - prints diff of non-compliant files
            fix       - modifies (fixes) non-compliant files

        Note that all files generated by uncrustify are removed.

        positional arguments:
          command            options: check(default) | detail | fix
          uncrustify config  specify an alternative uncrustify config (default=none)

        optional arguments:
          -h, --help         show this help message and exit

        Examples:

        Get a list of non-compliant files using the alljoyn uncrustify config:
            >python %(prog)s  --OR--
            >python %(prog)s check

        Get a list of non-compliant files using your own uncrustify config:
            >python %(prog)s check r'myconfig.cfg' (note: use raw string)

        Get a diff of non-compliant files using the alljoyn uncrustify config:
            >python %(prog)s detail

        Fix non-compliant files using the alljoyn uncrustify config:
            >python %(prog)s fix''' % { 'prog': prog } )

'''Search for the uncrustify config file'''
def find_config( ):
    tgtdir = "build_core"
    cfgname = "ajuncrustify.cfg"
    ajcfgrelpath = os.path.join(tgtdir, "tools", "conf", cfgname)
    ajcfgpath = None
    foundit = 0
    DIRDEPTHMAX = 6

    '''Limit directory search to depth DIRDEPTHMAX'''
    curdir = os.path.abspath(os.curdir)
    for i in range(DIRDEPTHMAX):
        if tgtdir in os.listdir(curdir):
            foundit = 1
            break
        else:
            curdir = os.path.abspath(os.path.join(curdir, ".."))

    if foundit == 1 and os.path.exists(os.path.join(curdir, ajcfgrelpath)):
        ajcfgpath = os.path.join(curdir, ajcfgrelpath)
    return ajcfgpath

'''Recurse through directories and locate files that match a given pattern'''
def locate(file_patterns, file_ignore_patterns, dir_ignore_patterns, root=os.curdir):
    for path, dirs, files in os.walk(os.path.abspath(root)):
        '''Remove unwanted dirs'''
        for dip in dir_ignore_patterns:
            for dyr in dirs:
                if dyr == dip:
                    dirs.remove(dyr)
        '''Remove unwanted files'''
        files_dict = {}
        for filename in files:
            files_dict[filename] = True             
        for filename in files:
            for fip in file_ignore_patterns:
                if re.search(fip, filename) != None:
                    del files_dict[filename]
        '''Collect the filtered list'''       
        filtered_files = []
        for filename in files_dict.keys():
            filtered_files.append(filename)        
        '''Filter the remainder using our wanted file pattern list'''
        for pattern in file_patterns:
            for filename in fnmatch.filter(filtered_files, pattern):
                yield os.path.join(path, filename)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        sys.exit(main(sys.argv[1:]))
    else:
        sys.exit(main())

#end
