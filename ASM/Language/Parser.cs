using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OSExp.ASM.Language
{
    public class Parser
    {
        public static List<SyntaxNode> Parse(string code)
        {
            var lines = code.Replace("\t", string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<SyntaxNode>();
            foreach (var line in lines)
            {
                var te = line.Trim();
                if (te.Contains("//"))
                {
                    te = te.Substring(0, te.IndexOf("//"));
                }
                if (!string.IsNullOrEmpty(te))
                {
                    list.Add(ParseSingleLine(te));
                }
            }
            return list;
        }

        public static SyntaxNode ParseSingleLine(string line)
        {
            var tr = line.Trim();
            var node = new SyntaxNode();
            if (tr.EndsWith(":")) // only label
            {
                node.Type = NodeType.Label;
                node.Label = tr.Substring(0, tr.Length - 1);
                return node;
            }
            var findLab = tr.Split(':').ToList();
            if (findLab.Count() == 2) // find label
            {
                node.Label = findLab[0];
                findLab.RemoveAt(0);
            }


            if (findLab.Count() == 1)  // no label found
            {
                var findOps = findLab[0].Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var opStr = findOps[0];
                var opResult = Enum.TryParse<Ops>(opStr, true, out var opEn);
                if (opResult)
                {
                    node.Type = NodeType.Operation;
                    node.Value = opEn;
                    findOps.RemoveAt(0);
                    foreach (var nums in findOps)
                    {
                        // memory seek
                        if (nums.Contains("[") && nums.Contains("]"))
                        {
                            var memSeek = parseMemorySeek(nums);
                            var valNode = new SyntaxNode
                            {
                                Type = NodeType.MemorySeek,
                                Value = memSeek
                            };
                            node.Children.Add(valNode);
                            continue;
                        }

                        // number
                        if (int.TryParse(nums, out var numIn))
                        {
                            var numNode = new SyntaxNode()
                            {
                                Type = NodeType.Number,
                                Value = numIn
                            };
                            node.Children.Add(numNode);
                            continue;
                        }

                        if (nums.ToLower().EndsWith("h")) // hex number
                        {
                            var removeH = nums.Substring(0, nums.Length - 1);
                            if (int.TryParse(removeH, NumberStyles.HexNumber, null, out var numIn2))
                            {
                                var numNode = new SyntaxNode()
                                {
                                    Type = NodeType.Number,
                                    Value = numIn2
                                };
                                node.Children.Add(numNode);
                                continue;
                            }
                        }

                        // register 
                        if (Enum.TryParse<Register>(nums, true, out var numEn))
                        {
                            var regNode = new SyntaxNode()
                            {
                                Type = NodeType.Register,
                                Value = numEn
                            };
                            node.Children.Add(regNode);
                            continue;
                        }

                        // label
                        if ((Ops)node.Value == Ops.Loop || (Ops)node.Value == Ops.Call || (Ops)node.Value == Ops.Jmp || (Ops)node.Value == Ops.Mov || (Ops)node.Value == Ops.Je || (Ops)node.Value == Ops.Jne || (Ops)node.Value == Ops.Ja || (Ops)node.Value == Ops.Jna || (Ops)node.Value == Ops.Jb || (Ops)node.Value == Ops.Jnb)
                        {
                            var labNode = new SyntaxNode()
                            {
                                Type = NodeType.Label,
                                Value = nums
                            };
                            node.Children.Add(labNode);
                            continue;
                        }

                        // push string
                        if ((Ops)node.Value == Ops.Push && nums.Contains("'"))
                        {
                            var strNode = new SyntaxNode()
                            {
                                Type = NodeType.String,
                                Value = string.Join(" ", findOps.ToArray()).Replace("'", string.Empty)
                            };
                            node.Children.Add(strNode);
                            break;
                        }
                    }
                    return node;
                }
                else
                {
                    throw new SyntaxException($"Unexpected token '{opStr}'")
                    {
                        Line = line,
                    };
                }

            }
            else
            {
                throw new SyntaxException("Unexpected token ':'")
                {
                    Line = line,
                };
            }
            throw new SyntaxException("Parse error")
            {
                Line = line,
            };
        }

        private static MemorySeek parseMemorySeek(string nums)
        {
            var memSeek = new MemorySeek();
            if (nums.Contains("."))
            {
                var dotSp = nums.Split('.');
                memSeek.Addition = int.Parse(dotSp[1]);
            }
            var regVal = between(nums, "[", "]");
            if (regVal.Contains("+")) // has offset
            {
                var plusSp = regVal.Split('+');
                Enum.TryParse(plusSp[0], true, out memSeek.Register);
                memSeek.Offset = int.Parse(plusSp[1]);
            }
            else // no offset
            {
                Enum.TryParse(regVal, true, out memSeek.Register);
            }
            return memSeek;
        }
        internal static string between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }
    }
}
