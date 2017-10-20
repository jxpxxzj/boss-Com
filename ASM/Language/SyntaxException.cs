using System;

namespace OSExp.ASM.Language
{
    [Serializable]
    internal class SyntaxException : Exception
    {
        public SyntaxException(string message) : base(message)
        {
        }

        public string Line { get; set; }
    }
}