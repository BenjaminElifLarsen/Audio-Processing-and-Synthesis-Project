using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Audio_Code_Testbed
{
    static class Helper
    { //make into a libery at some point with different classes. 



        /// <summary>
        /// Loads in all wav files in the storage folder.
        /// </summary>
        /// <param name="pathway">The pathway to the folder that contains the files to be loaded.</param>
        /// <returns></returns>
        static public string[] DirectoryLoad(string pathway)
        {
            DataWriter.FolderCreator(pathway);
            return Directory.GetFiles(pathway, "*.wav");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="valid"></param>
        static public void NumberCollector(string s, out uint valid)
        { //Asks the user to input a number and if they give a string that cannot be converted it will stay on asking. When a convertible string is inputed it will convert the number to float.
            string Value;
            Console.CursorVisible = true;
            Console.Write(s);
            Value = Console.ReadLine();
            Console.WriteLine();
            while (!UInt32.TryParse(Value, out uint value_))
            {
                Console.Write("Input is NAN. " + s);
                Value = Console.ReadLine();
                Console.WriteLine();
            }
            Console.CursorVisible = false;
            Value = Value.Replace('.', ','); //12.3 gives 123 while 12,3 gives 12,3
            valid = Convert.ToUInt32(Value);
        }

        /// <summary>
        /// Ask the user to enter a number. If valid, returns it. Else, it will ask to user for a new number.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        static public void NumberCollector(string s, out float value) 
        { //Asks the user to input a number and if they give a string that cannot be converted it will stay on asking. When a convertible string is inputed it will convert the number to float.
            string Value;
            Console.CursorVisible = true;
            Console.Write(s);
            Value = Console.ReadLine();
            Console.WriteLine();
            while (!Single.TryParse(Value, out float value_))
            {
                Console.Write("Input is NAN. " + s); //make a function???
                Value = Console.ReadLine();
                Console.WriteLine();
            }
            Console.CursorVisible = false;
            Value = Value.Replace('.', ','); //12.3 gives 123 while 12,3 gives 12,3
            value = Convert.ToSingle(Value);
        }

        /// <summary>
        /// Used to detect if a value is zero. If not, asks for a new value. 
        /// </summary>
        /// <param name="value">Value to check for zero value.</param>
        /// <returns>Returns the new, non-zero, value.</returns>
        static public uint ZeroNumberDetector(uint value)
        {
            uint newValue;
            while (value == 0)
            {
                Console.WriteLine("Error: A zero value has been detected.");
                NumberCollector("Write a value to overwrite the zero value: ", out newValue);
                Console.WriteLine();
                if (newValue != 0)
                    return newValue;
            }
            return value;
        }

        /// <summary>
        /// Used to detect if a value is zero. If not, asks for a new value. 
        /// </summary>
        /// <param name="value">Value to check for zero value.</param>
        /// <returns>Returns the new, non-zero, value.</returns>
        static public float ZeroNumberDetector(float value)
        { 
            while (value == 0)
            {
                Console.WriteLine("Error: A zero value has been detected.");
                NumberCollector("Write a value to overwrite the zero value: ", out uint newValue);
                Console.WriteLine();
                if (newValue != 0)
                    return newValue;
            }
            return value;
        }

        /// <summary>
        /// Flushes the key buffer without displaying. 
        /// </summary>
        static public void KeyFlusher()
        {
            //flushes the key buffer, by reading from it without displaying anything.
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }

        /// <summary>
        /// Checks if <paramref name="filename"/> is valid after Window's Conventions. 
        /// By default it will consider an empty <paramref name="filename"/> to be invalid. 
        /// To consider it valid, change <paramref name="emptyValid"/> to true.
        /// </summary>
        /// <param name="filename">File to check.</param>
        /// <param name="emptyValid">Whether empty <paramref name="filename"/>is valid or not.</param>
        /// <returns>Returns true if <paramref name="filename"/> is valid, elser false.</returns>
        static public bool FilenameValid(string filename, bool emptyValid = false)
        { //might want to report what characters and names are invalid. 
            //Maybe have an out string that contain the characters that was invalid in the filename and print it out.
            if (filename == "" && !emptyValid)
                return false;
            char[] nonValidSigns =
            {
                '<', '>', ':', '"', '/', '\\', '|', '?', '*','\0'
            };
            string[] reservedNames =
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
            };
            foreach (string str in reservedNames)
                if (str == filename.ToUpper())
                    return false;
            foreach (char chr in filename)
                foreach (char chrCompare in nonValidSigns)
                    if (chr == chrCompare)
                        return false;
            byte removeAmount = LengthToRemove(filename);
            if (removeAmount != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Checks if <paramref name="filename"/> is valid after Window's Conventions. 
        /// Checks if any end signs are invalid after <paramref name="invalidEndSigns"/>. 
        /// By default it will consider an empty <paramref name="filename"/> to be invalid. 
        /// To consider it valid, change <paramref name="emptyValid"/> to true.
        /// Note: It does not by default consider '.' and ' ' to be invalid end signs.
        /// </summary>
        /// <param name="filename">File to check.</param>
        /// <param name="emptyValid">Whether empty <paramref name="filename"/>is valid or not.</param>
        /// <param name="invalidEndSigns">Char array of invalid end signs.</param>
        /// <returns>Returns true if <paramref name="filename"/> is valid, elser false.</returns>
        static public bool FilenameValid(string filename, char[] invalidEndSigns, bool emptyValid = false)
        { //might want to report what characters and names are invalid. 
            //Maybe have an out string that contain the characters that was invalid in the filename and print it out.
            if (filename == "" && !emptyValid)
                return false;
            char[] nonValidSigns =
            {
                '<', '>', ':', '"', '/', '\\', '|', '?', '*','\0'
            };
            string[] reservedNames =
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
            };
            foreach (string str in reservedNames)
                if (str == filename.ToUpper())
                    return false;
            foreach (char chr in filename)
                foreach (char chrCompare in nonValidSigns)
                    if (chr == chrCompare)
                        return false;
            byte removeAmount = LengthToRemove(filename, invalidEndSigns);
            if (removeAmount != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Removes invalid signs from the end of <paramref name="filename"/>, if there are any. 
        /// </summary>
        /// <param name="filename">File to check and remove from.</param>
        /// <returns></returns>
        static public string FileRemoveInvalidEndSigns(string filename)
        {
            byte removeAmount = LengthToRemove(filename);
            if (removeAmount != 0)
                filename = filename.Remove(filename.Length - removeAmount);
            return filename;
        }

        /// <summary>
        /// Removes invalid signs from the end of <paramref name="filename"/>, if there are any. 
        /// The invalid end signs are given in <paramref name="invalidEndSigns"/>.
        /// </summary>
        /// <param name="filename">File to check and remove from.</param>
        /// <param name="invalidEndSigns">The invalid end signs.</param>
        /// <returns></returns>
        static public string FileRemoveInvalidEndSigns(string filename, char[] invalidEndSigns)
        {
            byte removeAmount = LengthToRemove(filename, invalidEndSigns);
            if (removeAmount != 0)
                filename = filename.Remove(filename.Length - removeAmount);
            return filename;
        }

        /// <summary>
        /// Checks if string <paramref name="file"/> ends with not-valid signs given by Windows. 
        /// Returns a value that indicate the amount of non-valid end signs.
        /// </summary>
        /// <param name="file">The string to be checked for invalid end signs.</param>
        /// <returns>The amount of non-valid end signs.</returns>
        static private byte LengthToRemove(string file)
        {
            byte amount = 0;
            char[] shouldNotBeLast =
            {
                ' ','.'
            };
            foreach (char chr in file) 
                foreach (char chrBad in shouldNotBeLast)
                    if (chrBad == chr)
                    {
                        amount++;
                        break;
                    }
                        
                    else
                        if (amount != 0)
                            amount = 0;
            return amount;
        }

        /// <summary>
        /// Checks if string <paramref name="file"/> ends with not-valid signs given by <paramref name="invalidEndSigns"/>. 
        /// Returns a value that indicate the amount of non-valid end signs.
        /// </summary>
        /// <param name="file">The string to be checked for invalid end signs.</param>
        /// <param name="invalidEndSigns">Char array of invalid end signs.</param>
        /// <returns>The amount of non-valid end signs.</returns>
        static private byte LengthToRemove(string file, char[] invalidEndSigns)
        {
            byte amount = 0;
            char[] shouldNotBeLast = invalidEndSigns;
            foreach (char chr in file)
                foreach (char chrBad in shouldNotBeLast)
                    if (chrBad == chr)
                    {
                        amount++;
                        break;
                    }
                    else
                        if (amount != 0)
                            amount = 0;
            return amount;
        }


    }
}
