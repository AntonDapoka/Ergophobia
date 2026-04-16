using System.Collections.Generic;

public static class ArrayUtilitiesScript
{
    public static void Resize<T>(ref T[] array, int size)
    {
        T[] tempArray = array;
        array = new T[size];
        for (int i = 0; i < tempArray.Length; i++)
            if (size > i) array[i] = tempArray[i];
    }

    public static void Add<T>(ref T[] array, T value)
    {
        int length = array.Length;
        Resize(ref array, length + 1);
        array[length] = value;
    }

    public static T[] AddReturn<T>(T[] array, T value)
    {
        int length = array.Length;
        T[] newArray = array;
        Resize(ref newArray, length + 1);
        newArray[length] = value;
        return newArray;
    }

    public static void Remove<T>(ref T[] array, T value)
    {
        List<T> list = new();
        list.AddRange(array);
        list.Remove(value);
        array = list.ToArray();
    }

    public static T[] FindAll<T>(ref T[] array, System.Predicate<T> match)
    {
        return System.Array.FindAll(array, match);
    }

    public static T Find<T>(ref T[] array, System.Predicate<T> match)
    {
        return System.Array.Find(array, match);
    }
}

