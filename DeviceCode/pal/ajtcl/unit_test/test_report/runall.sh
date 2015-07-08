#!/bin/bash

# Copyright AllSeen Alliance. All rights reserved.
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

function Usage() {
	set +ex
	echo >&2 "
Runs GTest executable ajtcltest

Usage: $(basename -- "$0") [ -s -d alljoyn_dist ] [-c configfile ]
where
	-s		# start and stop our own AllJoyn-Daemon (default internal transport address, --no-bt)
	-d alljoyn_dist	# path to the build/.../dist tree, used to find cpp/bin/alljoyn-daemon exe, if used
	-c configfile	# name of config file
			#	default 'ajtcltest.conf'
"
	exit 2
}

: read commandline options

set -e

start_daemon=false
alljoyn_dist=''
configfile='*.conf'

while getopts sSc:d: option
do
	case "$option" in
	( c ) configfile="$OPTARG" ;;
	( d ) alljoyn_dist="$OPTARG" ;;
	( s ) start_daemon=true ;;
	( S ) start_daemon=false ;;
	( \? ) Usage ;;
	esac
done

if cygpath -wa . > /dev/null 2>&1
then
	# found Cygwin, which means Windows
	: there is no alljoyn-daemon on Windows
	start_daemon=false
fi

gtests=''
if test "$OPTIND" -gt 0 -a "$OPTIND" -le $#
then
	shift ` expr $OPTIND - 1 `
	while test $# -ge 1
	do
		case "$1" in
		( *[!a-zA-Z0-9_-]* | '' ) echo >&2 "error, $1:  allowed characters are [a-zA-Z0-9_-]" ; Usage ;;
		( * ) gtests="$gtests $1" ;;
		esac
		shift
	done
fi

if test -z "$gtests"
then
	gtests="ajtcltest"
fi

: check commandline options

ckbin() {
	ckvar=$1
	binvar=$2
	eval ckval="\$$1"
	binval=` cd "$ckval/cpp/bin" > /dev/null && pwd ` || : ok
	if test -z "$binval"
	then
		echo >&2 "error, could not find 'cpp/bin' subdirectory in $ckvar=$ckval"
		exit 2
	fi
	eval $binvar="'$binval'"
}

# alljoyn_dist
if $start_daemon
then
	if test -z "$alljoyn_dist"
	then
		echo >&2 "error, -d path is required"
		Usage
	fi
	ckbin alljoyn_dist daemon_bin
fi

gtest_bin=$( cd "$(dirname -- "$0")/.." > /dev/null && pwd )
if test -z "$gtest_bin" -o "gtest_bin" = /
then
	: unknown error trap
	exit 2
fi

if cygpath -wa . > /dev/null 2>&1
then
	# found Cygwin, which means Windows
	: there is no alljoyn-daemon on Windows
	options=''
	# gtest_bin needs to be Windows-style because we use pure Windows Python, not Cygwin Python
	gtest_bin_p="$( cygpath -wa "$gtest_bin" )"
	# sometimes Windows "home" does not work for keystore tests
	export USERPROFILE="$( cygpath -wa . )"
	export LOCALAPPDATA="$USERPROFILE"
else
	: set up alljoyn-daemon
	options="--print-address"
	gtest_bin_p="$gtest_bin"
	# MBUS-1589: remove .alljoyn_keystore, if any
	export HOME="$PWD"
        LinuxLibDir="$alljoyn_dist/cpp/lib"
        if [ -f "$LinuxLibDir/liballjoyn.so" ]; then
            ls -ld "$LinuxLibDir/liballjoyn.so"
            export LD_LIBRARY_PATH="$LinuxLibDir${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
        else
            echo "warning, liballjoyn.so does NOT exist!"
        fi
fi
# MBUS-1589: remove .alljoyn_keystore, if any
rm -rf .alljoyn_keystore

echo "# runall test plan:"
if $start_daemon
then
	ls >/dev/null -ld "$daemon_bin/alljoyn-daemon" || ls >/dev/null -ld "$daemon_bin/alljoyn-daemon.exe" || {
		echo >&2 "error, alljoyn-daemon exe not found"
		exit 2
	}
	echo "# $daemon_bin/alljoyn-daemon $options > alljoyn-daemon.log &"
fi

for i in $gtests
do
	ls >/dev/null -ld "$gtest_bin/$i" || ls >/dev/null -ld "$gtest_bin/$i.exe" || {
		echo >&2 "error, $i exe not found"
		exit 2
	}
	c="$( echo "$configfile" | sed -e 's,\*,'$i',g' )"
	ls >/dev/null -ld "$c" || {
		echo >&2 "error, configfile $c not found"
		exit 2
	}
	echo "# python test_harness.py -c $c -t $i -p $gtest_bin_p > $i.log"
done

: begin

export ER_DEBUG_ALL=0

if $start_daemon
then

	: start alljoyn-daemon

	rm -f alljoyn-daemon.log

	killall -v alljoyn-daemon || : ok
	(
		set -ex
		cd "$daemon_bin"
		pwd
		date

		./alljoyn-daemon $options; xit=$?

		date
		set +x
		echo exit status $xit
	) > alljoyn-daemon.log 2>&1 </dev/null &

	sleep 5
fi

: run gtest executables

xit=0
for i in $gtests
do
	# configfile for gtest $i
	c="$( echo "$configfile" | sed -e 's,\*,'$i',g' )"

	rm -f $c.t
	if $start_daemon
	then
		cat $c > $c.t
	else
		echo '    BusAttachmentTest.*=No' | cat $c - > $c.t
	fi
	rm -f "$i.log"

	sleep 5
	set -x
	: run $i

	date
	python -u test_harness.py -c $c.t -t $i -p "$gtest_bin_p" > "$i.log" 2>&1 < /dev/null || : exit status is $? / IGNORE IT
	date
	set +x
	tail -1 "$i.log" | grep "exiting with status 0" || xit=1

	case "$xit" in
	0 ) echo $i PASSED ;;
	* ) echo $i FAILED, see $i.log for info ;;
	esac

	sleep 5
done

if $start_daemon
then
	sleep 5
	killall -v alljoyn-daemon || : ok
	sleep 5
fi

echo exit status $xit
exit $xit
