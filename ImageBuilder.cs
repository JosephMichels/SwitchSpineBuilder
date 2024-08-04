using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace SwitchSpineBuilder
{
    public static class ImageBuilder
    {
        public static string[] BuildImages(string[] spines, int gap, Action<int, int> imageGenerated = null)
        {
            List<string> output = new List<string>();
            //assume 8.5x11 printing landscape with a 1 inch margin for 9inch.
            // 300 DPI
            var widthInches = 9;
            var dpi = 300;
            int fit = widthInches * dpi / (122 + gap);

            int current = 0;
            int page = 0;
            while (current < spines.Length)
            {
                var image = new Image<Rgba32>(dpi * widthInches, 1906);

                for(int i = 0; i < fit; i++)
                {
                    int spineIndex = i + (page * fit);
                    if (spineIndex >= spines.Length) break;

                    using (var spine = Image.Load(spines[spineIndex]))
                    {
                        //Resize image if needed.
                        if(spine.Width != 122 || spine.Height != 1906)
                        {
                            spine.Mutate(s => s.Resize(122, 1906));
                        }

                        image.Mutate(x => x.DrawImage(spine, new Point(i * (122 + gap), 0), 1));
                    }
                }
                image.Mutate(x => x.Rotate(90));

                var file = $"spine_page_{page + 1}.png";
                image.Save(file);
                output.Add(file);

                // Parameters: Current Image, Total Images
                imageGenerated?.Invoke(page + 1, (int)Math.Ceiling(spines.Length / (float)fit));

                page++;
                current += fit;

            }
            return output.ToArray();
        }

        public static void BuildPdf(string[] images)
        {
            
            Document.Create(container =>
            {
                
                foreach(var image in images)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Content().Container()
                        .AlignCenter().AlignMiddle()
                        //.Container()
                        .Width(6.35f, QuestPDF.Infrastructure.Unit.Inch)
                        .Image(image);
                    });
                }

            }).GeneratePdf("output.pdf");
        }

    }
}
