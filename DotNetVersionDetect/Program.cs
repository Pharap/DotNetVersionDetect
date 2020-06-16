using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;
using System.Security;

//
// MIT License
// 
// Copyright (c) 2020 Pharap (@Pharap)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

namespace DotNetVersionDetect
{
    class Program
    {
        static readonly Dictionary<int, string> versionDictionary = new Dictionary<int, string>
        {
            { 378389, ".NET Framework 4.5" },
            { 378675, ".NET Framework 4.5.1 (Windows 8.1 or Server 2012)" },
            { 378758, ".NET Framework 4.5.1" },
            { 379893, ".NET Framework 4.5.2" },
            { 393295, ".NET Framework 4.6 (Windows 10)" },
            { 393297, ".NET Framework 4.6" },
            { 394254, ".NET Framework 4.6.1 (Windows 10 November Update)" },
            { 394271, ".NET Framework 4.6.1" },
            { 394802, ".NET Framework 4.6.2 (Windows 10 Anniversary Update or Windows Server 2016)" },
            { 394806, ".NET Framework 4.6.2" },
            { 460798, ".NET Framework 4.7 (Windows 10 Creators Update)" },
            { 460805, ".NET Framework 4.7" },
            { 461308, ".NET Framework 4.7.1 (Windows 10 Fall Creators Update or Windows Server)" },
            { 461310, ".NET Framework 4.7.1" },
            { 461808, ".NET Framework 4.7.2 (Windows 10 April 2018 Update or Windows Server)" },
            { 461814, ".NET Framework 4.7.2" },
            { 528040, ".NET Framework 4.8 (Windows 10 May 2019 Update and Windows 10 November 2019)" },
            { 528209, ".NET Framework 4.8 (Windows 10 May 2020 Update)" },
            { 528049, ".NET Framework 4.8" },
        };

        static string MakeBestGuess(int version)
        {
            var pairs = versionDictionary.OrderBy(pair => pair.Key).ToArray();

            string bestGuess = null;

            for (int index = 0; index < pairs.Length; ++index)
            {
                if (version > pairs[index].Key)
                    break;

                bestGuess = pairs[index].Value;
            }

            return bestGuess ?? ("At least " + pairs[pairs.Length - 1].Value);
        }

        static RegistryKey GetFullKey()
        {
            var localMachineKey = Registry.LocalMachine;

            var softwareKey = localMachineKey.OpenSubKey("SOFTWARE");
            var microsoftKey = softwareKey.OpenSubKey("Microsoft");
            var netFrameworkKey = microsoftKey.OpenSubKey("NET Framework Setup");
            var ndpKey = netFrameworkKey.OpenSubKey("NDP");
            var v4Key = ndpKey.OpenSubKey("v4");
            var fullKey = v4Key.OpenSubKey("Full");

            return fullKey;
        }

        static void ReportVersion(int version)
        {
            string versionString;

            if (versionDictionary.TryGetValue(version, out versionString))
            {
                Console.WriteLine("Exact version identified: {0}", versionString);
            }
            else
            {
                Console.WriteLine("Could not identify exact version.");
                Console.WriteLine("Best guess is {0}", MakeBestGuess(version));
            }
        }

        static void Main(string[] args)
        {
            try
            {
                RegistryKey fullKey = null;
                try
                {
                    fullKey = GetFullKey();
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Couldn't find registry key.");
                    Console.WriteLine("Likely version is .NET Framework prior to 4.5");
                }

                var value = fullKey.GetValue("Release", null);

                if (value is int)
                {
                    ReportVersion((int)value);
                }
                else if (value is string)
                {
                    int version;

                    Console.WriteLine("Registry value is a string, attempting to interpret.");

                    if (int.TryParse((string)value, out version))
                    {
                        ReportVersion(version);
                    }
                    else
                    {
                        Console.WriteLine("Could not interpret registry value as an integer.");
                        Console.WriteLine("Cannot identify .Net framework version.");
                    }
                }
                else
                {
                    Console.WriteLine("Target registry value was not an integer.");
                    Console.WriteLine("Could not identify framework version");
                }
            }
            catch (SecurityException exception)
            {
                Console.WriteLine("Insufficient permission to read the required registry key.");
                Console.WriteLine(exception);
            }
            catch (Exception exception)
            {
                Console.WriteLine("An unexpected exception occurred:");
                Console.WriteLine(exception);
            }

            Console.ReadKey();
        }
    }
}
