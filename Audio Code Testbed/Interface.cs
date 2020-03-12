using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Extensions;

namespace Audio_Code_Testbed
{
    //when asking the user to write turn the cursor visible while they do it
    //figure out a way to ensure the threads always write on the right side 

    /// <summary>
    /// Interface class for the audio processing program.
    /// </summary>
    class Interface
    {
        private InterfaceValues IFValues = new InterfaceValues();
        private CurrentAudioInformation currentAudio;


        /// <summary>
        /// Contains variablse that multiple functions belonging to the class <c>Interface</c> uses.
        /// </summary>
        internal class InterfaceValues
        {
            private byte height;
            private byte width;
            private string control;
            private byte threadWriteHeight;
            private byte writWidth;
            private byte amountOfLines;
            private ConcurrentBag<string> tempKeys;
            private string programStorageLocation;
            private string saveLocation;
            private byte threadSeperatorXLocation;
            private byte[] offSet;
            private byte[] writtenStartLocation;
            private byte[] writtenEndLocation;
            private byte[] optionColours;
            private byte[] selectedOption;
            private bool validationTrailRemover;
            private string pitchEstimationSaveLocation;
            private string audioSaveLocation;
            private string loadLocation;
            private string plotSaveLocation;
            private readonly string saveLocationDefault;
            private readonly string loadLocationDefault;


            public InterfaceValues()
            {
                height = 20;
                width = 100;
                control = "Arrowkeys to select option. Enter to comfirm. ";
                threadWriteHeight = 2;
                writWidth = 80;
                tempKeys = new ConcurrentBag<string>();
                programStorageLocation = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\Code\Audio Processing Program\";
                threadSeperatorXLocation = (byte)(writWidth + 1);
                offSet = new byte[] { 3, 1 }; //x,y
                writtenStartLocation = new byte[] { 5, 2 }; //x,y
                writtenEndLocation = new byte[] { writWidth, 17 }; //x,y
                optionColours = new byte[] { 0, 150, 0 };
                selectedOption = new byte[] { 200, 0, 0 };
                saveLocation = $@"{programStorageLocation}Files\Saved";
                loadLocation = $@"{programStorageLocation}Files\Load";
                pitchEstimationSaveLocation = $@"{saveLocation}\Pitch Estimation";
                saveLocationDefault = saveLocation;
                loadLocationDefault = loadLocation;
                audioSaveLocation = $@"{saveLocation}\Audio";
                plotSaveLocation = $@"{saveLocation}\Plot";
                validationTrailRemover = false;
                amountOfLines = 16;
            }

            public string DefaultLoadLocation
            {
                get => loadLocationDefault;
            }

            public string DefaultSaveLocation
            {
                get => saveLocationDefault;
            }

            public string PlotSaveLocation
            {
                get => plotSaveLocation;
                set => plotSaveLocation = value;
            }

            public string AudioSaveLocation
            {
                get => audioSaveLocation;
                set => audioSaveLocation = value;
            }

            public string LoadLocation
            {
                get => loadLocation;
                set => loadLocation = value;
            }

            public string PitchEstimationSaveLocation
            {
                get => pitchEstimationSaveLocation;
                set => pitchEstimationSaveLocation = value;
            }

            public byte AmountOfWrittenLines
            {
                get => amountOfLines;
            }

            public bool ValidationTrailRemover
            {
                get => validationTrailRemover;
                set => validationTrailRemover = value;
            }

            public byte ThreadSeperatorXLocation
            {
                get => threadSeperatorXLocation;
            }

            public byte[] SelectedOption
            {
                get => selectedOption;
            }

            public byte[] OptionColours
            {
                get => optionColours;
            }

            public byte[] WrittenEndLocation
            {
                get => writtenEndLocation;
            }

            /// <summary>
            /// Returns a new array which entires are based upon the entires from the original array. 
            /// Modifications to the returned array does not affect the original array. 
            /// </summary>
            public byte[] WrittenStartLocation
            {
                get
                {
                    byte[] writeStartLocation =
                    {
                        writtenStartLocation[0],
                        writtenStartLocation[1]
                    };
                    return writeStartLocation;
                }
            }

            public byte[] OffSet
            {
                get => offSet;
            }

            public string SaveLocation
            {
                get => saveLocation;
                set => saveLocation = value;
            }

            public string StorageLocation
            {
                get => programStorageLocation;
            }

            public void SetTempKeyStorage(string key)
            {
                tempKeys.Add(key);
            }
            public ConcurrentBag<string> GetTempKeyStorage
            {
                get => tempKeys;
            }

            public byte Height
            {
                get => height;
                set => height = value;
            }
            public byte Width
            {
                get => width;
                set => width = value;
            }
            public string DefaultControlString
            {
                get => control;
            }

            public byte ThreadYLocation
            {
                get => threadWriteHeight;
                set => threadWriteHeight = value;
            }

            public byte WrittingWidth
            {
                get => writWidth;
                set => writWidth = value;
            }
        }

        /// <summary>
        /// Sets up the console window. Turns off cursor visibility. 
        /// </summary>
        /// <param name="height">Height of the console window.</param>
        /// <param name="width">width of the console window.</param>
        private void Setup(int height, int width)
        {
            Thread.CurrentThread.Name = "Main Thread";
            FolderCreation();
            Console.CursorVisible = false;
            Console.WindowHeight = height;
            Console.WindowWidth = width;
            Console.Title = "APSP";
            ScreenSeperator(height);
        }

