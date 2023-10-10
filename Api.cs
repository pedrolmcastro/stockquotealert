namespace Api;

using System.Text.Json.Nodes;


internal readonly record struct Info(string Token, uint Period);


internal class Client
{
    private const int ErrorCode = 4;
    
    private readonly string _token;
    private readonly HttpClient _client;


    public Client(Info info)
    {
        _token = info.Token;
        _client = new HttpClient();
    }


    public async Task<decimal> Get(Stock.Info stock)
    {
        var content = "";
        
        try
        {
            using var response = await _client.GetAsync(Url(stock.Ticker, _token));
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception exception)
        {
            Util.Error.Exit($"Failed to get the API response: {exception.Message}", ErrorCode);
        }

        var price = 0.0M;

        try
        {
            price = JsonNode.Parse(content)!["results"]![0]!["regularMarketPrice"]!.GetValue<decimal>();
        }
        catch (Exception exception)
        {
            Util.Error.Exit($"Failed to parse the API response JSON: {exception.Message}", ErrorCode);
        }

        return price;
    }


    private static string Url(string stock, string token)
    {
        return $"https://brapi.dev/api/quote/{stock}?token={token}&fundamental=false&dividends=false";
    }
}
