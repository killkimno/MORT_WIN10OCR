using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Globalization;

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

    //ready = 이미지 받을 준비.
    //process = ocr 처리중
    //
    public enum ProcessType
    {
        Ready = 0, Process = 1, ImgLoading =2,
    }

    public class Class1
    {
        string resultString = "";
        int result = 0;
        SoftwareBitmap bitmap = null;
        SoftwareBitmap bitmap2 = null;
        private ProcessType processType = ProcessType.Ready;
        public static Class1 instance = new Class1();
        public OcrEngine ocrEngine = null;

        //OCR 에 사용할 이미지 저장.
        public static string TestOpenCv(List<int> r, List<int> g, List<int> b, int x, int y)
        {
            if(instance.processType != ProcessType.Process)
            {
                instance.LoadBitMapFromData(r, g, b, x, y);
            }
            
            //instance.OCR();
            //return "...!"
            return instance.resultString;
        }

        public void LoadBitMapFromData(List<int> r, List<int> g, List<int> b, int x, int y)
        {
            processType = ProcessType.ImgLoading;
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
            processType = ProcessType.Ready;
        }

        //ocr 처리.
        public static string ProcessOCR()
        {
            instance.OCR();
           
            return instance.resultString;
        }

        //OCR 사용 가능한지 확인.
        public static bool GetIsAvailable()
        {
            bool isAvailable = false;

            if(instance.processType == ProcessType.Ready)
            {
                isAvailable = true;
            }

            return isAvailable;
        }
        
        public static List<string> GetAvailableLanguageList()
        {
            List<string> codeList = new List<string>();
            IReadOnlyList<Language> list = OcrEngine.AvailableRecognizerLanguages;
            
            for(int i = 0; i < list.Count; i++)
            {
                codeList.Add(list[i].LanguageTag + "," + list[i].DisplayName);
            }

            return codeList;
        }

        //OCR 처리
        public static void InitOcr(string code)
        {            
            instance.ocrEngine = null;
            if (code == "")
            {
                instance.ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            }
            else
            {              
                Language ocrLanguage = new Language(code);
                if (OcrEngine.IsLanguageSupported(ocrLanguage))
                {
                    instance.ocrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage);
                }
                else
                {
                    instance.ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                }
                   
            }
            
            instance.processType = ProcessType.Ready;
            
        }

        //이 dll 자체를 사용 가능한지 확인.
        public static bool GetIsAvailableDLL()
        {
            bool isAvailable = false;

            return isAvailable;
        }

        private async void OCR()
        {
            //resultString = "sdfsdf@!ER";
            if (processType != ProcessType.Process)
            {
                //await LoadSampleImage();
                /*
                OcrEngine ocrEngine = null;
                if (code == "")
                {
                    ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                }
                else
                {
                    Language ocrLanguage = new Language(code);
                    ocrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage);
                }
                */

                if (bitmap2 != null && ocrEngine != null)
                {
                    processType = ProcessType.Process;
                    //resultString = "1";
                    OcrResult ocrResult = await ocrEngine.RecognizeAsync(bitmap2);
                    resultString = ocrResult.Text;
                    processType = ProcessType.Ready;
                }
                else
                {
                    resultString = "bitmap null";
                    processType = ProcessType.Ready;
                }
            }
            else
            {
                resultString = " not ready";
            }

           
        }

        public static string GetText()
        {
            return instance.resultString;
        }

        public static string Test(byte[] test)
        {
           // instance.OCR();

            return instance.resultString;
        }

        public static string TestMat(byte[] data)
        {
            return "";
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
