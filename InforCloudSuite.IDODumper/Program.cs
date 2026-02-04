using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using SyteLine.Models;

namespace SyteLine
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Credentials credentials;
            using (StreamReader r = new StreamReader("credentials.ionapi"))
            {
                string json = r.ReadToEnd();
                credentials = JsonSerializer.Deserialize<Credentials>(json);
            }

            SyteLineService.LoadCredentials(credentials);
            await SyteLineService.GetAccessToken();

            Console.WriteLine("Input IDO Name: ");
            string ido = Console.ReadLine();

            Console.WriteLine("Input Order By: ");
            string orderBy = Console.ReadLine();

            Console.WriteLine("Input Bookmark (default empty): ");
            string bookmark = Console.ReadLine();

            string filePath = "data.csv";

            using FileStream fs = new FileStream(filePath, FileMode.Append);

            using StreamWriter writer = new StreamWriter(fs);
            
            await SyteLineService.LoadCollection(ido, fs, writer, orderBy, bookmark);
            
        }
    }
}
