using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Converter;
using Extensions;

namespace Audio_Code_Testbed
{
    class Program
    {

        static void Main(string[] args)
        {
            //http://www.topherlee.com/software/pcm-tut-wavformat.html
            //http://soundfile.sapp.org/doc/WaveFormat/
            //https://xiph.org/flac/documentation_format_overview.html
            Interface ui = new Interface();
            ui.MenuMain();
        }
    }

    static class Resizer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Resize(short value)
        {
            return value / 32768.0;
        }
        public static double Resize(byte value)
        {
            return value / 128.0;
        }
        public static double Resize(int value)
        {
            return value / 2147483658.0;
        }
        public static double Resize(long value)
        {
            return value / 9223372036854775808.0;
        }
        public static void ResizeToInterger(double doubleValue, out short value)
        {
            value = (short)(doubleValue * 32768);
        }
        public static void ResizeToInterger(double doubleValue, out int value)
        {
            value = (int)(doubleValue * 2147483658.0);
        }
        public static void ResizeToInterger(double doubleValue, out long value)
        {
            value = (long)(doubleValue * 9223372036854775808.0);
        }
        public static void ResizeToInterger(double doubleValue, out sbyte value)
        {
            value = (sbyte)(doubleValue * 128.0);
        }

    }

    static class DataWriter
    {
        /// <summary>
        /// Checks if a folder already exist with the given <paramref name="pathway"/>, else it will create a new one.
        /// </summary>
        /// <param name="pathway">The pathway of the new folder.</param>
        /// <returns>Returns 0 if it success, 2 if the parth is to long, else 1.</returns>
        public static byte FolderCreator(string pathway)
        {
            try
            {
                if (pathway != "")
                    Directory.CreateDirectory(pathway);
                return 0;
            }
            catch (PathTooLongException e)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("The path is to long. \nEnter to continue.");
                Console.ReadLine();
                Debug.WriteLine(e);
                return 2;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return 1;
            }
        }

        /// <summary>
        /// Checks if a file exist with the given <paramref name="filename"/> and <paramref name="pathway"/>, else it will create a new file.
        /// </summary>
        /// <param name="pathway">Pathway for folder of the location of the file.</param>
        /// <param name="filename">Name and type of the new file. </param>
        public static void FileCreator(string pathway, string filename)
        {
            string path = Path.Combine(pathway, filename);
            if (!File.Exists(path))
            #pragma warning disable CS0642 // Possible mistaken empty statement
                using (StreamWriter file = new StreamWriter(path)) ;
            #pragma warning restore CS0642 // Possible mistaken empty statement
        }

        /// <summary>
        /// Writes data from a multi-dimensional <paramref name="array"/>. 
        /// </summary>
        /// <param name="array">Multi-dimensional array containing the data that should be written.</param>
        /// <param name="pathAndName">The pathway and filename with filetype for where the data should be written out too.</param>
        /// <param name="numberSeperator">Sign, if any, that should be used to help seperate data in the file for better overview.</param>
        public static void WriteToText(double[,] array, string pathAndName, char? numberSeperator = null)
        { //catch any errors
            using (StreamWriter file = new StreamWriter(pathAndName, false))
            {
                uint[] size =
                {
                    (uint)array.GetLength(0),
                    (uint)array.GetLength(1)
                };
                for (uint n = 0; n < size[0]; n++)
                {
                    for (uint m = 0; m < size[1]; m++)
                    {
                        int padLength = numberSeperator == null ? 0 : array[n, m].ToString().Length;
                        file.Write($"{array[n, m]}".PadRight(padLength, ' '));
                        file.Write($"{numberSeperator}");
                    }
                    file.WriteLine();
                }
            }
        }

        /// <summary>
        /// Writes the information in <paramref name="array"/> to the file <paramref name="pathAndName"/>.
        /// The function expect the <paramref name="array"/> to only contain two arrays of similar length. 
        /// The char <paramref name="numberSeperator"/> can be used to add a char between the collumns. 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="pathAndName"></param>
        /// <param name="numberSeperator"></param>
        public static void WriteToText(double[][] array, string pathAndName, char? numberSeperator = null)
        { //
            using (StreamWriter file = new StreamWriter(pathAndName, false))
            {
                for (uint m = 0; m < array[1].Length; m++)
                {
                    int padLength = numberSeperator == null ? 0 : array[0][m].ToString().Length;
                    file.Write($"{array[0][m]}".PadRight(padLength, ' ') + $" {numberSeperator} { array[1][m]}");
                    file.WriteLine();
                }
                file.WriteLine();
            }
        }

        /// <summary>
        /// Writes data from a 1D <paramref name="array"/> to the <paramref name="pathAndName"/>.
        /// </summary>
        /// <param name="array">The array containing the data that should be written.</param>
        /// <param name="pathAndName">The pathway and filename with filetype for where the data should be written out too.</param>
        public static void WriteToText(double[] array, string pathAndName)
        {
            using (StreamWriter file = new StreamWriter(pathAndName, false))
            {
                foreach (double value in array)
                    file.WriteLine(value);
            }
        }

    }

    class WaveClass
    {
   
        public static byte[] LoadAudioFile(string signalPathwayAndName)
        {
            return File.ReadAllBytes(signalPathwayAndName);
        }

        public static void WriteAudioFile(string key, string saveLocation)
        {
            double[,] audioWithHeader = Storage.SignalFromStorage(key);
            //Storage.SignalStorage.TryGetValue(key, out audioWithHeader);
            double[,] audioHeaderless = AudioStorageProcessing.RemoveHeader(audioWithHeader, out byte[] header);
            AudioStorageProcessing.WavSegmentFinder(header, out uint fmtChunkStart, out uint factChunkStart, out uint dataChunkStart, out uint headerSize);
            ushort channelAmount = ChannelAmount(header, fmtChunkStart);
            short bytePerSample = (short)(BitsPerSample(header, fmtChunkStart) / 8);
            uint dataLength = (uint)(audioHeaderless.GetLength(0) * bytePerSample * channelAmount);
            Conversion.ValueToBitArrayQuick(dataLength, out byte[] dataSegmentSize);
            for (uint n = dataChunkStart + 4; n < dataChunkStart + 8; n++)
                header[n] = dataSegmentSize[n - dataChunkStart - 4];
            byte[] audioByte = TimeDomainToByteArray(audioHeaderless, channelAmount, BitsPerSample(header, fmtChunkStart));
            List<byte> list = new List<byte>(header.Length + audioByte.Length);
            list.AddRange(header);
            list.AddRange(audioByte);
            byte[] completeWaveFile = list.ToArray();
            string saveFile = key + ".wav";
            File.WriteAllBytes(saveLocation + "\\" + saveFile, completeWaveFile);
        }

        /// <summary>
        /// Converts a double[,] array into a byte[] array.  
        /// </summary>
        /// <param name="audioFile">The audio file to convert. Note: It is the pure audio data without any header.</param>
        /// <param name="channelAmount">The amount of channels.</param>
        /// <param name="bitsPerSample">The amount of samples per bit.</param>
        /// <returns>Returns a byte array of size audioFile.length / (bitsPerSamples * channelAmount)</returns>
        public static byte[] TimeDomainToByteArray(double[,] audioFile, ushort channelAmount, short bitsPerSample)
        {
            byte[] byteArray = new byte[audioFile.Length * (bitsPerSample / 8)];
            ulong byteAmount = (ushort)(bitsPerSample / 8);
            for (uint n = 0; n < channelAmount; n++)
            {

                for (uint m = 0; m < audioFile.GetLength(0); m++)
                {
                    ulong posistion = (m * (ushort)(channelAmount) * byteAmount) + (n * byteAmount);
                    byte[] currentSampleInBytes;
                    if (byteAmount == 2)
                    {
                        Resizer.ResizeToInterger(audioFile[m, n], out short value);
                        Conversion.ValueToBitArrayQuick(value, out currentSampleInBytes);
                        PlacingInArray(posistion, currentSampleInBytes);
                    }
                    else if (byteAmount == 4)
                    {
                        Resizer.ResizeToInterger(audioFile[m, n], out int value);
                        Conversion.ValueToBitArrayQuick(value, out currentSampleInBytes);
                        PlacingInArray(posistion, currentSampleInBytes);
                    }
                    else if (byteAmount == 8)
                    {
                        Resizer.ResizeToInterger(audioFile[m, n], out long value);
                        Conversion.ValueToBitArrayQuick(value, out currentSampleInBytes);
                        PlacingInArray(posistion, currentSampleInBytes);
                    }
                    else if (byteAmount == 1)
                    {
                        Resizer.ResizeToInterger(audioFile[m, n], out short value);
                        Conversion.ValueToBitArrayQuick(value, out currentSampleInBytes);
                        PlacingInArray(posistion, currentSampleInBytes);
                    }
                }
            }
            return byteArray;

            void PlacingInArray(ulong location, byte[] takeFromByteArray)
            {
                for (byte k = 0; k < byteAmount; k++)
                    byteArray[location + k] = takeFromByteArray[k];
            }
        }

        /// <summary>
        /// Converts an array, <paramref name="wavefile"/>, of bytes to double array. 
        /// </summary>
        /// <param name="wavefile">The entire wavefile with the data that need processing. Should also contain the header.</param>
        /// <param name="dataSectionSize">The size of the data section.</param>
        /// <param name="channelAmount">The amount of channels.</param>
        /// <param name="bitsPerSample">The amount of bits per samples.</param>
        /// <returns>Returns an array with the converted data.</returns>
        public static double[,] ByteArrayToTimeDomain(byte[] wavefile, ulong dataSectionSize, ushort channelAmount, short bitsPerSample, ulong waveHeaderEnding)
        {
            double[,] timeDomain = new double[(dataSectionSize) / ((ulong)(bitsPerSample / 8) * channelAmount), channelAmount]; //length of data / ((total amount of bits/bits per byte) * amount of channels) , amount of channels 
            ulong postion = waveHeaderEnding;
            byte byteAmount = (byte)(bitsPerSample / 8);
            for (int n = 0; n < channelAmount; n++)
            {
                ulong currentPosition = postion;
                ulong arrayIndexLocation = 0;
                while (currentPosition + byteAmount <= (ulong)(dataSectionSize + 44))
                {
                    byte[] array = new byte[byteAmount];
                    for (uint m = 0; m < byteAmount; m++)
                    {
                        array[m] = wavefile[currentPosition + m];
                    }
                    double endValue = 0;
                    if (bitsPerSample / 8 == 2)
                    {
                        short value = 0;
                        Conversion.ByteConverterToInterger(array, ref value);
                        endValue = Resizer.Resize(value);
                    }
                    else if (bitsPerSample / 8 == 4)
                    {
                        int value = 0;
                        Conversion.ByteConverterToInterger(array, ref value);
                        endValue = Resizer.Resize(value);
                    }
                    else if (bitsPerSample / 8 == 8)
                    {
                        long value = 0;
                        Conversion.ByteConverterToInterger(array, ref value);
                        endValue = Resizer.Resize(value);
                    }
                    else
                    {
                        byte value = array[0];
                        endValue = Resizer.Resize(value);
                    }
                    timeDomain[arrayIndexLocation, n] = endValue;
                    arrayIndexLocation++;
                    currentPosition += (ulong)(byteAmount * channelAmount);
                }
            }
            return timeDomain;
        }

        public static uint FrequencyRate(byte[] wavefile, uint startLocation)
        {
            byte[] frequencyBytes =
                {
                wavefile[12 + startLocation],
                wavefile[13 + startLocation],
                wavefile[14 + startLocation],
                wavefile[15 + startLocation]
            };
            uint value = 0;
            Conversion.ByteConverterToInterger(frequencyBytes, ref value);
            return value;
        }

        public static uint FrequencyRate(double[,] audioWithHeader)
        {
            string currentASCII = "";
            uint posistion = 0;
            do
            {
                byte[] wordLooking =
                {
                    (byte)audioWithHeader[posistion,0],
                    (byte)audioWithHeader[posistion+1,0],
                    (byte)audioWithHeader[posistion+2,0],
                    (byte)audioWithHeader[posistion+3,0]
                };
                currentASCII = Conversion.ByteArrayToASCII(wordLooking);
                posistion++;
            } while (currentASCII != "fmt ");
            uint fmtChunkStartLocation = --posistion;

            byte[] frequencyBytes =
            {
                (byte)audioWithHeader[12 + fmtChunkStartLocation,0],
                (byte)audioWithHeader[13 + fmtChunkStartLocation,0],
                (byte)audioWithHeader[14 + fmtChunkStartLocation,0],
                (byte)audioWithHeader[15 + fmtChunkStartLocation,0]
            };
            uint value = 0;
            Conversion.ByteConverterToInterger(frequencyBytes, ref value);
            return value;
        }

        public static uint DataSectionSize(byte[] wavefile, uint startLocation)
        { //need to check for fact and data chunk section it seems like
            byte[] dataSectionSizeByte =
                {
                wavefile[4 + startLocation],
                wavefile[5 + startLocation],
                wavefile[6 + startLocation],
                wavefile[7 + startLocation]
            };
            uint value = 0;
            Conversion.ByteConverterToInterger(dataSectionSizeByte, ref value);
            return value;
        }

        public static ushort ChannelAmount(byte[] wavefile, uint startLocation)
        {
            byte[] channelAmountByte =
                {
                wavefile[10 + startLocation],
                wavefile[11 + startLocation]
            };

            ushort value = 0;
            Conversion.ByteConverterToInterger(channelAmountByte, ref value);
            return value;
        }

        public static short BitsPerSample(byte[] wavefile, uint startLocation)
        {
            byte[] bitsPerSample =
                {
                wavefile[22 + startLocation],
                wavefile[23 + startLocation]
            };
            short value = 0;
            Conversion.ByteConverterToInterger(bitsPerSample, ref value);
            return value;
        }

    }
}