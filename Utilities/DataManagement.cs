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
    }
}
