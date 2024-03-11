using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray16<T>
    {
        public static RcStackArray16<T> Empty => new RcStackArray16<T>();

        private const int Size = 16;
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
        
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                return index >= 0 && index < 16 ?
                       index == 0 ? V0 :
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
                       throw new IndexOutOfRangeException($"{index}") :
                       throw new IndexOutOfRangeException($"{index}");
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
                }
            }
        }
    }
}