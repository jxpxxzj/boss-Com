using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSExp.Simulator
{
    public class ProcessConflictException : ApplicationException
    {
        public string Name { get; set; }
    }
}
