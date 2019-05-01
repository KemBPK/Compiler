// Brandon Kem
// CPSC 323
// MW 1:00 PM - 2:15 PM

using CompilerLib.LexicalAnalyzer;
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
            ProductionRules.Add(new ProductionRule { A = 'E', B = "TQ" });
            
            ProductionRules.Add(new ProductionRule { A = 'Q', B = "+TQ" });
            ProductionRules.Add(new ProductionRule { A = 'Q', B = "-TQ" });
            ProductionRules.Add(new ProductionRule { A = 'Q', IsEpsilon = true });
           
            ProductionRules.Add(new ProductionRule { A = 'T', B = "FR" });

            ProductionRules.Add(new ProductionRule { A = 'R', B = "*FR" });
            ProductionRules.Add(new ProductionRule { A = 'R', B = "/FR" });    
            ProductionRules.Add(new ProductionRule { A = 'R', IsEpsilon = true });
            
            ProductionRules.Add(new ProductionRule { A = 'F', B = "(E)Z" });
            ProductionRules.Add(new ProductionRule { A = 'F', B = "iZ" });

            ProductionRules.Add(new ProductionRule { A = 'Z', B = ";" });
            ProductionRules.Add(new ProductionRule { A = 'Z', IsEpsilon = true });

            ProductionRules.Add(new ProductionRule { A = 'S', B = "i=E" });

            Table = new ProductionRule[6, 9]
            {
                { ProductionRules[0], null, null, null, null, ProductionRules[0], null, null, null  },
                { null, ProductionRules[1], ProductionRules[2], null, null, null, ProductionRules[3], ProductionRules[3], null },
                { ProductionRules[4], null, null, null, null, ProductionRules[4], null, null , null },
                { null, ProductionRules[7], ProductionRules[7], ProductionRules[5], ProductionRules[6], null, ProductionRules[7], ProductionRules[7], null  },
                { ProductionRules[9], null, null, null, null, ProductionRules[8], null, null, null  },
                { null, ProductionRules[11], ProductionRules[11], ProductionRules[11], ProductionRules[11], null, ProductionRules[11], ProductionRules[11], ProductionRules[10]  }
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
            InputColumns.Add(new InputColumn { Index = 8, Input = ';' });
            InputColumns.Add(new InputColumn { Index = 9, Input = '=' });

            AlphaRows = new List<AlphaRow>();
            AlphaRows.Add(new AlphaRow { Index = 0, A = 'E' });
            AlphaRows.Add(new AlphaRow { Index = 1, A = 'Q' });
            AlphaRows.Add(new AlphaRow { Index = 2, A = 'T' });
            AlphaRows.Add(new AlphaRow { Index = 3, A = 'R' });
            AlphaRows.Add(new AlphaRow { Index = 4, A = 'F' });
            AlphaRows.Add(new AlphaRow { Index = 5, A = 'Z' });

        }

        /*
         * Parse:
         *                  Checks if string has a valid syntax
         * 
         */
        public bool Parse(string str)
        {
            var Lexer = new LexicalAnalyzer.LexicalAnalyzer(str);

            List<Record> Records = Lexer.GetRecords();

            bool semicolon = true;
            if(!Records[Records.Count() - 1].Lexeme.Equals(";", StringComparison.OrdinalIgnoreCase))
            {            
                semicolon = false;
            }

            Stack.Push('$');
            Stack.Push(ProductionRules[0].A);

            Records.Add(new Record { Token = Token.separator, Lexeme = "$" });


            for(int i=0;i<Records.Count();++i)
            //foreach(Record r in Records)
            {
                Console.WriteLine("Token: " + Records[i].Token + "\t" + "Lexeme: " + Records[i].Lexeme);
                while (true)
                {
                    //Compare r.lexeme (unless it the token is an identifier) with top of stack
                    //if r == top, pop top and break
                    //else
                    //Find r.lexeme in row of Stack.Pop. Pop Top and Push the production rule backward.
                    
                    char input;
                    if (Records[i].Token == Token.identifier)
                    {
                        input = 'i'; //special case to compare with the table symbols
                    }
                    else
                    {
                        input = Records[i].Lexeme[0];
                    }

                    bool match = Stack.Peek().Equals(input);

                    if (match)
                    {
                        Stack.Pop(); //Pops stack if top of stack equals the current character
                        break;
                    }
                    else
                    {

                        if (IsTerminal(Stack.Peek())) //At this stage of the analysis, a terminal sybmol should not be at the top of the stack
                        {
                            Console.WriteLine("ERROR: Expected character " + Stack.Peek());              
                            return false;
                        }
                        ProductionRule newRule = null;
                        if (input.Equals('i') && (i+1) < Records.Count())
                        {

                            if (Lexer.IsFirstIdentifier(i) && Records[i+1].Lexeme.Equals("=", StringComparison.OrdinalIgnoreCase))
                            {
                                newRule = ProductionRules[12]; //assigns the special production rule for the assignment operator
                            }
                            else
                            {
                                newRule = FindCell(Stack.Peek(), input);
                            }
                            
                        }
                        else
                        {
                            newRule = FindCell(Stack.Peek(), input);
                        }
                        
                        if (newRule == null)
                        {
                            if (Records[i].Lexeme[0].Equals('$'))
                            {
                                Console.WriteLine("ERROR: incomplete syntax");
                            }
                            else
                            {
                                Console.WriteLine("ERROR: at character " + Records[i].Lexeme);
                            }
                            
                            return false; //error
                        }
                        
                        Stack.Pop(); //pops stack and push in new rule (backward)
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
            if (!semicolon)
            {
                Console.WriteLine("ERROR: Expected character ;");
                return false;
            }

            return Stack.Count() == 0;
        }

        /*
         * GetColIndex:
         *                  Gets column index for the character input (for the table)
         * 
         */
        public int GetColIndex(char Input)
        {
            return InputColumns.FirstOrDefault(m => m.Input.Equals(Input)).Index;
        }

        /*
         * GetRowIndex:
         *                  Gets row index for the symbol input (for the table)
         * 
         */
        public int GetRowIndex(char A)
        {
            return AlphaRows.FirstOrDefault(m => m.A.Equals(A)).Index;
        }

        /*
         * FindCell:
         *                  returns the production rule within the table cell
         * 
         */
        public ProductionRule FindCell(char A, char Input)
        {
            return Table[GetRowIndex(A), GetColIndex(Input)];
        }

        /*
         * IsNonTerminal:
         *                  Checks if the input symbol is non-terminal
         * 
         */
        public bool IsNonTerminal(char A)
        {
            return AlphaRows.Any(m => m.A.Equals(A));
        }

        /*
         * IsTerminal:
         *                  Checks if the input symbol is terminal
         * 
         */
        public bool IsTerminal(char A)
        {
            return InputColumns.Any(m => m.Input.Equals(A));
        }

        /*
         * Reverse:
         *                  Returns the reverse of a string
         * 
         */
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /*
         * OutputProductionRules:
         *                  Outputs the production rules
         * 
         */
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
