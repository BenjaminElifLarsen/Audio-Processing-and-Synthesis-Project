using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extensions;

namespace Audio_Code_Testbed
{
    static class PitchEstimation
    {
        /// <summary>
        /// Calculates the upper and lower boundries, given in angular frequencies.
        /// </summary>
        /// <param name="lowerBoundry">The lower boundry, given in ordinary frequency.</param>
        /// <param name="upperBoundry">The upper boundry, given in ordinary frequency.</param>
        /// <param name="samplingRate">The sampling rate.</param>
        /// <returns>Returns array containing the upper and lower boundries in angular frequencies.</returns>
        private static uint[] TauBoundries(uint lowerBoundry, uint upperBoundry, uint samplingRate)
        {
            double[] fb =
            {
                (double)lowerBoundry/(double)samplingRate,
                (double)upperBoundry/(double)samplingRate
            };
            uint[] tauBoundies =
            {
                (uint)Math.Ceiling(1d/fb[1]),
                (uint)Math.Floor(1d/fb[0])
            };
            return tauBoundies;
        }

        /// <summary>
        /// Gets the mean of an array
        /// </summary>
        /// <param name="signal">The array to calculate the mean of.</param>
        /// <returns></returns>
        private static double Mean(double[] signal)
        { //make this an extension
            double value = 0;
            foreach (double dbl in signal)
                value += dbl;
            return value / (double)signal.Length;
        }

        /// <summary>
        /// Function that calculates the ordinary f0 estimation of signal using auto correlation.
        /// If the frequency boundies are to low compared to the signal, both the cost function and estimated frequency will return null.
        /// </summary>
        /// <param name="signal">The signal to estimate f0 of.</param>
        /// <param name="lowerBoundry">The lower ordinary frequency boundry.</param>
        /// <param name="upperBoundry">The upper ordinary frequency boundry.</param>
        /// <param name="samplingRate">The sampling rate of the signal.</param>
        /// <param name="costFunction">A jaggered array where the first array is the cost and the second array is the frequencies the cost should be plotted over.</param>
        /// <returns>Returns an unsigned interger that is the estimation of the ordinary f0</returns>
        public static uint? AutoCorrelation(double[,] signal, uint lowerBoundry, uint upperBoundry, uint samplingRate, out double[][] costFunction)
        { //make it return a value for each channel
            double[] currentSignal = new double[signal.GetLength(0)];
            for (uint i = 0; i < currentSignal.Length; i++)
                currentSignal[i] = signal[i, 0];
            return  AutoCorrelation(currentSignal, lowerBoundry, upperBoundry, samplingRate, out costFunction);
        }

        /// <summary>
        /// Function that, actually, calculates the ordinary f0 estimation of signal using auto correlation.
        /// If the frequency boundies are to low compared to the signal, both the cost function and estimated frequency will return null.
        /// </summary>
        /// <param name="signal">The signal to estimate f0 of.</param>
        /// <param name="lowerBoundry">The lower ordinary frequency boundry.</param>
        /// <param name="upperBoundry">The upper ordinary frequency boundry.</param>
        /// <param name="samplingRate">The sampling rate of the signal.</param>
        /// <param name="costFunction">A jaggered array where the first array is the cost and the second array is the frequencies the cost should be plotted over.</param>
        /// <returns>Returns an unsigned interger that is the estimation of the ordinary f0</returns>
        public static uint? AutoCorrelation(double[] signal, uint lowerBoundry, uint upperBoundry, uint samplingRate, out double[][] costFunction)
        { //find some way to optime them.
            try
            {
                uint[] boundies = TauBoundries(lowerBoundry, upperBoundry, samplingRate);
                List<uint> delayVector = new List<uint>();
                for (uint i = boundies[0]; i < boundies[1]; i++)
                    delayVector.Add(i);
                int delayAmount = delayVector.Count;
                double[] cost = new double[delayAmount];
                double[] truncatedSignal;
                double[] shiftedSignal;
                double[] toMeanSignal;
                for (int i = 0; i < delayAmount; i++)
                {
                    double tauDelay = delayVector[i];
                    truncatedSignal = signal.Selection(0, (uint)(signal.Length - 1 - tauDelay));
                    shiftedSignal = signal.Selection((uint)(tauDelay), (uint)signal.Length - 1);
                    toMeanSignal = new double[truncatedSignal.Length];
                    for (int k = 0; k < toMeanSignal.Length; k++)
                        toMeanSignal[k] = truncatedSignal[k] * shiftedSignal[k];
                    cost[i] = Mean(toMeanSignal);
                }
                int index = 0;
                double indexMaxValue = double.MinValue;
                for (int i = 0; i < cost.Length; i++)
                    if (cost[i] > indexMaxValue)
                    {
                        index = i;
                        indexMaxValue = cost[i];
                    }
                double[] frequencyVector = new double[delayVector.Count];
                double[] freqVector = new double[delayVector.Count];
                for (int i = frequencyVector.Length - 1; i >= 0; i--)
                    frequencyVector[delayVector.Count - i - 1] = (double)samplingRate / (double)delayVector[i]; //check old code at home to see if you use 1 or sampling rate
                     //in the code that calls the functions and create the tables. Also refind that book
                double[] costFlipped = new double[cost.Length];
                for (int i = cost.Length - 1; i >= 0; i--)
                    costFlipped[cost.Length - i - 1] = cost[i];
                costFunction = new double[2][];
                costFunction[0] = costFlipped;
                costFunction[1] = frequencyVector;

                uint pitchEstimated = samplingRate / delayVector[index];
                return pitchEstimated;
            }
            catch(IndexOutOfRangeException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                System.Diagnostics.Debug.WriteLine("Signal is to short for the tau boundries.");
                costFunction = null;
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                costFunction = null;
                return null;
            }
        }

