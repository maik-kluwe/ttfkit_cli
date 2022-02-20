using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

namespace ttf_kit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid arguments");
                Console.WriteLine();

                Console.WriteLine("Usage: ttfkit_cli SVG_PATH FONT_PATH FONT_NAME [OPTIONS]\n");
                Console.WriteLine("  SVG_Path:\tPath to folder containing svg images");
                Console.WriteLine("  Font_Path:\t\tDestination path for created ttf file");
                Console.WriteLine("  Font_Name:\t\tName for created ttf font");
                Console.WriteLine();
                Console.WriteLine("Options:\n");
                Console.WriteLine("  --host=HOST\t\tTarget ttfkit server address (default \"http://localhost:3000\")");
                Console.WriteLine();
                return;
            }
            
            try
            {
                string requestUrl = ParseHostUrl(args);
                CreateFont(requestUrl, args[0], args[1], args[2]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        static string ParseHostUrl(string[] args)
        {
            foreach (var item in args)
            {
                if (item.StartsWith("--host"))
                    return item.Replace("--host=", "");
            }
            return "http://localhost:3000";
        }

        static void CreateFont(string requestUrl, string iconFolderPath, string outputPath, string fontName)
        {
            var request = new CreateFontRequest(fontName, iconFolderPath);

            using var client = new HttpClient();

            var response = client.PostAsJsonAsync(requestUrl, request).Result;
            var result = response.Content.ReadFromJsonAsync<CreateFontResponse>().Result;

            if (result == null)
                throw new Exception("Invalid server answer: no data");

            result.ToFile(fontName, outputPath);
            Console.WriteLine($"Font file created at {outputPath}");
        }
    }

    internal class CreateFontRequest
    {
        public Config config { get; set; }
        public List<Icon> files { get; set; }

        public CreateFontRequest(string fontName, string iconFolderPath)
        {
            this.config = new Config
            {
                fontName = fontName
            };
            files = new List<Icon>();

            foreach (string filePath in Directory.GetFiles(iconFolderPath, "*.svg"))
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                byte[] data = File.ReadAllBytes(filePath);

                files.Add(new Icon(name, data));
            }
        }
    }

    internal class CreateFontResponse
    {
        public string config { get; set; }
        public string ttf { get; set; }

        public void ToFile(string fontName, string outputPath)
        {
            File.WriteAllBytes(Path.Combine(outputPath, "config.json"), Convert.FromBase64String(config));
            File.WriteAllBytes(Path.Combine(outputPath, $"{fontName}.ttf"), Convert.FromBase64String(ttf));
        }
    }

    internal class Config
    {
        public string fontName { get; set; }
    }

    internal class Icon
    {
        public string fileName { get; set; }
        public string data { get; set; }

        public Icon(string name, byte[] data)
        {
            this.fileName = name;
            this.data = Convert.ToBase64String(data);
        }
    }
}

