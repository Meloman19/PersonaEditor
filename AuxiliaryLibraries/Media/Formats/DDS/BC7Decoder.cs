using System;
using System.Diagnostics;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    /*
     * https://github.com/nickbabcock/Pfim
     */
    internal static class BC7Decoder
    {
        internal static class Constants
        {
            public const int BC6H_MAX_REGIONS = 2;
            public const int BC6H_MAX_INDICES = 16;
            public const int BC7_MAX_REGIONS = 3;
            public const int BC7_MAX_INDICES = 16;

            public const ushort F16S_MASK = 0x8000;   // f16 sign mask
            public const ushort F16EM_MASK = 0x7fff;   // f16 exp & mantissa mask
            public const ushort F16MAX = 0x7bff;   // MAXFLT bit pattern for XMHALF

            public const int BC6H_NUM_CHANNELS = 3;
            public const int BC6H_MAX_SHAPES = 32;

            public const int BC7_NUM_CHANNELS = 4;
            public const int BC7_MAX_SHAPES = 64;

            public const int BC67_WEIGHT_MAX = 64;
            public const int BC67_WEIGHT_SHIFT = 6;
            public const int BC67_WEIGHT_ROUND = 32;

            public const float fEpsilon = (0.25f / 64.0f) * (0.25f / 64.0f);
            public static readonly float[] pC3 = { 2.0f / 2.0f, 1.0f / 2.0f, 0.0f / 2.0f };
            public static readonly float[] pD3 = { 0.0f / 2.0f, 1.0f / 2.0f, 2.0f / 2.0f };
            public static readonly float[] pC4 = { 3.0f / 3.0f, 2.0f / 3.0f, 1.0f / 3.0f, 0.0f / 3.0f };
            public static readonly float[] pD4 = { 0.0f / 3.0f, 1.0f / 3.0f, 2.0f / 3.0f, 3.0f / 3.0f };

            // Partition, Shape, Pixel (index into 4x4 block)
            public static readonly byte[][][] g_aPartitionTable = new byte[3][][]
        {
        new byte[64][] {   // 1 Region case has no subsets (all 0)
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        },

        new byte[64][] {   // BC6H/BC7 Partition Set for 2 Subsets
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 0
            new byte[16] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 }, // Shape 1
            new byte[16] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, // Shape 2
            new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 4
            new byte[16] { 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 5
            new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 7
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 8
            new byte[16] { 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 9
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 10
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1 }, // Shape 11
            new byte[16] { 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 12
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 13
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 14
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 15
            new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1 }, // Shape 16
            new byte[16] { 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 17
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 18
            new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 19
            new byte[16] { 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 20
            new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 21
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 22
            new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1 }, // Shape 23
            new byte[16] { 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 24
            new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 25
            new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0 }, // Shape 26
            new byte[16] { 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0 }, // Shape 27
            new byte[16] { 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0 }, // Shape 28
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 29
            new byte[16] { 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 30
            new byte[16] { 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 31

                                                                // BC7 Partition Set for 2 Subsets (second-half)
            new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, // Shape 32
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 33
            new byte[16] { 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0 }, // Shape 34
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0 }, // Shape 35
            new byte[16] { 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0 }, // Shape 36
            new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0 }, // Shape 37
            new byte[16] { 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1 }, // Shape 38
            new byte[16] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1 }, // Shape 39
            new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 40
            new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0 }, // Shape 41
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0 }, // Shape 42
            new byte[16] { 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0 }, // Shape 43
            new byte[16] { 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0 }, // Shape 44
            new byte[16] { 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1 }, // Shape 45
            new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1 }, // Shape 46
            new byte[16] { 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0 }, // Shape 47
            new byte[16] { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, // Shape 48
            new byte[16] { 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0 }, // Shape 49
            new byte[16] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0 }, // Shape 50
            new byte[16] { 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0 }, // Shape 51
            new byte[16] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1 }, // Shape 52
            new byte[16] { 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 53
            new byte[16] { 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 54
            new byte[16] { 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0 }, // Shape 55
            new byte[16] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 56
            new byte[16] { 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1 }, // Shape 57
            new byte[16] { 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1 }, // Shape 58
            new byte[16] { 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1 }, // Shape 59
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 60
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 61
            new byte[16] { 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 }, // Shape 62
            new byte[16] { 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1 }  // Shape 63
        },

        new byte[64][] {   // BC7 Partition Set for 3 Subsets
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2 }, // Shape 0
            new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 1
            new byte[16] { 0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 2
            new byte[16] { 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 4
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2 }, // Shape 5
            new byte[16] { 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 7
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 8
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 9
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 10
            new byte[16] { 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2 }, // Shape 11
            new byte[16] { 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2 }, // Shape 12
            new byte[16] { 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 13
            new byte[16] { 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 14
            new byte[16] { 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0 }, // Shape 15
            new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2 }, // Shape 16
            new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0 }, // Shape 17
            new byte[16] { 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 18
            new byte[16] { 0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1 }, // Shape 19
            new byte[16] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2 }, // Shape 20
            new byte[16] { 0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1 }, // Shape 21
            new byte[16] { 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 22
            new byte[16] { 0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0 }, // Shape 23
            new byte[16] { 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0 }, // Shape 24
            new byte[16] { 0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2 }, // Shape 25
            new byte[16] { 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0 }, // Shape 26
            new byte[16] { 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1 }, // Shape 27
            new byte[16] { 0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2 }, // Shape 28
            new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2 }, // Shape 29
            new byte[16] { 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1 }, // Shape 30
            new byte[16] { 0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 31
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 32
            new byte[16] { 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1 }, // Shape 33
            new byte[16] { 0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2 }, // Shape 34
            new byte[16] { 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0 }, // Shape 35
            new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0 }, // Shape 36
            new byte[16] { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0 }, // Shape 37
            new byte[16] { 0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0 }, // Shape 38
            new byte[16] { 0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1 }, // Shape 39
            new byte[16] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1 }, // Shape 40
            new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 41
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 42
            new byte[16] { 0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2 }, // Shape 43
            new byte[16] { 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1 }, // Shape 44
            new byte[16] { 0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1 }, // Shape 45
            new byte[16] { 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1 }, // Shape 46
            new byte[16] { 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 47
            new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2 }, // Shape 48
            new byte[16] { 0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1 }, // Shape 49
            new byte[16] { 0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2 }, // Shape 50
            new byte[16] { 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 51
            new byte[16] { 0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2 }, // Shape 52
            new byte[16] { 0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2 }, // Shape 53
            new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2 }, // Shape 54
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 55
            new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 56
            new byte[16] { 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2 }, // Shape 57
            new byte[16] { 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2 }, // Shape 58
            new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2 }, // Shape 59
            new byte[16] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1 }, // Shape 60
            new byte[16] { 0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2 }, // Shape 61
            new byte[16] { 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 62
            new byte[16] { 0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0 }  // Shape 63
        }
        };

            // Partition, Shape, Fixup
            public static readonly byte[][][] g_aFixUp = new byte[3][][]
            {
        new byte[64][] {   // No fix-ups for 1st subset for BC6H or BC7
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 }
        },

        new byte[64][] {   // BC6H/BC7 Partition Set Fixups for 2 Subsets
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },
            new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 8, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
            new byte[] { 0, 8, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },

            // BC7 Partition Set Fixups for 2 Subsets (second-half)
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 6, 0 },new byte[] { 0, 8, 0 },
            new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
            new byte[] { 0, 2, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 6, 0 },
            new byte[] { 0, 6, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 6, 0 },new byte[] { 0, 8, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
            new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },new byte[] { 0,15, 0 }
        },

        new byte[64][] {   // BC7 Partition Set Fixups for 3 Subsets
            new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0,15, 8 },new byte[] { 0,15, 3 },
            new byte[] { 0, 8,15 },new byte[] { 0, 3,15 },new byte[] { 0,15, 3 },new byte[] { 0,15, 8 },
            new byte[] { 0, 8,15 },new byte[] { 0, 8,15 },new byte[] { 0, 6,15 },new byte[] { 0, 6,15 },
            new byte[] { 0, 6,15 },new byte[] { 0, 5,15 },new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },
            new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0, 8,15 },new byte[] { 0,15, 3 },
            new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0, 6,15 },new byte[] { 0,10, 8 },
            new byte[] { 0, 5, 3 },new byte[] { 0, 8,15 },new byte[] { 0, 8, 6 },new byte[] { 0, 6,10 },
            new byte[] { 0, 8,15 },new byte[] { 0, 5,15 },new byte[] { 0,15,10 },new byte[] { 0,15, 8 },
            new byte[] { 0, 8,15 },new byte[] { 0,15, 3 },new byte[] { 0, 3,15 },new byte[] { 0, 5,10 },
            new byte[] { 0, 6,10 },new byte[] { 0,10, 8 },new byte[] { 0, 8, 9 },new byte[] { 0,15,10 },
            new byte[] { 0,15, 6 },new byte[] { 0, 3,15 },new byte[] { 0,15, 8 },new byte[] { 0, 5,15 },
            new byte[] { 0,15, 3 },new byte[] { 0,15, 6 },new byte[] { 0,15, 6 },new byte[] { 0,15, 8 },
            new byte[] { 0, 3,15 },new byte[] { 0,15, 3 },new byte[] { 0, 5,15 },new byte[] { 0, 5,15 },
            new byte[] { 0, 5,15 },new byte[] { 0, 8,15 },new byte[] { 0, 5,15 },new byte[] { 0,10,15 },
            new byte[] { 0, 5,15 },new byte[] { 0,10,15 },new byte[] { 0, 8,15 },new byte[] { 0,13,15 },
            new byte[] { 0,15, 3 },new byte[] { 0,12,15 },new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 }
        }
            };

            public static readonly int[] g_aWeights2 = { 0, 21, 43, 64 };
            public static readonly int[] g_aWeights3 = { 0, 9, 18, 27, 37, 46, 55, 64 };
            public static readonly int[] g_aWeights4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };
        }

        internal class LDRColorA
        {
            public byte r, g, b, a;

            public LDRColorA() { }
            public LDRColorA(byte _r, byte _g, byte _b, byte _a)
            {
                r = _r;
                g = _g;
                b = _b;
                a = _a;
            }

            public ref byte this[int uElement]
            {
                get
                {
                    switch (uElement)
                    {
                        case 0: return ref r;
                        case 1: return ref g;
                        case 2: return ref b;
                        case 3: return ref a;
                        default: Debug.Assert(false); return ref r;
                    }
                }
            }

            public static void InterpolateRGB(LDRColorA c0, LDRColorA c1, int wc, int wcprec, LDRColorA outt)
            {
                int[] aWeights = null;
                switch (wcprec)
                {
                    case 2: aWeights = Constants.g_aWeights2; Debug.Assert(wc < 4); break;
                    case 3: aWeights = Constants.g_aWeights3; Debug.Assert(wc < 8); break;
                    case 4: aWeights = Constants.g_aWeights4; Debug.Assert(wc < 16); break;
                    default: Debug.Assert(false); outt.r = outt.g = outt.b = 0; return;
                }
                outt.r = (byte)(((uint)(c0.r) * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc]) + (uint)(c1.r) * (uint)(aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
                outt.g = (byte)(((uint)(c0.g) * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc]) + (uint)(c1.g) * (uint)(aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
                outt.b = (byte)(((uint)(c0.b) * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc]) + (uint)(c1.b) * (uint)(aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            }

            public static void InterpolateA(LDRColorA c0, LDRColorA c1, int wa, int waprec, LDRColorA outt)
            {
                int[] aWeights = null;
                switch (waprec)
                {
                    case 2: aWeights = Constants.g_aWeights2; Debug.Assert(wa < 4); break;
                    case 3: aWeights = Constants.g_aWeights3; Debug.Assert(wa < 8); break;
                    case 4: aWeights = Constants.g_aWeights4; Debug.Assert(wa < 16); break;
                    default: Debug.Assert(false); outt.a = 0; return;
                }
                outt.a = (byte)(((uint)(c0.a) * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wa]) + (uint)(c1.a) * (uint)(aWeights[wa]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            }

            public static void Interpolate(LDRColorA c0, LDRColorA c1, int wc, int wa, int wcprec, int waprec, LDRColorA outt)
            {
                InterpolateRGB(c0, c1, wc, wcprec, outt);
                InterpolateA(c0, c1, wa, waprec, outt);
            }
        }

        private struct ModeInfo
        {
            public byte uPartitions;
            public byte uPartitionBits;
            public byte uPBits;
            public byte uRotationBits;
            public byte uIndexModeBits;
            public byte uIndexPrec;
            public byte uIndexPrec2;
            public LDRColorA RGBAPrec;
            public LDRColorA RGBAPrecWithP;

            public ModeInfo(byte uParts, byte uPartBits, byte upBits, byte uRotBits, byte uIndModeBits, byte uIndPrec, byte uIndPrec2, LDRColorA rgbaPrec, LDRColorA rgbaPrecWithP)
            {
                uPartitions = uParts;
                uPartitionBits = uPartBits;
                uPBits = upBits;
                uRotationBits = uRotBits;
                uIndexModeBits = uIndModeBits;
                uIndexPrec = uIndPrec;
                uIndexPrec2 = uIndPrec2;
                RGBAPrec = rgbaPrec;
                RGBAPrecWithP = rgbaPrecWithP;
            }
        }

        private static readonly ModeInfo[] ms_aInfo = new ModeInfo[]
        {
            new ModeInfo(2, 4, 6, 0, 0, 3, 0, new LDRColorA(4,4,4,0), new LDRColorA(5,5,5,0)),
                // Mode 0: Color only, 3 Subsets, RGBP 4441 (unique P-bit), 3-bit indecies, 16 partitions
            new ModeInfo(1, 6, 2, 0, 0, 3, 0, new LDRColorA(6,6,6,0), new LDRColorA(7,7,7,0)),
                // Mode 1: Color only, 2 Subsets, RGBP 6661 (shared P-bit), 3-bit indecies, 64 partitions
            new ModeInfo(2, 6, 0, 0, 0, 2, 0, new LDRColorA(5,5,5,0), new LDRColorA(5,5,5,0)),
                // Mode 2: Color only, 3 Subsets, RGB 555, 2-bit indecies, 64 partitions
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new LDRColorA(7,7,7,0), new LDRColorA(8,8,8,0)),
                // Mode 3: Color only, 2 Subsets, RGBP 7771 (unique P-bit), 2-bits indecies, 64 partitions
            new ModeInfo(0, 0, 0, 2, 1, 2, 3, new LDRColorA(5,5,5,6), new LDRColorA(5,5,5,6)),
                // Mode 4: Color w/ Separate Alpha, 1 Subset, RGB 555, A6, 16x2/16x3-bit indices, 2-bit rotation, 1-bit index selector
            new ModeInfo(0, 0, 0, 2, 0, 2, 2, new LDRColorA(7,7,7,8), new LDRColorA(7,7,7,8)),
                // Mode 5: Color w/ Separate Alpha, 1 Subset, RGB 777, A8, 16x2/16x2-bit indices, 2-bit rotation
            new ModeInfo(0, 0, 2, 0, 0, 4, 0, new LDRColorA(7,7,7,7), new LDRColorA(8,8,8,8)),
                // Mode 6: Color+Alpha, 1 Subset, RGBAP 77771 (unique P-bit), 16x4-bit indecies
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new LDRColorA(5,5,5,5), new LDRColorA(6,6,6,6))
                // Mode 7: Color+Alpha, 2 Subsets, RGBAP 55551 (unique P-bit), 2-bit indices, 64 partitions
        };

        private static byte GetBit(ReadOnlySpan<byte> block, ref uint uStartBit)
        {
            uint uIndex = uStartBit >> 3;
            var ret = (byte)((block[(int)uIndex] >> (int)(uStartBit - (uIndex << 3))) & 0x01);
            uStartBit++;
            return ret;
        }

        private static byte GetBits(ReadOnlySpan<byte> block, ref uint uStartBit, uint uNumBits)
        {
            if (uNumBits == 0) return 0;
            Debug.Assert(uStartBit + uNumBits <= 128 && uNumBits <= 8);
            byte ret;
            uint uIndex = uStartBit >> 3;
            uint uBase = uStartBit - (uIndex << 3);
            if (uBase + uNumBits > 8)
            {
                uint uFirstIndexBits = 8 - uBase;
                uint uNextIndexBits = uNumBits - uFirstIndexBits;
                ret = (byte)((uint)(block[(int)uIndex] >> (int)uBase) | ((block[(int)uIndex + 1] & ((1u << (int)uNextIndexBits) - 1)) << (int)uFirstIndexBits));
            }
            else
            {
                ret = (byte)((block[(int)uIndex] >> (int)uBase) & ((1 << (int)uNumBits) - 1));
            }
            Debug.Assert(ret < (1 << (int)uNumBits));
            uStartBit += uNumBits;
            return ret;
        }

        private static byte Unquantize(byte comp, uint uPrec)
        {
            Debug.Assert(0 < uPrec && uPrec <= 8);
            comp = (byte)(comp << (int)(8u - uPrec));
            return (byte)(comp | (comp >> (int)uPrec));
        }

        private static LDRColorA Unquantize(LDRColorA c, LDRColorA RGBAPrec)
        {
            LDRColorA q = new LDRColorA();
            q.r = Unquantize(c.r, RGBAPrec.r);
            q.g = Unquantize(c.g, RGBAPrec.g);
            q.b = Unquantize(c.b, RGBAPrec.b);
            q.a = RGBAPrec.a > 0 ? Unquantize(c.a, RGBAPrec.a) : (byte)255u;
            return q;
        }

        private static void ByteSwap(ref byte a, ref byte b)
        {
            byte tmp = a;
            a = b;
            b = tmp;
        }

        public static void DDS_BC7_GetPixels(byte[,,] pixels, int x, int y, ReadOnlySpan<byte> block)
        {
            var newPixels = GetNewPixels(block);

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);
            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                {
                    var color = newPixels[i * 4 + k];
                    pixels[y + i, x + k, 0] = color.b;
                    pixels[y + i, x + k, 1] = color.g;
                    pixels[y + i, x + k, 2] = color.r;
                    pixels[y + i, x + k, 3] = color.a;
                }
        }

        private static LDRColorA[] GetNewPixels(ReadOnlySpan<byte> block)
        {
            var newPixels = new LDRColorA[DDSConstants.NUM_PIXELS_PER_BLOCK];

            // by default fill with transparent black
            for (int i = 0; i < DDSConstants.NUM_PIXELS_PER_BLOCK; ++i)
            {
                var color = new LDRColorA(0, 0, 0, 0);
                newPixels[i] = color;
            }

            uint uFirst = 0;
            while (uFirst < 128 && GetBit(block, ref uFirst) == 0) { }
            byte uMode = (byte)(uFirst - 1);

            if (uMode < 8)
            {
                byte uPartitions = ms_aInfo[uMode].uPartitions;
                Debug.Assert(uPartitions < Constants.BC7_MAX_REGIONS);

                var uNumEndPts = (byte)((uPartitions + 1u) << 1);
                byte uIndexPrec = ms_aInfo[uMode].uIndexPrec;
                byte uIndexPrec2 = ms_aInfo[uMode].uIndexPrec2;
                int i;
                uint uStartBit = uMode + 1u;
                int[] P = new int[6];
                byte uShape = GetBits(block, ref uStartBit, ms_aInfo[uMode].uPartitionBits);
                Debug.Assert(uShape < Constants.BC7_MAX_SHAPES);

                byte uRotation = GetBits(block, ref uStartBit, ms_aInfo[uMode].uRotationBits);
                Debug.Assert(uRotation < 4);

                byte uIndexMode = GetBits(block, ref uStartBit, ms_aInfo[uMode].uIndexModeBits);
                Debug.Assert(uIndexMode < 2);

                LDRColorA[] c = new LDRColorA[Constants.BC7_MAX_REGIONS << 1];
                for (i = 0; i < c.Length; ++i) c[i] = new LDRColorA();
                LDRColorA RGBAPrec = ms_aInfo[uMode].RGBAPrec;
                LDRColorA RGBAPrecWithP = ms_aInfo[uMode].RGBAPrecWithP;

                Debug.Assert(uNumEndPts <= (Constants.BC7_MAX_REGIONS << 1));

                // Red channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.r > 128)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }

                    c[i].r = GetBits(block, ref uStartBit, RGBAPrec.r);
                }

                // Green channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.g > 128)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }

                    c[i].g = GetBits(block, ref uStartBit, RGBAPrec.g);
                }

                // Blue channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.b > 128)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }

                    c[i].b = GetBits(block, ref uStartBit, RGBAPrec.b);
                }

                // Alpha channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.a > 128)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }

                    c[i].a = (byte)(RGBAPrec.a != 0 ? GetBits(block, ref uStartBit, RGBAPrec.a) : 255u);
                }

                // P-bits
                Debug.Assert(ms_aInfo[uMode].uPBits <= 6);
                for (i = 0; i < ms_aInfo[uMode].uPBits; i++)
                {
                    if (uStartBit > 127)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }

                    P[i] = GetBit(block, ref uStartBit);
                }

                if (ms_aInfo[uMode].uPBits != 0)
                {
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        int pi = i * ms_aInfo[uMode].uPBits / uNumEndPts;
                        for (byte ch = 0; ch < Constants.BC7_NUM_CHANNELS; ch++)
                        {
                            if (RGBAPrec[ch] != RGBAPrecWithP[ch])
                            {
                                c[i][ch] = (byte)((c[i][ch] << 1) | P[pi]);
                            }
                        }
                    }
                }

                for (i = 0; i < uNumEndPts; i++)
                {
                    c[i] = Unquantize(c[i], RGBAPrecWithP);
                }

                byte[] w1 = new byte[DDSConstants.NUM_PIXELS_PER_BLOCK], w2 = new byte[DDSConstants.NUM_PIXELS_PER_BLOCK];

                // read color indices
                for (i = 0; i < DDSConstants.NUM_PIXELS_PER_BLOCK; i++)
                {
                    uint uNumBits = IsFixUpOffset(ms_aInfo[uMode].uPartitions, uShape, i) ? uIndexPrec - 1u : uIndexPrec;
                    if (uStartBit + uNumBits > 128)
                    {
                        Debug.WriteLine("BC7: Invalid block encountered during decoding");
                        return newPixels;
                    }
                    w1[i] = GetBits(block, ref uStartBit, uNumBits);
                }

                // read alpha indices
                if (uIndexPrec2 != 0)
                {
                    for (i = 0; i < DDSConstants.NUM_PIXELS_PER_BLOCK; i++)
                    {
                        uint uNumBits = (i != 0 ? uIndexPrec2 : uIndexPrec2 - 1u);
                        if (uStartBit + uNumBits > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            return newPixels;
                        }
                        w2[i] = GetBits(block, ref uStartBit, uNumBits);
                    }
                }

                for (i = 0; i < DDSConstants.NUM_PIXELS_PER_BLOCK; ++i)
                {
                    byte uRegion = Constants.g_aPartitionTable[uPartitions][uShape][i];
                    LDRColorA outPixel = new LDRColorA();
                    if (uIndexPrec2 == 0)
                    {
                        LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w1[i], uIndexPrec, uIndexPrec, outPixel);
                    }
                    else
                    {
                        if (uIndexMode == 0)
                        {
                            LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w2[i], uIndexPrec, uIndexPrec2, outPixel);
                        }
                        else
                        {
                            LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w2[i], w1[i], uIndexPrec2, uIndexPrec, outPixel);
                        }
                    }

                    switch (uRotation)
                    {
                        case 1: ByteSwap(ref outPixel.r, ref outPixel.a); break;
                        case 2: ByteSwap(ref outPixel.g, ref outPixel.a); break;
                        case 3: ByteSwap(ref outPixel.b, ref outPixel.a); break;
                    }

                    newPixels[i] = outPixel;
                }
            }

            return newPixels;
        }

        public static bool IsFixUpOffset(byte uPartitions, byte uShape, int uOffset)
        {
            Debug.Assert(uPartitions < 3 && uShape < 64 && uOffset < 16 && uOffset >= 0);
            for (byte p = 0; p <= uPartitions; p++)
            {
                if (uOffset == Constants.g_aFixUp[uPartitions][uShape][p])
                {
                    return true;
                }
            }
            return false;
        }
    }
}