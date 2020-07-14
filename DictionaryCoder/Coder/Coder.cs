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

        public Coder(int codeBufferSize, int lookAheadBufferSize)
        {
            this.CodeBufferSize = codeBufferSize;
            this.LookAheadBufferSize = lookAheadBufferSize;
        }

        /**
        Function encodes given data using LZ77 algorithm
        @param: data
        @return: 
        */
        public void EncodeLZ77(string inputFilePath, string outputFilePath)
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
                        byte[] readBuffer = new byte[LookAheadBufferSize];
                        // Read may return anything from 0 to LookAheadBufferSize.
                        Console.WriteLine("Offset: " + Offset);
                        int InputSize = fsSource.Read(LookAheadBuffer, 0, LookAheadBufferSize);
                        Console.WriteLine("Input Size: " + InputSize);

                        foreach (var content in LookAheadBuffer) Console.Write((char)content);
                        Console.WriteLine();

                        // Break when the end of the file is reached.
                        if (InputSize == 0) break;

                        // Fill Code buffer with first byte
                        if (Offset == 0) Array.Fill(CodeBuffer, LookAheadBuffer[0]);


                        /*
                        */
                        while (readBuffer.Length > 0)
                        {

                            //iterate over code buffer to find matching sequence
                            Sequence Repetition = FindRepetition(CodeBuffer, LookAheadBuffer);

                            WriteOutput(Repetition.Offset, Repetition.Length, Repetition.NextChar);

                            int Shift = Repetition.Length + 1;
                            CodeBuffer = CodeBuffer.Skip(Shift).Concat(LookAheadBuffer.Take(Shift)).ToArray();
                            LookAheadBuffer = LookAheadBuffer.Skip(Shift).Concat(readBuffer.Take(Shift)).ToArray();
                            readBuffer = readBuffer.Skip(Shift).ToArray();

                            Offset += Shift;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"not keked {e}");
            }
        }

        private void WriteOutput(int offset, int length, byte nextChar)
        {
            Console.WriteLine($"({offset},{length}){(char)nextChar}");
        }

        /*
        Structure that contains information about LZ77 encoding output value
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
        private Sequence FindRepetition(byte[] codeBuffer, byte[] lookAheadBuffer)
        {
            bool Debug = false;

            byte[] WindowBuffer = codeBuffer.Concat(lookAheadBuffer).ToArray();
            List<Sequence> Sequences = new List<Sequence>();

            int Offset = 0;
            while (Offset < codeBuffer.Length)
            {
                int Length = 0;
                int CodePointer = Offset;
                int LookAheadPointer = codeBuffer.Length;

                if (Debug) Console.WriteLine("Codep " + CodePointer);

                while (WindowBuffer[CodePointer] == WindowBuffer[LookAheadPointer])
                {
                    if (Debug) Console.WriteLine("content :" + WindowBuffer[CodePointer]);

                    Length++;
                    CodePointer++;
                    if (++LookAheadPointer > WindowBuffer.Length) break;
                }

                Sequences.Add(new Sequence
                {
                    Length = Length,
                    Offset = Offset,
                    NextChar = WindowBuffer[LookAheadPointer]
                });

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


