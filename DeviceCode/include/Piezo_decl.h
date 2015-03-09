////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_PIEZO_DECL_H_
#define _DRIVERS_PIEZO_DECL_H_ 1

//--//

//
// The intervals, or ratios between the frequencies of two notes, in the Western musical scale are called by these names:
//
// Name:            Frequency   Distance in
//                  ratio:      semitones:
// Unison            1:1            0
// Minor second     16:15           1
// Major second      9:8            2
// Minor third       6:5            3
// Major third       5:4            4
// Perfect fourth    4:3            5
// Augmented fourth 45:32 or 25:18  6
// Diminished fifth 64:45 or 36:25  6
// Perfect fifth     3:2            7
// Major sixth       8:5            8
// Minor sixth       5:3            9
// Major seventh    16:9           10
// Minor seventh    15:8           11
// Octave            2:1           12
//
// However, this isn't feasible for all scales from all notes,
//
// so it is approximated by:
//
//   P = 440 * 2^(n / 12)
//
//

#define TONE_A    880
#define TONE_As   932
#define TONE_B    988
#define TONE_C   1047
#define TONE_Cs  1109
#define TONE_D   1175
#define TONE_Ds  1245
#define TONE_Ef  1245
#define TONE_E   1319
#define TONE_F   1397
#define TONE_Fs  1480
#define TONE_G   1568
#define TONE_Gs  1661
#define TONE_Af  1661
#define TONE_A2  1760

// normalized for 60
#define TONE_WHOLE      1000
#define TONE_HALF        500
#define TONE_QUARTER     250
#define TONE_EIGHTH      125

// this value, when passed to Piezo_Tone in the Frequency_Hertz parameter clears the buffer and stops the current
// tone from playing

#define TONE_CLEAR_BUFFER   0xFFFFFFFF

//--//

void Piezo_Initialize  (                                                      );
void Piezo_Uninitialize(                                                      );
BOOL Piezo_Tone        ( UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds );
BOOL Piezo_IsEnabled   (                                                      );

//--//

#endif // _DRIVERS_PIEZO_DECL_H_
