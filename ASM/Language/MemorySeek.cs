using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OSExp.ASM.Language
{
    public struct MemorySeek
    {
        public MemorySeek(Register register = Register.ax, int offset = 0, int addition = 0)
        {
            Register = register;
            Offset = offset;
            Addition = addition;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Register Register;
        public int Offset;
        public int Addition;

        public override string ToString()
        {
            var str = "[" + Register;
            if (Offset != 0)
            {
                str += $"+{Offset}";
            }
            str += "]";
            if (Addition != 0)
            {
                str += $".{Addition}";
            }
            return str;
        }
    }
}
