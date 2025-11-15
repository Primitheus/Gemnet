using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Gemnet.Shop.Boxes
{
    public static class BoxLoader
    {
        public static void LoadBoxes(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Box folder not found: {folderPath}");

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                string json = File.ReadAllText(file);
                var box = JsonSerializer.Deserialize<Box>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (box != null)
                    BoxRegistry.RegisterBox(box);
            }
        }
    }
}
