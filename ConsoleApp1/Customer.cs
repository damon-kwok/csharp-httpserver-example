namespace ConsoleApp1;

// Model: CustomerInfo
public class CustomerInfo
{
    public string ID { get; set; }
    public Int64 Score { get; set; }

    public CustomerInfo(string id, Int64 score)
    {
        this.ID = id;
        this.Score = score;
    }

    public CustomerInfo(string id)
    {
        this.ID = id;
        this.Score = 0;
    }

    public CustomerInfo(Int64 score)
    {
        this.ID = "";
        this.Score = score;
    }

    public override string ToString()
    {
        return $"CustomerInfo:: (ID:{ID}, Score:{Score})";
    }
}