using System;
using System.Collections.Generic;
using System.Threading;

namespace GenericLogParser
{
    class CommandLineParser : Dictionary<string, string>
    {
        public CommandLineParser(string firstParameterIfNoFlag = "", bool allowMultipleInstances = false, char multipleInstanceSeparator = '\t')
        {
            ParseCommandLine(Environment.GetCommandLineArgs(), firstParameterIfNoFlag, allowMultipleInstances, multipleInstanceSeparator);
        }

        public CommandLineParser(string[] commandLine, string firstParameterIfNoFlag = "", bool allowMultipleInstances = false, char multipleInstanceSeparator = '\t')
        {
            ParseCommandLine(commandLine, firstParameterIfNoFlag, allowMultipleInstances, multipleInstanceSeparator);
        }

        // What firstParameterIfNoFlag is for is if the first value on the command line is
        // not a flag, if this value is set to something other than "" then this
        // value will be used as the flag name.  For example:  if firstParameterIfNoFlag
        // is set to "InputFile" then if the first thing on the command line is
        // C:\blah.txt then the InputFile parameter will be set to "C:\blah.txt".
        // If firstParameterIfNoFlag = "" and the first thing on the command line is
        // C:\blah.txt, then C:\blah.txt will be used as both the key and the value.
        // If this first value on the command line is a flag (/something) then
        // _firstParameterIfNoFlag will not be used.

        // Note: Generally you will get the commandLine argument from Environment.GetCommandLineArgs()
        public void ParseCommandLine(string[] commandLine, string firstParameterIfNoFlag = "", bool allowMultipleInstances = false, char multipleInstanceSeparator = '\t')
        {
            bool lastWasFlag = false;
            string lastFlag = "";

            foreach (string item in commandLine)
            {
                if (IsFlag(item))
                {
                    if (lastWasFlag)
                        Add(lastFlag, "1");

                    lastWasFlag = true;
                    lastFlag = item.Substring(1);
                }
                else
                {
                    if (lastWasFlag)
                        AddItem(lastFlag, item, allowMultipleInstances, multipleInstanceSeparator);
                    else if (Count == 0 && !string.IsNullOrEmpty(firstParameterIfNoFlag))
                        AddItem(firstParameterIfNoFlag, item, allowMultipleInstances, multipleInstanceSeparator);
                    else
                        AddItem(item, item, allowMultipleInstances, multipleInstanceSeparator);

                    lastWasFlag = false;
                }
            }

            // If we got here and the last thing was a flag we need to add it.
            if (lastWasFlag)
                Add(lastFlag, "1");
        }

        private void AddItem(string key, string value, bool allowMultipleInstances, char multipleInstanceSeparator)
        {
            // If we already have the item in the list what we do with it depends on
            // whether allowMultipleInstances is true or not.  If it is true, then we
            // will add the new value to the already existing element separated by
            // the multiple instance separator.  Otherwise the new value will replace
            // the old value.
            if (ContainsKey(key))
            {
                this[key] = allowMultipleInstances ? this[key] + multipleInstanceSeparator + value : value;
            }
            else
            {
                Add(key, value);
            }
        }

        private bool IsFlag(string item)
        {
            string firstChar = item.Substring(0, 1);
            return (firstChar == "/" || firstChar == "-");
        }
    }
}
