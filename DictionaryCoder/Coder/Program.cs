using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DictionaryCoder.Coder
{
    class Program
    {
        [Flags]
        enum konia
        {
            asd
        }
        static void Main(string[] args)
        {
            string Data;
            string Encoded;
            string InputFile = GetInputFile(out int readMode);
            Coder Coder = new Coder();



            string asd = "qwe";
            asd += 0x55;
            asd += 0b0010_0110_0000_0011;
            Console.WriteLine(asd);

            using (StreamReader sr = new StreamReader(InputFile))
            {
                Data = sr.ReadToEnd();
            }

            if (Data != null && Data.Length >= 0)
            {
                Encoded = Coder.EncodeLZ77(Data);
            }
            else
            {
                //handle null data
            }
            // return Viev(CodeMessage)
        }

        private static string GetInputFile(out int readMode)
        {
            //check for text
            //check for file
            readMode = 0;
            bool FileAttached = false;
            bool TextAttached = true;
            if (FileAttached) readMode += 1;
            if (TextAttached) readMode += 2;
            //write text to file

            string InputFile = "text.txt";
            return InputFile;
        }
    }
}

