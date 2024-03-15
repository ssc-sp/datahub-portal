namespace Datahub.Core.Data;

public static class EnumExtentions
{
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> self)
        => self.Select((item, index) => (item, index));
}