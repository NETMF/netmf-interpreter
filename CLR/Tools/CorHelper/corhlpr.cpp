////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include "stdafx.h"
#include "stdafx.h"

#if 0
#ifdef _BLD_CLR
#include "utilcode.h"
#endif
#include "corhlpr.h"
#include <stdlib.h>
#endif 

/*************************************************************************************
*
* implementation of CQuickMemoryBase
*
*************************************************************************************/

template <DWORD SIZE, DWORD INCREMENT>
HRESULT CQuickMemoryBase <SIZE, INCREMENT>::ReSizeNoThrow(SIZE_T iItems)
{
#ifdef _BLD_CLR
#ifdef _DEBUG
    // Exercise heap for OOM-fault injection purposes
    // But we can't do this if current thread suspends EE
    if (!IsSuspendEEThread ())
    {
        BYTE *pTmp = NEW_NOTHROW(iItems);
        if (!pTmp)
        {
            return E_OUTOFMEMORY;
        }
        delete pTmp;
    }
#endif
#endif
    BYTE *pbBuffNew;
    if (iItems <= cbTotal)
    {
        iSize = iItems;
        return NOERROR;
    }
    
#ifdef _BLD_CLR
    // not allowed to do allocation if current thread suspends EE
    if (IsSuspendEEThread ())
        return E_OUTOFMEMORY;
#endif
    pbBuffNew = NEW_NOTHROW(iItems + INCREMENT);
    if (!pbBuffNew)
        return E_OUTOFMEMORY;
    if (pbBuff)
    {
        memcpy(pbBuffNew, pbBuff, cbTotal);
        delete [] pbBuff;
    }
    else
    {
        _ASSERTE(cbTotal == SIZE);
        memcpy(pbBuffNew, rgData, cbTotal);
    }
    cbTotal = iItems + INCREMENT;
    iSize = iItems;
    pbBuff = pbBuffNew;
    return NOERROR;
}


