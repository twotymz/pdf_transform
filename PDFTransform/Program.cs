using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using Ghostscript.NET.Rasterizer;
using Tesseract;

namespace PDFTransform
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"C:\Users\Josh\Desktop\pdfs\headliner - Copy\venue\date\time\George AXS.pdf";
            //string path = @"C:\Users\Josh\Desktop\pdfs\headliner\venue\date\time\George TM.pdf";
            //string path = @"C:\Users\Josh\Desktop\pdfs\George AXS.pdf";
            string path = @"C:\Users\Josh\Desktop\pdfs\George TM.pdf";
            string text = ocr(path, 300, 300);

            System.IO.File.WriteAllText(@"C:\Users\Josh\Desktop\Josh\out.txt", text);
        }

        static string ocr (string inputPdfPath, int desired_x_dpi, int desired_y_dpi)
        {
            List<Bitmap> bitmaps = toBitmap(inputPdfPath, desired_x_dpi, desired_y_dpi);
            return doOCR(bitmaps[0]);
        }

        static string doOCR (Bitmap bitmap)
        {
            try
            {
                string tessDataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "tessdata");

                using (var engine = new TesseractEngine(@".\tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadTiffFromMemory(bitmapToByteArray(bitmap, System.Drawing.Imaging.ImageFormat.Tiff)))
                    {
                        using (var page = engine.Process(img))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
            catch(Exception)
            {
            }

            return null;
        }

        static List<Bitmap> toBitmap (string inputPdfPath, int desired_x_dpi, int desired_y_dpi)
        {
            string outputPath = @"C:\Users\Josh\Desktop\josh\";
            List<Bitmap> bitmaps = new List<Bitmap>();

            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(inputPdfPath);

                for (var pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    var pageFilePath = Path.Combine(outputPath, string.Format("{0}.bmp", pageNumber));
                    var img = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);
                    Bitmap bmp = new Bitmap(img);

                    //bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    //Rectangle rect = new Rectangle (1904, 350, 336, 43);
                    //bmp = cropBitmap(bmp, rect);

                    bmp = toGreyScale(bmp);
                    bmp.Save(pageFilePath);
                    bitmaps.Add(bmp);
                }
            }

            return bitmaps;
        }

        static Bitmap toGreyScale (Bitmap original)
        {
            //create a blank bitmap the same size as original
           Bitmap newBitmap = new Bitmap(original.Width, original.Height);

           //get a graphics object from the new image
           Graphics g = Graphics.FromImage(newBitmap);

           //create the grayscale ColorMatrix
           ColorMatrix colorMatrix = new ColorMatrix(
              new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

           //create some image attributes
           ImageAttributes attributes = new ImageAttributes();

           //set the color matrix attribute
           attributes.SetColorMatrix(colorMatrix);

           //draw the original image on the new image
           //using the grayscale color matrix
           g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
              0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

           //dispose the Graphics object
           g.Dispose();
           return newBitmap;
        }

        static byte[] bitmapToByteArray (Bitmap input, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.Save(ms, format);
                return ms.ToArray();
            }
        }

        static Bitmap cropBitmap (Bitmap input, Rectangle rect)
        {
            Bitmap output = new Bitmap(rect.Width, rect.Height);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.DrawImage(input, new Rectangle(0, 0, output.Width, output.Height), rect, GraphicsUnit.Pixel);
            }

            return output;
        }

        static void saveBitmap (Bitmap input, string path)
        {
            input.Save(path);
        }
    }
}
