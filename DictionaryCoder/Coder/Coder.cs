using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DictionaryCoder.Coder
{

    public class Coder
    {

        static int InputBufferSize = 128;
        static int CodeBufferSize = 128;


        public Coder() { }


        public void WriteOutput()
        {
            throw new NotImplementedException("function not implemented");
        }

        public void Decode()
        {
            throw new NotImplementedException("function not implemented");

        }
        public string EncodeLZ77(string data)
        {
            if (data.Length <= 0) throw new ArgumentException("data is invalid");
            byte[] inputbuffer = new byte[InputBufferSize];
            byte[] codeBuffer = new byte[CodeBufferSize];

            string Encoded = "";
            int codePointer = 0;
            int inputPointer = CodeBufferSize;


            // main while loop
            while (inputPointer + InputBufferSize <= data.Length - 1)
            {
                //iterate over code buffer to find match, when match found, try to find longer
                for (int i = inputPointer; i >= 0; i--)
                {
                    if (data[inputPointer] == data[codePointer])
                    {
                        string repetition = FindRepetition(out int offset, data, codePointer, inputPointer);
                        if (repetition.Length >= 0)
                        {
                            int shift = repetition.Length + 1;
                            inputPointer += shift;
                            codePointer += shift;
                            char lastChar = data[codePointer - 1];
                            WriteOutput(offset, repetition.Length, lastChar);
                            Encoded += $"{offset},{repetition.Length},{}";
                        }
                    }
                }
            }
            return Encoded;
        }

        private void WriteOutput(int offset, int Length, char lastChar)
        {


        }

        private string FindRepetition(out int offset, string data, int codePointer, int inputPointer)
        {
            offset = 0;
            List<string> reps = new List<string>();
            bool isRepeating = true;
            bool isFinished = true;
            while (isFinished)
            {
                offset = 0;
                string repetition = "";
                while (isRepeating)
                {
                    offset++;
                    if (data[codePointer] == data[inputPointer])
                    {
                        repetition += data[codePointer];
                        codePointer++;
                        inputPointer++;
                    }
                    else break;
                }
            }

            int MaxLength = reps.Max(s => s.Length);
            string LongestRepetition = reps.FirstOrDefault(s => s.Length == MaxLength);
            return LongestRepetition;
        }
    }
}