/*************************************************************************************
*
* get number of bytes consumed by one argument/return type
*
*************************************************************************************/
#define CHECK_REMAINDER  if(cbTotal >= cbTotalMax){hr=E_FAIL; goto ErrExit;}
HRESULT _CountBytesOfOneArg(
    PCCOR_SIGNATURE pbSig,
    ULONG       *pcbTotal)  // Initially, *pcbTotal contains the remaining size of the sig blob
{
    ULONG       cb;
    ULONG       cbTotal=0;
    ULONG       cbTotalMax;
    CorElementType ulElementType;
    ULONG       ulData;
    ULONG       ulTemp;
    int         iData;
    mdToken     tk;
    ULONG       cArg;
    ULONG       callingconv;
    ULONG       cArgsIndex;
    HRESULT     hr = NOERROR;

    if(pcbTotal==NULL) return E_FAIL;
    cbTotalMax = *pcbTotal;

    CHECK_REMAINDER;
    cbTotal = CorSigUncompressElementType(pbSig, &ulElementType);
    while (CorIsModifierElementType((CorElementType) ulElementType))
    {
        CHECK_REMAINDER;
        cbTotal += CorSigUncompressElementType(&pbSig[cbTotal], &ulElementType);
    }
    switch (ulElementType)
    {
        case ELEMENT_TYPE_SZARRAY:
        case 0x1e /* obsolete */:
            // skip over base type
            CHECK_REMAINDER;
            cb = cbTotalMax - cbTotal;
            IfFailGo( _CountBytesOfOneArg(&pbSig[cbTotal], &cb) );
            cbTotal += cb;
            break;

        case ELEMENT_TYPE_FNPTR:
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressData (&pbSig[cbTotal], &callingconv);

            // remember number of bytes to represent the arg counts
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressData (&pbSig[cbTotal], &cArg);

            // how many bytes to represent the return type
            CHECK_REMAINDER;
            cb = cbTotalMax - cbTotal;
            IfFailGo( _CountBytesOfOneArg( &pbSig[cbTotal], &cb) );
            cbTotal += cb;

            // loop through argument
            for (cArgsIndex = 0; cArgsIndex < cArg; cArgsIndex++)
            {
                CHECK_REMAINDER;
                cb = cbTotalMax - cbTotal;
                IfFailGo( _CountBytesOfOneArg( &pbSig[cbTotal], &cb) );
                cbTotal += cb;
            }

            break;

        case ELEMENT_TYPE_ARRAY:
            // syntax : ARRAY BaseType <rank> [i size_1... size_i] [j lowerbound_1 ... lowerbound_j]

            // skip over base type
            CHECK_REMAINDER;
            cb = cbTotalMax - cbTotal;
            IfFailGo( _CountBytesOfOneArg(&pbSig[cbTotal], &cb) );
            cbTotal += cb;

            // Parse for the rank
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressData(&pbSig[cbTotal], &ulData);

            // if rank == 0, we are done
            if (ulData == 0)
                break;

            // any size of dimension specified?
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressData(&pbSig[cbTotal], &ulData);
            while (ulData--)
            {
                CHECK_REMAINDER;
                cbTotal += CorSigUncompressData(&pbSig[cbTotal], &ulTemp);
            }

            // any lower bound specified?
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressData(&pbSig[cbTotal], &ulData);

            while (ulData--)
            {
                CHECK_REMAINDER;
                cbTotal += CorSigUncompressSignedInt(&pbSig[cbTotal], &iData);
            }

            break;
        case ELEMENT_TYPE_VALUETYPE:
        case ELEMENT_TYPE_CLASS:
        case ELEMENT_TYPE_CMOD_REQD:
        case ELEMENT_TYPE_CMOD_OPT:
            // count the bytes for the token compression
            CHECK_REMAINDER;
            cbTotal += CorSigUncompressToken(&pbSig[cbTotal], &tk);
            if ( ulElementType == ELEMENT_TYPE_CMOD_REQD ||
                 ulElementType == ELEMENT_TYPE_CMOD_OPT)
            {
                // skip over base type
                CHECK_REMAINDER;
                cb = cbTotalMax - cbTotal;
                IfFailGo( _CountBytesOfOneArg(&pbSig[cbTotal], &cb) );
                cbTotal += cb;
            }
            break;
        default:
            break;
    }

    *pcbTotal = cbTotal;
ErrExit:
    return hr;
}
#undef CHECK_REMAINDER

