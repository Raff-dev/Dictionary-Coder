using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DictionaryCoder.Coder
{

    public class Coder
    {
        int LookAheadBufferSize { get; set; }
        int CodeBufferSize { get; set; }

        public Coder(int CodeBufferSize, int LookAheadBufferSize)
        {
            this.CodeBufferSize = CodeBufferSize;
            this.LookAheadBufferSize = LookAheadBufferSize;
        }

        /**
        * Function encodes given data using LZ77 algorithm
        * Parameters : fileStream - file stream from witch to read bytes and compress data from.
        *              outputPath - path to where create and write encoded data to
        */
        public void EncodeLZ77(Stream fileStream, string outputPath)
        {
            byte[] codeBuffer = new byte[CodeBufferSize];
            byte[] lookAheadBuffer = new byte[LookAheadBufferSize];

            int shift = 0;
            int bytesToRead = 0;
            int inputSize = -1;

            while (LookAheadBufferSize > 0)
            {
                byte[] readBuffer = new byte[LookAheadBufferSize];

                // bytesToRead == 0 means it's the beggining of compression, therefore fully load the buffer.
                // Min(shift, LookAheadBufferSize) Makes sure not to exceed the buffer's capacity.
                bytesToRead = bytesToRead == 0 ? LookAheadBufferSize : Math.Min(shift, LookAheadBufferSize);

                if (inputSize != 0) inputSize = fileStream.Read(readBuffer, 0, bytesToRead);

                // Filling code buffer with leading byte of read bytes.
                if (shift == 0) Array.Fill(codeBuffer, readBuffer[0]);

                // Filling the buffers with the amount of bytes processed in previous iteration.
                codeBuffer = codeBuffer.Skip(shift).Concat(lookAheadBuffer.Take(shift)).ToArray();
                lookAheadBuffer = lookAheadBuffer.Skip(shift).Concat(readBuffer.Take(bytesToRead)).ToArray();

                // Check wether the file had been entirely processed.
                if (lookAheadBuffer[0] == 0x00 && inputSize == 0)
                {
                    Console.WriteLine("\n\nCompression finished!");
                    break;
                }

                // Longest found matching sequence between lookahead and code buffers
                Sequence repetition = FindRepetition(codeBuffer, lookAheadBuffer);
                WriteOutput(repetition, outputPath);

                shift = repetition.Length + 1;
            }
        }
        /*
         * Creates and writes down the encoded sequence to a given file path.
         */
        private void WriteOutput(Sequence sequence, string outputPath)
        {
            int offset = sequence.offset;
            int length = sequence.Length;
            string nextChar = (char)sequence.NextChar == ' ' ? "space" : "" + (char)sequence.NextChar;
            string repetition = length == 0 ? "None" : sequence.Repetition;
            Console.WriteLine($"\n({offset},{length}){nextChar}: {repetition}");
        }

        /*
        * Structure which contains information about LZ77 encoding output value
        */
        struct Sequence
        {
            public int Length { get; set; }
            public int offset { get; set; }
            public byte NextChar { get; set; }
            public string Repetition { get; set; }
        }

        /**
        * Finds the longest repeated sequence from lookAheadBuffer within window buffer
        * Parameters  : codeBuffer - byte array used as dictionary to search for repetitions.
        *             : lookAheadBuffer - byte array containing input data.
        * Return      : longest repeated sequence.
        */
        private Sequence FindRepetition(byte[] codeBuffer, byte[] lookAheadBuffer)
        {
            byte[] WindowBuffer = codeBuffer.Concat(lookAheadBuffer).ToArray();
            List<Sequence> Sequences = new List<Sequence>();

            int offset = 0;
            while (offset < codeBuffer.Length)
            {
                string repetition = "";
                int Length = 0;
                int CodePointer = offset;
                int LookAheadPointer = codeBuffer.Length;

                while (WindowBuffer[CodePointer] == WindowBuffer[LookAheadPointer])
                {
                    Length++;
                    CodePointer++;
                    repetition += (char)WindowBuffer[LookAheadPointer];
                    if (++LookAheadPointer > WindowBuffer.Length) break;
                }

                Sequences.Add(new Sequence
                {
                    Length = Length,
                    offset = offset,
                    NextChar = WindowBuffer[LookAheadPointer],
                    Repetition = repetition
                }); ;

                offset++;
            }

            //finding the longest matching sequence
            int MaxLength = Sequences.Max(s => s.Length);
            return Sequences.FirstOrDefault(s => s.Length == MaxLength);
        }

        public void DecodeLZ77()
        {
            throw new NotImplementedException("function not implemented");
        }
    }

}


