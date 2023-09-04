using System;
using System.Linq;

namespace CryptoGames.Security
{
    public class Encoder
    {
        #region Protected Members

        protected const string Base32Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        //The following wheels should be manually rearrange for your own security:
        protected const string MainWheel = "NPUV5KLQ9AH7BWX6G8JYZ2RSD34CEFTM";
        protected const string InitialWheel = "QD32R7BFT9AKLG8HJYZUV5WX6NP4CESM";

        // Returns an ASCII ordered alphabet, derived from the supplied code wheel (used internally)
        protected static char[] sortedAlphabet(string wheel)
        {
            char[] alphabet = wheel.ToCharArray();
            Array.Sort<char>(alphabet);
            return alphabet;
        }

        #endregion

        #region Conceal/Reveal Methods

        // Conceal an input integer by converting to Base32 and applying the code wheels (above), to return 
        // a pseudo-random alphanumeric value. (Note: the rightmost input char is handled differently to the 
        // others to reduce predictability)
        public static string Conceal(int value)
        {
            string base32value = IntToBase32(value);
            base32value = base32value.PadLeft(5, 'A'); // all results should have at least 5 chars
            var alphabet = sortedAlphabet(MainWheel);
            int offset = Array.IndexOf(alphabet, base32value.Last());
            string wheel = MainWheel.Substring(offset) + MainWheel.Substring(0, offset);
            return Conceal(
                base32value.Substring(0, base32value.Length - 1), wheel)
                + InitialWheel[offset];
        }

        // Conceal the input string using the code wheels defined above, and return an encoded string
        // (an alternative version of "Conceal" that may be useful for encoding string values)
        public static string Conceal(string value)
        {
            return Conceal(value, MainWheel);
        }

        // A generic method that encodes all input chars using the supplied wheel (including the rightmost char)
        public static string Conceal(string value, string wheel)
        {
            var alphabet = sortedAlphabet(wheel);
            var distinctChars = wheel.Distinct().ToArray();
            if (distinctChars.Length != wheel.Length)
                throw (new ArgumentException("Error: Wheel contains duplicate characters."));
            string result = "";
            for (int i = 0; i < value.Length; i++)
            {
                int letterPosition = Array.IndexOf<char>(alphabet, value[i]);
                if (letterPosition == -1)
                    throw (new ArgumentException("Error: supplied character '"
                        + value[i] + "' does not appear in code wheel.", value));
                char encodedLetter = wheel[(letterPosition + i) % wheel.Length];
                result += encodedLetter;
            }
            return result;
        }

        // Returns an int32 value by decoding the input string, and using the code wheels above.
        // It handles the rightmost character being encoded using a different code wheel to the remaining characters.
        public static int Reveal(string encodedValue)
        {
            var input = encodedValue.ToUpper();
            var alphabet = sortedAlphabet(MainWheel);
            int decodedIndex = InitialWheel.IndexOf(input.Last());
            string wheel = MainWheel.Substring(decodedIndex) + MainWheel.Substring(0, decodedIndex);
            string base32Result =
                Reveal(input.Substring(0, input.Length - 1), wheel)
                + alphabet[decodedIndex];
            return Base32ToInt(base32Result);
        }

        // Generic method to decode an input string, using the supplied code wheel
        public static string Reveal(string input, string wheel)
        {
            var alphabet = sortedAlphabet(wheel);
            string result = "";
            int alphabetIndex;
            for (int i = 0; i < input.Length; i++)
            {
                var currentCharPos = wheel.IndexOf(input[i]);
                alphabetIndex = (currentCharPos - i) % wheel.Length;
                if (alphabetIndex < 0)
                    alphabetIndex += wheel.Length;
                result += alphabet[alphabetIndex];
            }
            return result;
        }

        // For unit testing the Reveal method, only
        public static string GetWheelFromLastChar(char lastChar)
        {
            char[] alphabet = MainWheel.ToCharArray();
            int offset = InitialWheel.IndexOf(lastChar);
            return MainWheel.Substring(offset) + MainWheel.Substring(0, offset); ;
        }

        #endregion

        #region Base32 Encoding

        public static string IntToBase32(int input)
        {
            return ConvertToBase(input, Base32Alphabet);
        }

        public static int Base32ToInt(string input)
        {
            return ConvertFromBase(input, Base32Alphabet);
        }

        public static string ConvertToBase(int input, string baseAlphabet)
        {
            string result = "";

            if (input == 0)
            {
                result += baseAlphabet[0];
            }
            else
            {
                while (input > 0)
                {
                    //Must make sure result is in the correct order 
                    result = baseAlphabet[input % baseAlphabet.Length] + result;
                    input /= baseAlphabet.Length;
                }
            }

            return result;
        }

        public static int ConvertFromBase(string input, string baseAlphabet)
        {
            var inputString = input.ToUpper();

            int result = 0;
            for (int i = 0; i < inputString.Length; i++)
            {
                result *= baseAlphabet.Length;
                var character = inputString[i];
                result += baseAlphabet.IndexOf(character);
            }
            return result;
        }

        #endregion
    }
}
