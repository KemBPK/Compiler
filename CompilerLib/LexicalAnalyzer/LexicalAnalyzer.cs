// Brandon Kem
// CPSC 323
// MW 1:00 PM - 2:15 PM

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CompilerLib.LexicalAnalyzer
{
    /*
     * INSTRUCTIONS: 
     *                  To run the lexical analyzer, initialize a LexicalAnalyzer object and pass in
     *                  the input as a string. It will automatically run the lexer and output the tokens
     *                  and lexemes. For now, the user cannot do anything else with the object after
     *                  initialization.
     * 
     */
    public class LexicalAnalyzer
    {
        /*
         * Constructor:
         *                  Gets a string input and runs the lexer through the characters.
         *                  Outputs the records at the end (token/lexeme).
         * 
         */
        public LexicalAnalyzer(string i)
        {
            Records = new List<Record>();
            Input = i + ' ';
            FSM = new FSM();
            Run();
            Console.WriteLine("TOKENS" + "\t\t\t\t\t" + "Lexemes\n");
            foreach (var record in Records)
            {   
                Console.WriteLine(record.Token + "\t\t" + (record.Token == Token.keyword || record.Token == Token.integer ? "\t" : "") + "=\t\t" + record.Lexeme);
            }
        }

        /*
         * Run:
         *                  Reads in the input string into the Finite State Machine.
         *                  Adds a new record when the FSM hits an ending state.
         * 
         */
        private void Run()
        {
            if (Input != null)
            {
                Input = Input.Replace("\r\n", " "); // Replaces enters and newlines in the input string.
                Input = Input.Replace("\n", " ");

                string Buffer = "";
                for (int i = 0; i < Input.Length; ++i)
                {
                    var test = Input[i]; // For debugging

                    FSM.Lexer(Input[i]);
                    if (FSM.CurrentState == State.integer       || 
                        FSM.CurrentState == State.identifier    ||
                        FSM.CurrentState == State.real          || 
                        FSM.CurrentState == State.end_separator || 
                        FSM.CurrentState == State._operator     || 
                        FSM.CurrentState == State.brace         || FSM.CurrentState == State.parenthesis     || FSM.CurrentState == State.bracket   ||
                        FSM.CurrentState == State.end_brace     || FSM.CurrentState == State.end_parenthesis || FSM.CurrentState == State.end_bracket ||
                        FSM.CurrentState == State.quote         || FSM.CurrentState == State.end_quote       || FSM.CurrentState == State.comment)
                    {
                        Buffer += Input[i]; // Adds character to buffer if the FSM are in one the states specified above.
                    }
                    if (FSM.IsEndingState()) //If the FSM is in an ending state, add a record
                    {
                        if (FSM.CurrentState == State.end_identifier)
                        {
                            //Search for keywords
                            //if not, then the buffer string is an identifier
                            //add record
                            if (IsKeyWord(Buffer))
                            {
                                Records.Add(new Record { Token = Token.keyword, Lexeme = Buffer });
                            }
                            else
                            {
                                Records.Add(new Record { Token = Token.identifier, Lexeme = Buffer });
                            }
                        }
                        else if (FSM.CurrentState == State.end_integer)
                        {
                            Records.Add(new Record { Token = Token.integer, Lexeme = Buffer });
                        }
                        else if (FSM.CurrentState == State.end_real)
                        {
                            Records.Add(new Record { Token = Token.realnumber, Lexeme = Buffer });
                        }
                        else if (FSM.CurrentState == State.end_separator)
                        {
                            Records.Add(new Record { Token = Token.separator, Lexeme = Buffer });
                        }
                        else if (FSM.CurrentState == State._operator)
                        {
                            Records.Add(new Record { Token = Token._operator, Lexeme = Buffer });
                        }
                        else if (FSM.CurrentState == State.end_brace)
                        {
                            Records.Add(new Record { Token = Token.separator, Lexeme = "{" });
                            //call run again but without brackets
                            if (Buffer.Length > 2)
                            {
                                RunInsideSeparators(Buffer);
                            }
                            Records.Add(new Record { Token = Token.separator, Lexeme = "}" });
                        }
                        else if (FSM.CurrentState == State.end_parenthesis)
                        {
                            Records.Add(new Record { Token = Token.separator, Lexeme = "(" });
                            if (Buffer.Length > 2)
                            {
                                RunInsideSeparators(Buffer);
                            }
                            Records.Add(new Record { Token = Token.separator, Lexeme = ")" });
                        }
                        else if (FSM.CurrentState == State.end_bracket)
                        {
                            Records.Add(new Record { Token = Token.separator, Lexeme = "[" });
                            if (Buffer.Length > 2)
                            {
                                RunInsideSeparators(Buffer);
                            }
                            Records.Add(new Record { Token = Token.separator, Lexeme = "]" });
                        }
                        else if (FSM.CurrentState  == State.end_quote)
                        {
                            Records.Add(new Record { Token = Token.quotation, Lexeme = Buffer });
                        }

                        Buffer = ""; //resets buffer

                        if (FSM.CurrentState == State.end_identifier || FSM.CurrentState == State.end_integer || FSM.CurrentState == State.end_real) //back up one character in the input
                        {
                            --i;
                        }
                        //FSM.Lexer(Input[i]); // loads start state
                        //or
                        FSM.FormerState = FSM.CurrentState;
                        FSM.CurrentState = State.start;
                        //or
                        //FSM.Lexer(' ');
                        

                    }
                }

                //If there are no closing comment bracket, run the lexer again but ignore the initial comment bracket
                if (FSM.CurrentState == State.comment)
                { 
                    Records.Add(new Record { Token = Token.separator, Lexeme = "!" });
                    Buffer = Buffer.Substring(1, Buffer.Length - 1) + ' ';
                    string temp = Input;
                    Input = Buffer;
                    FSM.CurrentState = State.start;
                    FSM.FormerState = State.end_separator;
                    Run();
                    Input = temp;
                }
            }

        }

        private int GetClosingCommentIndex(int openingCommentIndex)
        {
            int ClosingCommentIndex = Input.IndexOf('!', openingCommentIndex + 1);
            //returns -1 if not found
            if (ClosingCommentIndex == -1) return openingCommentIndex;
            else return ClosingCommentIndex;
        }

        /*
         * RunInsideSeparators:
         *                  Reads in the input string minus the first and last character 
         *                  into the Finite State Machine. Adds a new record when the FSM 
         *                  hits an ending state.
         * 
         */
        private void RunInsideSeparators(string Buffer)
        {
            string tempInput = Input;
            State tempCurrentState = FSM.CurrentState;
            State tempFormerState = FSM.FormerState;
            string substring = Buffer.Substring(1, Buffer.Length - 2) + ' ';
            Input = substring;
            FSM.CurrentState = State.start;
            FSM.FormerState = State.start;
            Run();
            Input = tempInput;
            FSM.CurrentState = tempCurrentState;
            FSM.FormerState = tempFormerState;
        }

        /*
         * IsKeyWord:
         *                  Checks if string is a keyword.
         * 
         */
        private static bool IsKeyWord(string s)
        {
            //KEYWORDS 	=	int, float, bool, if, else, then, do, while, whileend, do, doend, for, and, or, function
            return s.Equals("int") || s.Equals("float") || s.Equals("bool") || s.Equals("if") || s.Equals("else") || s.Equals("then") ||
                   s.Equals("do") || s.Equals("while") || s.Equals("whileend") || s.Equals("doend") || s.Equals("for") || s.Equals("and") ||
                   s.Equals("or") || s.Equals("function");
        }

        private string Input { get; set; }

        private readonly List<Record> Records;

        private FSM FSM;
    }

    public enum Token { keyword, identifier, integer, realnumber, _operator, separator, whitespace, blankline, quotation }

    public class Record
    {
        public Record() { }

        public Token Token { get; set; }

        public string Lexeme { get; set; } 
    }

    public enum State
    {
        start = 1,
        identifier,
        end_identifier,
        integer,
        end_integer,
        dot,
        end_dot,
        real,
        end_real,
        brace,
        end_brace,
        parenthesis,
        end_parenthesis,
        bracket,
        end_bracket,
        end_separator,
        _operator,
        quote,
        end_quote,
        comment,
        end_comment
    }

    public class Attribute
    {
        public char Character { get; set; }
        public int  Index { get; set; }
    }

    public class FSM
    {
        public readonly int[,] Table;

        public readonly List<int> EndStates;

        public readonly List<Attribute> Columns;

        public State CurrentState { get; set; }

        public State FormerState { get; set; }

        /*
         * Constructor:
         *                  Sets up the transition table and other member variables.
         * 
         */
        public FSM()
        {
            CurrentState = State.start;
            FormerState = State.start;
            Table = new int[21, 25]
            {
                { 2 , 4 , 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 1 , 16, 17, 17, 17, 17, 17, 17, 17, 17, 18, 1 , 20 },
                { 2 , 2 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 2 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3 , 3  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 5 , 4 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 8 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5 , 5  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 7 , 8 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7 , 7  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 9 , 8 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9 , 9  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 10, 10, 10, 11, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 12, 12, 12, 12, 12, 13, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12 },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 14, 14, 14, 14, 14, 14, 14, 15, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 13 },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 19, 18, 18 },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  },
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 21 },
                { 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1  }
            };
            EndStates = new List<int>();
            EndStates.Add(3);
            EndStates.Add(5);
            EndStates.Add(7);
            EndStates.Add(9);
            EndStates.Add(11);
            EndStates.Add(13);
            EndStates.Add(15);
            EndStates.Add(16);
            EndStates.Add(17);
            EndStates.Add(19);
            EndStates.Add(21);

            Columns = new List<Attribute>();
            Columns.Add(new Attribute { Index = 0, Character = 'l' });
            Columns.Add(new Attribute { Index = 1, Character = 'd' });
            Columns.Add(new Attribute { Index = 2, Character = '{' });
            Columns.Add(new Attribute { Index = 3, Character = '}' });
            Columns.Add(new Attribute { Index = 4, Character = '(' });
            Columns.Add(new Attribute { Index = 5, Character = ')' });
            Columns.Add(new Attribute { Index = 6, Character = '[' });
            Columns.Add(new Attribute { Index = 7, Character = ']' });
            Columns.Add(new Attribute { Index = 8, Character = ',' });
            Columns.Add(new Attribute { Index = 9, Character = '.' });
            Columns.Add(new Attribute { Index = 10, Character = ':' });
            Columns.Add(new Attribute { Index = 11, Character = ';' });
            Columns.Add(new Attribute { Index = 12, Character = ' ' });
            Columns.Add(new Attribute { Index = 13, Character = '$' });
            Columns.Add(new Attribute { Index = 14, Character = '*' });
            Columns.Add(new Attribute { Index = 15, Character = '+' });
            Columns.Add(new Attribute { Index = 16, Character = '-' });
            Columns.Add(new Attribute { Index = 17, Character = '=' });
            Columns.Add(new Attribute { Index = 18, Character = '/' });
            Columns.Add(new Attribute { Index = 19, Character = '>' });
            Columns.Add(new Attribute { Index = 20, Character = '<' });
            Columns.Add(new Attribute { Index = 21, Character = '%' });
            Columns.Add(new Attribute { Index = 22, Character = '\'' });
            Columns.Add(new Attribute { Index = 23, Character = '\t' });
            Columns.Add(new Attribute { Index = 24, Character = '!' });    

        }

        /*
         * Lexer:
         *                  Reads a character into the FSM. The transition table
         *                  will determines the FSM's next state.
         * 
         */
        public void Lexer(char i)
        {
            if (char.IsLetter(i))
            {
                var stateInteger = Table[((int)CurrentState) - 1, 0];
                FormerState = CurrentState;
                CurrentState = (State)stateInteger;
            }
            else if (char.IsDigit(i))
            {
                var stateInteger = Table[((int)CurrentState) - 1, 1];
                FormerState = CurrentState;
                CurrentState = (State)stateInteger;
            }
            else
            {
                var attribute = Columns.FirstOrDefault(m => m.Character.Equals(i));
                var stateInteger = Table[((int)CurrentState) - 1, attribute.Index];
                FormerState = CurrentState;
                CurrentState = (State)stateInteger;
            }
        }

        /*
         * IsEndingState:
         *                  Checks if the FSM's current state is an accepting state.
         * 
         */
        public bool IsEndingState()
        {
            return EndStates.Any(m => m.Equals((int)CurrentState));
        }
       
    }

}
