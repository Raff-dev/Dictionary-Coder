using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Coder;

namespace DictionaryCoder.Coder
{

    public class Coder
    {
        int LookAheadBufferSize { get; set; }
        int CodeBufferSize { get; set; }
        int OffsetBytesCount;
        int LengthBytesCount;
        int SizeSum;

        public Coder(int CodeBufferSize = 4095, int LookAheadBufferSize = 15)
        {
            this.CodeBufferSize = CodeBufferSize;
            this.LookAheadBufferSize = LookAheadBufferSize;

            // Calculation of count of bytes needed to write down given numbers.
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
        * Parameters : inputStream - file stream from witch to read and compress data from.
        *              outputPath - path to where create and write encoded data to
        */
        public void EncodeLZ77(string inputFileName, string outputFileName)
        {
            using (Stream inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read))
            {
                byte[] codeBuffer = new byte[CodeBufferSize];
                byte[] lookAheadBuffer = new byte[LookAheadBufferSize + 1];

                int shift = 0;
                int bytesToRead = 0;
                int bytesEncoded = 0;
                int inputSize = -1;

                ProgressBar progressBar = new ProgressBar()
                {
                    OffsetY = 0,
                    TitleMessages = new List<string>() { $"Compressing {inputFileName}", $"Size: {String.Format("{0:0.00}", inputStream.Length / 1024.0)}kB" },
                    FinishMessages = new List<string>() { $"Compressing to {outputFileName} completed." },
                    MaxValue = inputStream.Length,
                    StepsCount = 50
                };

                using (Stream outputStream = File.OpenWrite(outputFileName))
                {
                    BinaryWriter binaryWriter = new BinaryWriter(outputStream);

                    while (LookAheadBufferSize > 0)
                    {
                        byte[] readBuffer = new byte[LookAheadBufferSize + 1];

                        // bytesToRead == 0 means it's the beggining of compression, therefore fully load the buffer.
                        // Min(shift, LookAheadBufferSize) Makes sure not to exceed the buffer's capacity.
                        bytesToRead = bytesToRead == 0 ? lookAheadBuffer.Length : Math.Min(shift, lookAheadBuffer.Length);

                        if (inputSize != 0) inputSize = inputStream.Read(readBuffer, 0, bytesToRead);

                        // Filling the buffers with the amount of bytes processed in previous iteration.
                        codeBuffer = codeBuffer.Skip(shift).Concat(lookAheadBuffer.Take(shift)).ToArray();

                        if (shift == 0) Array.Copy(readBuffer, lookAheadBuffer, bytesToRead);
                        else lookAheadBuffer = lookAheadBuffer.Skip(shift).Concat(readBuffer.Take(bytesToRead)).ToArray();

                        // Longest found matching sequence between lookahead and code buffers
                        Sequence repetition = FindRepetition(codeBuffer, lookAheadBuffer);

                        WriteEncoded(binaryWriter, repetition);

                        shift = repetition.Length + 1;
                        bytesEncoded += shift;

                        // Check whether the file had been entirely processed.
                        if (bytesEncoded >= inputStream.Length)
                        {
                            progressBar.FinishMessages.Add($"Size: {String.Format("{0:0.00}", outputStream.Length / 1024.0)}kB");
                            progressBar.Value += shift;
                            break;
                        }

                        progressBar.Value += shift;
                    }
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
            //Console.WriteLine($"\n({offset},{length}){(char)nextChar}");

            // Saving offset and length to bytearrays of previously calculated size
            byte[] offsetData = IntToByteArray(OffsetBytesCount, offset);
            byte[] lengthData = IntToByteArray(LengthBytesCount, length);

            //writing length information within the same byte as last part of offset
            // if you can save a byte do this
            offsetData[^1] = (byte)(offsetData[^1] | (lengthData[0] << 4));
            byte[] writeBuffer = offsetData.Concat(new byte[] { nextChar }).ToArray();
            binaryWriter.Write(writeBuffer);
        }

        /**
         * Parameters  : bytesCount - count of bytes to contain data within.
         *             : data - integer value to format to byte array.
         * Return      : bytearray containing given information contained within desired size,
         *               written in little endian notation
         * Byte arrangement difference: (example for 4 bytes)
         *               Big endian    : most significant byte at the beggining -> 04 03 02 01
         *               Little endian : 01 02 03 04 <- most significant byte at the end
         */
        byte[] IntToByteArray(int bytesCount, int data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            if (!BitConverter.IsLittleEndian) Array.Reverse(byteArray);
            byteArray = byteArray.Take(bytesCount).ToArray();
            return byteArray;
        }

        /**
        * Decompresses given data using LZ77 algorithm.
        * Compressed data has a structure of <O,L,C>, where:
        *           O - Offset of the first character
        *           L - Length of the repeated sequence
        *           C - Character following the sentence
        * Parameters : inputStream - file stream to whitch write decompressed data.
        *              outputPath - path to where create and write encoded data to
        */
        public void DecodeLZ77(string inputFileName, string outputFileName)
        {
            using (Stream inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read))
            {
                int bytesDecoded = 0;
                int bytesToRead = SizeSum;
                byte[] readBuffer = new byte[bytesToRead];
                byte[] codeBuffer = new byte[CodeBufferSize];
                byte[] outputArray = new byte[0];

                ProgressBar progressBar = new ProgressBar()
                {
                    OffsetY = Console.CursorTop + 1,
                    TitleMessages = new List<string>() {
                        $"Decompressing {inputFileName}", $"Size: {String.Format("{0:0.00}", inputStream.Length / 1024.0)}kB"  },
                    FinishMessages = new List<string>() {
                        $"Decompression to {outputFileName} completed." },
                    MaxValue = inputStream.Length,
                    StepsCount = 50
                };

                using (Stream outputStream = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(outputStream, Encoding.UTF8))
                    {
                        while (bytesToRead > 0)
                        {
                            int bytesRead = inputStream.Read(readBuffer, 0, bytesToRead);
                            if (bytesRead == 0) break;
                            if (bytesRead != bytesToRead) Console.WriteLine("Invalid data format!");

                            // Extract length information from second byte
                            byte[] lenBytes = new byte[] { (byte)((readBuffer[^2] & 0xF0) >> 4), 0x00 };

                            // Remove length data from the array
                            readBuffer[^2] = (byte)(readBuffer[^2] & 0x0F);

                            int offset = BitConverter.ToInt16(readBuffer);
                            int length = BitConverter.ToInt16(lenBytes);
                            byte nextChar = readBuffer[^1];
                            //Console.WriteLine($"\n({offset},{length}){(char)nextChar}");

                            byte[] decoded = new byte[length + 1];
                            byte[] tempbuf = new byte[codeBuffer.Length];
                            Array.Copy(codeBuffer, tempbuf, codeBuffer.Length);

                            for (int i = 0; i < length; i++)
                            {
                                decoded[i] = tempbuf[offset + i];

                                // Appending decoded byte to the temporary bufer,
                                // so data can be continuously read from within lookahead buffer.
                                tempbuf = tempbuf.Concat(decoded.Skip(i).Take(1)).ToArray();
                            }
                            decoded[^1] = nextChar;

                            // Remove unnecessary bytes, append newly decoded ones.
                            codeBuffer = codeBuffer.Skip(decoded.Length).Concat(decoded).ToArray();

                            // Store decoded information into an array.
                            outputArray = outputArray.Concat(decoded).ToArray();

                            // Add up length of decoded bytes, check for decompression completion.
                            bytesDecoded += bytesRead;
                            if (bytesDecoded >= inputStream.Length)
                            {
                                progressBar.FinishMessages.Add($"Size: {String.Format("{0:0.00}", (outputStream.Length + bytesRead) / 1024.0)}kB");
                                progressBar.Value += bytesRead;
                                break;
                            }
                            progressBar.Value += bytesRead;
                        }

                        string message = Encoding.UTF8.GetString(outputArray);
                        streamWriter.WriteLine(message);
                    };
                }
            }
        }
    }
}


