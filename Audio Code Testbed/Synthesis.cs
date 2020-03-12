using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Converter;
using Extensions;

namespace Audio_Code_Testbed
{
    static class Synthesis
    {

        public class Rnd
        {
            private Rnd() { }
            public static Random rnd = new Random();
        }


        /// <summary>
        /// Constructs a wave header with a length of 44 bytes. It assumes the data, it going to be added to, to be PCM. 
        /// </summary>
        /// <param name="audio"></param> //remove this parameter
        /// <param name="dataLength">The length of the data <c>T[].Length</c></param>
        /// <param name="samplingRate">The sampling rate.</param>
        /// <param name="channelAmount">The amount of channels.</param>
        /// <returns></returns>
        static private byte[] WaveHeaderToSignal(int dataLength, uint samplingRate, uint channelAmount) //move to somewhere else so it can be called by other classes
        { 
            byte[] header = new byte[44];
            header[0] = 82;
            header[1] = 73;
            header[2] = 70;
            header[3] = 70;

            Conversion.ValueToBitArrayQuick(dataLength+44, out byte[] waveChunkSize);
            header[4] = waveChunkSize[0];
            header[5] = waveChunkSize[1];
            header[6] = waveChunkSize[2];
            header[7] = waveChunkSize[3];

            header[8] = 87;
            header[9] = 65;
            header[10] = 86;
            header[11] = 69;

            header[12] = 102;
            header[13] = 109;
            header[14] = 116;
            header[15] = 32;

            header[16] = 16;
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;

            header[20] = 1;
            header[21] = 0;

            Conversion.ValueToBitArrayQuick(channelAmount, out byte[] channelAmountInBytes);
            header[22] = channelAmountInBytes[0];
            header[23] = channelAmountInBytes[1];

            Conversion.ValueToBitArrayQuick(samplingRate, out byte[] frequencyRate);
            header[24] = frequencyRate[0];
            header[25] = frequencyRate[1];
            header[26] = frequencyRate[2];
            header[27] = frequencyRate[3];

            Conversion.ValueToBitArrayQuick(16/2*channelAmount*samplingRate, out byte[] byteRate);
            header[28] = byteRate[0];
            header[29] = byteRate[1];
            header[30] = byteRate[2];
            header[31] = byteRate[3];

            header[32] = 4;
            header[33] = 0;

            header[34] = 16;
            header[35] = 0;

            header[36] = 100;
            header[37] = 97;
            header[38] = 116;
            header[39] = 97;

            Conversion.ValueToBitArrayQuick(dataLength, out byte[] dataSegment);
            header[40] = dataSegment[0];
            header[41] = dataSegment[1];
            header[42] = dataSegment[2];
            header[43] = dataSegment[3];

            return header;
        }

        /// <summary>
        /// Synthesis for a bubble. 
        /// Based upon research by 
        /// K. van den Doel, "Physically-based models for liquid sounds," in Proceedings of ICAD 04-Tenth Meeting of the International Conference on Auditory Display, 2004
        /// </summary>
        /// <param name="minRadius">The minimum radius of a water drop.</param>
        /// <param name="maxRadius">The maximum radius of a water drop.</param>
        /// <param name="frequencySampling">The sampling rate.</param>
        /// <param name="amount">The amount of water drops.</param>
        /// <returns></returns>
        static public double[,] WaterDrops(double minRadius, double maxRadius, uint frequencySampling, uint amount = 1)
        { //need to make it so the arrays can overlap each other. instead of uint amount have a param that contain the delays before the next bubble is created, out from the param figure out 
          //their placement in the array
            System.Diagnostics.Debug.WriteLine("Water Drops " + System.Threading.Thread.CurrentThread.Name);
            double[] output = new double[0];
            for (int i = 0; i < amount; i++)
                output = output.Concat(Waterdrop(minRadius, maxRadius, frequencySampling));

            double maxValue = output.Max();
            double minValue = output.Min();
            for (int m = 0; m < output.Length; m++) //sounds like it is causing a little amount of clipping, save audio data and check it. Data does not indicate that
                output[m] = 2 * ((output[m] - minValue) / (maxValue - minValue)) - 1;

            double[,] multiChanAudio = new double[output.Length, 1];
            for (int i = 0; i < output.Length; i++)
                multiChanAudio[i, 0] = output[i];

            byte[] header = WaveHeaderToSignal(output.GetLength(0), frequencySampling, 1);
            double[,] audio = AudioStorageProcessing.AddWaveToSignal(multiChanAudio, header);
            return audio;
        }


