using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Converter;

namespace Audio_Code_Testbed
{
    public class Storage
    {
        private Storage() { }
        private static ConcurrentDictionary<string, double[,]> signalStorageWithHeader = new ConcurrentDictionary<string, double[,]>();

        private static ConcurrentDictionary<string, double[,]> SignalStorage
        {
            get => signalStorageWithHeader;
        }

        public static double[,] SignalFromStorage(string key)
        {
            SignalStorage.TryGetValue(key, out double[,] array);
            return array;
        }

        public static int SignalCount
        {
            get => SignalStorage.Count;
        }

        public static bool RemoveSignalFromStorage(string key)
        {
            SignalStorage.TryRemove(key, out double[,] removedValue);
            if (removedValue != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Attemps to place a double[,] array <paramref name="audioWithHeader"/> to the dictionary using <paramref name="key"/>.
        /// It will return 0 if it succed, 1 otherwise.
        /// It will not overwrite data already stored using <paramref name="key"/>.
        /// </summary>
        /// <param name="audioWithHeader">The double[,] array to add to the dictionary, it assume the array already got its header.</param>
        /// <param name="key">The string key to attempt to add to the dictionary.</param>
        /// <returns>Returns an int which value indicate whether it succeded or not.</returns>
        public static int SignalToStorage(double[,] audioWithHeader, string key)
        {
            Debug.WriteLine("Signal To Storage - start");
            Debug.WriteLine("Signal To Storage - "+ Thread.CurrentThread.Name);
            //System.Diagnostics.Debug.WriteLine(Thread.CurrentThread.Name);
            try //does not overwrite the existing key value it seems, neither should it
            {
                bool sucess = SignalStorage.TryAdd(key, audioWithHeader);
                Debug.WriteLine("Signal To Storage - Done");
                return 0;
            }
            catch
            {
                Debug.WriteLine("Signal To Storage - Failed");
                return 1;
            }
        }

        /// <summary>
        /// Returns all keys in use by the dictionary.
        /// </summary>
        /// <returns>Returns a string array containing the keys in use by the dictionary.</returns>
        public static string[] GetSignalStorageKeys()
        {
            Debug.WriteLine("Get Keys From Storage - start");
            uint i = 0;
            List<string> keys = new List<string>();
            Task tast = Task.Factory.StartNew(
                () =>
                { //creates two threads, for some reason
                    Thread.CurrentThread.Name = "Signal Storage Keys Task";
                    Debug.WriteLine(Thread.CurrentThread.Name + " - Start");
                    foreach (var key in SignalStorage.Keys)
                        keys.Add(key.ToString());
                    Debug.WriteLine(Thread.CurrentThread.Name + " - Done");
                }
            );
            Task.WaitAll(tast);
            string[] keyArray = new string[keys.Count];
            foreach (string key in keys)
            {
                keyArray[i] = key;
                i++;
            }
            Debug.WriteLine("Get Keys From Storage - Done");
            return keyArray;
        }
    }


    public class CurrentAudioInformation 
    {
        private uint samplingRate;
        private string name;
        private double[,] audioWithOutHeader;
        private byte[] header;

        /// <summary>
        /// Stores an audio signal, from the dictionary using <paramref name="waveSignalKey"/>.
        /// Allows for easy acess to its filename, its headless audio, its header and its sampling rate.
        /// </summary>
        /// <param name="waveSignalKey">The key used to load the audio from the dictionary.</param>
        public CurrentAudioInformation(string waveSignalKey)
        { //catch any errors here, e.g. key does not exist 
            string[] seperation = waveSignalKey.Split("\\");
            string[] nameWithFormat = seperation[seperation.Length - 1].Split(".");
            string nameWithoutFormat = nameWithFormat[0];
            double[,] audioWithHeader = Storage.SignalFromStorage(nameWithoutFormat);
            //Storage.SignalStorage.TryGetValue(nameWithoutFormat, out audioWithHeader);
            uint headerLength = (uint)audioWithHeader[0, 0];
            uint fmtSegmentStartLocation = (uint)audioWithHeader[1, 0];
            uint dataSegmentstartLocation = (uint)audioWithHeader[2, 0];
            header = new byte[headerLength];
            audioWithOutHeader = new double[audioWithHeader.GetLength(0) - 3 - headerLength,audioWithHeader.GetLength(1)];
            for (int i = 3; i < headerLength+3; i++)
                header[i - 3] = (byte)audioWithHeader[i,0];
            samplingRate = WaveClass.FrequencyRate(header, fmtSegmentStartLocation);
            for (uint i = 3+headerLength; i < audioWithHeader.GetLength(0); i++)
                for (int k = 0; k < audioWithHeader.GetLength(1); k++)
                    audioWithOutHeader[i - 3 - headerLength, k] = audioWithHeader[i, k];
            name = nameWithoutFormat;
        }

        /// <summary>
        /// Returns the sampling rate.
        /// </summary>
        public uint SamplingRate
        {
            get => samplingRate;
        }
        /// <summary>
        /// Returns the audio without a header attached to it.
        /// </summary>
        public double[,] AudioHeaderLesss
        {
            get => audioWithOutHeader;
        }
        /// <summary>
        /// Returns the header.
        /// </summary>
        public byte[] Header
        {
            get => header;
        }
        /// <summary>
        /// Returns the filename.
        /// </summary>
        public string Filename
        {
            get => name;
        }
    }

    /// <summary>
    /// Class used to do process for audio data that are to be stored or retrived from the dictionary storage. 
    /// </summary>
    public static class AudioStorageProcessing
    {
        /// <summary>
        /// Removes the header from an audio double[,].
        /// </summary>
        /// <param name="audioWithHeader">The audio double[,] with header.</param>
        /// <param name="header">The header of the audio double[,].</param>
        /// <returns></returns>
        public static double[,] RemoveHeader(double[,] audioWithHeader, out byte[] header)
        {
            uint headerLength = (uint)audioWithHeader[0, 0];
            double[,] audioWithOutHeader = new double[audioWithHeader.GetLength(0) - 3 - headerLength, audioWithHeader.GetLength(1)];
            header = new byte[headerLength];
            for (int i = 3; i < headerLength + 3; i++) //retrives the header
                header[i - 3] = (byte)audioWithHeader[i, 0];
            for (uint i = 3 + headerLength; i < audioWithHeader.GetLength(0); i++) //removes the header from all channels
                for (int k = 0; k < audioWithHeader.GetLength(1); k++)
                    audioWithOutHeader[i - 3 - headerLength, k] = audioWithHeader[i, k];
            return audioWithOutHeader;
        }

        /// <summary>
        /// Places a header in the front of the audio double[,]. A header is placed in each channel.
        /// Note the first 3 entries in each channel, in the return double[,], contain information related to the header and is not part of the header. 
        /// </summary>
        /// <param name="audio">The double[,] array to add the header to.</param>
        /// <param name="header">The header to add.</param>
        /// <returns>Returns a double[,] with headers in each channel. 
        /// Note the first 3 entries in each channel, in the return double[,], contain information related to the header and is not part of the header.</returns>
        public static double[,] AddWaveToSignal(double[,] audio, byte[] header)
        {
            WavSegmentFinder(header, out uint fmtChunkStart, out uint _, out uint dataChunkStart, out uint _);
            ushort channelAmount = WaveClass.ChannelAmount(header, fmtChunkStart);
            short bitsPerSample = WaveClass.BitsPerSample(header, fmtChunkStart);
            double[,] signalInfoAdded = new double[3 + header.Length + audio.GetLength(0), channelAmount];
            Conversion.ValueToBitArrayQuick(audio.GetLength(0)*channelAmount*(bitsPerSample/8), out byte[] dataSegment);
            for (uint i = dataChunkStart+4; i < dataChunkStart+8; i++)
                header[i] = dataSegment[i - dataChunkStart - 4];
            for (int i = 0; i < channelAmount; i++)
            {
                signalInfoAdded[0, i] = header.Length;
                signalInfoAdded[1, i] = fmtChunkStart;
                signalInfoAdded[2, i] = dataChunkStart;
                for (int k = 3; k < 3 + header.Length; k++)
                    signalInfoAdded[k, i] = header[k - 3];
                for (int n = header.Length+3; n < signalInfoAdded.GetLength(0); n++)
                    signalInfoAdded[n, i] = audio[n-header.Length-3,i];
            }
            return signalInfoAdded;
        }


        /// <summary>
        /// Converts a 1-dimensional header into a 2-dimensional.
        /// </summary>
        /// <param name="header">The header to replicate.</param>
        /// <param name="amountOfChannels">The amount of times the header should be replicated</param>
        /// <returns></returns>
        private static byte[,] HeaderHighDimensional(byte[] header, int amountOfChannels)
        {
            byte[] toAdd = header;
            byte[,] newArray = new byte[header.Length,amountOfChannels];
            for (int i = 0; i < amountOfChannels; i++)
                for (int k = 0; k < header.Length; k++)
                        newArray[k, i] = toAdd[k];
            return newArray;
        }

        /// <summary>
        /// Finds different segments, fmt, fact and data, if they exist, in a <paramref name="wav"/> byte array, and the size of the header itself. 
        /// </summary>
        /// <param name="wav">The wave byte array to analyse.</param>
        /// <param name="fmtChunkStartLocation">The first byte location of the fmt chunk.</param>
        /// <param name="factChunkStartLocation">The first byte location of the fact chunk.</param>
        /// <param name="dataChunkStartLocation">The first byte location of the data chunk.</param>
        /// <param name="headerSize">The size of the wave header with disregarding the audio data itself.</param>
        public static void WavSegmentFinder(byte[] wav, out uint fmtChunkStartLocation, out uint factChunkStartLocation, out uint dataChunkStartLocation, out uint headerSize)
        {
            string currentASCII = "";
            uint posistion = 0;
            uint fmtFormatCode = 0;
            do
            {
                byte[] wordLooking =
                {
                    wav[posistion],
                    wav[posistion+1],
                    wav[posistion+2],
                    wav[posistion+3]
                };
                currentASCII = Conversion.ByteArrayToASCII(wordLooking);
                posistion++;
            } while (currentASCII != "fmt ");
            fmtChunkStartLocation = --posistion;
            uint mainHeaderChunkSize = fmtChunkStartLocation;
            byte[] sizeArray = new byte[4]
            {
                wav[fmtChunkStartLocation+4],//+ 4 to placement because of ID chunk is 4 bytes
                wav[fmtChunkStartLocation+5],
                wav[fmtChunkStartLocation+6],
                wav[fmtChunkStartLocation+7]
            };
            uint fmtChunkSize = 0;
            Conversion.ByteConverterToInterger(sizeArray, ref fmtChunkSize);
            sizeArray = new byte[2]
            {
                 wav[fmtChunkStartLocation + 8],
                 wav[fmtChunkStartLocation + 9]
            };
            fmtChunkSize += 8;

            Conversion.ByteConverterToInterger(sizeArray, ref fmtFormatCode);

            factChunkStartLocation = 0;
            uint factChuckSize = 0;
            uint factNumberOfSamples;
            if (fmtFormatCode != 1)
            {
                currentASCII = "";
                posistion = fmtChunkStartLocation + fmtChunkSize;
                do
                {
                    byte[] wordLooking =
                    {
                        wav[posistion],
                        wav[posistion+1],
                        wav[posistion+2],
                        wav[posistion+3]
                    };
                    currentASCII = Conversion.ByteArrayToASCII(wordLooking);
                    posistion++;
                } while (currentASCII != "fact");
                factChunkStartLocation = --posistion;
            }
            posistion = fmtChunkStartLocation+fmtChunkSize;
            currentASCII = "";
            do
            {
                byte[] wordLooking =
                {
                    wav[posistion],
                    wav[posistion+1],
                    wav[posistion+2],
                    wav[posistion+3]
                };
                currentASCII = Conversion.ByteArrayToASCII(wordLooking);
                posistion++;
            } while (currentASCII != "data");
            dataChunkStartLocation = --posistion;
            headerSize = dataChunkStartLocation > factChunkStartLocation ? dataChunkStartLocation + 8 : factChunkStartLocation + factChuckSize;
        }

    }

}
