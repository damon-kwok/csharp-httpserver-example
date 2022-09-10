namespace ConsoleApp1;

public static class Utils
{
    public static string ToString(IEnumerable<CustomerInfo> list)
    {
        var result = list.Aggregate("List:: <",
            (current, item) => current + (item + ", "));
        result += ">";
        return result;
    }
}
