using System.Collections.Generic;


namespace MG_BlocksEngine2.Utils
{
    public static class BE2_ArrayUtils
    {
        public static void Resize<T>(ref T[] array, int size)
        {
            System.Array.Resize(ref array, size);
        }

        public static void Add<T>(ref T[] array, T value)
        {
            int length = array.Length;
            Resize<T>(ref array, length + 1);
            array[length] = value;
        }

        public static T[] AddReturn<T>(T[] array, T value)
        {
            int length = array.Length;
            Resize<T>(ref array, length + 1);
            array[length] = value;
            return array;
        }

        public static void Remove<T>(ref T[] array, T value)
        {
            int count = array.Length;
            int index = System.Array.IndexOf(array, value);
            if (index < 0) return;

            T[] newArray = new T[count - 1];
            System.Array.Copy(array, 0, newArray, 0, index);
            System.Array.Copy(array, index + 1, newArray, index, count - index - 1);
            array = newArray;
        }

        // v2.10 - BE2_ArrayUtins FindAll and Find methods refactored to use System.Array class 
        public static T[] FindAll<T>(ref T[] array, System.Predicate<T> match)
        {
            return System.Array.FindAll(array, match);
        }

        // v2.10 - BE2_ArrayUtins FindAll and Find methods refactored to use System.Array class
        public static T Find<T>(ref T[] array, System.Predicate<T> match)
        {
            return System.Array.Find(array, match);
        }
    }
}
