#ifndef _AJ_DEBUG_H
#define _AJ_DEBUG_H

/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

/**
 * @file aj_debug.h
 * This file contains the debug logging support for the Thin Client.
 *
 * @defgroup aj_debug Debug Logging
 *
 * @brief The debug logging module provides structured support for what is
 * sometimes called "printf debugging" or "debug tracing."  As in most such
 * facilities, support for different levels of verbosity is provided.  Since a
 * Thin Client program can be run on different devices with very different
 * capabilities, the debug logging module allows for convenient and verbose
 * debug logging on off-target (desktop-like) platforms; and it provides the
 * ability to constrain debug logging in on-target (embedded system) platforms.
 * Because debug logging needs to work in both desktop and embedded environments
 * there are a few more "knobs" than one might expect.  The next sections
 * describe how to quickly enable debug output and then continues to explain how
 * to configure debug logging in more detail on platforms of different
 * capabilities.
 *
 * @section aj_debug_qs Quick Start
 *
 * To enable all debug logging from all modules, open inc/aj_debug.h in your
 * favorite editor and change the definition of @c AJ_DEBUG_RESTRICT to:
 *
 * @code
 *     #define AJ_DEBUG_RESTRICT AJ_DEBUG_ALL
 * @endcode
 *
 * Open src/aj_debug.c in your favorite editor and set the initial value of the
 * verbosity setting @c AJ_DbgLevel to:
 *
 * @code
 *     AJ_DebugLevel AJ_DbgLevel = AJ_DEBUG_ALL;
 * @endcode
 *
 * If you want to enable logging from all modules, change the initial value of
 * @c dbgALL in src/aj_debug.c to:
 *
 * @code
 *     uint8_t dbgALL = 1;
 * @endcode
 *
 * After the changes are made, do a debug build (NDEBUG must be set to false to
 * allow any logging above the warning level).  Debug logging will now be
 * enabled for all modules.  When a Thin Client program is run, the system will
 * begin logging debug messages to the device console formatted as:
 *
 * @code
 *     seconds.milliseconds filename:line-number message
 * @endcode
 *
 * If you find the logging from all modules to be too verbose, you can enable
 * logging from specific modules.  In this case, leave @c dbgALL set to zero and
 * enable logging for each module individually.  For a module named @c MODULE
 * change the initial value of the variable @c dbgMODULE in src/aj_MODULE.c to
 * nonzero.  For example, to enable logging in the CONNECT module, change the
 * variable @c dbgCONNECT in src/aj_connect.c to:
 *
 * @code
 *     uint8_t dbgCONNECT = 1;
 * @endcode
 *
 * @note Setting @c dbgALL or @c dbgCONNECT may be done by changing the variable
 * in memory using a debugger at runtime instead of by hardcoding as shown here.
 *
 * @section aj_debug_env Enabling Debug Logging Using Environment Variables
 *
 * On targets that support it (for example Linux or Windows), debug logging may
 * also enabled using environment variables.  Instead of setting a memory variable
 * named as @c dbgMODULE one can set a corresponding environment variable named as
 * @c ER_DEBUG_MODULE.  If one wanted to enable debug logging in the CONNECT module
 * as done in the @ref aj_debug_qs section, one would set the environment variable
 * for the CONNECT module:
 *
 * @code
 *     export ER_DEBUG_CONNECT=1
 * @endcode
 *
 * @note There is an environment variable corresponding to the @c dbgALL memory
 * variable.  to enable logging on @c ALL modules, simply set the @c
 * ER_DEBUG_ALL environment variable.
 *
 * @section aj_debug_ver Changing Verbosity of Debug Logging
 *
 * Often, the amount of debug logging printed can be quite large.  To minimize
 * the amount of "debug spew" it is possible to control the verbosity of the
 * debug output.  This is done by changing the value of the memory variable @c
 * AJ_DbgLevel.
 *
 * There are several different levels of verbosity: @par
 * @ref AJ_DEBUG_OFF @par
 * @ref AJ_DEBUG_ERROR @par
 * @ref AJ_DEBUG_WARN @par
 * @ref AJ_DEBUG_INFO @par
 * @ref AJ_DEBUG_DUMP @par
 * @ref AJ_DEBUG_ALL @par
 *
 * To use this feature, set the variable @c AJ_DbgLevel either by hardcoding in
 * src/aj_debug.c or by setting the memory variable using a debugger.  Think of
 * this value as enabling messages of the specified verbosity and lesser.  For
 * example, in order to enable error, warning and informational messages, go to
 * src/aj_debug.c and set the initial value of @c AJ_DbgLevel to:
 *
 * @code
 *    AJ_DebugLevel AJ_DbgLevel = AJ_DEBUG_INFO;
 * @endcode
 *
 * @note Again, one can set AJ_DbgLevel in the debugger to dynamically control the
 * verbosity of logging at runtime.
 *
 * @section aj_debug_com Restricting Compilation of Debug Logging
 *
 * It is possible that some target environments are restricted to such a degree that
 * it is not possible to store all of the strings required for the various log
 * statements in memory.  To accommodate such environments a @c RESTRICT mechanism
 * is provided.  This restriction mechanism is controlled by the definition of
 * @c AJ_DEBUG_RESTRICT in the inc/aj_debug.h header file.
 *
 * The same verbosity levels are used in the @c RESTRICT mechanism as were shown
 * in the @ref aj_debug_ver section, but the meaning is different.  Think of the
 * definition of AJ_DEBUG_RESTRICT as meaning, restrict messages of levels
 * greater than the specified level from even being compiled into the code.  The
 * default value of AJ_DEBUG_RESTRICT is given as AJ_DEBUG_WARN so by default
 * only error and warning messages will be logged.  Messages of AJ_DEBUG_INFO
 * level and greater are not compiled into the code by default.  In the @ref
 * aj_debug_qs section, AJ_DEBUG_RESTRICT was set to AJ_DEBUG_ALL to allow all
 * messages to be compiled into the code so they could be logged.
 *
 * Typically, if one is running on a platform that has enough memory to
 * accommodate all of the log messages one would change AJ_DEBUG_RESTRICT to
 * AJ_DEBUG_ALL and simply leave it along.  The usefulness of this definition is
 * when the target cannot accommodate all of the strings.  In that case, it may
 * be useful to relax the restriction on a per-module basis to enable subsets of
 * logging when required.  To accomplish this, one would leave AJ_DEBUG_RESTRICT
 * set to AJ_DEBUG_INFO in inc/aj_debug.h and add the following code to the
 * source file of the module where logging was to be enabled before inclusion of
 * aj_debug.h:
 *
 * @code
 *     #define AJ_DEBUG_RESTRICT AJ_DEBUG_ALL
 * @endcode
 *
 * @{
 */

