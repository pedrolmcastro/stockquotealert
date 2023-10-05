namespace Price;

public static class Parser
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
