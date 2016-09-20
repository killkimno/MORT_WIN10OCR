using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Storage;

using System.Runtime.InteropServices;


namespace MORT_WIN10OCR
{
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public class Class1
    {
        string resultString = "";
        int result = 0;
        bool isReady = true;
        SoftwareBitmap bitmap = null;
        SoftwareBitmap bitmap2 = null;
        public static Class1 instance = new Class1();

        public static string TestOpenCv(List<int> r, List<int> g, List<int> b, int x, int y)
        {
            instance.LoadBitMapFromData(r, g, b, x, y);
            //instance.OCR();
            //return "...!"
            return instance.resultString;
        }
        public static string ProcessOCR()
        {
            instance.OCR();
            //return "...!"
            return instance.resultString;
        }

        

        public void LoadBitMapFromData(List<int> r, List<int> g, List<int> b, int x, int y)
        {
            int BYTES_PER_PIXEL = 4;
            bitmap2 = new SoftwareBitmap(BitmapPixelFormat.Bgra8, x, y);
            unsafe
            {
                using (BitmapBuffer buffer = bitmap2.LockBuffer(BitmapBufferAccessMode.Write))
                {
                    using (var referenceDest = buffer.CreateReference())
                    {
                        byte* data;
                        uint capacity;
                        var desc = buffer.GetPlaneDescription(0);
                        ((IMemoryBufferByteAccess)referenceDest).GetBuffer(out data, out capacity);
                        int count = 0;
                        for (uint row = 0; row < y; row++)
                        {
                            for (uint col = 0; col < x; col++)
                            {
                                var currPixel = desc.StartIndex + desc.Stride * row + BYTES_PER_PIXEL * col;

                                // Index of the current pixel in the buffer (defined by the next 4 bytes, BGRA8)

                                //data[currPixel + 0] = (byte)b[(int)(row * y + col)]; // Blue
                                //data[currPixel + 1] = (byte)g[(int)(row * y + col)];  // Green
                                //data[currPixel + 2] = (byte)r[(int)(row * y + col)]; // Red

                                data[currPixel + 0] = (byte)b[(int)(count)]; // Blue
                                data[currPixel + 1] = (byte)g[(int)(count)];  // Green
                                data[currPixel + 2] = (byte)r[(int)(count)]; // Red

                                count++;

                                //resultString = currPixel.ToString() + "/" + count.ToString() + " /r: " + r.Count.ToString() + " / g: " + g.Count.ToString() + " / b :" + b.Count;ToString() ;
                            }
                        }
                    }
                }
            }



        }

        public static string Test(byte[] test)
        {
            instance.OCR();

            return instance.resultString;
        }

        public static string TestMat(byte[] data)
        {
            return "";
        }

        private async void OCR()
        {
            //resultString = "sdfsdf@!ER";
            if (isReady)
            {
                isReady = false;
                //await LoadSampleImage();
                OcrEngine ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                if (bitmap2 != null)
                {
                    byte[] test = new byte[5];

                    //resultString = "1";
                    OcrResult ocrResult = await ocrEngine.RecognizeAsync(bitmap2);
                    resultString = ocrResult.Text;
                    
                }
                else
                {
                    resultString = "bitmap null";
                }


                isReady = true;
            }
            else
            {
                resultString = " not ready";
            }



            //string extractedText = ocrResult.Text;

        }

        public string GetText()
        {
            return resultString;
        }

        private async Task LoadImage(StorageFile file)
        {
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);

                bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                Windows.Graphics.Imaging.PixelDataProvider pixelData = await decoder.GetPixelDataAsync();


            }

        }


        private async Task LoadSampleImage()
        {

            var file = await KnownFolders.MusicLibrary.GetFileAsync("mortresult.png");

            await LoadImage(file);
        }


    }
}
