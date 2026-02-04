using System.Text.Json;
using InforCloudSuite.DataDumper.Models;
using InforCloudSuite.DataDumper.Services;

Credentials? credentials;
using (StreamReader r = new StreamReader("credentials.ionapi"))
{
    string json = r.ReadToEnd();
    credentials = JsonSerializer.Deserialize<Credentials>(json);
}

if (credentials == null) throw new Exception("Missing Credentials File");

SyteLineService.LoadCredentials(credentials);
await SyteLineService.GetAccessToken();

Console.WriteLine("Input IDO Name: ");
string? ido = Console.ReadLine();
if (string.IsNullOrEmpty(ido)) throw new Exception("IDO required");

Console.WriteLine("Input Order By (default empty): ");
string? orderBy = Console.ReadLine();

Console.WriteLine("Input Bookmark (default empty): ");
string? bookmark = Console.ReadLine();

string filePath = "data.csv";

using FileStream fs = new FileStream(filePath, FileMode.Append);

using StreamWriter writer = new StreamWriter(fs);

await SyteLineService.LoadCollection(ido, fs, writer, orderBy ?? "", bookmark ?? "");