#include "aj_target.h"
#include "aj_msg.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Always print a message in a fashion similar to other conditional log outputs.
 * Do not include time stamp, file and line number.
 *
 * @param msg  A format string and arguments
 */
#define AJ_AlwaysPrintf(msg) \
    do { \
        AJ_Printf msg; \
    } while (0)

#ifndef NDEBUG

/**
 * Dump message name and content. if body is true, dump raw data
 *
 * @param tag       tag name of message
 * @param msg       message header
 * @param body      if true, dump raw data
 */
void _AJ_DumpMsg(const char* tag, AJ_Message* msg, uint8_t body);

/**
 * Dump raw (byte) data in a convenient format.
 *
 * @param tag       tag name of message
 * @param data      start address to dump
 * @param len       length to dump
 */
void _AJ_DumpBytes(const char* tag, const uint8_t* data, uint32_t len);

/*
 * Threshold level for debug output.  When used with AJ_DbgLevel the setting
 * controls which debug messages are actually printed.  These values are also
 * used in the AJ_DEBUG_RESTRICT mechanism to control which log messages are
 * actually compiled into the code.
 */
#define AJ_DEBUG_OFF   0  /**< Suppresses all debug output */
#define AJ_DEBUG_ERROR 1  /**< Indicates a log message conveying an error condition */
#define AJ_DEBUG_WARN  2  /**< Indicates a log message corresponding to a warning */
#define AJ_DEBUG_INFO  3  /**< Indicates a log message with general information */
#define AJ_DEBUG_DUMP  4  /**< Indicates a message with a detailed, possibly byte-by-byte dump */
#define AJ_DEBUG_ALL   5  /**< A placeholder level above other levels */

/**
 * Type definition for a value used to control the debug level (verbosity)
 * threshold.
 */
typedef uint32_t AJ_DebugLevel;

/**
 * We allow the verbosity of debug output to be controlled programmatically using
 * predefined AJ_DEBUG_* threshold levels.  The macro AJ_DEBUG_RESTRICT is used
 * in the sense of restricting (not compiling in) messages with verbosity levels
 * greater than the given level.
 *
 * By default, all messages of all verbosity at info level and above are not
 * compiled into the code (by defining AJ_DEBUG_RESTRICT to be AJ_DEBUG_WARN).
 */
