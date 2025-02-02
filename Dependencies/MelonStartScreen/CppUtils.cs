﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MelonLoader.MelonStartScreen
{
    internal static class CppUtils
    {
        public static unsafe IntPtr ResolveRelativeInstruction(IntPtr instruction)
        {
            byte opcode = *(byte*)instruction;
            if (opcode != 0xE8 && opcode != 0xE9)
                return IntPtr.Zero;

            return ResolvePtrOffset((IntPtr)((long)instruction + 1), (IntPtr)((long)instruction + 5)); // CALL: E8 [rel32] / JMP: E9 [rel32]
        }

        public static unsafe IntPtr ResolvePtrOffset(IntPtr offset32Ptr, IntPtr nextInstructionPtr)
        {
            uint jmpOffset = *(uint*)offset32Ptr;
            uint valueUInt = new ConvertClass() { valueULong = (ulong)nextInstructionPtr }.valueUInt;
            long delta = nextInstructionPtr.ToInt64() - valueUInt;
            uint newPtrInt = unchecked(valueUInt + jmpOffset);
            return new IntPtr(newPtrInt + delta);
        }

        internal static unsafe IntPtr Sigscan(IntPtr module, int moduleSize, string signature)
        {
            string signatureSpaceless = signature.Replace(" ", "");
            int signatureLength = signatureSpaceless.Length / 2;
            byte[] signatureBytes = new byte[signatureLength];
            bool[] signatureNullBytes = new bool[signatureLength];
            for (int i = 0; i < signatureLength; ++i)
            {
                if (signatureSpaceless[i * 2] == '?')
                    signatureNullBytes[i] = true;
                else
                    signatureBytes[i] = (byte)((GetHexVal(signatureSpaceless[i * 2]) << 4) + (GetHexVal(signatureSpaceless[(i * 2) + 1])));
            }

            long index = module.ToInt64();
            long maxIndex = index + moduleSize;
            long tmpAddress = 0;
            int processed = 0;

            while (index < maxIndex)
            {
                if (signatureNullBytes[processed] || *(byte*)index == signatureBytes[processed])
                {
                    if (processed == 0)
                        tmpAddress = index;

                    ++processed;

                    if (processed == signatureLength)
                        return (IntPtr)tmpAddress;
                }
                else
                {
                    processed = 0;
                }

                ++index;
            }

            return IntPtr.Zero;
        }

        // Credits: https://stackoverflow.com/a/9995303
        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        [StructLayout(LayoutKind.Explicit)]
        private class ConvertClass
        {
            [FieldOffset(0)]
            public ulong valueULong;

            [FieldOffset(0)]
            public uint valueUInt;
        }
    }
}
