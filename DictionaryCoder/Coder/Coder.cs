using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DictionaryCoder.Coder
{

    public class Coder
    {
        int LookAheadBufferSize { get; set; }
        int CodeBufferSize { get; set; }
        int OffsetBytesCount;
        int LengthBytesCount;
        int SizeSum;

        public Coder(int CodeBufferSize, int LookAheadBufferSize)
        {
            this.CodeBufferSize = CodeBufferSize;
            this.LookAheadBufferSize = LookAheadBufferSize;
            OffsetBytesCount = (int)Math.Floor(Math.Log10(CodeBufferSize + LookAheadBufferSize) / Math.Log10(256) + 1);
            LengthBytesCount = (int)Math.Floor(Math.Log10(LookAheadBufferSize) / Math.Log10(256) + 1);
            SizeSum = 3;
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
        * Compresses given data using LZ77 algorithm
        * Parameters : fileStream - file stream from witch to read and compress data from.
        *              outputPath - path to where create and write encoded data to
        */
        public void EncodeLZ77(Stream fileStream, string outputPath)
        {
            byte[] codeBuffer = new byte[CodeBufferSize];
            byte[] lookAheadBuffer = new byte[LookAheadBufferSize + 1];

            int shift = 0;
            int bytesToRead = 0;
            int inputSize = -1;
            bool initialize = true;
            using (Stream outputStream = File.OpenWrite(outputPath))
            {
                BinaryWriter binaryWriter = new BinaryWriter(outputStream);

                while (LookAheadBufferSize > 0)
                {
                    byte[] readBuffer = new byte[LookAheadBufferSize + 1];

                    // bytesToRead == 0 means it's the beggining of compression, therefore fully load the buffer.
                    // Min(shift, LookAheadBufferSize) Makes sure not to exceed the buffer's capacity.
                    bytesToRead = bytesToRead == 0 ? lookAheadBuffer.Length : Math.Min(shift, lookAheadBuffer.Length);

                    if (inputSize != 0) inputSize = fileStream.Read(readBuffer, 0, bytesToRead);

                    // Filling code buffer with leading byte of read bytes.
                    if (shift == 0) Array.Fill(codeBuffer, readBuffer[0]);

                    // Filling the buffers with the amount of bytes processed in previous iteration.
                    codeBuffer = codeBuffer.Skip(shift).Concat(lookAheadBuffer.Take(shift)).ToArray();
                    lookAheadBuffer = lookAheadBuffer.Skip(shift).Concat(readBuffer.Take(bytesToRead)).ToArray();

                    // Check whether the file had been entirely processed.
                    if (!initialize & lookAheadBuffer[0] == 0x00 && inputSize == 0) break;
                    initialize = false;

                    // Longest found matching sequence between lookahead and code buffers
                    Sequence repetition = FindRepetition(codeBuffer, lookAheadBuffer);

                    WriteEncoded(binaryWriter, repetition);

                    shift = repetition.Length + 1;
                }
            }
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
                    // Index out of bounds
                    if (++lookAheadPointer == windowBuffer.Length) break;
                    // Found longest possible sequence
                    if (++length == LookAheadBufferSize) break;


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
        private void WriteEncoded(BinaryWriter binaryWriter, Sequence sequence)
        {
            int offset = sequence.Offset;
            int length = sequence.Length;
            byte nextChar = sequence.NextChar;
            Console.WriteLine($"\n({offset},{length}){(char)nextChar}");

            // Saving offset and length to bytearrays of given size in little endian notation
            byte[] offsetData = FormatToByteArray(OffsetBytesCount, offset);
            byte[] lengthData = FormatToByteArray(LengthBytesCount, length);

            //writing length information within the same byte as last part of offset
            // if you can save a byte do this
            offsetData[^1] = (byte)(offsetData[^1] | (lengthData[0] << 4));
            byte[] writeBuffer = offsetData.Concat(new byte[] { nextChar }).ToArray();
            binaryWriter.Write(writeBuffer);
        }

        /**
         * Parameters  : bytesCount - count of bytes to wtore data within.
         *             : data - integer value to format to byte array.
         * Return      : bytearray containing given information within desired size
         *               written in little endian notation
         */
        byte[] FormatToByteArray(int bytesCount, int data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
            {
                byteArray = byteArray.Take(bytesCount).ToArray();
            }
            else
            {
                byteArray = byteArray.Skip(byteArray.Length - bytesCount).ToArray();
                Array.Reverse(byteArray);
            }
            return byteArray;
        }

        /**
        * Decompresses given data using LZ77 algorithm.
        * Parameters : fileStream - file stream to whitch write decompressed data.
        *              outputPath - path to where create and write encoded data to
        */
        public void DecodeLZ77(Stream fileStream, string outputPath)
        {
            int bytesToRead = SizeSum;
            byte[] readBuffer = new byte[bytesToRead];
            byte[] codeBuffer = new byte[CodeBufferSize];
            bool initialize = true;
            string output = "";
            byte[] outputArray = new byte[0];

            Console.WriteLine("DECOOODEEE");
            using (Stream outputStream = new FileStream("test.txt", FileMode.Open, FileAccess.Write))
            {

                Console.WriteLine();
                StreamWriter streamWriter = new StreamWriter(outputStream);

                streamWriter.Flush();
                while (bytesToRead > 0)
                {
                    int bytesRead = fileStream.Read(readBuffer, 0, bytesToRead);
                    if (bytesRead == 0) break;
                    if (bytesRead != bytesToRead) Console.WriteLine("Invalid file compression!");

                    byte[] lenBytes = new byte[] { (byte)((readBuffer[^2] & 0xF0) >> 4), 0x00 };

                    // Remove length data from the array
                    readBuffer[^2] = (byte)(readBuffer[^2] & 0x0F);

                    int offset = BitConverter.ToInt16(readBuffer);
                    int length = BitConverter.ToInt16(lenBytes);
                    byte nextChar = readBuffer[^1];
                    Console.WriteLine($"\n({offset},{length}){(char)nextChar}");

                    // Filling codeBuffer with first byte read if it's the beginning of decoding
                    if (initialize)
                    {
                        Array.Fill(codeBuffer, (byte)0x01);
                        initialize = false;
                    }
                    byte[] tempbuf = new byte[codeBuffer.Length];
                    Array.Copy(codeBuffer, tempbuf, codeBuffer.Length);

                    byte[] decoded = new byte[length + 1];
                    for (int i = 0; i < length; i++)
                    {
                        decoded[i] = tempbuf[offset + i];
                        tempbuf = tempbuf.Concat(decoded.Skip(i).Take(1)).ToArray();
                    }
                    decoded[^1] = nextChar;
                    codeBuffer = codeBuffer.Skip(decoded.Length).Concat(decoded).ToArray();
                    outputArray = outputArray.Concat(decoded).ToArray();
                    //string message = Encoding.UTF8.GetString(decoded);
                    //output += message;
                    //Console.WriteLine("output: " + output);
                    //Console.WriteLine("message: " + message);
                    //streamWriter.WriteLine(message);
                }
                string message = Encoding.UTF8.GetString(outputArray);
                Console.WriteLine(message);
            }
        }
    }
}


