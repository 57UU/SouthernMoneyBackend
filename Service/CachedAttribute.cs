namespace Service;

[AttributeUsage(AttributeTargets.Method)]
public class CachedAttribute : Attribute
{
    public string Key { get; set; } = "";
    public string Expiry { get; set; } = "10m";
}