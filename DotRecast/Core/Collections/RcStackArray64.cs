using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray64<T>
    {
        public static RcStackArray64<T> Empty => new RcStackArray64<T>();

        private const int Size = 64;
        public int Length => Size;
        
        public T V0;
        public T V1;
        public T V2;
        public T V3;
        public T V4;
        public T V5;
        public T V6;
        public T V7;
        public T V8;
        public T V9;
        public T V10;
        public T V11;
        public T V12;
        public T V13;
        public T V14;
        public T V15;
        public T V16;
        public T V17;
        public T V18;
        public T V19;
        public T V20;
        public T V21;
        public T V22;
        public T V23;
        public T V24;
        public T V25;
        public T V26;
        public T V27;
        public T V28;
        public T V29;
        public T V30;
        public T V31;
        public T V32;
        public T V33;
        public T V34;
        public T V35;
        public T V36;
        public T V37;
        public T V38;
        public T V39;
        public T V40;
        public T V41;
        public T V42;
        public T V43;
        public T V44;
        public T V45;
        public T V46;
        public T V47;
        public T V48;
        public T V49;
        public T V50;
        public T V51;
        public T V52;
        public T V53;
        public T V54;
        public T V55;
        public T V56;
        public T V57;
        public T V58;
        public T V59;
        public T V60;
        public T V61;
        public T V62;
        public T V63;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);


                return index == 0 ? V0 :
                       index == 1 ? V1 :
                       index == 2 ? V2 :
                       index == 3 ? V3 :
                       index == 4 ? V4 :
                       index == 5 ? V5 :
                       index == 6 ? V6 :
                       index == 7 ? V7 :
                       index == 8 ? V8 :
                       index == 9 ? V9 :
                       index == 10 ? V10 :
                       index == 11 ? V11 :
                       index == 12 ? V12 :
                       index == 13 ? V13 :
                       index == 14 ? V14 :
                       index == 15 ? V15 :
                       index == 16 ? V16 :
                       index == 17 ? V17 :
                       index == 18 ? V18 :
                       index == 19 ? V19 :
                       index == 20 ? V20 :
                       index == 21 ? V21 :
                       index == 22 ? V22 :
                       index == 23 ? V23 :
                       index == 24 ? V24 :
                       index == 25 ? V25 :
                       index == 26 ? V26 :
                       index == 27 ? V27 :
                       index == 28 ? V28 :
                       index == 29 ? V29 :
                       index == 30 ? V30 :
                       index == 31 ? V31 :
                       index == 32 ? V32 :
                       index == 33 ? V33 :
                       index == 34 ? V34 :
                       index == 35 ? V35 :
                       index == 36 ? V36 :
                       index == 37 ? V37 :
                       index == 38 ? V38 :
                       index == 39 ? V39 :
                       index == 40 ? V40 :
                       index == 41 ? V41 :
                       index == 42 ? V42 :
                       index == 43 ? V43 :
                       index == 44 ? V44 :
                       index == 45 ? V45 :
                       index == 46 ? V46 :
                       index == 47 ? V47 :
                       index == 48 ? V48 :
                       index == 49 ? V49 :
                       index == 50 ? V50 :
                       index == 51 ? V51 :
                       index == 52 ? V52 :
                       index == 53 ? V53 :
                       index == 54 ? V54 :
                       index == 55 ? V55 :
                       index == 56 ? V56 :
                       index == 57 ? V57 :
                       index == 58 ? V58 :
                       index == 59 ? V59 :
                       index == 60 ? V60 :
                       index == 61 ? V61 :
                       index == 62 ? V62 :
                       index == 63 ? V63 :
                       throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            set
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                switch (index)
                {
                    case 0: V0 = value; break;
                    case 1: V1 = value; break;
                    case 2: V2 = value; break;
                    case 3: V3 = value; break;
                    case 4: V4 = value; break;
                    case 5: V5 = value; break;
                    case 6: V6 = value; break;
                    case 7: V7 = value; break;
                    case 8: V8 = value; break;
                    case 9: V9 = value; break;
                    case 10: V10 = value; break;
                    case 11: V11 = value; break;
                    case 12: V12 = value; break;
                    case 13: V13 = value; break;
                    case 14: V14 = value; break;
                    case 15: V15 = value; break;
                    case 16: V16 = value; break;
                    case 17: V17 = value; break;
                    case 18: V18 = value; break;
                    case 19: V19 = value; break;
                    case 20: V20 = value; break;
                    case 21: V21 = value; break;
                    case 22: V22 = value; break;
                    case 23: V23 = value; break;
                    case 24: V24 = value; break;
                    case 25: V25 = value; break;
                    case 26: V26 = value; break;
                    case 27: V27 = value; break;
                    case 28: V28 = value; break;
                    case 29: V29 = value; break;
                    case 30: V30 = value; break;
                    case 31: V31 = value; break;
                    case 32 : V32 = value; break;
                    case 33 : V33 = value; break;
                    case 34 : V34 = value; break;
                    case 35 : V35 = value; break;
                    case 36 : V36 = value; break;
                    case 37 : V37 = value; break;
                    case 38 : V38 = value; break;
                    case 39 : V39 = value; break;
                    case 40 : V40 = value; break;
                    case 41 : V41 = value; break;
                    case 42 : V42 = value; break;
                    case 43 : V43 = value; break;
                    case 44 : V44 = value; break;
                    case 45 : V45 = value; break;
                    case 46 : V46 = value; break;
                    case 47 : V47 = value; break;
                    case 48 : V48 = value; break;
                    case 49 : V49 = value; break;
                    case 50 : V50 = value; break;
                    case 51 : V51 = value; break;
                    case 52 : V52 = value; break;
                    case 53 : V53 = value; break;
                    case 54 : V54 = value; break;
                    case 55 : V55 = value; break;
                    case 56 : V56 = value; break;
                    case 57 : V57 = value; break;
                    case 58 : V58 = value; break;
                    case 59 : V59 = value; break;
                    case 60 : V60 = value; break;
                    case 61 : V61 = value; break;
                    case 62 : V62 = value; break;
                    case 63 : V63 = value; break;
                }
            }
        }
    }
}