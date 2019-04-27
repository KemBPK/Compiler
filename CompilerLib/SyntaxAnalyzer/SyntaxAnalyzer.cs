﻿using CompilerLib.LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompilerLib.SyntaxAnalyzer
{
    public class SyntaxAnalyzer
    {
        public Stack<char> Stack { get; set; }

        public readonly ProductionRule[,] Table;

        public readonly List<ProductionRule> ProductionRules;

        public readonly List<InputColumn> InputColumns;

        public readonly List<AlphaRow> AlphaRows;

        public SyntaxAnalyzer()
        {

            Stack = new Stack<char>();

            ProductionRules = new List<ProductionRule>();
            ProductionRules.Add(new ProductionRule { A = 'E', B = "Te" });
            
            ProductionRules.Add(new ProductionRule { A = 'e', B = "+Te" });
            ProductionRules.Add(new ProductionRule { A = 'e', B = "-Te" });
            ProductionRules.Add(new ProductionRule { A = 'e', IsEpsilon = true });
           
            ProductionRules.Add(new ProductionRule { A = 'T', B = "Ft" });

            ProductionRules.Add(new ProductionRule { A = 't', B = "*Ft" });
            ProductionRules.Add(new ProductionRule { A = 't', B = "/Ft" });    
            ProductionRules.Add(new ProductionRule { A = 't', IsEpsilon = true });
            
            ProductionRules.Add(new ProductionRule { A = 'F', B = "(E)" });
            ProductionRules.Add(new ProductionRule { A = 'F', B = "i" });

            //ProductionRules.Add(new ProductionRule { A = 'e', B = "STe" });
            //ProductionRules.Add(new ProductionRule { A = 'S', B = "+" });
            //ProductionRules.Add(new ProductionRule { A = 'S', B = "-" });

            //ProductionRules.Add(new ProductionRule { A = 't', B = "QFt" });
            //ProductionRules.Add(new ProductionRule { A = 'Q', B = "*" });
            //ProductionRules.Add(new ProductionRule { A = 'Q', B = "/" });

            //Table = new ProductionRule[7, 8]
            //{
            //    { new ProductionRule { A = 'E', B = "Te" }, null, null, null, null, new ProductionRule { A = 'E', B = "Te" }, null, null  },
            //    { null, new ProductionRule { A = 'e', B = "STe" }, new ProductionRule { A = 'e', B = "STe" }, null, null, null, new ProductionRule { IsEpsilon = true}, new ProductionRule { IsEpsilon = true}  },
            //    { null, new ProductionRule { A = 'S', B = "+" }, new ProductionRule { A = 'S', B = "-" }, null, null, null, null, null  },
            //    { new ProductionRule { A = 'T', B = "Ft" }, null, null, null, null, new ProductionRule { A = 'T', B = "Ft" }, null, null  },
            //    { null, new ProductionRule { IsEpsilon = true}, new ProductionRule { IsEpsilon = true}, new ProductionRule { A = 't', B = "QFt" }, new ProductionRule { A = 't', B = "QFt" }, null, new ProductionRule { IsEpsilon = true}, new ProductionRule { IsEpsilon = true}  },
            //    { null, null, null, new ProductionRule { A = 'Q', B = "*" }, new ProductionRule { A = 'Q', B = "/" }, null, null, null  },
            //    { new ProductionRule { A = 'F', B = "i" }, null, null, null, null, new ProductionRule { A = 'F', B = "(E)" }, null, null  }

            //};

            Table = new ProductionRule[5, 8]
            {
                { ProductionRules[0], null, null, null, null, ProductionRules[0], null, null  },
                { null, ProductionRules[1], ProductionRules[2], null, null, null, ProductionRules[3], ProductionRules[3]  },
                { ProductionRules[4], null, null, null, null, ProductionRules[4], null, null  },
                { null, ProductionRules[7], ProductionRules[7], ProductionRules[5], ProductionRules[6], null, ProductionRules[7], ProductionRules[7]  },
                { ProductionRules[9], null, null, null, null, ProductionRules[8], null, null  }

            };

          
            InputColumns = new List<InputColumn>();
            InputColumns.Add(new InputColumn { Index = 0, Input = 'i' });
            InputColumns.Add(new InputColumn { Index = 1, Input = '+' });
            InputColumns.Add(new InputColumn { Index = 2, Input = '-' });
            InputColumns.Add(new InputColumn { Index = 3, Input = '*' });
            InputColumns.Add(new InputColumn { Index = 4, Input = '/' });
            InputColumns.Add(new InputColumn { Index = 5, Input = '(' });
            InputColumns.Add(new InputColumn { Index = 6, Input = ')' });
            InputColumns.Add(new InputColumn { Index = 7, Input = '$' });

            AlphaRows = new List<AlphaRow>();
            AlphaRows.Add(new AlphaRow { Index = 0, A = 'E' });
            AlphaRows.Add(new AlphaRow { Index = 1, A = 'e' });
            AlphaRows.Add(new AlphaRow { Index = 2, A = 'T' });
            AlphaRows.Add(new AlphaRow { Index = 3, A = 't' });
            AlphaRows.Add(new AlphaRow { Index = 4, A = 'F' });
            //AlphaRows.Add(new AlphaRow { Index = 2, A = 'S' });
            //AlphaRows.Add(new AlphaRow { Index = 3, A = 'T' });
            //AlphaRows.Add(new AlphaRow { Index = 4, A = 't' });
            //AlphaRows.Add(new AlphaRow { Index = 5, A = 'Q' });
            //AlphaRows.Add(new AlphaRow { Index = 6, A = 'F' });

        }

        public bool Parse(string str)
        {
            var Lexer = new LexicalAnalyzer.LexicalAnalyzer(str);

            List<Record> Records = Lexer.GetRecords();

            Stack.Push('$');
            Stack.Push(ProductionRules[0].A);

            Records.Add(new Record { Token = Token.separator, Lexeme = "$" });

            foreach(Record r in Records)
            {
                Console.WriteLine("Token: " + r.Token + "\t" + "Lexeme: " + r.Lexeme);
                while (true)
                {
                    //Compare r.lexeme (unless it the token is an identifier) with top of stack
                    //if r == top, pop top and break
                    //else
                    //Find r.lexeme in row of Stack.Pop. Pop Top and Push the production rule backward.
                    
                    char input;
                    if (r.Token == Token.identifier)
                    {
                        input = 'i'; //special case
                    }
                    else
                    {
                        input = r.Lexeme[0];
                    }

                    bool match = Stack.Peek().Equals(input);

                    if (match)
                    {
                        Stack.Pop();
                        break;
                    }
                    else
                    {

                        if (IsTerminal(Stack.Peek()))
                        {
                            Console.WriteLine("ERROR: Expected character " + Stack.Peek());              
                            return false;
                        }
                        ProductionRule newRule = FindCell(Stack.Peek(), input);                       
                        if (newRule == null)
                        {
                            if (r.Lexeme[0].Equals('$'))
                            {
                                Console.WriteLine("ERROR: incomplete syntax");
                            }
                            else
                            {
                                Console.WriteLine("ERROR: at character " + r.Lexeme);
                            }
                            
                            return false; //error
                        }
                        
                        Stack.Pop();
                        if (!newRule.IsEpsilon)
                        {
                            Console.WriteLine("\t" + newRule.A + " -> " + newRule.B);
                            string reverseString = Reverse(newRule.B);
                            foreach (char c in reverseString)
                            {
                                Stack.Push(c);
                            }
                        }
                        else
                        {
                            Console.WriteLine("\t" + newRule.A + " ->  epsilon");
                        }
                    }
                }
            }
            return Stack.Count() == 0;
        }

        public int GetColIndex(char Input)
        {
            return InputColumns.FirstOrDefault(m => m.Input.Equals(Input)).Index;
        }

        public int GetRowIndex(char A)
        {
            return AlphaRows.FirstOrDefault(m => m.A.Equals(A)).Index;
        }

        public ProductionRule FindCell(char A, char Input)
        {
            return Table[GetRowIndex(A), GetColIndex(Input)];
        }

        public bool IsNonTerminal(char A)
        {
            return AlphaRows.Any(m => m.A.Equals(A));
        }

        public bool IsTerminal(char A)
        {
            return InputColumns.Any(m => m.Input.Equals(A));
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public void OutputProductionRules()
        {
            foreach(var rule in ProductionRules)
            {
                Console.WriteLine(rule.A + " -> " + ( rule.IsEpsilon? "epsilon" : rule.B));
            }
        }
    }

    public class InputColumn
    {
        public int Index { get; set; }

        public char Input { get; set; }
    }

    public class AlphaRow
    {
        public int Index { get; set; }

        public char A { get; set; }
    }

    public class ProductionRule
    {
        public char A { get; set; }

        public string B { get; set; }

        public bool IsEpsilon { get; set; } = false;
    }
}
