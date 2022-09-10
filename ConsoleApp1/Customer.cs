﻿namespace ConsoleApp1;

// Model: CustomerInfo
public class CustomerInfo
{
    public string Id { get; set; }
    public long Score { get; set; }

    public CustomerInfo(string id, Int64 score)
    {
        this.Id = id;
        this.Score = score;
    }

    public CustomerInfo(string id)
    {
        this.Id = id;
        this.Score = 0;
    }

    public CustomerInfo(Int64 score)
    {
        this.Id = "";
        this.Score = score;
    }

    public override string ToString()
    {
        return $"CustomerInfo:: (ID:{Id}, Score:{Score})";
    }
}
