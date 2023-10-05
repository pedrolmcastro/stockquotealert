namespace Price;

public static class Parser
{
    public static bool TryParse(string input, out double result)
    {
        if (!double.TryParse(input, out result))
        {
            return false;
        }

        if (result < 0.0)
        {
            return false;
        }

        return true;
    }
}
