using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Simulation
{
    public interface ICommandTranslator
    {
        public List<string> Translate(object input);
    }
}
