using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InforCloudSuite.DataDumper.Models;

namespace InforCloudSuite.DataDumper.Services;

public class SyteLineService
{
    public static HttpClient Client = new HttpClient();
    public static Credentials? credentials;

    public static string Config = "";

    public static void LoadCredentials(Credentials credentials)
    {
        SyteLineService.credentials = credentials;
    }
    
    public static async Task GetAccessToken()
    {
        if (credentials == null) throw new Exception("Credentials not loaded");
        using HttpClient client = new HttpClient();
        Dictionary<string, string> requestData = new Dictionary<string, string>
        {
            {"grant_type", "password"},
            {"client_authentication", "header"},
            {"client_id", credentials.ci},
            {"client_secret", credentials.cs},
            {"username", credentials.saak},
            {"password", credentials.sask}
        };

        FormUrlEncodedContent payload = new FormUrlEncodedContent(requestData);

        var response = await client.PostAsync(new Uri(credentials.pu + credentials.ot), payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Token exchange failed: {response.StatusCode} - {errorContent}");
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        JsonElement tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.GetProperty("access_token").GetString());
        Client.BaseAddress = new Uri($"{credentials.iu}/{credentials.ti}/CSI/IDORequestService/ido/");
        Client.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", credentials.cf);
    }

    public static async Task LoadCollection(string ido, FileStream fs, StreamWriter writer, string orderBy = "", string bookmark = "", int previousRecordsFetched = 0)
    {
        string loadType = string.IsNullOrEmpty(bookmark) ? "FIRST" : "NEXT";
        string message = $"load/{ido}?properties=*&recordCap=1000&loadType={loadType}";
        if (!string.IsNullOrEmpty(bookmark)) message += $"&bookmark={bookmark}";
        if (!string.IsNullOrEmpty(orderBy)) message += $"&orderBy={orderBy}";

        Console.WriteLine($"Fetching {loadType} 1000");

        JsonElement response = await Client.GetFromJsonAsync<JsonElement>(message);

        Console.WriteLine($"Success: {response.GetProperty("Success").GetBoolean()}");

        int recordsFetched = response.GetProperty("Items").GetArrayLength();

        Console.WriteLine($"Number of Records Fetched: {recordsFetched}");

        Console.WriteLine("Writing Records to data.csv...");

        if (fs.Position == 0 && recordsFetched > 0)
        {
            JsonElement item = response.GetProperty("Items").EnumerateArray().FirstOrDefault();
            foreach (JsonProperty prop in item.EnumerateObject())
            {
                writer.Write(prop.Name + ",");
            }
            writer.WriteLine();
        }

        foreach (JsonElement item in response.GetProperty("Items").EnumerateArray())
        {
            foreach (JsonProperty prop in item.EnumerateObject())
            {
                writer.Write(prop.Value + ",");
            }
            writer.WriteLine();
        }

        string? bookmarkResult = response.GetProperty("Bookmark").GetString();
        bool moreRowsExist = response.GetProperty("MoreRowsExist").GetBoolean();

        Console.WriteLine($"Total Records Fetched: {recordsFetched + previousRecordsFetched}");

        if (moreRowsExist)
        {
            Console.WriteLine("More rows exist");
            Console.WriteLine($"Bookmark: {bookmarkResult}");
            await LoadCollection(ido, fs, writer, orderBy, bookmarkResult ?? "", recordsFetched + previousRecordsFetched);
        }
    }
}
