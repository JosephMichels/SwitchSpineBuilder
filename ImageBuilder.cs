using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchSpineBuilder
{
    public static class ImageBuilder
    {
        public static void BuildImage(string[] spines, int gap)
        {
            //assume 8.5x11 printing landscape with a 1 inch margin for 9inch.
            // 220 DPI
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
                        image.Mutate(x => x.DrawImage(spine, new Point(i * (122 + gap), 0), 1));
                    }
                }

                image.Save($"spine_page_{page + 1}.png");

                page++;
                current += fit;
            }
        }

    }
}
