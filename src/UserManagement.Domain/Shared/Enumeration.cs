using System.Reflection;

namespace UserManagement.Domain.Shared;

public class Enumeration<T> : IEquatable<Enumeration<T>>
    where T : Enumeration<T>
{
    private static readonly Lazy<Dictionary<int, T>> _dictionary = new(() => PopulateDictionary(typeof(T)));

    public T? GetById(int id) => _dictionary.Value.FirstOrDefault(d => d.Key == id).Value;
    public T? GetByName(string name) => _dictionary.Value.Values.FirstOrDefault(d => d.Name == name);
    public static IReadOnlyCollection<T> GetValues() => _dictionary.Value.Values.ToList();

    public int Id { get; protected init; }
    public string Name { get; protected init; }

    protected Enumeration(int id, string name)
        : this()
    {
        Id = id;
        Name = name;
    }

    protected Enumeration()
    {
        Name = string.Empty;
    }

    private static Dictionary<int, T> PopulateDictionary(Type T)
    {
        return T
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => T.IsAssignableFrom(f.FieldType))
            .Select(f => (T)f.GetValue(default)!)
            .ToDictionary(t => t.Id);
    }

    public bool Equals(Enumeration<T>? other)
    {
        if (other == null) return false;

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        if (GetType() != obj.GetType()) return false;

        return obj is Enumeration<T> other && Id == other.Id;
    }

    public static bool operator ==(Enumeration<T>? left, Enumeration<T>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration<T>? left, Enumeration<T>? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Id * 30;
    }
}