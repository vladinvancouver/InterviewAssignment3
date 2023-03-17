using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common
{
    public static class BugRules
    {
        public static string RemoveUnicodeCharacters(string text)
        {
            //Source: https://stackoverflow.com/questions/123336/how-can-you-strip-non-ascii-characters-from-a-string-in-c

            string ascii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                        ),
                    Encoding.UTF8.GetBytes(text)
                )
            );

            return ascii;
        }
    }
}
