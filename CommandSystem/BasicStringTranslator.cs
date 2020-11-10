using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;

namespace Simulation.CommandSystem
{
    public class BasicStringTranslator : ICommandTranslator
    {
        public static readonly Dictionary<Type, Translator> translatorDict = new Dictionary<Type, Translator> { { typeof(string), StringToCommandArgs } };

        public delegate List<string> Translator(object input);

        public static List<string> StringToCommandArgs(object input)
        {
            return Regex.Matches((string)input, @"[A-Za-z0-9.]+").Select(match => match.Value.ToLower()).ToList();
        }

        public List<string> Translate(object input)
        {
            if (input.GetType() != typeof(string)) return null;
            else return StringToCommandArgs((string)input);
        }
    }
}
