#ifndef _RESOURCESTRUCTURES_H_
#define _RESOURCESTRUCTURES_H_

#include <cstdint>
#include "EnumFlags.h"
#include "Utilities.h"

namespace NETMF
{
    namespace Graphics
    {
        enum class BitmapImageType : uint8_t
        {
            TinyCLRBitmap,  // NETMF specific bitmap format
            Gif,
            JPeg,
            Bmp, // Windows BMP format
        };

        enum class BitmapDescriptorFlags : uint16_t
        {
                  None = 0,
              ReadOnly = 0x0001,
            Compressed = 0x0002
        };
        ENUM_FLAGS( BitmapDescriptorFlags )

        struct BitmapDescriptor
        {
            static uint32_t const MaxWidth = 0x7ffff;
            static uint32_t const MaxHeight = 0xFFFF;

            static uint8_t const NativeBpp = 0;

            uint32_t Width;
            uint32_t Height;
            uint16_t Flags;
            uint8_t BitsPerPixel;
            uint8_t ImageType;
        };
        ASSERT_STRUCT_IS_POD( BitmapDescriptor )
        static_assert( sizeof( BitmapDescriptor ) == sizeof( std::uint32_t ) * 3, "Size mismatch" );

        struct FontMetrics
        {
            uint16_t Height;
            int16_t Offset; // The bitmap may be smaller than the logical font height.
            int16_t Ascent;
            int16_t Descent;

            int16_t InternalLeading;
            int16_t ExternalLeading;

            int16_t AvgCharWidth;
            int16_t MaxCharWidth;
        };
        ASSERT_STRUCT_IS_POD( FontMetrics )

        enum FontDescriptorFlags : uint16_t
        {
                     None = 0x0000,
                     Bold = 0x0001,
                   Italic = 0x0002,
                Underline = 0x0004,
                   FontEx = 0x0008,
            AntiAliasMask = 0x00F0
        };
        ENUM_FLAGS( FontDescriptorFlags )
        uint16_t const FontDescriptionFlagsAntiAliasShift = 4;

        struct FontDescriptor
        {
            FontMetrics Metrics;
            uint16_t Ranges;
            uint16_t Characters;
            FontDescriptorFlags Flags;
            uint16_t Pad;
        };
        ASSERT_STRUCT_IS_POD( FontDescriptor )
    }
}
#endif
