using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Audio_Code_Testbed
{

    static class ChebyshevFiltering
    { //page 615

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingRate"></param>
        /// <param name="passbandFrequency"></param>
        /// <param name="stopbandFrequency"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[,] LowpassFilteringType1(double[,] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation) //consider making it call a thread for each signal.
        {
            double[,] modifiedAudioArray2D = new double[audio.GetLength(0), audio.GetLength(1)];
            double[] lastFiltered = new double[audio.GetLength(0)];
            for (int k = 0; k < audio.GetLength(1); k++)
            {
                for (uint i = 0; i < lastFiltered.Length; i++)
                    lastFiltered[i] = audio[i, k];
                lastFiltered = LowpassFilteringType1(lastFiltered, samplingRate, passbandFrequency, stopbandFrequency, passbandAttenuation, stopbandAttenuation);
                for (uint i = 0; i < lastFiltered.Length; i++)
                    modifiedAudioArray2D[i, k] = lastFiltered[i];
            }
            return modifiedAudioArray2D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingRate"></param>
        /// <param name="passbandFrequency"></param>
        /// <param name="stopbandFrequency"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[] LowpassFilteringType1(double[] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation)
        {
            const double PI = Math.PI;

            double wPass = (2 * PI * passbandFrequency) / samplingRate; //same problem with negative N's as with the highpass filter
            double wStop = (2 * PI * stopbandFrequency) / samplingRate;
            double omegaPass = Math.Tan(wPass / 2);
            double omegaStop = Math.Tan(wStop / 2);
            double ePass = Math.Sqrt(Math.Pow(10, (passbandAttenuation / 10)) - 1);
            double eStop = Math.Sqrt(Math.Pow(10, (stopbandAttenuation / 10)) - 1);
            double w = omegaStop / omegaPass;
            double e = eStop / ePass;
            double nExact = (Math.Log(e + Math.Sqrt(Math.Pow(e, 2) - 1))) / (Math.Log(w + Math.Sqrt(Math.Pow(w, 2) - 1)));
            int nUsed = (int)Math.Ceiling(nExact);
            int k = (int)Math.Floor(nUsed / 2d);
            double a = 1d / (double)nUsed * Math.Log(1d / ePass + Math.Sqrt(1d / Math.Pow(ePass, 2) + 1));
            double omega0 = omegaPass * Math.Sinh(a);

            double[] omega = new double[k];
            double[] theta = new double[k];
            for (int i = 0; i < k; i++)
            {
                theta[i] = (PI / (2 * nUsed)) * (nUsed - 1 + (2 * (i + 1)));
                omega[i] = omegaPass*Math.Sin(theta[i]);
            }
                
            double[] filterCoefficientG = new double[(int)k + 1];
            double[,] filterCoefficientA = new double[(int)k + 1, 2];
            double[,] filterCoefficientB = new double[(int)k + 1, 3];

            double[] matrixBHelp =
            {
                1,
                1,
                0
            };

            for (int i = 0; i < k; i++)
            {
                filterCoefficientG[i + 1] = (Math.Pow(omega0, 2) + Math.Pow(omega[i], 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2) + Math.Pow(omega[i], 2));
                filterCoefficientA[i + 1, 0] = (2 * (Math.Pow(omega0, 2) + Math.Pow(omega[i], 2) - 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2) + Math.Pow(omega[i], 2));
                filterCoefficientA[i + 1, 1] = (1 + 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2) + Math.Pow(omega[i], 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2) + Math.Pow(omega[i], 2));
                filterCoefficientB[i + 1, 0] = filterCoefficientG[i + 1] * 1;
                filterCoefficientB[i + 1, 1] = filterCoefficientG[i + 1] * 2;
                filterCoefficientB[i + 1, 2] = filterCoefficientG[i + 1] * 1;
            }
            if (nUsed % 2 != 0)
            {
                filterCoefficientG[0] = omega0 / (omega0 + 1);
                filterCoefficientA[0, 0] = (omega0 - 1) / (omega0 + 1);
                filterCoefficientA[0, 1] = 0;
                for (int i = 0; i < 2; i++)
                    filterCoefficientB[0, i] = filterCoefficientG[0] * matrixBHelp[i];
            }
            return FilteringClass.SOSFiltering(audio, filterCoefficientA, filterCoefficientB, nUsed);
        }



    }

    /// <summary>
    /// Butterworth Filters. 
    /// 
    /// </summary>
    static class ButterworthFiltering
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingRate"></param>
        /// <param name="passbandFrequencyA"></param>
        /// <param name="passbandFrequencyB"></param>
        /// <param name="stopbandFrequencyA"></param>
        /// <param name="stopbandFrequencyB"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[,] BandstopFiltering(double[,] audio, uint samplingRate, uint passbandFrequencyA, uint passbandFrequencyB, uint stopbandFrequencyA, uint stopbandFrequencyB, float passbandAttenuation, float stopbandAttenuation) //consider making it call a thread for each signal.
        {
            double[,] modifiedAudioArray2D = new double[audio.GetLength(0), audio.GetLength(1)];
            double[] lastFiltered = new double[audio.GetLength(0)];
            for (int k = 0; k < audio.GetLength(1); k++)
            {
                for (uint i = 0; i < lastFiltered.Length; i++)
                    lastFiltered[i] = audio[i, k];
                lastFiltered = BandstopFiltering(lastFiltered, samplingRate, passbandFrequencyA, passbandFrequencyB, stopbandFrequencyA, stopbandFrequencyB, passbandAttenuation, stopbandAttenuation);
                for (uint i = 0; i < lastFiltered.Length; i++)
                    modifiedAudioArray2D[i, k] = lastFiltered[i];
            }
            return modifiedAudioArray2D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingrate"></param>
        /// <param name="passbandFrequencyA"></param>
        /// <param name="passbandFrequencyB"></param>
        /// <param name="stopbandFrequencyA"></param>
        /// <param name="stopbandFrequencyB"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[] BandstopFiltering(double[] audio, uint samplingrate, uint passbandFrequencyA, uint passbandFrequencyB, uint stopbandFrequencyA, uint stopbandFrequencyB, float passbandAttenuation, float stopbandAttenuation)
        {
            const double PI = Math.PI;
            
            double wPassA = 2 * PI * passbandFrequencyA / samplingrate;
            double wPassB = 2 * PI * passbandFrequencyB / samplingrate;
            double wStopA = 2 * PI * stopbandFrequencyA / samplingrate;
            double wStopB = 2 * PI * stopbandFrequencyB / samplingrate;
            double c = (Math.Sin(wPassA + wPassB)) / (Math.Sin(wPassA) + Math.Sin(wPassB));
            double omegaPass = Math.Abs((Math.Sin(wPassB)) / (Math.Cos(wPassB) - c));
            double omegaStopA = (Math.Sin(wStopA)) / (Math.Cos(wStopA) - c);
            double omegaStopB = (Math.Sin(wStopB)) / (Math.Cos(wStopB) - c);
            double omegaStop = Math.Abs(omegaStopA) < Math.Abs(omegaStopB) ? Math.Abs(omegaStopA) : Math.Abs(omegaStopB);

            double ePass = Math.Sqrt(Math.Pow(10, (passbandAttenuation / 10)) - 1);
            double eStop = Math.Sqrt(Math.Pow(10, (stopbandAttenuation / 10)) - 1);
            double passE = eStop / ePass;
            double w = omegaStop / omegaPass;
            double nExact = Math.Abs(Math.Log(passE) / Math.Log(w));
            int nUsed = (int)Math.Ceiling(nExact);
            int k = (int)Math.Floor(nUsed / 2d);
            double omega0 = omegaPass / (Math.Pow(ePass, 1d / (double)nUsed));

            double[] theta = new double[k];
            for (int i = 0; i < k; i++)
                theta[i] = (PI / (2 * nUsed)) * (nUsed - 1 + (2 * (i + 1)));

            double[] filterCoefficientG = new double[(int)k + 1];
            double[,] filterCoefficientA = new double[(int)k + 1, 4];
            double[,] filterCoefficientB = new double[(int)k + 1, 5];

            for (int i = 0; i < k; i++)
                filterCoefficientG[i + 1] = Math.Pow(omega0, 2) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
            for (int i = 0; i < k; i++)
            {
                filterCoefficientA[i + 1, 0] = (4 * c * omega0 * (Math.Cos(theta[i]) - omega0)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 1] = (2 * (2 * Math.Pow(c, 2) * Math.Pow(omega0, 2) + Math.Pow(omega0, 2) - 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 2] = -(4 * c * omega0 * (Math.Cos(theta[i]) + omega0)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 3] = (1 + 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                double[] filter =
                {
                    1,
                    -2*c, //(s-1)*c
                    1 //-s
                };
                double[] vector =
                {
                    1,
                    -2*c, //(s-1)*c
                    1 //-s
                };
                double[] result = MathSupport.Convolution(vector, filter);
                filterCoefficientB[i + 1, 0] = filterCoefficientG[i + 1] * result[0];
                filterCoefficientB[i + 1, 1] = filterCoefficientG[i + 1] * result[1];
                filterCoefficientB[i + 1, 2] = filterCoefficientG[i + 1] * result[2];
                filterCoefficientB[i + 1, 3] = filterCoefficientG[i + 1] * result[3];
                filterCoefficientB[i + 1, 4] = filterCoefficientG[i + 1] * result[4];
            }

            double[] matrixBHelp =
            {
                1,
                -2*c, //(s-1)*c
                1, //-s, s is 1 for band, -1 for stop
                0,
                0
            };
            if (nUsed % 2 != 0)
            {
                filterCoefficientG[0] = omega0 / (1 + omega0);
                filterCoefficientA[0, 0] = -(2 * c * omega0) / (1 + omega0);
                filterCoefficientA[0, 1] = -(1 - omega0) / (1 + omega0);
                filterCoefficientA[0, 2] = 0;
                filterCoefficientA[0, 3] = 0;
                for (int i = 0; i < 5; i++)
                    filterCoefficientB[0, i] = filterCoefficientG[0] * matrixBHelp[i];
            }

            return FilteringClass.FOSFiltering(audio, filterCoefficientA, filterCoefficientB, nUsed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingRate"></param>
        /// <param name="passbandFrequencyA"></param>
        /// <param name="passbandFrequencyB"></param>
        /// <param name="stopbandFrequencyA"></param>
        /// <param name="stopbandFrequencyB"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[,] BandpassFiltering(double[,] audio, uint samplingRate, uint passbandFrequencyA, uint passbandFrequencyB, uint stopbandFrequencyA, uint stopbandFrequencyB, float passbandAttenuation, float stopbandAttenuation) //consider making it call a thread for each signal.
        {
            double[,] modifiedAudioArray2D = new double[audio.GetLength(0), audio.GetLength(1)];
            double[] lastFiltered = new double[audio.GetLength(0)];
            for (int k = 0; k < audio.GetLength(1); k++)
            {
                for (uint i = 0; i < lastFiltered.Length; i++)
                    lastFiltered[i] = audio[i, k];
                lastFiltered = BandpassFiltering(lastFiltered, samplingRate, passbandFrequencyA, passbandFrequencyB, stopbandFrequencyA, stopbandFrequencyB, passbandAttenuation, stopbandAttenuation);
                for (uint i = 0; i < lastFiltered.Length; i++)
                    modifiedAudioArray2D[i, k] = lastFiltered[i];
            }
            return modifiedAudioArray2D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="samplingRate"></param>
        /// <param name="passbandFrequencyA"></param>
        /// <param name="passbandFrequencyB"></param>
        /// <param name="stopbandFrequencyA"></param>
        /// <param name="stopbandFrequencyB"></param>
        /// <param name="passbandAttenuation"></param>
        /// <param name="stopbandAttenuation"></param>
        /// <returns></returns>
        public static double[] BandpassFiltering(double[] audio, uint samplingRate, uint passbandFrequencyA, uint passbandFrequencyB, uint stopbandFrequencyA, uint stopbandFrequencyB, float passbandAttenuation, float stopbandAttenuation)
        { //page 753
            const double PI = Math.PI;
            double wPassA = 2 * PI * passbandFrequencyA / samplingRate;
            double wPassB = 2 * PI * passbandFrequencyB / samplingRate;
            double c = (Math.Sin(wPassA+wPassB)) / (Math.Sin(wPassA) + Math.Sin(wPassB));
            double omegaPass = Math.Abs((c-Math.Cos(wPassB)) / (Math.Sin(wPassB)));

            double wStopA = 2 * PI * stopbandFrequencyA / samplingRate;
            double wStopB = 2 * PI * stopbandFrequencyB / samplingRate;
            double omegaStopA = (c-Math.Cos(wStopA)) / (Math.Sin(wStopA));
            double omegaStopB = (c - Math.Cos(wStopB)) / (Math.Sin(wStopB));
            double omegaStop =  Math.Abs(omegaStopA) < Math.Abs(omegaStopB) ? Math.Abs(omegaStopA) : Math.Abs(omegaStopB);

            double ePass = Math.Sqrt(Math.Pow(10, (passbandAttenuation / 10)) - 1);
            double eStop = Math.Sqrt(Math.Pow(10, (stopbandAttenuation / 10)) - 1);
            double passE = eStop / ePass;
            double w = omegaStop / omegaPass;
            double nExact = Math.Abs(Math.Log(passE) / Math.Log(w));
            int nUsed = (int)Math.Ceiling(nExact);
            int k = (int)Math.Floor(nUsed / 2d);
            double omega0 = omegaPass / (Math.Pow(ePass, 1d / (double)nUsed));

            double[] theta = new double[k];
            for (int i = 0; i < k; i++)
                theta[i] = (PI / (2 * nUsed)) * (nUsed - 1 + (2 * (i + 1)));

            double[] filterCoefficientG = new double[(int)k + 1];
            double[,] filterCoefficientA = new double[(int)k + 1, 4];
            double[,] filterCoefficientB = new double[(int)k + 1, 5];

            for (int i = 0; i < k; i++)
                filterCoefficientG[i+1] = Math.Pow(omega0, 2) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
            for (int i = 0; i < k; i++)
            {
                filterCoefficientA[i + 1, 0] = (4 * c * (omega0 * Math.Cos(theta[i]) - 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 1] = (2 * (2 * Math.Pow(c, 2) + 1 - Math.Pow(omega0, 2))) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0,2));
                filterCoefficientA[i + 1, 2] = -(4 * c * (omega0 * Math.Cos(theta[i]) + 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 3] = (1 + 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                double[] filter =
                {
                    1,
                    0, //(s-1)*c
                    -1 //-s
                };
                double[] vector =
                {
                    1,
                    0, //(s-1)*c
                    -1 //-s
                };
                double[] result = MathSupport.Convolution(vector, filter);
                filterCoefficientB[i + 1, 0] = filterCoefficientG[i + 1] * result[0];
                filterCoefficientB[i + 1, 1] = filterCoefficientG[i + 1] * result[1];
                filterCoefficientB[i + 1, 2] = filterCoefficientG[i + 1] * result[2];
                filterCoefficientB[i + 1, 3] = filterCoefficientG[i + 1] * result[3];
                filterCoefficientB[i + 1, 4] = filterCoefficientG[i + 1] * result[4];
            }

            double[] matrixBHelp =
            {
                1,
                0, //(s-1)*c
                -1, //-s, s is 1 for band, -1 for stop
                0,
                0
            };
            if (nUsed % 2 != 0)
            {
                filterCoefficientG[0] = omega0 / (omega0 + 1);
                filterCoefficientA[0, 0] = -(2 * c) / (omega0 + 1);
                filterCoefficientA[0, 1] = (1 - omega0) / (1 + omega0);
                filterCoefficientA[0, 2] = 0;
                filterCoefficientA[0, 3] = 0;
                for (int i = 0; i < 5; i++)
                    filterCoefficientB[0, i] = filterCoefficientG[0] * matrixBHelp[i];
            }

            return FilteringClass.FOSFiltering(audio, filterCoefficientA, filterCoefficientB,nUsed);
        }

        /// <summary>
        /// Butterworth highpass filtering with parameters calculated from
        /// <paramref name="passbandFrequency"/>, <paramref name="stopbandFrequency"/>, <paramref name="passbandAttenuation"/> and <paramref name="stopbandAttenuation"/>.
        /// </summary>
        /// <param name="audio">Audio to filter.</param>
        /// <param name="samplingRate">Sampling Rate of the audio to be processed.</param>
        /// <param name="passbandFrequency">The ordinary frequency of the passband.</param>
        /// <param name="stopbandFrequency">The ordinary frequency of the stopband.</param>
        /// <param name="passbandAttenuation">The attenuation in dB of the passband.</param>
        /// <param name="stopbandAttenuation">The attenuation in dB of the stopband.</param>
        /// <returns>Returns the filtered version of <paramref name="audio"/></returns>
        public static double[,] HighpassFiltering(double[,] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation) //consider making it call a thread for each signal.
        {
            double[,] modifiedAudioArray2D = new double[audio.GetLength(0), audio.GetLength(1)];
            double[] lastFiltered = new double[audio.GetLength(0)];
            for (int k = 0; k < audio.GetLength(1); k++)
            {
                for (uint i = 0; i < lastFiltered.Length; i++)
                    lastFiltered[i] = audio[i, k];
                lastFiltered = HighpassFiltering(lastFiltered, samplingRate, passbandFrequency, stopbandFrequency, passbandAttenuation, stopbandAttenuation);
                for (uint i = 0; i < lastFiltered.Length; i++)
                    modifiedAudioArray2D[i, k] = lastFiltered[i];
            }
            return modifiedAudioArray2D;
        }

        /// <summary>
        /// Butterworth highpass filtering with parameters calculated from
        /// <paramref name="passbandFrequency"/>, <paramref name="stopbandFrequency"/>, <paramref name="passbandAttenuation"/> and <paramref name="stopbandAttenuation"/>.
        /// </summary>
        /// <param name="audio">Audio to filter.</param>
        /// <param name="samplingRate">Sampling Rate of the audio to be processed.</param>
        /// <param name="passbandFrequency">The ordinary frequency of the passband.</param>
        /// <param name="stopbandFrequency">The ordinary frequency of the stopband.</param>
        /// <param name="passbandAttenuation">The attenuation in dB of the passband.</param>
        /// <param name="stopbandAttenuation">The attenuation in dB of the stopband.</param>
        /// <returns>Returns the filtered version of <paramref name="audio"/></returns>
        static public double[] HighpassFiltering(double[] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation)
        {
            const double PI = Math.PI;

            uint fPass = passbandFrequency;
            uint fStop = stopbandFrequency;
            float aPass = passbandAttenuation;
            float aStop = stopbandAttenuation;
            double wPass;
            double wStop;
            double omegaPass;
            double omegaStop;
            double ePass;
            double eStop;
            double w;
            double passE;
            double nExact;
            int nUsed;
            double omega0;
            double[] theta;
            double[] thetaD;

            double[] filterCoefficientG;
            double[,] filterCoefficientA;
            double[,] filterCoefficientB;

            wPass = (2 * PI * fPass) / samplingRate; //same problem with negative N's as with the highpass filter
            wStop = (2 * PI * fStop) / samplingRate;
            omegaPass = MathSupport.Cot(wPass / 2);
            omegaStop = MathSupport.Cot(wStop / 2);
            ePass = Math.Sqrt(Math.Pow(10, (aPass / 10)) - 1);
            eStop = Math.Sqrt(Math.Pow(10, (aStop / 10)) - 1);
            w = omegaStop / omegaPass;
            passE = eStop / ePass;
            nExact = Math.Abs(Math.Log(passE) / Math.Log(w));
            nUsed = (int)Math.Ceiling(nExact);
            int k = (int)Math.Floor(nUsed / 2d);
            theta = new double[k];
            omega0 = omegaPass / (Math.Pow(ePass, 1d / (double)nUsed));
            thetaD = new double[2 * nUsed - 1];

            for (int i = 0; i < k; i++)
                theta[i] = (PI / (2 * nUsed)) * (nUsed - 1 + (2 * (i + 1)));
            for (int t = 0; t < k; t++)
                thetaD[t] = -2 * Math.Cos(theta[t]);
            if (nUsed % 2 == 1)
                thetaD[0] = 1 + omega0;

            filterCoefficientG = new double[(int)k + 1];
            filterCoefficientA = new double[(int)k + 1, 2];
            filterCoefficientB = new double[(int)k + 1, 3];

            double[] matrixBHelp =
            {
                1,
                -1,
                0
            };

            for (int i = 0; i < k; i++)
            {
                filterCoefficientG[i + 1] = Math.Pow(omega0, 2) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 0] = -(2 * (Math.Pow(omega0, 2) - 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 1] = (1 + 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientB[i + 1, 0] = filterCoefficientG[i + 1] * 1;
                filterCoefficientB[i + 1, 1] = filterCoefficientG[i + 1] * -2;
                filterCoefficientB[i + 1, 2] = filterCoefficientG[i + 1] * 1;
            }
            if (nUsed % 2 != 0)
            {
                filterCoefficientG[0] = omega0 / (omega0 + 1);
                filterCoefficientA[0, 0] = -(omega0 - 1) / (omega0 + 1);
                filterCoefficientA[0, 1] = 0;
                for (int i = 0; i < 2; i++)
                    filterCoefficientB[0, i] = filterCoefficientG[0] * matrixBHelp[i];
            }
            return FilteringClass.SOSFiltering(audio, filterCoefficientA, filterCoefficientB, nUsed);
        }


        /// <summary>
        /// Butterworth lowpass filtering with parameters calculated from     
        /// <paramref name="passbandFrequency"/>, <paramref name="stopbandFrequency"/>, <paramref name="passbandAttenuation"/> and <paramref name="stopbandAttenuation"/>.
        /// </summary>
        /// <param name="audio">Audio to filter.</param>
        /// <param name="samplingRate">Sampling Rate of the audio to be processed.</param>
        /// <param name="passbandFrequency">The ordinary frequency of the passband.</param>
        /// <param name="stopbandFrequency">The ordinary frequency of the stopband.</param>
        /// <param name="passbandAttenuation">The attenuation in dB of the passband.</param>
        /// <param name="stopbandAttenuation">The attenuation in dB of the stopband.</param>
        /// <returns>Returns the filtered version of <paramref name="audio"/></returns>
        public static double[,] LowpassFiltering(double[,] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation) //consider making it call a thread for each signal.
        {
            double[,] modifiedAudioArray2D = new double[audio.GetLength(0), audio.GetLength(1)];
            double[] lastFiltered = new double[audio.GetLength(0)];
            for (int k = 0; k < audio.GetLength(1); k++)
            {
                for (uint i = 0; i < lastFiltered.Length; i++)
                    lastFiltered[i] = audio[i, k];
                lastFiltered = LowpassFiltering(lastFiltered, samplingRate, passbandFrequency, stopbandFrequency, passbandAttenuation, stopbandAttenuation);
                for (uint i = 0; i < lastFiltered.Length; i++)
                    modifiedAudioArray2D[i, k] = lastFiltered[i];
            }
            return modifiedAudioArray2D;
        }

        /// <summary>
        /// Butterworth lowpass filtering with parameters calculated from
        /// <paramref name="passbandFrequency"/>, <paramref name="stopbandFrequency"/>, <paramref name="passbandAttenuation"/> and <paramref name="stopbandAttenuation"/>.
        /// </summary>
        /// <param name="audio">Audio to filter.</param>
        /// <param name="samplingRate">Sampling Rate of the audio to be processed.</param>
        /// <param name="passbandFrequency">The ordinary frequency of the passband.</param>
        /// <param name="stopbandFrequency">The ordinary frequency of the stopband.</param>
        /// <param name="passbandAttenuation">The attenuation in dB of the passband.</param>
        /// <param name="stopbandAttenuation">The attenuation in dB of the stopband.</param>
        /// <returns>Returns the filtered version of <paramref name="audio"/></returns>
        static public double[] LowpassFiltering(double[] audio, uint samplingRate, uint passbandFrequency, uint stopbandFrequency, float passbandAttenuation, float stopbandAttenuation)
        {
            const double PI = Math.PI;

            uint fPass = passbandFrequency;
            uint fStop = stopbandFrequency;
            float aPass = passbandAttenuation;
            float aStop = stopbandAttenuation;
            double wPass;
            double wStop;
            double omegaPass;
            double omegaStop;
            double ePass;
            double eStop;
            double w;
            double passE;
            double nExact;
            int nUsed;
            double omega0;
            double[] theta;
            double[] thetaD;

            double[] filterCoefficientG;
            double[,] filterCoefficientA;
            double[,] filterCoefficientB;

            wPass = (2 * PI * fPass) / samplingRate; //same problem with negative N's as with the highpass filter
            wStop = (2 * PI * fStop) / samplingRate;
            omegaPass = Math.Tan(wPass / 2);
            omegaStop = Math.Tan(wStop / 2);
            ePass = Math.Sqrt(Math.Pow(10, (aPass / 10)) - 1);
            eStop = Math.Sqrt(Math.Pow(10, (aStop / 10)) - 1);
            w = omegaStop / omegaPass;
            passE = eStop / ePass;
            nExact = Math.Abs(Math.Log(passE) / Math.Log(w));
            nUsed = (int)Math.Ceiling(nExact);
            int k = (int)Math.Floor(nUsed / 2d);
            theta = new double[k];
            omega0 = omegaPass / (Math.Pow(ePass, 1d / (double)nUsed));
            thetaD = new double[2 * nUsed - 1];

            for (int i = 0; i < k; i++)
                theta[i] = (PI / (2 * nUsed)) * (nUsed - 1 + (2 * (i + 1)));
            for (int t = 0; t < k; t++)
                thetaD[t] = -2 * Math.Cos(theta[t]);
            if (nUsed % 2 == 1)
                thetaD[0] = 1 + omega0;

            filterCoefficientG = new double[(int)k + 1];
            filterCoefficientA = new double[(int)k + 1, 2];
            filterCoefficientB = new double[(int)k + 1, 3];

            double[] matrixBHelp =
            {
                1,
                1,
                0
            };

            for (int i = 0; i < k; i++)
            {
                filterCoefficientG[i + 1] = Math.Pow(omega0, 2) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 0] = (2 * (Math.Pow(omega0, 2) - 1)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientA[i + 1, 1] = (1 + 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2)) / (1 - 2 * omega0 * Math.Cos(theta[i]) + Math.Pow(omega0, 2));
                filterCoefficientB[i + 1, 0] = filterCoefficientG[i + 1] * 1;
                filterCoefficientB[i + 1, 1] = filterCoefficientG[i + 1] * 2;
                filterCoefficientB[i + 1, 2] = filterCoefficientG[i + 1] * 1;
            }
            if (nUsed % 2 != 0)
            {
                filterCoefficientG[0] = omega0 / (omega0 + 1);
                filterCoefficientA[0, 0] = (omega0 - 1) / (omega0 + 1);
                filterCoefficientA[0, 1] = 0;
                for (int i = 0; i < 2; i++)
                    filterCoefficientB[0, i] = filterCoefficientG[0] * matrixBHelp[i];
            }
            return FilteringClass.SOSFiltering(audio, filterCoefficientA, filterCoefficientB, nUsed);
        }



    }


    /// <summary>
    /// Class that implement different kind of filters. Currently a Second Order Section (SOSFiltering) and a Fourth Order Section (FOSFiltering) are implemented. 
    /// </summary>
    internal static class FilteringClass
    {//both of the filtering methods should capture errors, e.g. arrays being null or A and B having different ranks
        /// <summary>
        /// Second Order Section (SOS) filtering.
        /// </summary>
        /// <param name="signal">The signal to be filtered.</param>
        /// <param name="filterCoefficientA"> The feedback filter coefficients.</param>
        /// <param name="filterCoefficientB">The feedforward filter coefficients</param>
        /// <param name="nUsed">The order of the filter.</param>
        /// <returns>Returns the filtered <paramref name="signal"/>.</returns>
        static public double[] SOSFiltering(double[] signal, double[,] filterCoefficientA, double[,] filterCoefficientB, int nUsed)
        {
            int startSOS = nUsed % 2 == 0 ? 1 : 0;
            int k = filterCoefficientA.GetLength(0);
            double[,] delayMatrix = new double[k, 3];
            double[] dataOut = new double[signal.Length];
            double[] dataY = new double[k];
            double[] dataIn = new double[k + 1];

            for (long n = 0; n < signal.Length; n++)
            {
                dataIn[0 + startSOS] = signal[n];
                for (int i = startSOS; i < k; i++)
                {
                    delayMatrix[i, 0] = dataIn[i] - filterCoefficientA[i, 0] * delayMatrix[i, 1] - filterCoefficientA[i, 1] * delayMatrix[i, 2];
                    dataY[i] = filterCoefficientB[i, 0] * delayMatrix[i, 0] + filterCoefficientB[i, 1] * delayMatrix[i, 1] + filterCoefficientB[i, 2] * delayMatrix[i, 2];
                    delayMatrix[i, 2] = delayMatrix[i, 1];
                    delayMatrix[i, 1] = delayMatrix[i, 0];
                    dataIn[i + 1] = dataY[i];
                }
                dataOut[n] = dataY[dataY.Length - 1];
            }
            //DataWriter.WriteToText(dataOut, "check.txt");
            return dataOut;
        }


        /// <summary>
        /// Fourth Order Section (FOS) filtering.
        /// </summary>
        /// <param name="signal">The signal to be filtered.</param>
        /// <param name="filterCoefficientA"> The feedback filter coefficients.</param>
        /// <param name="filterCoefficientB">The feedforward filter coefficients</param>
        /// <param name="nUsed">The order of the filter.</param>
        /// <returns>Returns the filtered <paramref name="signal"/>.</returns>
        static public double[] FOSFiltering(double[] signal, double[,] filterCoefficientA, double[,] filterCoefficientB, int nUsed)
        {
            int startSOS = nUsed % 2 == 0 ? 1 : 0;
            int k = filterCoefficientA.GetLength(0);
            double[,] delayMatrix = new double[k, 5];
            double[] dataOut = new double[signal.Length];
            double[] dataY = new double[k];
            double[] dataIn = new double[k + 1];

            for (long n = 0; n < signal.Length; n++)
            {
                dataIn[0 + startSOS] = signal[n];
                for (int i = startSOS; i < k; i++)
                {
                    delayMatrix[i, 0] = dataIn[i] - filterCoefficientA[i, 0] * delayMatrix[i, 1] - filterCoefficientA[i, 1] * delayMatrix[i, 2] - filterCoefficientA[i, 2] * delayMatrix[i, 3] - filterCoefficientA[i, 3] * delayMatrix[i, 4];
                    dataY[i] = filterCoefficientB[i, 0] * delayMatrix[i, 0] + filterCoefficientB[i, 1] * delayMatrix[i, 1] + filterCoefficientB[i, 2] * delayMatrix[i, 2] + filterCoefficientB[i, 3] * delayMatrix[i, 3] + filterCoefficientB[i, 4] * delayMatrix[i, 4];
                    delayMatrix[i, 4] = delayMatrix[i, 3];
                    delayMatrix[i, 3] = delayMatrix[i, 2];
                    delayMatrix[i, 2] = delayMatrix[i, 1];
                    delayMatrix[i, 1] = delayMatrix[i, 0];
                    dataIn[i + 1] = dataY[i];
                }
                dataOut[n] = dataY[dataY.Length - 1];
            }
            //DataWriter.WriteToText(dataOut, "FOScheck.txt");
            return dataOut;
        }
    }

    /// <summary>
    /// Contains math that System.Math does not.
    /// </summary>
    static class MathSupport
    {
        /// <summary>
        /// Calculates and returns the Cot of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value given to Cot.</param>
        /// <returns>Returns the Cot of <paramref name="value"/>.</returns>
        public static double Cot(double value) //consider moving this into its own class or liberay
        {
            return 1d / Math.Tan(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="arrayToFilter"></param>
        /// <returns></returns>
        static public double[] Convolution(double[] filter, double[] arrayToFilter)
        {
            //int m = u.Length;
            //int n = v.Length;
            double[] w = new double[filter.Length + arrayToFilter.Length - 1];
            int k = w.Length;

            for (int n = 0; n < k; n++)
                for (int m = Math.Max(0, n - arrayToFilter.Length + 1); m <= Math.Min(n, filter.Length - 1); m++)
                    w[n] += filter[m] * arrayToFilter[n - m];
            return w;
        }

    }

}