#ifndef AJ_DEBUG_RESTRICT
#define AJ_DEBUG_RESTRICT AJ_DEBUG_WARN
#endif

/**
 * Set this value to control the debug output threshold level. The default is AJ_DEBUG_ERROR
 */
AJ_EXPORT extern AJ_DebugLevel AJ_DbgLevel;
AJ_EXPORT extern uint8_t dbgALL;

extern int _AJ_DbgEnabled(const char* module);

/**
 * Internal debug printf function. Don't call this directly, use the AJ_*Printf() macros.
 *
 * @param level The level associated with this debug print
 * @param file  File name for file calling this function
 * @param line  Line number for line this function was called from
 */
AJ_EXPORT
int _AJ_DbgHeader(AJ_DebugLevel level, const char* file, int line);

#define QUOTE(x) # x
#define STR(x) QUOTE(x)

#define CONCAT(x, y) x ## y
#define MKVAR(x, y) CONCAT(x, y)

#if AJ_DEBUG_RESTRICT >= AJ_DEBUG_ERROR
/**
 * Print an error message.  Error messages may be suppressed by AJ_DEBUG_RESTRICT
 *
 * @param msg  A format string and arguments
 */
#define AJ_ErrPrintf(msg) \
    do { \
        if (_AJ_DbgHeader(AJ_DEBUG_ERROR, __FILE__, __LINE__)) { AJ_Printf msg; } \
    } while (0)
#else
#define AJ_ErrPrintf(_msg)
#endif

#if AJ_DEBUG_RESTRICT >= AJ_DEBUG_WARN
/**
 * Print a warning message. Warnings may be suppressed by AJ_DEBUG_RESTRICT
 *
 * @param msg  A format string and arguments
 */
#define AJ_WarnPrintf(msg) \
    do { \
        if (_AJ_DbgHeader(AJ_DEBUG_WARN, __FILE__, __LINE__)) { AJ_Printf msg; } \
    } while (0)
#else
#define AJ_WarnPrintf(_msg)
#endif

#if AJ_DEBUG_RESTRICT >= AJ_DEBUG_INFO
/**
 * Print an informational message.  Informational messages may be suppressed by
 * AJ_DEBUG_RESTRICT or by the module selection (global memory value or shell
 * environment variable) mechanism.
 *
 * @param msg  A format string and arguments
 */
#define AJ_InfoPrintf(msg) \
    do { \
        if (dbgALL || MKVAR(dbg, AJ_MODULE) || _AJ_DbgEnabled(STR(AJ_MODULE))) { \
            if (_AJ_DbgHeader(AJ_DEBUG_INFO, __FILE__, __LINE__)) { AJ_Printf msg; } \
        } \
    } while (0)
#else
#define AJ_InfoPrintf(_msg)
#endif

#if AJ_DEBUG_RESTRICT >= AJ_DEBUG_DUMP
/**
 * Dump the bytes in a buffer in a human readable way.  Byte dumps messages may
 * be suppressed by AJ_DEBUG_RESTRICT or by the module selection (global memory
 * value or shell environment variable) mechanism.
 *
 * @param msg A format string
 * and arguments
 */
#define AJ_DumpBytes(tag, data, len) \
    do { \
        if (MKVAR(dbg, AJ_MODULE) || _AJ_DbgEnabled(STR(AJ_MODULE))) { _AJ_DumpBytes(tag, data, len); } \
    } while (0)
#else
#define AJ_DumpBytes(tag, data, len)
#endif

#if AJ_DEBUG_RESTRICT >= AJ_DEBUG_DUMP
/**
 * Print a human readable summary of a message.  Message dumps messages may be
 * suppressed by AJ_DEBUG_RESTRICT or by the module selection (global memory
 * value or shell environment variable) mechanism.
 *
 * @param msg  A format string and arguments
 */
#define AJ_DumpMsg(tag, msg, body) \
    do { \
        if (MKVAR(dbg, AJ_MODULE) || _AJ_DbgEnabled(STR(AJ_MODULE))) { _AJ_DumpMsg(tag, msg, body); } \
    } while (0)
#else
#define AJ_DumpMsg(tag, msg, body)
#endif

#else

#define AJ_DumpMsg(tag, msg, body)
#define AJ_DumpBytes(tag, data, len)
#define AJ_ErrPrintf(_msg)
#define AJ_WarnPrintf(_msg)
#define AJ_InfoPrintf(_msg)

#endif

/**
 * Utility function that converts numerical status to a readable string
 *
 * @param status  A status code
 */
AJ_EXPORT const char* AJ_StatusText(AJ_Status status);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