//*****************************************************************************
// copy fixed part of VarArg signature to a buffer
//*****************************************************************************
HRESULT _GetFixedSigOfVarArg(           // S_OK or error.
    PCCOR_SIGNATURE pvSigBlob,          // [IN] point to a blob of COM+ method signature
    ULONG   cbSigBlob,                  // [IN] size of signature
    CQuickBytes *pqbSig,                // [OUT] output buffer for fixed part of VarArg Signature
    ULONG   *pcbSigBlob)                // [OUT] number of bytes written to the above output buffer
{
    HRESULT     hr = NOERROR;
    ULONG       cbCalling;
    ULONG       cbArgsNumber;           // number of bytes to store the original arg count
    ULONG       cbArgsNumberTemp;       // number of bytes to store the fixed arg count
    ULONG       cbTotal = 0;            // total of number bytes for return type + all fixed arguments
    ULONG       cbCur = 0;              // index through the pvSigBlob
    ULONG       cb;
    ULONG       cArg;
    ULONG       callingconv;
    ULONG       cArgsIndex;
    CorElementType ulElementType;
    BYTE        *pbSig;

    _ASSERTE (pvSigBlob && pcbSigBlob);

    // remember the number of bytes to represent the calling convention
    cbCalling = CorSigUncompressData (pvSigBlob, &callingconv);
    if (cbCalling == ((ULONG)(-1)))
    {
        return E_INVALIDARG;
    }
    _ASSERTE (isCallConv(callingconv, IMAGE_CEE_CS_CALLCONV_VARARG));
    cbCur += cbCalling;

    // remember number of bytes to represent the arg counts
    cbArgsNumber= CorSigUncompressData (&pvSigBlob[cbCur], &cArg);
    if (cbArgsNumber == ((ULONG)(-1)))
    {
        return E_INVALIDARG;
    }
    
    cbCur += cbArgsNumber;

    // how many bytes to represent the return type
    cb = cbSigBlob-cbCur;
    IfFailGo( _CountBytesOfOneArg( &pvSigBlob[cbCur], &cb) );
    cbCur += cb;
    cbTotal += cb;

    // loop through argument until we found ELEMENT_TYPE_SENTINEL or run
    // out of arguments
    for (cArgsIndex = 0; cArgsIndex < cArg; cArgsIndex++)
    {
        _ASSERTE(cbCur < cbSigBlob);

        // peak the outter most ELEMENT_TYPE_*
        CorSigUncompressElementType (&pvSigBlob[cbCur], &ulElementType);
        if (ulElementType == ELEMENT_TYPE_SENTINEL)
            break;
        cb = cbSigBlob-cbCur;
        IfFailGo( _CountBytesOfOneArg( &pvSigBlob[cbCur], &cb) );
        cbTotal += cb;
        cbCur += cb;
    }

    cbArgsNumberTemp = CorSigCompressData(cArgsIndex, &cArg);

    // now cbCalling : the number of bytes needed to store the calling convention
    // cbArgNumberTemp : number of bytes to store the fixed arg count
    // cbTotal : the number of bytes to store the ret and fixed arguments

    *pcbSigBlob = cbCalling + cbArgsNumberTemp + cbTotal;

    // resize the buffer
    IfFailGo( pqbSig->ReSizeNoThrow(*pcbSigBlob) );
    pbSig = (BYTE *)pqbSig->Ptr();

    // copy over the calling convention
    cb = CorSigCompressData(callingconv, pbSig);

    // copy over the fixed arg count
    cbArgsNumberTemp = CorSigCompressData(cArgsIndex, &pbSig[cb]);

    // copy over the fixed args + ret type
    memcpy(&pbSig[cb + cbArgsNumberTemp], &pvSigBlob[cbCalling + cbArgsNumber], cbTotal);

ErrExit:
    return hr;
}





//*****************************************************************************
//
//***** File format helper classes
//
//*****************************************************************************

