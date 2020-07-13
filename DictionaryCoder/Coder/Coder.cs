using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DictionaryCoder.Coder
{

    public class Coder
    {
        int LookAheadBufferSize { get; set; }
        int CodeBufferSize { get; set; }

        public Coder(int LookAheadBufferSize, int codeBufferSize)
        {
            this.LookAheadBufferSize = LookAheadBufferSize;
            this.CodeBufferSize = codeBufferSize;
        }

        /**
        Function encodes given data using LZ77 algorithm
        @param: data
        @return: 
        */
        public int EncodeLZ77(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (FileStream fsSource = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] CodeBuffer = new byte[CodeBufferSize];
                    byte[] LookAheadBuffer = new byte[LookAheadBufferSize];

                    int Offset = 0;
                    while (LookAheadBufferSize > 0)
                    {
                        // Read may return anything from 0 to LookAheadBufferSize.
                        int InputSize = fsSource.Read(LookAheadBuffer, Offset, LookAheadBufferSize);

                        Console.WriteLine(InputSize);
                        // Break when the end of the file is reached.
                        if (InputSize == 0) break;
                        if (Offset == 0) Array.Fill(CodeBuffer, LookAheadBuffer[0]);

                        Console.WriteLine(CodeBuffer + " | " + LookAheadBuffer);

                        //iterate over code buffer to find matching sequence
                        Sequence Repetition = FindRepetition(CodeBuffer, LookAheadBuffer);

                        WriteOutput(Repetition.Offset, Repetition.Length, Repetition.NextChar);
                        Offset += Repetition.Length + 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"not keked {e}");
                // Handle invalid input or null data here
                return -1;
            }
            Console.WriteLine("keked");

            return 1;
        }

        private void WriteOutput(int offset, int length, byte nextChar)
        {
            Console.WriteLine($"({offset},{length}){nextChar}");
        }

        /*
        Structure that holds information about LZ77 encoding output value
        */
        struct Sequence
        {
            public int Length { get; set; }
            public int Offset { get; set; }
            public byte NextChar { get; set; }
        }

        /**
        finds longest repeated sequence from lookaheadbuffer within window buffer
        @return: longest repeated sequence
        */
        private Sequence FindRepetition(byte[] windowBuffer, byte[] lookAheadBuffer)
        {
            List<Sequence> Sequences = new List<Sequence>();
            bool IsFinished = true;
            int Offset = 0;

            while (IsFinished)
            {
                if (Offset >= windowBuffer.Length) break;
                bool IsRepeating = true;
                int Length = 0;
                int CodePointer = 0;

                ///?????
                int LookAheadPointer = 0;

                while (IsRepeating)
                {
                    if (windowBuffer[CodePointer] == windowBuffer[LookAheadPointer])
                    {
                        CodePointer++;
                        LookAheadPointer++;
                    }
                    else
                    {
                        Sequences.Add(new Sequence
                        {
                            Length = Length,
                            Offset = Offset,
                            NextChar = windowBuffer[CodePointer]
                        });
                        IsRepeating = false;
                    }
                    Length++;
                }
                Offset++;
            }

            //finding the longest matching sequence
            int MaxLength = Sequences.Max(s => s.Length);
            Sequence LongestRepetition = Sequences.FirstOrDefault(s => s.Length == MaxLength);
            return LongestRepetition;
        }

        public void DecodeLZ77()
        {
            throw new NotImplementedException("function not implemented");
        }

    }

}


