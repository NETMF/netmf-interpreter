// tinyclr - JPEG Configuration file

// <Hacks from CE team>
/* Kill any definition of FAR - else we get a warning when it is redefined
	below. */
#ifdef FAR
	#undef FAR
#endif

/* boolean is defined in rpcndr.h, we can't define it the same way here
	(unsigned char) because a == b is passed as an argument to a function
	declared as taking a boolean, and MSVC treats this as warnable (hence
	errorable)!  The hack is to hide the IJG definition. */
#define boolean jpeg_boolean
// </Hacks from CE team>

#define HAVE_PROTOTYPES
#define HAVE_UNSIGNED_CHAR
#define HAVE_UNSIGNED_SHORT

#undef CHAR_IS_UNSIGNED

#define HAVE_STDDEF_H

#undef HAVE_STDLIB_H
#undef NEED_BSD_STRINGS
#undef NEED_SYS_TYPES_H
#undef NEED_FAR_POINTERS
#undef NEED_SHORT_EXTERNAL_NAMES
#undef INCOMPLETE_TYPES_BROKEN


#ifdef JPEG_INTERNALS
#undef RIGHT_SHIFT_IS_UNSIGNED
#endif 


#define JDCT_DEFAULT JDCT_IFAST
#define NO_GETENV // to prevent jmemmgr.c from attempting to get enviornment variables
