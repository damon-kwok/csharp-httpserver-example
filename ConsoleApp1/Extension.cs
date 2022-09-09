namespace ConsoleApp1;

public static class Utils
{
    public static string ToString(List<CustomerInfo> list)
    {
        string result = "List:: <";
        foreach (var item in list)
        {
            result += item.ToString() + ", ";
        }

        result += ">";
        return result;
    }
}