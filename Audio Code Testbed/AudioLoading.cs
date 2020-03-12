using System;
using System.Collections.Generic;
using System.Text;
using Converter;

namespace Audio_Code_Testbed
{
    static class AudioLoading
    {
        public static void LoadAudio(string signalPathwayAndName)
        {
            byte[] wav = WaveClass.LoadAudioFile(signalPathwayAndName);
            string[] seperation = signalPathwayAndName.Split("\\");
            string[] nameWithFormat = seperation[seperation.Length - 1].Split(".");
            string nameWithoutFormat = nameWithFormat[0];
            uint headerSize = 44;
            string currentASCII = "";
            uint posistion = 0;
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
            uint fmtChunkStartLocation = --posistion;
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
            uint fmtFormatCode = 0;
            Conversion.ByteConverterToInterger(sizeArray, ref fmtFormatCode);

            uint factChunkStartLocation = 0;
            uint factChuckSize = 0;
            uint factNumberOfSamples;
            if (fmtFormatCode != 1)
            {
                currentASCII = "";
                posistion = 8;
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

            currentASCII = "";
            posistion = 8;
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
            uint dataChunkStartLocation = --posistion;
            headerSize = dataChunkStartLocation > factChunkStartLocation ? dataChunkStartLocation + 8 : factChunkStartLocation + factChuckSize;

            byte[] wavHeader = new byte[headerSize];
            for (int i = 0; i < headerSize; i++)
                wavHeader[i] = wav[i];
            ushort channelAmount = WaveClass.ChannelAmount(wavHeader, fmtChunkStartLocation);
            uint dataSegmentSize = WaveClass.DataSectionSize(wavHeader, dataChunkStartLocation);
            short bitsPerSample = WaveClass.BitsPerSample(wavHeader, fmtChunkStartLocation);
            double[,] audioScaled = WaveClass.ByteArrayToTimeDomain(wav, dataSegmentSize, channelAmount, bitsPerSample, (ulong)wavHeader.Length);
            double[,] audioWithHeader = AudioStorageProcessing.AddWaveToSignal(audioScaled, wavHeader);
            Storage.SignalToStorage(audioWithHeader, nameWithoutFormat);

        }


    }
}
