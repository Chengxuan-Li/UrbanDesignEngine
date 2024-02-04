using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanDesignEngine.Utilities
{
    public static class DataManagement
    {
        public static int ArgMax<T>(List<int> indices, List<T> items) where T: IComparable
        {
            int count = Math.Min(indices.Count, items.Count);
            int result = 0;
            for(int i = 0; i < count; i++)
            {
                if (items[i].CompareTo(items[result]) > 0) result = i;
            }
            return indices[result];
        }

        public static int ArgMin<T>(List<int> indices, List<T> items) where T : IComparable
        {
            int count = Math.Min(indices.Count, items.Count);
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                if (items[i].CompareTo(items[result]) < 0) result = i;
            }
            return indices[result];
        }

        public static void Nearest<TKey, T>(TKey refKey, List<TKey> keys, List<T> items, out T smaller, out T bigger) where TKey : IComparable
        {
            List<TKey> keysSorted;
            List<T> itemsSorted;
            SortByKeys<TKey, T>(keys, items, out keysSorted, out itemsSorted);
            if (refKey.CompareTo(keysSorted[0]) < 0)
            {
                smaller = itemsSorted[itemsSorted.Count - 1];
                bigger = itemsSorted[0];
                return;
            }
            if (refKey.CompareTo(keysSorted[itemsSorted.Count - 1]) > 0)
            {
                smaller = itemsSorted[itemsSorted.Count - 1];
                bigger = itemsSorted[0];
                return;
            }
            for (int i = 0; i < keysSorted.Count; i++)
            {
                if (refKey.CompareTo(keysSorted[i]) < 0)
                {
                    smaller = itemsSorted[i - 1];
                    bigger = itemsSorted[i];
                    return;
                }
            }
            smaller = items[0];
            bigger = items[items.Count - 1];
            return;
        }


        public static bool Nearest<T>(double refKey, List<double> keys, List<T> items, out T smaller, out T bigger)
        {
            bool result = false;
            smaller = items[0];
            double differenceFromSmaller = 9999999;
            bigger = items[0];
            double differenceFromBigger = 9999999;
            int count = Math.Min(keys.Count, items.Count);
            for(int i = 0; i < count; i++)
            {
                double key = keys[i];
                if (key < refKey)
                {
                    double tempDifference = refKey - key;
                    if (tempDifference < differenceFromSmaller)
                    {
                        differenceFromSmaller = tempDifference;
                        smaller = items[i];
                        result = true;
                    }
                } else if (key > refKey)
                {
                    double tempDifference = key - refKey;
                    if (tempDifference < differenceFromBigger)
                    {
                        differenceFromBigger = tempDifference;
                        bigger = items[i];
                        result = true;
                    }
                }
            }
            return result;
        }

        public static void SortByKeys<TKey, T>(List<TKey> keys, List<T> items, out List<TKey> keysSorted, out List<T> itemsSorted) where TKey: IComparable
        {
            //items.Sort((x, y) => keys[items.IndexOf(x)].CompareTo(keys[items.IndexOf(y)]));
            itemsSorted = items.OrderBy(o => keys[items.IndexOf(o)]).ToList();
            //keys.Sort((x, y) => x.CompareTo(y));
            keysSorted = keys.OrderBy(o => o).ToList();
            return;
        }
    }
}