        private void ScreenSeperator(int height)
        {
            int[] posistion = { Console.CursorLeft, Console.CursorTop };
            string threadInformation = "Threads";
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + ((int)Math.Round((float)threadInformation.Length / 2)), 1); //spent more time aligning it
            Console.WriteLine(threadInformation);
            for (int i = 0; i < height; i++)
            {
                Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation, i);
                Console.Write("|");
            }
            Console.SetCursorPosition(posistion[0], posistion[1]);
        }

        private uint FolderCreation()
        {
            uint error = 0;
            error += DataWriter.FolderCreator(IFValues.SaveLocation);
            error += DataWriter.FolderCreator(IFValues.LoadLocation);
            error += DataWriter.FolderCreator(IFValues.PitchEstimationSaveLocation);
            error += DataWriter.FolderCreator(IFValues.AudioSaveLocation);
            error += DataWriter.FolderCreator(IFValues.PlotSaveLocation);
            return error;
        }

        private void DataLocationRestore(string save, string load)
        {
            if (save != "")
                SetSaveLocations(save);
            if (load != "")
                SetLoadLocation(load);
            FolderCreation();
        }

        private void SetLoadLocation(string location)
        {
            IFValues.LoadLocation = location + "\\Load";
        }

        private void SetSaveLocations(string location)
        {
            IFValues.SaveLocation = location;
            IFValues.PitchEstimationSaveLocation = location + "\\Pitch Estimation";
            IFValues.AudioSaveLocation = location + "\\Audio";
            IFValues.PlotSaveLocation = location + "\\Plot";
        }

        private uint DataLocationUpdate(string newSaveLocation, string newLoadLocation)
        {
            if (newSaveLocation != IFValues.DefaultSaveLocation)
                SetSaveLocations(newSaveLocation);
            if (newLoadLocation != IFValues.DefaultLoadLocation)
                SetLoadLocation(newLoadLocation);
            return FolderCreation();
        }

        /// <summary>
        /// The main menu of the audio processing program
        /// </summary>
        public void MenuMain()
        { //allow the setting of the loading location
            Setup(IFValues.Height, IFValues.Width);

            string[] mainMenuOptions =
            {
                "Audio",
                "Options",
                "Information",
                "Exit"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, mainMenuOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Audio":
                        MenuAudio();
                        break;

                    case "Options":
                        MenuOptions();
                        break;

                    case "Information":
                        MenuInformation();
                        break;

                    case "Exit":
                        Console.Clear();
                        Environment.Exit(0);
                        break;
                }
            } while (true);
        }

        private void MenuInformation()
        {
            bool menuRun = true;
            string[] options =
            {
                "About",
                "Naming Policy",
                "Back"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "About":
                        MenuAbout();
                        break;

                    case "Naming Policy":
                        MenuNamePolicy();
                        break;

                    case "Back":
                        menuRun = false;
                        break;
                }
            } while (menuRun);
        }

        private void MenuOptions()
        {
            bool menu = false;
            string[] options =
            {
                "Naming Validation",
                "Save Location",
                "Load Location",
                "Back"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Naming Validation":
                        NameValidation();
                        break;

                    case "Save Location":
                        MenuSaveLocation();
                        break;

                    case "Load Location":
                        MenuLoadLocation();
                        break;

                    case "Back":
                        menu = true;
                        break;
                }
            } while (!menu);

            void NameValidation()
            {
                string canDelete = IFValues.ValidationTrailRemover ? "Is currently permitted. " : "Is currently not permiited.";
                string control = $"Auto remove trailing parts of a filename, if it prevents validation? \n{canDelete}";
                string[] valiOptions =
                {
                    "Yes",
                    "No"
                };
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(control, valiOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                IFValues.ValidationTrailRemover = answer == "Yes" ? true : false;
            }
        }

        private void MenuNamePolicy()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            string text = "NamingPolicy.txt";
            TextWrittenOut(text);
        }

        private void MenuAbout()
        { //make it read from a file and write out
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            string text = "About.txt";
            TextWrittenOut(text);
        }

        private void TextWrittenOut(string text)
        {
            string[] about = File.ReadAllLines(text);
            about.Display("\n");
            Console.WriteLine("\nEnter to return.");
            Console.ReadLine();
        }

        /// <summary>
        /// Menu used to select the different methods of calculating the filters. 
        /// </summary>
        private void MenuFiltering()
        {
            bool back = false;
            string[] filterMenu =
            {
                "Butterworth",
                "Chebyshev",
                "Back"
            };
            if (currentAudio != null)
            {
                do
                {
                    PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                    DrawAndSelect(IFValues.DefaultControlString, filterMenu, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                    switch (answer)
                    {
                        case "Butterworth":
                            MenuButterworth();
                            break;

                        case "Chebyshev":
                            MenuChebyshev();
                            break;

                        case "Back":
                            back = true;
                            break;
                    }
                } while (!back);
            }
            else
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("No current selected audio to filter on. \nEnter to return to last main menu.");
                Console.ReadLine();
            }
        }

        private void Testing()
        { //to be deleted when the bug have been figured out
            string[] testing = { "Machine Learning Menu - NI", "Machine Learning Menu - NI", "Machine Learning Menu - NI", "Machine Learning Menu - NI", "Machine Learning Menu - NI" };
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            DrawAndSelect(IFValues.DefaultControlString, testing, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string _, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
        }

        private string ChangeLocation(string type, string currentLocation)
        {
            string user = Environment.UserName;
            string[] saveOptions =
            {
                "Yes",
                "No"
            };
            string saveControl = String.Format("Current {1} location is: \n{0} \nDo you want to change it?", currentLocation, type);
            string tempSave = "";
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            byte[] writeLocation = IFValues.WrittenStartLocation;
            writeLocation[1]++;
            DrawAndSelect(saveControl, saveOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, writeLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
            if (answer == saveOptions[0])
            {
                Console.CursorVisible = true;
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("Please enter new {0} location. \nIf wanting not to change it, press enter while new save location is blank.", type);
                byte? success = null;
                do
                {
                    ScreenSeperator(IFValues.Height); //<- this function caused the scrolling
                    tempSave = Console.ReadLine(); //check if it is a valid path and also handle errors 
                    bool qualifiedPath = Path.IsPathFullyQualified(tempSave);
                    if (qualifiedPath || tempSave == "")
                    {
                        PartlyClear(IFValues.ThreadSeperatorXLocation + 1, 0, IFValues.Width, IFValues.Height);
                        success = 0;
                    }
                    else
                    {
                        PartlyClear(IFValues.ThreadSeperatorXLocation + 1, 0, IFValues.Width, IFValues.Height);
                        ScreenSeperator(IFValues.Height);
                        PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                        string write = String.Format("{0} location was not qualified. \nPlease enter a save location or black to not change.", type);
                        Console.WriteLine(write.ToUpperFirst()); //not done yet
                    }

                } while (success != 0);
                if (tempSave != "")
                    return tempSave;
                Console.CursorVisible = false;
            }
            ScreenSeperator(IFValues.Height);
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            return "";
        }

        private void MenuLoadLocation()
        {
            uint saveError = 0;
            string result = ChangeLocation("load", IFValues.LoadLocation);
            if (result != "")
                saveError = DataLocationUpdate(IFValues.SaveLocation, result);
            if (saveError != 0)
            {
                DataLocationRestore("", IFValues.DefaultLoadLocation);
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("No acccess to location. \nFailed at creating the new folder(s). \n Enter to return to the menu.");
                Console.ReadLine();
                PartlyClear(IFValues.ThreadSeperatorXLocation + 1, 0, IFValues.Width, IFValues.Height);
            }
            else
                if (result != "")
                    IFValues.LoadLocation = result;
        }

        private void MenuSaveLocation()
        {
            uint saveError = 0;
            string result = ChangeLocation("save", IFValues.SaveLocation);
            if (result != "")
                saveError = DataLocationUpdate(result, IFValues.LoadLocation);
            if (saveError != 0)
            {
                DataLocationRestore(IFValues.DefaultSaveLocation, "");
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("Failed at creating the new folder(s). \n Enter to return to the menu.");
                Console.ReadLine();
                PartlyClear(IFValues.ThreadSeperatorXLocation + 1, 0, IFValues.Width, IFValues.Height);
            }
            else
                if (result != "")
                    IFValues.SaveLocation = result;
        }
        
        private void MenuChebyshev()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            bool back = false;
            string[] chebyOptions =
            {
                "Type 1 Lowpass Filtering",
                "Back to Last Menu"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, chebyOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Type 1 Lowpass Filtering":
                        ChebyshevInterfaceType1LowPass();
                        break;

                    case "Back to Last Menu":
                        back = true;
                        break;
                }
            } while (!back);
        }

        /// <summary>
        /// The menu interface for the Butterworth filtering. Allow the selection of different filters. 
        /// </summary>
        private void MenuButterworth()
        { //filtering multiple times seems to cause a problem, could be from the high pass filtering not working as it should.
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            bool back = false;
            string[] butterOptions =
            {
                "Lowpass Filtering",
                "Highpass Filtering",
                "Bandpass Filtering",
                "Bandstop Filtering",
                "Back to Last Menu"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, butterOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Lowpass Filtering":
                        ButterworthInterfaceLowPass();
                        break;

                    case "Highpass Filtering":
                        ButterworthInterfaceHighPass();
                        break;

                    case "Bandpass Filtering":
                        ButterworthInterfaceBandPass();
                        break;

                    case "Bandstop Filtering":
                        ButterworthInterfaceBandStop();
                        break;

                    case "Back to Last Menu":
                        back = true;
                        break;
                }
            } while (!back);
        }

        private void GettingAttenuation(out float bandpassdB, out float bandstopdB)
        {
            Helper.NumberCollector("Enter passband attenuation and press enter: ", out bandpassdB);
            bandpassdB = Helper.ZeroNumberDetector(bandpassdB);
            Helper.NumberCollector("Enter stopband attenuation and press enter: ", out bandstopdB);
            bandstopdB = Helper.ZeroNumberDetector(bandstopdB);
        }

        private void GettingFrequencies(out uint frequencyBandpass, out uint frequencyBandstop)
        {
            Helper.NumberCollector("Enter passband frequency and press enter: ", out frequencyBandpass);
            frequencyBandpass = Helper.ZeroNumberDetector(frequencyBandpass);
            Helper.NumberCollector("Enter stopband frequency and press enter: ", out frequencyBandstop);
            frequencyBandstop = Helper.ZeroNumberDetector(frequencyBandstop);
        }

        private void GettingFrequencies(out uint frequencyBandpassA, out uint frequencyBandstopA, out uint frequencyBandpassB, out uint frequencyBandstopB)
        {
            Helper.NumberCollector("Enter lower passband frequency and press enter: ", out frequencyBandpassA);
            frequencyBandpassA = Helper.ZeroNumberDetector(frequencyBandpassA);
            Helper.NumberCollector("Enter lower stopband frequency and press enter: ", out frequencyBandstopA);
            frequencyBandstopA = Helper.ZeroNumberDetector(frequencyBandstopA);
            Helper.NumberCollector("Enter upper passband frequency and press enter: ", out frequencyBandpassB);
            frequencyBandpassB = Helper.ZeroNumberDetector(frequencyBandpassB);
            Helper.NumberCollector("Enter upper stopband frequency and press enter: ", out frequencyBandstopB);
            frequencyBandstopB = Helper.ZeroNumberDetector(frequencyBandstopB);
        }


        private void ChebyshevInterfaceType1LowPass()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            double[,] audioNoHeader = currentAudio.AudioHeaderLesss;
            uint samplingRate = currentAudio.SamplingRate;
            byte[] header = currentAudio.Header;
            bool smallerFrequency = false;
            bool smallerAttenuation = false;
            bool nyquistFrequency = false;
            float bandpassdB;
            float bandstopdB;
            string error = "";
            uint frequencyBandpass;
            uint frequencyBandstop;
            Console.CursorVisible = true;
            Console.WriteLine($"Sampling Rate: {samplingRate}");
            do
            {
                GettingFrequencies(out frequencyBandpass, out frequencyBandstop);
                smallerFrequency = frequencyBandstop >= frequencyBandpass ? true : false;
                nyquistFrequency = frequencyBandpass >= samplingRate / 2 | frequencyBandstop >= samplingRate / 2 ? true : false;
                error += nyquistFrequency ? "One or more frequencies are equal or above Nyquist frequency. \n" : "";
                error += smallerFrequency ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
            } while (nyquistFrequency || !smallerFrequency);
            do
            {
                GettingAttenuation(out bandpassdB, out bandstopdB);
                smallerAttenuation = bandstopdB >= bandpassdB ? true : false;
                error = smallerAttenuation ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (!smallerAttenuation);
            Console.CursorVisible = false;
            string name = KeyUsageCheckAndAdd();
            Thread chebyshevLowpassType1Thread = new Thread(ThreadFilteringHighPass);
            chebyshevLowpassType1Thread.Name = "Chebyshev Type 1 Lowpass";
            chebyshevLowpassType1Thread.Start();
            Thread.Sleep(10);
            void ThreadFilteringHighPass()
            {
                Debug.WriteLine(Thread.CurrentThread.Name);
                string workingIndicator = "Processing";
                int y = PreProcessingThreadText(workingIndicator);
                double[,] audio = ChebyshevFiltering.LowpassFilteringType1(audioNoHeader, samplingRate, frequencyBandpass, frequencyBandstop, bandpassdB, bandstopdB);
                FilterThreadHelpPostAudioFiltering(audio, name, workingIndicator, y, header);
            }
            Debug.WriteLine(Thread.CurrentThread.Name);
        }

        /// <summary>
        /// The interface for the Butterworth lowpass filtering.
        /// </summary>
        private void ButterworthInterfaceLowPass()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            bool smallerFrequency = false;
            bool smallerAttenuation = false;
            bool nyquistFrequency = false;
            float bandpassdB;
            float bandstopdB;
            string error = "";
            uint frequencyBandpass;
            uint frequencyBandstop;
            uint samplingRate = currentAudio.SamplingRate;
            byte[] header = currentAudio.Header;
            double[,] audioNoHeader = currentAudio.AudioHeaderLesss;
            Console.WriteLine($"Sampling Rate: {samplingRate}");
            Console.CursorVisible = true;
            do
            {
                GettingFrequencies(out frequencyBandpass, out frequencyBandstop);
                smallerFrequency = frequencyBandstop >= frequencyBandpass ? true : false;
                nyquistFrequency = frequencyBandpass >= samplingRate / 2 | frequencyBandstop >= samplingRate / 2 ? true : false;
                error += nyquistFrequency ? "One or more frequencies are equal or above Nyquist frequency. \n" : "";
                error += smallerFrequency ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
                error = "";
            } while (nyquistFrequency || !smallerFrequency);
            do
            {
                GettingAttenuation(out bandpassdB, out bandstopdB);
                smallerAttenuation = bandstopdB >= bandpassdB ? true : false;
                error = smallerAttenuation ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (!smallerAttenuation);
            Console.CursorVisible = false;
            string name = KeyUsageCheckAndAdd();
            Thread lowpassThread = new Thread(ThreadFilteringHighPass);
            lowpassThread.Name = "Butterworth Lowpass";
            lowpassThread.Start();
            Thread.Sleep(10);
            void ThreadFilteringHighPass()
            {
                Debug.WriteLine(Thread.CurrentThread.Name);
                string workingIndicator = "Processing";
                int y = PreProcessingThreadText(workingIndicator);
                double[,] audio = ButterworthFiltering.LowpassFiltering(audioNoHeader, samplingRate, frequencyBandpass, frequencyBandstop, bandpassdB, bandstopdB);
                FilterThreadHelpPostAudioFiltering(audio, name, workingIndicator, y, header);
            }
            Debug.WriteLine(Thread.CurrentThread.Name);
        }

        private void ButterworthInterfaceBandStop()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            double[,] audioNoHeader = currentAudio.AudioHeaderLesss;
            uint samplingRate = currentAudio.SamplingRate;
            byte[] header = currentAudio.Header;
            bool smallerFrequencyPassA;
            bool smallerFrequencyStopA;
            bool smallerFreuqencyStopB;
            bool smallerAttenuation = false;
            bool nyquistFrequencyLow = true;
            bool nyquistFrequencyUp = true;
            float bandpassdB;
            float bandstopdB;
            string error = "";
            uint frequencyBandpassA;
            uint frequencyBandstopA;
            uint frequencyBandpassB;
            uint frequencyBandstopB;
            Console.WriteLine($"Sampling Rate: {samplingRate}");
            Console.CursorVisible = true;
            do
            {
                error = "";
                GettingFrequencies(out frequencyBandpassA, out frequencyBandstopA, out frequencyBandpassB, out frequencyBandstopB);
                smallerFrequencyPassA = frequencyBandpassA <= frequencyBandstopA ? true : false;
                smallerFrequencyStopA = frequencyBandstopA <= frequencyBandstopB ? true : false;
                smallerFreuqencyStopB = frequencyBandstopB <= frequencyBandpassB ? true : false;

                if (smallerFrequencyPassA && smallerFrequencyStopA && smallerFreuqencyStopB)
                {
                    nyquistFrequencyLow = frequencyBandpassA >= samplingRate / 2 ? true : false;
                    nyquistFrequencyUp = frequencyBandpassB >= samplingRate / 2 ? true : false;
                    if (nyquistFrequencyUp || nyquistFrequencyLow)
                        error = "One or more of the frequencies equal or are above the nyquist frequency. \nReenter values";
                }
                else
                    error = "Passband and stopband frequencies are not given correctly. \nReenter values";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (nyquistFrequencyUp || nyquistFrequencyLow);
            do
            {
                GettingAttenuation(out bandpassdB, out bandstopdB);
                smallerAttenuation = bandstopdB >= bandpassdB ? true : false;
                error = smallerAttenuation ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (!smallerAttenuation);


            Console.CursorVisible = false;
            string name = KeyUsageCheckAndAdd();
            Thread butterworthBandpstop = new Thread(ThreadFilteringHighPass);
            butterworthBandpstop.Name = "Butterworth Bandstop";
            butterworthBandpstop.Start();
            Thread.Sleep(10);
            void ThreadFilteringHighPass()
            {
                Debug.WriteLine(Thread.CurrentThread.Name);
                string workingIndicator = "Processing";
                int y = PreProcessingThreadText(workingIndicator);
                //ButterworthLowPass filter = new ButterworthLowPass(samplingRate, frequencyBandpass, frequencyBandstop, bandpassdB, bandstopdB, false);
                //double[,] audio = filter.Filter(audioNoHeader);
                double[,] audio = ButterworthFiltering.BandstopFiltering(audioNoHeader, samplingRate, frequencyBandpassA, frequencyBandpassB, frequencyBandstopA, frequencyBandstopB, bandpassdB, bandstopdB); 
                FilterThreadHelpPostAudioFiltering(audio, name, workingIndicator, y, header);
            }
            Debug.WriteLine(Thread.CurrentThread.Name);
        }


        private void ButterworthInterfaceBandPass()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            double[,] audioNoHeader = currentAudio.AudioHeaderLesss;
            uint samplingRate = currentAudio.SamplingRate;
            byte[] header = currentAudio.Header;
            bool smallerFrequencyStopA = false;
            bool smallerFrequencyPassA = false;
            bool smallerFrequencyPassB = false;
            bool smallerAttenuation = false;
            bool nyquistFrequencyLow = true;
            bool nyquistFrequencyUp = true;
            float bandpassdB;
            float bandstopdB;
            string error = "";
            uint frequencyBandpassA;
            uint frequencyBandstopA;
            uint frequencyBandpassB;
            uint frequencyBandstopB;
            Console.WriteLine($"Sampling Rate: {samplingRate}");
            Console.CursorVisible = true;
            do
            {
                error = "";
                GettingFrequencies(out frequencyBandpassA, out frequencyBandstopA, out frequencyBandpassB, out frequencyBandstopB);
                smallerFrequencyStopA = frequencyBandstopA < frequencyBandpassA ? true : false;
                smallerFrequencyPassA = frequencyBandpassA < frequencyBandpassB ? true : false;
                smallerFrequencyPassB = frequencyBandpassB < frequencyBandstopB ? true : false;

                if(smallerFrequencyPassA && smallerFrequencyPassB && smallerFrequencyStopA)
                {
                    nyquistFrequencyLow = frequencyBandstopA >= samplingRate / 2 ? true : false;
                    nyquistFrequencyUp = frequencyBandstopB >= samplingRate / 2 ? true : false;
                    if (nyquistFrequencyUp || nyquistFrequencyLow)
                        error = "One or more of the frequencies equal or are above the nyquist frequency. \nReenter values";
                }
                else
                    error = "Passband and stopband frequencies are not given correctly. \nReenter values";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (nyquistFrequencyUp || nyquistFrequencyLow);
            do
            {
                GettingAttenuation(out bandpassdB, out bandstopdB);
                smallerAttenuation = bandstopdB >= bandpassdB ? true : false;
                error = smallerAttenuation ? "" : "Bandpass is bigger than or equal to bandstop. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (!smallerAttenuation);
            Console.CursorVisible = false;
            string name = KeyUsageCheckAndAdd();
            Thread butterworthBandpass = new Thread(ThreadFilteringHighPass);
            butterworthBandpass.Name = "Butterworth Bandpass";
            butterworthBandpass.Start();
            Thread.Sleep(10);
            void ThreadFilteringHighPass()
            {
                Debug.WriteLine(Thread.CurrentThread.Name);
                string workingIndicator = "Processing";
                int y = PreProcessingThreadText(workingIndicator);
                //ButterworthLowPass filter = new ButterworthLowPass(samplingRate, frequencyBandpass, frequencyBandstop, bandpassdB, bandstopdB, false);
                //double[,] audio = filter.Filter(audioNoHeader);
                double[,] audio = ButterworthFiltering.BandpassFiltering(audioNoHeader, samplingRate, frequencyBandpassA, frequencyBandpassB, frequencyBandstopA, frequencyBandstopB, bandpassdB, bandstopdB);
                FilterThreadHelpPostAudioFiltering(audio, name, workingIndicator, y, header);
            }
            Debug.WriteLine(Thread.CurrentThread.Name);
        }

        /// <summary>
        /// The interface for the Butterworth highpass filtering.
        /// </summary>
        private void ButterworthInterfaceHighPass()
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            bool smallerFrequency = false;
            bool smallerAttenuation = false;
            bool nyquistFrequency = false;
            float bandpassdB;
            float bandstopdB;
            string error = "";
            uint frequencyBandpass;
            uint frequencyBandstop;
            uint samplingRate = currentAudio.SamplingRate;
            byte[] header = currentAudio.Header;
            double[,] audioNoHeader = currentAudio.AudioHeaderLesss;
            Console.CursorVisible = true;
            Console.WriteLine($"Sampling Rate: {samplingRate}");
            do
            {
                GettingFrequencies(out frequencyBandpass, out frequencyBandstop);
                smallerFrequency = frequencyBandpass >= frequencyBandstop ? true : false;
                nyquistFrequency = frequencyBandpass >= samplingRate / 2 | frequencyBandstop >= samplingRate / 2 ? true : false;
                error += nyquistFrequency ? "One or more frequencies are equal or above Nyquist frequency. \n" : "";
                error += smallerFrequency ? "" : "Bandstop is bigger than or equal to bandpass. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
                error = "";
            } while (!smallerFrequency || nyquistFrequency);
            do
            {
                GettingAttenuation(out bandpassdB, out bandstopdB);
                smallerAttenuation = bandstopdB >= bandpassdB ? true : false;
                error = smallerAttenuation ? "" : "Bandpass is bigger than or equal to bandpass. Reenter values. ";
                PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
            } while (!smallerAttenuation);
            Console.CursorVisible = false;
            string name = KeyUsageCheckAndAdd();
            Thread highpassThread = new Thread(ThreadFilteringHighPass);
            highpassThread.Name = "Butterworth Highpass";
            highpassThread.Start();
            Thread.Sleep(10);
            void ThreadFilteringHighPass()
            {
                Debug.WriteLine(Thread.CurrentThread.Name);
                string workingIndicator = "Processing";
                int y = PreProcessingThreadText(workingIndicator);
                double[,] audio = ButterworthFiltering.HighpassFiltering(audioNoHeader, samplingRate, frequencyBandpass, frequencyBandstop, bandpassdB, bandstopdB);
                FilterThreadHelpPostAudioFiltering(audio, name, workingIndicator, y, header);
            }
            Debug.WriteLine(Thread.CurrentThread.Name);
        }

        private void FilterThreadHelpPostAudioFiltering(double[,] audio, string key, string workingIndicator, int y, byte[] header)
        {
            double[,] audioWithHeader = AudioStorageProcessing.AddWaveToSignal(audio, header);
            Storage.SignalToStorage(audioWithHeader, key);
            RemoveKeyFromTempKeyStorage(key);
            PostProcessingThreadText(workingIndicator, y);
        }

        private int PreProcessingThreadText(string workingIndicator = "Processing")
        {
            int threadWriteY = IFValues.ThreadYLocation++;
            int[] location =
            {
                Console.CursorLeft,
                Console.CursorTop
            };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write(workingIndicator);
            Console.SetCursorPosition(location[0], location[1]);
            return threadWriteY;
        }
        private void PostProcessingThreadText(string workingIndicator, int threadWriteY)
        {
            //Debug.WriteLine(Thread.CurrentThread.Name);
            IFValues.ThreadYLocation--;
            int[] location = new int[]
            {
                    Console.CursorLeft,
                    Console.CursorTop
            };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write("".PadLeft(workingIndicator.Length));
            Console.SetCursorPosition(location[0], location[1]);
        }

        /// <summary>
        /// Removes <paramref name="key"/> from the temp storage, if the key exist.
        /// </summary>
        /// <param name="key">The key to try and remove. </param>
        private void RemoveKeyFromTempKeyStorage(string key)
        {
            //Debug.WriteLine(Thread.CurrentThread.Name);
            IFValues.GetTempKeyStorage.TryTake(out key);
        }

        /// <summary>
        /// Clears everything between <paramref name="xStart"/> to <paramref name="xEnd"/> and <paramref name="yStart"/> to <paramref name="yEnd"/>.
        /// If <paramref name="optionalCursorX"/> and/or <paramref name="optionalCursorY"/> are not null, their values will be used to set the cursor location
        /// after clearing, else <paramref name="xStart"/> and/or <paramref name="yStart"/> are used.
        /// The function is unable to clear more columns than the width of the console window.
        /// </summary>
        /// <param name="xStart">First column to start on.</param>
        /// <param name="yStart">First row to start on.</param>
        /// <param name="xEnd">Last column to end on.</param>
        /// <param name="yEnd">Last row to end on.</param>
        /// <param name="optionalCursorX">An optional placement for the curser, left, after clearing.</param>
        /// <param name="optionalCursorY">An optional placement for the curser, top, after clearing.</param>
        private void PartlyClear(int xStart, int yStart, int xEnd, int yEnd, int? optionalCursorX = null, int? optionalCursorY = null)
        { //When it jumps to the next line it will erase the first sign
            xEnd = xEnd == Console.WindowWidth ? xEnd-1: xEnd;
            int x = optionalCursorX != null ? (int)optionalCursorX : xStart;
            int y = optionalCursorY != null ? (int)optionalCursorY : yStart;
            if (!(xStart >= xEnd))
                for (int i = yStart; i <= yEnd; i++)
                {
                    Console.SetCursorPosition(xStart, i);
                    Console.Write("".PadLeft(xEnd - xStart + 1)); //plus one because of the first index is 0. E.g. xEnd = 80 and xStart is 0, it goes from 0-79 without the + 1
                    //however, if xEnd is the width, e.g. width = 80, columns go from 0 - 79 + 1, it will go down to the next line and remove the  first column there
                }
            Console.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Request the user to enter the name that will become a new key for an entry for a dictionary.
        /// Will check if the key is in used and reask if it is. It will also check if a key is reserved by another part of the program, but the key has not been added to the dictionary yet.
        /// If the key is not in use or reserved, the function will reserve the key, but not place it in the dictionary.
        /// The function returns the key.
        /// </summary>
        /// <param name="controlInformation">String to ask for user interaction.</param>
        /// <returns>Returns string that contain the new key</returns>
        private string KeyUsageCheckAndAdd(string controlInformation = "Enter filename: ")
        {
            //To fix: It permits empty strings, perhaps add empty '' to the forbidden list in FilenameValid, empty chars are not allowed... also, should still catch all errors and solve them in the function itself
            uint value;
            string key;
            string nameControl = controlInformation;
            string[] usedNames;
            Console.CursorVisible = true;
            do
            {
                bool valid = false;
                do
                {
                    Console.Write(nameControl);
                    key = Console.ReadLine();
                    if (IFValues.ValidationTrailRemover)
                        key = Helper.FileRemoveInvalidEndSigns(key);
                    valid = Helper.FilenameValid(key);
                    nameControl = "Name is not valid. Please try another one: ";
                } while (!valid);
                value = 0;
                usedNames = SaveNames();
                foreach (string str in usedNames)
                    if (str.ToUpper() == key.ToUpper())
                        value = 1;
                if (IFValues.GetTempKeyStorage.Count != 0)
                    foreach (string str in IFValues.GetTempKeyStorage)
                        if (str.ToUpper() == key.ToUpper())
                            value = 1;
                nameControl = "Name is already in use, try another one: ";
            } while (value == 1);
            Console.CursorVisible = false;
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            IFValues.SetTempKeyStorage(key);
            return key;
        }

        /// <summary>
        /// Function that implement the menu for primary audio function. 
        /// </summary>
        private void MenuAudio()
        {
            bool back = false;
            PartlyClear(IFValues.WrittenStartLocation[0], IFValues.WrittenStartLocation[1], IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            string[] audioMenuOptions =
            {
                "Load/Set Audio",
                "Save",
                "Filtering",
                "Machine Learning - NI",
                "Synthesis - IP",
                "Pitch Estimation - IP",
                "Back to Main Menu"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, audioMenuOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Load/Set Audio":
                        MenuAudioLoad();
                        break;

                    case "Save":
                        MenuSave();
                        break;

                    case "Filtering":
                        MenuFiltering();
                        break;

                    case "Machine Learning - NI":
                        Testing();
                        break;

                    case "Synthesis - IP":
                        MenuSyntehsis();
                        break;

                    case "Pitch Estimation - IP":
                        MenuPitchEstimation();
                        break;

                    case "Back to Main Menu":
                        back = true;
                        break;
                }
            } while (!back);

            
            void MenuAudioLoad() //consider moving this and the functions below outside the scope.
            {
                bool menu = false;
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                string[] options =
                {
                    "Load audio and set as current",
                    "Set current audio",
                    "Load audio",
                    "Remove audio",
                    "Back"
                };

                do
                {
                    PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                    DrawAndSelect(IFValues.DefaultControlString, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                    switch (answer)
                    {
                        case "Load audio and set as current":
                            MenuAudioLoadAndSet();
                            break;

                        case "Set current audio":
                            MenuAudioCurrent();
                            break;

                        case "Load audio":
                            MenuLoadAudio();
                            break;

                        case "Remove audio":
                            MenuRemoveAudio();
                            break;

                        case "Back":
                            menu = true;
                            break;
                    }
                } while (!menu);
            }

            void MenuRemoveAudio()
            {
                FindAudioFiles(out string[] filenames);
                string[] filenamesWithBack = AddStringToOptionArray(filenames, "Back"); //consider seperating into two strings, where the new string has a string parameter
                RemoveAudio(filenamesWithBack);
            }

            void MenuAudioCurrent()
            {
                FindAudioFiles(out string[] filenames);
                string[] filenamesWithBack = AddStringToOptionArray(filenames, "Back");
                SelectAudio(filenamesWithBack);
            }

            void MenuLoadAudio()
            {
                FindAudioFiles(out string[] filenames, out string[] existingFiles);
                string[] filenamesWithBack = AddStringToOptionArray(filenames, "Back");
                if (filenamesWithBack != null)
                    LoadAudio(filenamesWithBack, existingFiles);
            }

            void MenuAudioLoadAndSet()
            {
                FindAudioFiles(out string[] filenames, out string[] existingFiles);
                string[] filenamesWithBack = AddStringToOptionArray(filenames, "Back");
                if (filenamesWithBack != null)
                    SelectAudio(filenamesWithBack, existingFiles);
            }

            void MenuSave()
            { //sometimes, saving will cause "Threads" to disappear and a | too (it is the saving functions themsevlse that does it)
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                if (IFValues.SaveLocation == null || IFValues.SaveLocation == "")
                {
                    string error = "Save location does not exist, please set it in Save Location in main menu. \nEnter to return. ";
                    errorPrint(error);
                }
                else if (Storage.SignalCount == 0)
                {
                    string error = "No audio to save. Enter to return. ";
                    errorPrint(error);
                }
                else
                {
                    if (IFValues.SaveLocation == null)
                        IFValues.SaveLocation = IFValues.SaveLocation;
                    bool inMenu = true;
                    do
                    {
                        string[] saveOptions =
                        {
                            "Save Audio",
                            "Save Plot",
                            "Back"
                        };

                        PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                        DrawAndSelect(IFValues.DefaultControlString, saveOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                        switch (answer)
                        {
                            case "Save Audio":
                                MenuAudioSave();
                                break;

                            case "Save Plot":
                                MenuPlotSave();
                                break;

                            case "Back":
                                inMenu = false;
                                break;
                        }

                    } while (inMenu);
                }
                void errorPrint(string error)
                {
                    Console.WriteLine(error);
                    Console.ReadLine();
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    Console.Write(" ".PadRight(error.Length));
                }
            }
        }

        private void MenuPlotSave()
        {
            bool menuRun = true;
            string[] audioMenuSaveOptions =
            {
                "Save a Signal",
                "Save all Signals",
                "Back to Save Menu"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, audioMenuSaveOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Save a Signal":
                        SavePlotSingle();
                        break;

                    case "Save all Signals":
                        SavePlotAll();
                        break;

                    case "Back to Save Menu":
                        menuRun = false;
                        break;
                }
            } while (menuRun);

        }

        private void MenuAudioSave()
        {
            bool menuRun = true;
            string[] audioMenuSaveOptions =
            {
                "Save a Signal",
                "Save all Signals",
                "Back"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, audioMenuSaveOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Save a Signal":
                        SaveAudioSingle();
                        break;

                    case "Save all Signals":
                        SaveAudioAll();
                        break;

                    case "Back":
                        menuRun = false;
                        break;
                }
            } while (menuRun);
        }

        private void MenuPitchEstimation()
        {
            if (currentAudio != null)
            {
                bool menuRun = true;
                string[] options =
                {
                    "Auto Correlation",
                    "Comb Filtering",
                    "Back"
                };
                do
                {
                    PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                    DrawAndSelect(IFValues.DefaultControlString, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                    switch (answer)
                    {
                        case "Auto Correlation":
                            MenuPitchAuto();
                            break;

                        case "Comb Filtering":
                            MenuPitchComb();
                            break;

                        case "Back":
                            menuRun = false;
                            break;
                    }
                } while (menuRun);
            }
            else
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("No current selected audio. \nEnter to return to last menu.");
                Console.ReadLine();
            }
        }

        private void PrePitch(out uint upperBoundry, out uint lowerBoundry)
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            Console.WriteLine("Sampling rate: {0}", currentAudio.SamplingRate);
            bool done = false;
            do
            {
                Helper.NumberCollector("Please enter the upper ordinary frequency: ", out upperBoundry);
                Helper.NumberCollector("Please enter the lower ordinary frequency: ", out lowerBoundry);
                if (upperBoundry <= lowerBoundry)
                {
                    PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                    Console.WriteLine("Upper frequency is the same or below lower frequency.");
                }else if( upperBoundry >= currentAudio.SamplingRate / 2)
                {
                    PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                    Console.WriteLine("Frequency is equal or above nyquist frequency.");
                }
                else
                    done = true;
            } while (!done);
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
        }

        private bool? PostPitch(uint? pitch)
        {
            if (pitch == null)
            {
                Console.WriteLine("Signal is to short for the frequency boundries. \nTry higher minimum boundry. \nEnter to continue.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("The estimated ordinary frequency was {0}. \nEnter to continue.", pitch);
                Console.ReadLine();
                string control = "Do you want to save the cost function?";
                string[] options =
                {
                    "Yes",
                    "No"
                };
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(control, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Yes":
                        return true;

                    case "No":
                        return false;
                }
            }
            return null;
        }

        private void PitchSave(double[][] cost, string type)
        {
            string filename = String.Format("{0} {1} Pitch Cost Function.txt", currentAudio.Filename, type);
            DataWriter.FileCreator(IFValues.SaveLocation, filename);
            DataWriter.WriteToText(cost, IFValues.PitchEstimationSaveLocation + "\\" + filename);
        }

        private void MenuPitchComb()
        {
            string type = "Comb Filtering";
            PrePitch(out uint upperBoundry, out uint lowerBoundry);
            uint? value = PitchEstimation.CombFilter(currentAudio.AudioHeaderLesss, lowerBoundry, upperBoundry, currentAudio.SamplingRate, out double[][] cost);
            bool? save = PostPitch(value);
            if (save == true)
                PitchSave(cost, type);
        }

        private void MenuPitchAuto()
        {
            string type = "Auto Correlation";
            PrePitch(out uint upperBoundry, out uint lowerBoundry);
            uint? value = PitchEstimation.AutoCorrelation(currentAudio.AudioHeaderLesss, lowerBoundry, upperBoundry, currentAudio.SamplingRate, out double[][] cost);
            bool? save = PostPitch(value);
            if (save == true)
                PitchSave(cost, type);
        }

        private void MenuSyntehsis()
        {
            bool back = false;
            string[] options =
            {
                "Water Bubble",
                "Marimba",
                "Resonant Surface",
                "Back"
            };
            do
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                switch (answer)
                {
                    case "Water Bubble":
                        MenuWaterBubble();
                        break;

                    case "Marimba":
                        MenuMarimba();
                        break;

                    case "Resonant Surface":
                        MenuResonant();
                        break;

                    case "Back":
                        back = true;
                        break;
                }
            } while (!back);

            void MenuResonant()
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                double[,] audio = Synthesis.WaterHittingResonSurf(44100);
                string name = KeyUsageCheckAndAdd();
                Storage.SignalToStorage(audio, name);
            }

            void MenuMarimba()
            {
                uint samplingRate;
                uint frequency;
                bool nyquistFrequency;
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                do
                {
                    Helper.NumberCollector("Enter sampling rate: ", out samplingRate);
                    Helper.NumberCollector("Enter frequency of the marimba tone: ", out frequency);
                    nyquistFrequency = frequency >= samplingRate / 2 ? true : false;
                    if (nyquistFrequency)
                        Console.WriteLine("Frequency is equal or above Nyquist frequency.");
                } while (nyquistFrequency);
                Helper.NumberCollector("Enter pressure for the decay of the tone: ", out float pressure);
                Helper.NumberCollector("Enter posistion of the strike: ", out float posistion);
                Helper.NumberCollector("Enter velocity of the strike: ", out float velocity);
                double[,] audio = Synthesis.Marimba(posistion, velocity, pressure, frequency, samplingRate);
                string name = KeyUsageCheckAndAdd();
                Storage.SignalToStorage(audio, name);

            }

            void MenuWaterBubble()
            { //should ask for the arguments
                Debug.WriteLine("Water Bubble - start");
                Debug.WriteLine("Water Bubble " + Thread.CurrentThread.Name);
                double[,] audio = Synthesis.WaterDrops(0.002, 0.005, 44100, 1);
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                string name = KeyUsageCheckAndAdd();
                Storage.SignalToStorage(audio, name);
                Debug.WriteLine("Water Bubble - Done");
            }
        }

        private void RemoveAudio(string[] filenames)
        {
            if(Storage.SignalCount != 0)
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, filenames, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
                if (answer != filenames[filenames.Length - 1])
                {
                    Storage.RemoveSignalFromStorage(answer);
                    if(currentAudio != null)
                        if (answer == currentAudio.Filename)
                            currentAudio = null;
                }
            }
            else
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("No audio is present. Enter to return to last menu.");
                Console.ReadLine();
            }
        }

        private void LoadAudio(string[] filenames, string[] existingFiles)
        {
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            DrawAndSelect(IFValues.DefaultControlString, filenames, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
            if (answer != filenames[filenames.Length - 1])
                AudioLoading.LoadAudio(existingFiles[value]);
        }

        private void SelectAudio(string[] filenames)
        {
            if (Storage.SignalCount != 0)
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(IFValues.DefaultControlString, filenames, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint _, IFValues.OffSet[0], IFValues.OffSet[1]);
                if (answer != filenames[filenames.Length - 1])
                    currentAudio = new CurrentAudioInformation(answer);
            }
            else
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine("No audio is present. Enter to return to last menu.");
                Console.ReadLine();
            }
        }

        private void SelectAudio(string[] filenames, string[] existingFiles)
        { //slpit SelectAudio into two function, one being LoadAudio and the other should be this one, but without the load and drawAndSelect code
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            DrawAndSelect(IFValues.DefaultControlString, filenames, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
            if (answer != filenames[filenames.Length - 1])
            {
                AudioLoading.LoadAudio(existingFiles[value]);
                currentAudio = new CurrentAudioInformation(existingFiles[value]);
            }

        }

        private void FindAudioFiles(out string[] filenames)
        { //should set it up so it check if the storage contain any data and if not, catch the errors and handle them
            filenames = SaveNames();
        }

        /// <summary>
        /// Finds the files in the folder given by the IFValues.LoadLocation.
        /// If there is no files to load, it will inform the user about it.
        /// </summary>
        /// <param name="filenames">The filenames without their paths.</param>
        /// <param name="existingFiles">The filenames with their paths</param>
        private void FindAudioFiles(out string[] filenames, out string[] existingFiles)
        {
            uint i = 0;
            string pathway = IFValues.LoadLocation;
            existingFiles = Helper.DirectoryLoad(pathway);
            existingFiles = existingFiles.Length == 0 ? null : existingFiles;

            if (existingFiles != null)
            {
                string[][] seperation = new string[existingFiles.Length][];
                filenames = new string[existingFiles.Length];
                foreach (string str in existingFiles)
                {
                    seperation[i] = str.Split(@"\");
                    filenames[i] = seperation[i][seperation[i].Length - 1];
                    i++;
                }
            }
            else
            {
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                string error = "No audio present. Press any key to return to last menu. ";
                Console.WriteLine(error);
                Console.ReadLine();
                Helper.KeyFlusher();
                Console.SetCursorPosition(0, 0);
                Console.Write("".PadLeft(error.Length));
                filenames = null;
            }
        }

        private void SavePlotAll()
        {
            string[] options =
            {
                "Yes",
                "No"
            };
            string controlSave = "Are you sure?";
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            DrawAndSelect(controlSave, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
            byte naming = 0;
            if (answer == options[0])
            {
                string[] filenames = SaveNames();
                if (filenames != null)
                {
                    foreach (string str in filenames)
                    {
                        Thread saveThread = new Thread(ThreadedPlotSave);
                        saveThread.Name = naming.ToString();
                        naming++;
                        saveThread.Start(str);
                        Thread.Sleep(10);

                    }
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Saves all double[,] in the audioData's dictionary. Each file is named using their key. 
        /// Calls a new thread that runs the actually save code for each entry. 
        /// </summary>
        private void SaveAudioAll()
        {
            string[] options =
            {
                "Yes",
                "No"
            };
            string controlSave = "Are you sure?";
            PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
            DrawAndSelect(controlSave, options, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
            byte naming = 0;
            if (answer == options[0])
            {
                string[] filenames = SaveNames();
                if (filenames != null)
                {
                    foreach (string str in filenames)
                    {
                        Thread saveThread = new Thread(ThreadedAudioSave);
                        saveThread.Name = naming.ToString();
                        naming++;
                        saveThread.Start(str);
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(10);
                }
            }
        }


        private void SavePlotSingle()
        {
            string[] filenames = SaveNames();
            if (filenames != null) //sometimes some of the options will be null. AddStringToOptionArray can cause null, forgot to add to i...
            {
                string newOption = "Back";
                string[] filenamesOptions = AddStringToOptionArray(filenames, newOption);
                string saveControl = "Select file to save. ";
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(saveControl, filenamesOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
                if (answer != filenamesOptions[filenamesOptions.Length-1])
                {
                    Thread saveThread = new Thread(ThreadedPlotSave);
                    saveThread.Name = "Save Thread";
                    saveThread.Start(answer);
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Saves a double[,] in the audioData's dictionary. The file is named using its key. 
        /// Calls a new thread that runs the actually save code.
        /// </summary>
        private void SaveAudioSingle()
        {
            string[] filenames = SaveNames();
            if (filenames != null) //sometimes some of the options will be null. AddStringToOptionArray can cause null, forgot to add to i...
            {
                string newOption = "Back";
                string[] filenamesOptions = AddStringToOptionArray(filenames, newOption);
                string saveControl = "Select file to save. ";
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                DrawAndSelect(saveControl, filenamesOptions, IFValues.WrittingWidth, IFValues.AmountOfWrittenLines, IFValues.WrittenStartLocation, IFValues.OptionColours, IFValues.SelectedOption, out string answer, out uint value, IFValues.OffSet[0], IFValues.OffSet[1]);
                if (answer != filenamesOptions[filenamesOptions.Length - 1])
                {
                    Thread saveThread = new Thread(ThreadedAudioSave);
                    saveThread.Name = "Save Thread";
                    saveThread.Start(answer);
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Takes a string array <paramref name="options"/> and add string <paramref name="newOption"/> to it after the last entry.
        /// If <paramref name="newOption"/> is null, the function will return null.
        /// </summary>
        /// <param name="options">The old options in a string array.</param>
        /// <param name="newOption">The new option to be added to the string array.</param>
        /// <returns>Returns a string array with the <paramref name="newOption"/> added after the last location of <paramref name="options"/>.</returns>
        private string[] AddStringToOptionArray(string[] options, string newOption)
        {
            if (options != null && options.Length != 0)
            {
                string[] newOptionsArray = new string[options.Length + 1]; 
                int i = 0;
                foreach (string str in options)
                {
                    newOptionsArray[i] = str;
                    i++;
                }
                newOptionsArray[newOptionsArray.Length - 1] = newOption;
                return newOptionsArray;
            }
            else
                return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">The key of the entry to save.</param>
        private void ThreadedPlotSave(object obj)
        {//Bug: at times something in the saving code removes the top | of the seperator line.
            Debug.WriteLine(Thread.CurrentThread.Name);
            string workingIndicator = "Saving..."; //split this and the one below into multiple functions to recycle code
            int threadWriteY = IFValues.ThreadYLocation++;
            int[] location =
                {
                    Console.CursorLeft,
                    Console.CursorTop
                };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write(workingIndicator);
            Console.SetCursorPosition(location[0], location[1]);
            //WaveClass.WriteAudioFile((string)obj, IFValues.SaveLocation);
            double[,] dataHeader = Storage.SignalFromStorage((string)obj);
            double[,] dataPure = AudioStorageProcessing.RemoveHeader(dataHeader, out byte[] header);
            DataWriter.WriteToText(dataPure, IFValues.PlotSaveLocation + "\\" + (string)obj + ".txt", ' ');

            location = new int[]
                {
                    Console.CursorLeft,
                    Console.CursorTop
                };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write("".PadLeft(workingIndicator.Length));
            IFValues.ThreadYLocation--;
            Console.SetCursorPosition(location[0], location[1]);
        }

        /// <summary>
        /// Calls the WaveClass.WriteAudioFile.
        /// </summary>
        /// <param name="obj">The key of the entry to save.</param>
        private void ThreadedAudioSave(object obj)
        {//Bug: at times something in the saving code removes the top | of the seperator line.
            Debug.WriteLine(Thread.CurrentThread.Name);
            string workingIndicator = "Saving...";
            int threadWriteY = IFValues.ThreadYLocation++;
            int[] location =
            {
                    Console.CursorLeft,
                    Console.CursorTop
                };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write(workingIndicator);
            Console.SetCursorPosition(location[0], location[1]);
            WaveClass.WriteAudioFile((string)obj, IFValues.AudioSaveLocation);

            location = new int[]
                {
                    Console.CursorLeft,
                    Console.CursorTop
                };
            Console.SetCursorPosition(IFValues.ThreadSeperatorXLocation + (int)Math.Round((float)workingIndicator.Length / 2), threadWriteY);
            Console.Write("".PadLeft(workingIndicator.Length));
            IFValues.ThreadYLocation--;
            Console.SetCursorPosition(location[0], location[1]);
        }

        /// <summary>
        /// Get the keys, in string format, of Storage's dictionary.
        /// </summary>
        /// <returns>Returns an array of strings, each string being a key from a dictionary.</returns>
        private string[] SaveNames()
        { 
            try
            {
                return Storage.GetSignalStorageKeys(); //returns an array of zero, catch it
            }
            catch
            {
                string error = "No audio in storage. Press any key to return to last menu. ";
                PartlyClear(0, 0, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]);
                Console.WriteLine(error);
                Console.Read();
                Console.SetCursorPosition(0, 0);
                Console.Write("".PadLeft(error.Length));
                return null;
            }

        }


        /// <summary>
        /// 
        /// If there are more options that what it can display at ones, it will allow for multiple pages of options. 
        /// The <paramref name="startLocation"/> of x should be at least 3 for smooth display of the pages.
        /// If <paramref name="options"/> is empty, <paramref name="selectedOption"/> returns "No options." and <paramref name="optionSelected"/> returns the max value of uint.
        /// </summary>
        /// <param name="controlText">Explanational text, can e.g. be how to find and select an option.</param>
        /// <param name="options">The options to print out.</param>
        /// <param name="writtingWidth">How far on the x axi text can be printed too.</param>
        /// <param name="maxYLine">The max amount of lines that it can print on, is affected by <paramref name="rowOffset"/>.</param>
        /// <param name="startLocation">The start locations, x and y, of the text. The text can never go more left than what the first entry state.</param>
        /// <param name="colourOfOptions">The RGB colours of nonhovered over options.</param>
        /// <param name="colourOfHoveredOption">The RGB colours of the hovered over option.</param>
        /// <param name="selectedOption">The selected string.</param>
        /// <param name="optionSelected">The index of the selected string. </param>
        /// <param name="collOffset">The amount of collumns between each option.</param>
        /// <param name="rowOffset">The amount of rows between each option.</param>
        private void DrawAndSelect(string controlText, string[] options, uint writtingWidth, uint maxYLine, byte[] startLocation, byte[] colourOfOptions, byte[] colourOfHoveredOption, out string selectedOption, out uint optionSelected, byte collOffset = 2, byte rowOffset = 0)
        {
            byte longestestOption = 0;
            byte xLocation;
            byte yLocation;
            byte cursorTrackerX = 0;
            byte cursorTrackterY = 0;
            byte currentAmountOfColls = 0; //rename
            byte maxAmountOfRows = 0;
            List<byte> amountofRowsPerColls;
            List<byte> amountOfCollsPerRow;
            bool run = true;
            selectedOption = "";
            optionSelected = 0;

            Console.WriteLine(controlText);
            Console.CursorVisible = false;
            Console.CursorLeft = startLocation[0];
            Console.CursorTop = startLocation[1];

            if (options.Length == 0 && options == null)
            {
                Debug.WriteLine("DrawAndSelect's 'options' were passed as either null or empty.");
            }
            else { 
                foreach (string option in options)
                    if (option.Length > longestestOption)
                        longestestOption = (byte)(option.Length);
                longestestOption += collOffset;

                uint amountOfOptionsOnLine = (uint)Math.Floor((double)writtingWidth / (double)(longestestOption + collOffset));
                uint amountOfYLines = (uint)Math.Floor((double)maxYLine / (double)(rowOffset + 1));
                uint amountOfOptionsShowned = amountOfYLines * amountOfOptionsOnLine;
                uint amountOFScolls = (uint)Math.Ceiling((double)options.Length / (double)amountOfOptionsShowned) - 1;
                uint scrollLocation = 0;
                uint tempScrollLocation = 1;
                int LastScrollPosistionDownX = 0;
                int LastScrollPosistionDownY = 0;
                byte maxCursorTrackterY = 0;

                if (options.Length == 0)
                {
                    run = false;
                    selectedOption = "No options.";
                    optionSelected = uint.MaxValue;
                }

                while (run)
                {
                    currentAmountOfColls = 0;
                    if (tempScrollLocation != scrollLocation && amountOFScolls > 0)
                    {
                        int x = Console.CursorLeft;
                        int y = Console.CursorTop;
                        tempScrollLocation = scrollLocation;
                        PartlyClear(0, 1, IFValues.WrittenEndLocation[0], IFValues.WrittenEndLocation[1]); //figure out what to do with this

                        Console.SetCursorPosition(1, 2);
                        Console.Write("P");
                        Console.SetCursorPosition(1, 3);
                        Console.Write("a");
                        Console.SetCursorPosition(1, 4);
                        Console.Write("g");
                        Console.SetCursorPosition(1, 5);
                        Console.Write("e");
                        Console.SetCursorPosition(1, 7);
                        Console.Write(scrollLocation+1);
                        Console.SetCursorPosition(1, 8);
                        Console.Write("/");
                        Console.SetCursorPosition(1, 9);
                        Console.Write(amountOFScolls + 1); //can write into the option location

                        Console.SetCursorPosition(1, 12);
                        Console.Write("|");

                        if (scrollLocation != 0)
                        {
                            Console.SetCursorPosition(1, 11);
                            Console.Write("^");
                        }
                        if (scrollLocation < amountOFScolls)
                        {
                            Console.SetCursorPosition(1, 13);
                            Console.Write("v");
                        }

                        Console.CursorLeft = x;
                        Console.CursorTop = y;
                    }
                    amountOfCollsPerRow = new List<byte>();
                    amountofRowsPerColls = new List<byte>();
                    xLocation = (byte)Console.CursorLeft;
                    yLocation = (byte)Console.CursorTop;
                    byte rowNumber = 0;
                    bool direction = false;
                    byte m = 0;
                    uint index = 0 + (amountOfOptionsShowned * scrollLocation);
                    while (index < options.Length && amountOfCollsPerRow.Count + 1 <= amountOfYLines)
                    {
                        string write = options[index];
                        Console.CursorLeft = longestestOption * m + startLocation[0];
                        Console.CursorTop = startLocation[1] + rowNumber;
                        if (xLocation == Console.CursorLeft && Console.CursorTop == yLocation)
                            Console.Write("\x1b[38;2;" + colourOfHoveredOption[0] + ";" + colourOfHoveredOption[1] + ";" + colourOfHoveredOption[2] + "m" + write + "\x1b[0m");
                        else
                            Console.Write("\x1b[38;2;" + colourOfOptions[0] + ";" + colourOfOptions[1] + ";" + colourOfOptions[2] + "m" + write + "\x1b[0m");
                        m++;

                        if (longestestOption * (m + 1) + startLocation[0] >= writtingWidth) 
                        {
                            amountOfCollsPerRow.Add(m);
                            rowNumber += (byte)(1 + rowOffset);
                            currentAmountOfColls = m > currentAmountOfColls ? m : currentAmountOfColls;
                            maxAmountOfRows = maxAmountOfRows < currentAmountOfColls ? currentAmountOfColls : maxAmountOfRows;
                            m = 0;
                        }
                        index++;
                    }
                    if (rowNumber == 0 && m <= amountOfOptionsOnLine)
                    {
                        amountofRowsPerColls.Add(1);
                        for (int i = 0; i < amountofRowsPerColls[0]; i++)
                            amountOfCollsPerRow.Add(m);
                    }

                    if (currentAmountOfColls != 0)
                    {
                        if (m != 0)
                            amountOfCollsPerRow.Add(m);
                        for (int k = 0; k < currentAmountOfColls; k++)
                            amountofRowsPerColls.Add(0);
                        for (int i = 0; i < amountOfCollsPerRow.Count; i++)
                        {
                            for (int n = 0; n < amountOfCollsPerRow[i]; n++)
                            {
                                amountofRowsPerColls[n]++;
                            }
                        }
                    }

                    Console.SetCursorPosition(xLocation, yLocation);
                    do
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true); //for some reason, pressing down adds 1 to cursorLeft, just like pressing up did
                        if (keyInfo.Key == ConsoleKey.UpArrow)
                        {
                            if (cursorTrackterY > 0)
                            {
                                cursorTrackterY--;
                                Console.SetCursorPosition(xLocation, Console.CursorTop - rowOffset - 1);
                            }
                            else if (cursorTrackterY == 0 && scrollLocation > 0)
                            {
                                tempScrollLocation = scrollLocation;
                                scrollLocation--;
                                Console.SetCursorPosition(Console.CursorLeft, LastScrollPosistionDownY);
                                cursorTrackterY = maxCursorTrackterY;
                            }

                            if (Console.CursorTop == startLocation[1])
                                Console.CursorLeft = xLocation;
                            direction = true;
                        }
                        else if (keyInfo.Key == ConsoleKey.DownArrow)
                        {
                            direction = true;

                            if (cursorTrackterY == amountOfCollsPerRow.Count - 1 && (scrollLocation == amountOFScolls)) //latest addetion to fix a bug with pressing down and cursorTrackerX was equal or bigger than the amount of entires in amountofRowsPerColls
                            {

                            }
                            else if (cursorTrackterY == amountofRowsPerColls[cursorTrackerX] - 1 && scrollLocation < amountOFScolls)
                            { 
                                tempScrollLocation = scrollLocation;
                                scrollLocation++;
                                uint left = (uint)options.Length - amountOfOptionsShowned * scrollLocation;
                                while (cursorTrackerX >= left)
                                    cursorTrackerX--;
                                LastScrollPosistionDownY = Console.CursorTop;
                                LastScrollPosistionDownX = Console.CursorLeft = longestestOption * cursorTrackerX + startLocation[0];
                                Console.SetCursorPosition(LastScrollPosistionDownX, startLocation[1]);
                                maxCursorTrackterY = cursorTrackterY;
                                cursorTrackterY = 0;
                                currentAmountOfColls = 0;
                            }
                            else if (cursorTrackterY < amountofRowsPerColls[cursorTrackerX] - 1)
                            {
                                Console.SetCursorPosition(xLocation, Console.CursorTop + rowOffset + 1);
                                cursorTrackterY++;
                            }
                            else
                                Console.CursorLeft = xLocation;
                        }
                        else if (keyInfo.Key == ConsoleKey.LeftArrow)
                        {
                            if (cursorTrackerX != 0)
                                cursorTrackerX--;
                            Console.CursorLeft = longestestOption * cursorTrackerX + startLocation[0];

                            direction = true;
                        }
                        else if (keyInfo.Key == ConsoleKey.RightArrow)
                        {
                            if (cursorTrackerX < amountOfCollsPerRow[cursorTrackterY] - 1)
                                cursorTrackerX++;
                            Console.CursorLeft = longestestOption * cursorTrackerX + startLocation[0];

                            direction = true;
                        }

                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            uint totalPassed = 0;
                            for (int i = 0; i < cursorTrackterY; i++)
                                totalPassed += amountOfCollsPerRow[i];
                            totalPassed += amountOfOptionsShowned * scrollLocation;
                            optionSelected = (uint)(cursorTrackerX + totalPassed);
                            selectedOption = options[optionSelected];
                            run = false;
                            direction = true;
                        }

                    } while (!direction);
                }
            }
        }


    }
}
