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

                Console.WriteLine("Usage: ttfkit_cli SVG_FOLDER_PATH FONT_PATH [OPTIONS]\n");
                Console.WriteLine("  Svg_Folder_Path:\tPath to folder containing svg images");
                Console.WriteLine("  Font_Path:\t\tDestination path for created ttf file");
                Console.WriteLine();
                Console.WriteLine("Options:\n");
                Console.WriteLine("  --host=HOST\t\tTarget ttfkit server address (default \"http://localhost:3000\")");
                Console.WriteLine();
                return;
            }
            
            try
            {
                string requestUrl = ParseHostUrl(args);
                CreateFont(requestUrl, args[0], args[1]);
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

        static void CreateFont(string requestUrl, string iconFolderPath, string fontPath)
        {
            string fontName = Path.GetFileNameWithoutExtension(fontPath);
            var request = new CreateFontRequest(fontName, iconFolderPath);

            using var client = new HttpClient();

            var response = client.PostAsJsonAsync(requestUrl, request).Result;
            var result = response.Content.ReadFromJsonAsync<CreateFontResponse>().Result;

            if (result == null)
                throw new Exception("Invalid server answer: no data");

            result.ToFile(fontPath);
            Console.WriteLine($"Font file created at {fontPath}");
        }
    }

    internal class CreateFontRequest
    {
        public string fontName { get; set; }
        public List<Glyph> files { get; set; }

        public CreateFontRequest(string fontName, string iconFolderPath)
        {
            this.fontName = fontName;
            files = new List<Glyph>();

            foreach (string filePath in Directory.GetFiles(iconFolderPath, "*.svg"))
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                byte[] data = File.ReadAllBytes(filePath);

                files.Add(new Glyph(name, data));
            }
        }
    }

    internal class CreateFontResponse
    {
        public string name { get; set; }
        public string data { get; set; }

        public void ToFile(string filePath)
        {
            byte[] byteData = Convert.FromBase64String(data);
            File.WriteAllBytes(filePath, byteData);
        }
    }

    internal class Glyph
    {
        public string name { get; set; }
        public string data { get; set; }

        public Glyph(string name, byte[] data)
        {
            this.name = name;
            this.data = Convert.ToBase64String(data);
        }
    }
}