extern "C" {

/***************************************************************************/
/* Note that this construtor does not set the LocalSig, but has the
   advantage that it does not have any dependancy on EE structures.
   inside the EE use the FunctionDesc constructor */

void __stdcall DecoderInit(void * pThis, COR_ILMETHOD* header)
{
    COR_ILMETHOD_DECODER* decoder = (COR_ILMETHOD_DECODER*)pThis;

    memset(decoder, 0, sizeof(COR_ILMETHOD_DECODER));
    if (header->Tiny.IsTiny()) {
        decoder->SetMaxStack(header->Tiny.GetMaxStack());
        decoder->Code = header->Tiny.GetCode();
        decoder->SetCodeSize(header->Tiny.GetCodeSize());
        decoder->SetFlags(CorILMethod_TinyFormat);
        return;
    }
    if (header->Fat.IsFat()) {
        if((((size_t) header) & 3) == 0)        // header is aligned
        {
            *((COR_ILMETHOD_FAT*) decoder) = header->Fat;
            decoder->Code = header->Fat.GetCode();
            if(header->Fat.GetSize() >= 3)        // Size if valid
            {
                decoder->Sect = header->Fat.GetSect();
                if (decoder->Sect != 0 && decoder->Sect->Kind() == CorILMethod_Sect_EHTable) {
                    decoder->EH = (COR_ILMETHOD_SECT_EH*) decoder->Sect;
                    decoder->Sect = decoder->Sect->Next();
                }
            }
        }
        return;
    }
    // so we don't asert on trash  _ASSERTE(!"Unknown format");
}

// Calculate the total method size. First get address of end of code. If there are no sections, then
// the end of code addr marks end of COR_ILMETHOD. Otherwise find addr of end of last section and use it
// to mark end of COR_ILMETHD. Assumes that the code is directly followed
// by each section in the on-disk format
int __stdcall DecoderGetOnDiskSize(void * pThis, COR_ILMETHOD* header)
{
    COR_ILMETHOD_DECODER* decoder = (COR_ILMETHOD_DECODER*)pThis;

    if (!decoder->Code)
        return 0;
    
    BYTE *lastAddr = (BYTE*)decoder->Code + decoder->GetCodeSize();    // addr of end of code
    const COR_ILMETHOD_SECT *sect = decoder->EH;
    if (sect != 0 && sect->Next() == 0)
        lastAddr = (BYTE *)(&sect->Data()[sect->DataSize()]);
    else
    {
        const COR_ILMETHOD_SECT *nextSect;
        for (sect = decoder->Sect; sect; sect = nextSect) {
            nextSect = sect->Next();
            if (nextSect == 0) {
                // sect points to the last section, so set lastAddr
                lastAddr = (BYTE *)(&sect->Data()[sect->DataSize()]);
                break;
            }
        }
    }
    return (int)(lastAddr - (BYTE*)header);
}

/*********************************************************************/
/* APIs for emitting sections etc */

unsigned __stdcall IlmethodSize(COR_ILMETHOD_FAT* header, BOOL moreSections)
{
    if (header->GetMaxStack() <= 8 && (header->GetFlags() & ~CorILMethod_FormatMask) == 0
        && header->GetLocalVarSigTok() == 0 && header->GetCodeSize() < 64 && !moreSections)
        return(sizeof(COR_ILMETHOD_TINY));

    return(sizeof(COR_ILMETHOD_FAT));
}

/*********************************************************************/
        // emit the header (bestFormat) return amount emitted
unsigned __stdcall IlmethodEmit(unsigned size, COR_ILMETHOD_FAT* header,
                  BOOL moreSections, BYTE* outBuff)
{
#ifdef _DEBUG
    BYTE* origBuff = outBuff;
#endif
    if (size == 1) {
            // Tiny format
        *outBuff++ = (BYTE) (CorILMethod_TinyFormat | (header->GetCodeSize() << 2));
    }
    else {
            // Fat format
        _ASSERTE((((size_t) outBuff) & 3) == 0);               // header is dword aligned
        COR_ILMETHOD_FAT* fatHeader = (COR_ILMETHOD_FAT*) outBuff;
        outBuff += sizeof(COR_ILMETHOD_FAT);
        *fatHeader = *header;
        fatHeader->SetFlags(fatHeader->GetFlags() | CorILMethod_FatFormat);
        _ASSERTE((fatHeader->GetFlags() & CorILMethod_FormatMask) == CorILMethod_FatFormat);
        if (moreSections)
            fatHeader->SetFlags(fatHeader->GetFlags() | CorILMethod_MoreSects);
        fatHeader->SetSize(sizeof(COR_ILMETHOD_FAT) / 4);
    }
    _ASSERTE(&origBuff[size] == outBuff);
    return(size);
}

/*********************************************************************/
/* static */
IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT* __stdcall SectEH_EHClause(void *pSectEH, unsigned idx, IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT* buff)
{
    if (((COR_ILMETHOD_SECT_EH *)pSectEH)->IsFat())
        return(&(((COR_ILMETHOD_SECT_EH *)pSectEH)->Fat.Clauses[idx]));

    COR_ILMETHOD_SECT_EH_CLAUSE_FAT* fatClause = (COR_ILMETHOD_SECT_EH_CLAUSE_FAT*)buff;
    COR_ILMETHOD_SECT_EH_CLAUSE_SMALL* smallClause = (COR_ILMETHOD_SECT_EH_CLAUSE_SMALL*)&((COR_ILMETHOD_SECT_EH *)pSectEH)->Small.Clauses[idx];

    // mask to remove sign extension - cast just wouldn't work
    fatClause->SetFlags((CorExceptionFlag)(smallClause->GetFlags()&0x0000ffff));
    fatClause->SetClassToken(smallClause->GetClassToken());
    fatClause->SetTryOffset(smallClause->GetTryOffset());
    fatClause->SetTryLength(smallClause->GetTryLength());
    fatClause->SetHandlerLength(smallClause->GetHandlerLength());
    fatClause->SetHandlerOffset(smallClause->GetHandlerOffset());
    return(buff);
}
/*********************************************************************/
        // compute the size of the section (best format)
        // codeSize is the size of the method
    // deprecated
unsigned __stdcall SectEH_SizeWithCode(unsigned ehCount, unsigned codeSize)
{
    return((ehCount)? SectEH_SizeWorst(ehCount) : 0);
}

    // will return worse-case size and then Emit will return actual size
unsigned __stdcall SectEH_SizeWorst(unsigned ehCount)
{
    return((ehCount)? (COR_ILMETHOD_SECT_EH_FAT::Size(ehCount)) : 0);
}

    // will return exact size which will match the size returned by Emit
unsigned __stdcall SectEH_SizeExact(unsigned ehCount, IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT* clauses)
{
    if (ehCount == 0)
        return(0);

    unsigned smallSize = COR_ILMETHOD_SECT_EH_SMALL::Size(ehCount);
    if (smallSize > COR_ILMETHOD_SECT_SMALL_MAX_DATASIZE)
            return(COR_ILMETHOD_SECT_EH_FAT::Size(ehCount));
    for (unsigned i = 0; i < ehCount; i++) {
        COR_ILMETHOD_SECT_EH_CLAUSE_FAT* fatClause = (COR_ILMETHOD_SECT_EH_CLAUSE_FAT*)&clauses[i];
        if (fatClause->GetTryOffset() > 0xFFFF ||
                fatClause->GetTryLength() > 0xFF ||
                fatClause->GetHandlerOffset() > 0xFFFF ||
                fatClause->GetHandlerLength() > 0xFF) {
            return(COR_ILMETHOD_SECT_EH_FAT::Size(ehCount));
        }
    }
    return smallSize;
}

/*********************************************************************/

        // emit the section (best format);
unsigned __stdcall SectEH_Emit(unsigned size, unsigned ehCount,
                  IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT* clauses,
                  BOOL moreSections, BYTE* outBuff,
                  ULONG* ehTypeOffsets)
{
    if (size == 0)
       return(0);

    _ASSERTE((((size_t) outBuff) & 3) == 0);               // header is dword aligned
    BYTE* origBuff = outBuff;
    if (ehCount <= 0)
        return 0;

    // Initialize the ehTypeOffsets array.
    if (ehTypeOffsets)
    {
        for (unsigned int i = 0; i < ehCount; i++)
            ehTypeOffsets[i] = (ULONG) -1;
    }

    if (COR_ILMETHOD_SECT_EH_SMALL::Size(ehCount) < COR_ILMETHOD_SECT_SMALL_MAX_DATASIZE) {
        COR_ILMETHOD_SECT_EH_SMALL* EHSect = (COR_ILMETHOD_SECT_EH_SMALL*) outBuff;
        unsigned i;
        for (i = 0; i < ehCount; i++) {
            COR_ILMETHOD_SECT_EH_CLAUSE_FAT* fatClause = (COR_ILMETHOD_SECT_EH_CLAUSE_FAT*)&clauses[i];
            if (fatClause->GetTryOffset() > 0xFFFF ||
                    fatClause->GetTryLength() > 0xFF ||
                    fatClause->GetHandlerOffset() > 0xFFFF ||
                    fatClause->GetHandlerLength() > 0xFF) {
                break;  // fall through and generate as FAT
            }
            _ASSERTE((fatClause->GetFlags() & ~0xFFFF) == 0);
            _ASSERTE((fatClause->GetTryOffset() & ~0xFFFF) == 0);
            _ASSERTE((fatClause->GetTryLength() & ~0xFF) == 0);
            _ASSERTE((fatClause->GetHandlerOffset() & ~0xFFFF) == 0);
            _ASSERTE((fatClause->GetHandlerLength() & ~0xFF) == 0);

            COR_ILMETHOD_SECT_EH_CLAUSE_SMALL* smallClause = (COR_ILMETHOD_SECT_EH_CLAUSE_SMALL*)&EHSect->Clauses[i];
            smallClause->SetFlags((CorExceptionFlag) fatClause->GetFlags());
            smallClause->SetTryOffset(fatClause->GetTryOffset());
            smallClause->SetTryLength(fatClause->GetTryLength());
            smallClause->SetHandlerOffset(fatClause->GetHandlerOffset());
            smallClause->SetHandlerLength(fatClause->GetHandlerLength());
            smallClause->SetClassToken(fatClause->GetClassToken());
        }
        if (i >= ehCount) {
            // if actually got through all the clauses and they are small enough
            EHSect->Kind = CorILMethod_Sect_EHTable;
            if (moreSections)
                EHSect->Kind |= CorILMethod_Sect_MoreSects;
            EHSect->DataSize = EHSect->Size(ehCount);
            EHSect->Reserved = 0;
            _ASSERTE(EHSect->DataSize == EHSect->Size(ehCount)); // make sure didn't overflow
            outBuff = (BYTE*) &EHSect->Clauses[ehCount];
            // Set the offsets for the exception type tokens.
            if (ehTypeOffsets)
            {
                for (i = 0; i < ehCount; i++) {
                    COR_ILMETHOD_SECT_EH_CLAUSE_SMALL* smallClause = (COR_ILMETHOD_SECT_EH_CLAUSE_SMALL*)&EHSect->Clauses[i];
                    if (smallClause->GetFlags() == COR_ILEXCEPTION_CLAUSE_NONE)
                    {
                        _ASSERTE(! IsNilToken(smallClause->GetClassToken()));
                        ehTypeOffsets[i] = (ULONG)((BYTE *)&smallClause->ClassToken - origBuff);
                    }
                }
            }
            return(size);
        }
    }
    // either total size too big or one of constituent elements too big (eg. offset or length)
    COR_ILMETHOD_SECT_EH_FAT* EHSect = (COR_ILMETHOD_SECT_EH_FAT*) outBuff;
    EHSect->SetKind(CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat);
    if (moreSections)
        EHSect->SetKind(EHSect->GetKind() | CorILMethod_Sect_MoreSects);

    EHSect->SetDataSize(EHSect->Size(ehCount));
    memcpy(EHSect->Clauses, clauses, ehCount * sizeof(COR_ILMETHOD_SECT_EH_CLAUSE_FAT));
    outBuff = (BYTE*) &EHSect->Clauses[ehCount];
    _ASSERTE(&origBuff[size] == outBuff);
    // Set the offsets for the exception type tokens.
    if (ehTypeOffsets)
    {
        for (unsigned int i = 0; i < ehCount; i++) {
            COR_ILMETHOD_SECT_EH_CLAUSE_FAT* fatClause = (COR_ILMETHOD_SECT_EH_CLAUSE_FAT*)&EHSect->Clauses[i];
            if (fatClause->GetFlags() == COR_ILEXCEPTION_CLAUSE_NONE)
            {
                _ASSERTE(! IsNilToken(fatClause->GetClassToken()));
                ehTypeOffsets[i] = (ULONG)((BYTE *)&fatClause->ClassToken - origBuff);
            }
        }
    }
    return(size);
}

} // extern "C"