        /// <summary>
        /// Sythesis for the sound of a water drop hitting a water surface.
        /// </summary>
        /// <param name="minRadius">The minimum raduis of the drop.</param>
        /// <param name="maxRadius">The maximum radius of the drop.</param>
        /// <param name="frequencySampling">The sampling rate.</param>
        /// <returns></returns>
        static private double[] Waterdrop(double minRadius, double maxRadius, uint frequencySampling)
        { //what parameters should the user be allowed to control...
            //maybe not have the funtions that calculate the synthesis create the double[,] audio and wavheader. Rather, make them private and have 
            //functions that call them which also alter them to double[,] and call the waveheader adder function.
            double alpha = Rnd.rnd.Next(1500, 1700) / 1000d;
            double epsilon = Rnd.rnd.Next(10, 100) / 1000d;
            byte beta = 1;
            double radius = Rnd.rnd.Next((int)(minRadius * 1000), (int)(maxRadius * 1000)) / 1000d;
            double D = Math.Pow(Rnd.rnd.Next(-500, 500) / 1000d, beta);
            double a = D * Math.Pow(radius, alpha);

            double frequencyZero = 3 / radius;
            double damping = 0.043 * frequencyZero + 0.0014 * Math.Pow(frequencyZero, 3 / 2);
            double test = 1;
            uint index = 0;
            do
            {
                test = Math.Exp(-damping * ((double)index / (double)frequencySampling));
                index += 1;
            } while (test > 0.01);

            List<double> tContainer = new List<double>();

            double sigma = epsilon * damping;
            double t = 0;
            do
            {
                tContainer.Add(t);
                t = (1d / (double)frequencySampling) * tContainer.Count;
            } while (t <= (double)(index) / (double)(frequencySampling)); //make this do while loop into a function
            double[] ft = new double[tContainer.Count];
            for (int i = 0; i < ft.Length; i++)
                ft[i] = frequencyZero * (1 + sigma * tContainer[i]);

            double[] output = new double[ft.Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = a * Math.Sin(2 * Math.PI * ft[i] * tContainer[i]) * Math.Exp(-damping * tContainer[i]);
            return output;
        }


        static public double[,] WaterHittingResonSurf(uint samplingRate)
        {
            double[] output = new double[0];
            output = WaterHittingResonantSurface(samplingRate);
            double maxValue = output.Max();
            double minValue = output.Min();
            for (int m = 0; m < output.Length; m++) //sounds like it is causing a little amount of clipping, save audio data and check it. Data does not indicate it does
                output[m] = 2 * ((output[m] - minValue) / (maxValue - minValue)) - 1;

            double[,] multiChanAudio = new double[output.Length, 1];
            for (int i = 0; i < output.Length; i++)
                multiChanAudio[i, 0] = output[i];

            byte[] header = WaveHeaderToSignal(output.GetLength(0), samplingRate, 1);
            double[,] audio = AudioStorageProcessing.AddWaveToSignal(multiChanAudio, header);
            return audio;
        }

        static private double[] WaterHittingResonantSurface(uint samplingRate)
        {
            uint[] frequency =
            {
                3547,
                4236,
                198,
                1317,
                4257,
                4487,
                4617,
                3708,
                5753
            };
            double[] amplitude =
            {
                0.4004162,
                0.400558,
                0.43004344,
                0.200474,
                0.3003634,
                0.7004734,
                0.6003293,
                0.3002785,
                0.5004182
            };
            double[] alpha =
            {
                75.035,
                143.025,
                85.025,
                112.003,
                81.0015,
                455.003,
                134.051,
                235.015,
                335.006
            };

            double test = 1;
            uint index = 0;
            do
            {
                test = Math.Exp(-alpha.Min() * ((double)index/ (double)samplingRate));
                index++;
            } while (test > 0.01);

            List<double> tContainer = new List<double>();
            double t = 0;
            do
            {
                tContainer.Add(t);
                t = (1d / (double)samplingRate) * tContainer.Count;
            } while (t <= (double)(index) / (double)(samplingRate)); //make this do while loop into a function
            double[,] audioInProgress = new double[index,alpha.Length];
            for (int m = 0; m < alpha.Length; m++)
                for (int n = 0; n < audioInProgress.GetLength(0); n++)
                    audioInProgress[n, m] = amplitude[m] * Math.Sin(2 * Math.PI * frequency[m]*tContainer[n]) * Math.Exp(-alpha[m] * tContainer[n]);
            double[] audio = new double[index];
            for (int n = 0; n < audio.Length; n++)
            {
                double value = 0;
                for (int m = 0; m < alpha.Length; m++)
                    value += audioInProgress[n, m];
                audio[n] = value;
            }
            return audio;
        }

        static private double[] WaterHittingRigidSurface(uint samplingRate)
        {
            double subbands = (10000 - 100) / 8;
            double[] ESBS3 = new double[8];
            double[] alphaESB = new double[8];
            for (int i = 0; i < 8; i++)
            {
                ESBS3[i] = 0.108 * (subbands * i) + 24.7;
                alphaESB[i] = (56+(160-56))*Rnd.rnd.NextDouble();
            }

            double test = 1;
            uint index = 0;
            do
            {
                test = Math.Exp(-alphaESB.Min() * ((double)index / (double)samplingRate));
                index++;
            } while (test > 0.01);
            List<double> tContainer = new List<double>();
            double t = 0;
            do
            {
                tContainer.Add(t);
                t = (1d / (double)samplingRate) * tContainer.Count;
            } while (t <= (double)(index) / (double)(samplingRate)); //make this do while loop into a function
            double[] noise = new double[tContainer.Count];
            for (int i = 0; i < noise.Length; i++)
                noise[i] = Rnd.rnd.NextDouble();
            for (int i = 0; i < 8; i++)
            {
                //need to figure out if there is a bug in the filters if a signal is filtered twice before this can be finished
                //also implement bandpass filter
            }

            return null;
        }

        /// <summary>
        /// Synthesises a marimba tone. 
        /// Based upon research by... 
        /// </summary>
        /// <param name="tilt">The hit location on the ???. 0.5 is in the middle, 1 and -1 are the edges.</param>
        /// <param name="velocity"></param>
        /// <param name="pressure">Controls how quickly the node decay.</param>
        /// <param name="frequency"></param>
        /// <param name="frequencySampling"></param>
        /// <returns></returns>
        static public double[,] Marimba(float tilt, float velocity, float pressure, uint frequency, uint frequencySampling)
        { //program it such that if a note is exceeding -1 1 that it get resized to not clip

            tilt = tilt < -1 ? -1 : tilt;
            tilt = tilt > 1 ? 1 : tilt;

            double[] mode =
            {
                0.12*Math.Sin(1*tilt*Math.PI),
                0.02*Math.Sin(0.03+3.9*tilt*Math.PI),
                0.03*Math.Sin(0.5+9.3*tilt*Math.PI)
            };
            double[] decay =
            {
                -0.0001*pressure,
                -0.0001*pressure,
                -0.0010*pressure
            };

            double test = 1;
            uint index = 0;
            do
            {
                test = Math.Exp(decay[0] * index);
                index++;
            } while (test > 0.01);
            double[] time = new double[index-1];
            for (int i = 0; i < time.Length; i++)
                time[i] = i;
            double[] note = new double[time.Length];

            for (int i = 0; i < note.Length; i++)
                note[i] = velocity 
                    * ((mode[0] * Math.Exp(decay[0] * time[i]) * Math.Sin(frequency * 2 * Math.PI / frequencySampling * time[i]))
                    + (mode[1] * Math.Exp(decay[1] * time[i]) * Math.Sin(frequency * 2 * 2 * Math.PI / frequencySampling * time[i]))
                    + (mode[2] * Math.Exp(decay[2] * time[i]) * Math.Sin(frequency * 3 * 2 * Math.PI / frequencySampling * time[i])));

            double[,] multiChanAudio = new double[note.Length, 1];
            for (int i = 0; i < note.Length; i++)
                multiChanAudio[i, 0] = note[i];

            byte[] header = WaveHeaderToSignal(note.Length, frequencySampling, 1);
            double[,] audio = AudioStorageProcessing.AddWaveToSignal(multiChanAudio, header);
            return audio;

        }

    }
}