        /// <summary>
        /// Function that calculates the ordinary f0 estimation of signal using a comb filter.
        /// If the frequency boundies are to low compared to the signal, both the cost function and estimated frequency will return null.
        /// </summary>
        /// <param name="signal">The signal to estimate f0 of.</param>
        /// <param name="lowerBoundry">The lower ordinary frequency boundry.</param>
        /// <param name="upperBoundry">The upper ordinary frequency boundry.</param>
        /// <param name="samplingRate">The sampling rate of the signal.</param>
        /// <param name="costFunction">A jaggered array where the first array is the cost and the second array is the frequencies the cost should be plotted over.</param>
        /// <returns>Returns an unsigned interger that is the estimation of the ordinary f0</returns>
        public static uint? CombFilter(double[,] signal, uint lowerBoundry, uint upperBoundry, uint samplingRate, out double[][] costFunction)
        { //make it calculate for each
            double[] currentSignal = new double[signal.GetLength(0)];
            for (uint i = 0; i < currentSignal.Length; i++)
                currentSignal[i] = signal[i, 0];
            return CombFilter(currentSignal, lowerBoundry, upperBoundry, samplingRate, out costFunction);
        }

        /// <summary>
        /// Function that, actually, calculates the ordinary f0 estimation of signal using a comb filter.
        /// If the frequency boundies are to low compared to the signal, both the cost function and estimated frequency will return null.
        /// </summary>
        /// <param name="signal">The signal to estimate f0 of.</param>
        /// <param name="lowerBoundry">The lower ordinary frequency boundry.</param>
        /// <param name="upperBoundry">The upper ordinary frequency boundry.</param>
        /// <param name="samplingRate">The sampling rate of the signal.</param>
        /// <param name="costFunction">A jaggered array where the first array is the cost and the second array is the frequencies the cost should be plotted over.</param>
        /// <returns>Returns an unsigned interger that is the estimation of the ordinary f0</returns>
        public static uint? CombFilter(double[] signal, uint lowerBoundry, uint upperBoundry, uint samplingRate, out double[][] costFunction)
        {
            try
            {
                uint[] boundies = TauBoundries(lowerBoundry, upperBoundry, samplingRate);
                List<uint> delayVector = new List<uint>();
                for (uint i = boundies[0]; i < boundies[1]; i++)
                    delayVector.Add(i);
                int delayAmount = delayVector.Count;
                double[] cost = new double[delayAmount];
                for (int i = 0; i < delayAmount; i++)
                {
                    double tauDelay = delayVector[i];
                    double[] truncatedSignal = signal.Selection(0, (uint)(signal.Length - 1 - tauDelay));
                    double[] shiftedSignal = signal.Selection((uint)(tauDelay), (uint)signal.Length - 1);
                    double[] toMeanSignal = new double[truncatedSignal.Length];
                    for (int k = 0; k < toMeanSignal.Length; k++)
                        toMeanSignal[k] = truncatedSignal[k] - shiftedSignal[k];
                    for (int n = 0; n < toMeanSignal.Length; n++)
                        toMeanSignal[n] = Math.Pow(Math.Abs(toMeanSignal[n]), 2);
                    cost[i] = Mean(toMeanSignal);
                }
                int index = 0;
                double indexMinValue = double.MaxValue;
                for (int i = 0; i < cost.Length; i++)
                    if (cost[i] < indexMinValue)
                    {
                        index = i;
                        indexMinValue = cost[i];
                    }
                double[] frequencyVector = new double[delayVector.Count];
                double[] freqVector = new double[delayVector.Count];
                for (int i = frequencyVector.Length - 1; i >= 0; i--)
                    frequencyVector[delayVector.Count - i - 1] = (double)samplingRate / (double)delayVector[i]; //check old code at home to see if you use 1 or sampling rate                                                                                        
                //in the code that calls the functions and create the tables. Also refind that book
                double[] costFlipped = new double[cost.Length];
                for (int i = cost.Length - 1; i >= 0; i--)
                    costFlipped[cost.Length - i - 1] = cost[i];
                costFunction = new double[2][];
                costFunction[0] = costFlipped;
                costFunction[1] = frequencyVector;

                uint pitchEstimated = samplingRate / delayVector[index];
                return pitchEstimated;
            }
            catch (IndexOutOfRangeException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                System.Diagnostics.Debug.WriteLine("Signal is to short for the tau boundries.");
                costFunction = null;
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                costFunction = null;
                return null;
            }
        }

    }
}
