namespace MailEase.Extensions;

/// <summary>
/// Static class containing extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{T}"/> to perform on each element.</param>
    /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
        }
    }

    /// <summary>
    /// Adds the specified items to the collection.
    /// </summary>
    /// <param name="collection">The collection to add the items to.</param>
    /// <param name="items">The items to add to the collection.</param>
    /// <typeparam name="T">The type of the items.</typeparam>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (collection is List<T> list)
        {
            list.AddRange(items);
            return;
        }
        
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}