using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DictionaryCoder.Coder
{

    public class Coder
    {
        int LookAheadBufferSize { get; set; }
        int CodeBufferSize { get; set; }
        int offsetBytesSize;
        int lengthBytesSize;
        int sizeSum;

        public Coder(int CodeBufferSize, int LookAheadBufferSize)
        {
            this.CodeBufferSize = CodeBufferSize;
            this.LookAheadBufferSize = LookAheadBufferSize;
            offsetBytesSize = (int)Math.Floor(Math.Log10(CodeBufferSize + LookAheadBufferSize) / Math.Log10(256) + 1);
            lengthBytesSize = (int)Math.Floor(Math.Log10(LookAheadBufferSize) / Math.Log10(256) + 1);
            sizeSum = offsetBytesSize + lengthBytesSize + 1;
        }

        /**
        * Compresses given data using LZ77 algorithm
        * Parameters : fileStream - file stream from witch to read and compress data from.
        *              outputPath - path to where create and write encoded data to
        */
        public void EncodeLZ77(Stream fileStream, string outputPath)
        {
            byte[] codeBuffer = new byte[CodeBufferSize];
            byte[] lookAheadBuffer = new byte[LookAheadBufferSize];

            int shift = 0;
            int bytesToRead = 0;
            int inputSize = -1;
            using (Stream outputStream = File.OpenWrite(outputPath))
            {
                BinaryWriter binaryWriter = new BinaryWriter(outputStream);

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

                    // Check whether the file had been entirely processed.
                    if (lookAheadBuffer[0] == 0x00 && inputSize == 0) break;

                    // Longest found matching sequence between lookahead and code buffers
                    Sequence repetition = FindRepetition(codeBuffer, lookAheadBuffer);

                    WriteOutput(binaryWriter, repetition);

                    shift = repetition.Length + 1;
                }
            }
        }

        struct WriteStruct
        {
            public int MaxSize { get; set; }
            public byte Data { get; set; }
        }

        /**
        * Structure containing information about LZ77 encoding output value.
        */
        struct Sequence
        {
            public int Length { get; set; }
            public int Offset { get; set; }
            public byte NextChar { get; set; }
        }

        /**
        * Finds the longest sequence of bytes from lookAheadBuffer repeated within window buffer,
        * which is a composition of codeBuffer and lookAheadBuffer.
        * Parameters  : codeBuffer - byte array used as dictionary to search for repetitions.
        *             : lookAheadBuffer - byte array containing input data.
        * Return      : longest repeated sequence.
        */
        private Sequence FindRepetition(byte[] codeBuffer, byte[] lookAheadBuffer)
        {
            byte[] windowBuffer = codeBuffer.Concat(lookAheadBuffer).ToArray();
            List<Sequence> sequences = new List<Sequence>();

            int offset = 0;
            while (offset < codeBuffer.Length)
            {
                int length = 0;
                int codePointer = offset;
                int lookAheadPointer = codeBuffer.Length;

                while (windowBuffer[codePointer] == windowBuffer[lookAheadPointer])
                {
                    // Found longest possible sequence
                    if (++length == lookAheadBuffer.Length) break;

                    // Index out of bounds
                    if (++lookAheadPointer >= windowBuffer.Length) break;

                    codePointer++;
                }

                sequences.Add(new Sequence
                {
                    Length = length,
                    Offset = offset,
                    NextChar = windowBuffer[lookAheadPointer],
                });

                offset++;
            }

            // Finding the longest matching sequence
            int maxLength = sequences.Max(s => s.Length);
            Sequence longestSequence = sequences.FirstOrDefault(s => s.Length == maxLength);
            return longestSequence;
        }

        /**
         * Creates and writes down the encoded sequence to a given file path.
         * Parameters  : outputStream stream, which to use to write data into a file.
         *             : sequence - sequence struct containing information to write down.
         */
        private void WriteOutput(BinaryWriter binaryWriter, Sequence sequence)
        {
            int offset = sequence.Offset;
            int length = sequence.Length;
            byte nextChar = sequence.NextChar;
            //Console.WriteLine($"\n({offset},{length}){nextChar}");


            byte[] offsetData = new byte[offsetBytesSize];
            byte[] lengthData = new byte[lengthBytesSize];

            try
            {
                int missing;
                offsetData = BitConverter.GetBytes(offset);

                missing = offsetBytesSize - offsetData.Length;
                if (missing > 0) offsetData = offsetData.Concat(new byte[missing]).ToArray();


                lengthData = BitConverter.GetBytes(length);
                missing = lengthBytesSize - lengthData.Length;
                if (missing > 0) lengthData = lengthData.Concat(new byte[missing]).ToArray();

                offsetData = offsetData.Take(offsetBytesSize).ToArray();
                lengthData = lengthData.Take(lengthBytesSize).ToArray();
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(offsetData);
                    Array.Reverse(lengthData);
                }

                //Console.WriteLine("offset");
                //foreach (byte b in offsetData) Console.WriteLine(b);
                //Console.WriteLine("length");
                //foreach (byte b in lengthData) Console.WriteLine(b);

                byte[] writeBuffer = offsetData.Concat(lengthData.Concat(new byte[] { nextChar })).ToArray();
                if (writeBuffer.Length != sizeSum)
                {
                    Console.WriteLine("Invalid size: " + sizeSum + "|" + writeBuffer.Length);
                    Console.WriteLine("Offsetbuf: " + offsetData.Length);
                    Console.WriteLine("lengthbuf: " + lengthData.Length);
                }
                binaryWriter.Write(writeBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("ZAAAPIS: " + e.Message);
            }
        }

        /**
        * Decompresses given data using LZ77 algorithm.
        * Parameters : fileStream - file stream to whitch write decompressed data.
        *              outputPath - path to where create and write encoded data to
        */
        public void DecodeLZ77(Stream fileStream, string outputPath)
        {

            int bytesToRead = sizeSum;
            byte[] readBuffer = new byte[bytesToRead];

            byte[] lengthBits, offsetBits, dataBits;

            using (FileStream outputStream = File.OpenWrite(outputPath))
            {
                BinaryWriter binaryWriter = new BinaryWriter(outputStream);

                while (bytesToRead > 0)
                {
                    int bytesRead = fileStream.Read(readBuffer, 0, bytesToRead);
                    if (bytesRead == 0) break;
                    if (bytesRead != bytesToRead) Console.WriteLine("Invalid file compression!");



                }
            }
        }
    }
}


