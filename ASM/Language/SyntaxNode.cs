using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSExp.ASM.Language
{
    public class SyntaxNode
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NodeType Type { get; set; }
        public string Label { get; set; }
        public List<SyntaxNode> Children { get; set; } = new List<SyntaxNode>();

        private object inner_value = null;
        public object Value
        {
            get => inner_value;
            set
            {
                if(Type == NodeType.Register)
                {
                    inner_value = Enum.Parse(typeof(Register), value.ToString());
                }
                else if (Type == NodeType.Operation)
                {
                    inner_value = Enum.Parse(typeof(Ops), value.ToString());
                }
                else if (Type == NodeType.MemorySeek)
                {
                    if (value.GetType() == typeof(MemorySeek))
                    {
                        inner_value = value;
                    }
                    else
                    {
                        var jobject = ((JObject)(value));
                        var reg = (Register)Enum.Parse(typeof(Register), jobject["Register"].Value<string>());
                        var off = jobject["Offset"].Value<int>();
                        var add = jobject["Addition"].Value<int>();
                        inner_value = new MemorySeek(reg, off, add);
                    }
                    
                }
                else if (Type == NodeType.Number)
                {
                    inner_value = int.Parse(value.ToString());
                }
                else if (Type == NodeType.String || Type == NodeType.Label)
                {
                    inner_value = value?.ToString();
                }
            }
        }

        public override string ToString()
        {
            if (Type == NodeType.Label && string.IsNullOrEmpty(Label))
            {
                return $"{Value}";
            }
            else if (Type == NodeType.Label && string.IsNullOrEmpty((string)Value))
            {
                return $"{Label}:";
            }
            else if (Type == NodeType.Operation)
            {
                var val = string.Join(", ", Children.Select(t => t.ToString()));
                var hasLabel = string.IsNullOrEmpty(Label) ? "" : $"{Label}: ";
                return $"{hasLabel}{((Ops)Value).ToString()} {val}";
            }
            else if (Type == NodeType.String)
            {
                return $"'{Value.ToString()}'";
            }
            else
            {
                return Value.ToString();
            }
        }
    }
}
