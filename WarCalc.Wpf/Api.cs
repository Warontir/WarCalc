using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace WarCalc.Wpf;

public static class Api
{
    static readonly HttpClient Http = new() { BaseAddress = new Uri("http://localhost:5080/") };
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<T> Get<T>(string url) => Parse<T>(await Send(HttpMethod.Get, url));
    public static async Task<T> Post<T>(string url, object body) => Parse<T>(await Send(HttpMethod.Post, url, body));
    public static Task Put(string url, object body) => Send(HttpMethod.Put, url, body);
    public static Task Delete(string url) => Send(HttpMethod.Delete, url);
    public static async Task<T> Delete<T>(string url) => Parse<T>(await Send(HttpMethod.Delete, url));

    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;

    
   
    public static async Task Safe(Func<Task> action)
    {
        try { await action(); }
        catch (HttpRequestException) { MessageBox.Show("Cannot reach the API. Is WarCalc.Api running on http://localhost:5080 ?", "Connection error"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    public static bool Confirm(string text) =>
        MessageBox.Show(text, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    static async Task<string> Send(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        using var response = await Http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}\n{text}");
        return text;
    }

    static T Parse<T>(string text) => JsonSerializer.Deserialize<T>(text, Json)!;
}
