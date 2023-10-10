namespace Stock;


internal readonly record struct Info(string Ticker, decimal BuyPrice, decimal SellPrice)
{
    public override string ToString() => Ticker;
}


internal static class Price
{
    public static bool TryParse(string input, out decimal result)
    {
        if (!decimal.TryParse(input, out result))
        {
            return false;
        }

        if (result < 0.0m)
        {
            return false;
        }

        return true;
    }
}
