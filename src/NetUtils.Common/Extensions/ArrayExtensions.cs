namespace System.Collections.Generic
{
    public static class ArrayExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
        {
            foreach (T item in array)
            {
                action(item);
            }
        }
    }
}
