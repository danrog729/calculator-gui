using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace calculator_gui
{
    public class FreeformCalculator
    {
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                isValidExpression = true;
                hasAnEquals = false;
                Tokenise();
                CorrectTokens();
                ShuntingYard();
            }
        }
        private List<Token> _tokenList;
        public List<Token> TokenList
        {
            get { return _tokenList; }
            set
            {
                _tokenList = value;
                isValidExpression = true;
                _input = ToString();
                CorrectTokens();
                ShuntingYard();
            }
        }
        private List<Token> correctedTokenList;
        private Queue<Token> expression;
        private readonly HashSet<char> singleOperators;
        private readonly HashSet<string> otherOperators;
        public bool isValidExpression;
        private bool hasAnEquals = false;
        public uint CurrentBase;

        public FreeformCalculator()
        {
            _input = "";
            expression = new Queue<Token>();
            _tokenList = new List<Token>();
            isValidExpression = false;
            singleOperators = new HashSet<char> { '+', '-', '*', '×', '/', '÷', '^', '(', ')', '~', '&', '∨', '⊕', '≪', '≫', '|', '⫽', '%', '='};
            otherOperators = new HashSet<string> { "log_" , "ln", "sin", "cos", "tan", "arcsin", "arccos", "arctan" };
            CurrentBase = 10;
        }

        private void Tokenise()
        {
            List<Token> workingTokenList = new List<Token>();

            _input = _input.Replace(" ", "");
            _input = _input.Replace(",", "");

            int tokenStart = 0;
            int tokenEnd;
            bool hasDecimal = false;

            for (int index = 0; index < _input.Length; index++)
            {
                foreach (string key in otherOperators)
                {
                    if (index + key.Length <= _input.Length)
                    {
                        string operatorString = _input.Substring(index, key.Length);
                        if (String.Equals(operatorString, key))
                        {
                            // found a valid operator
                            if (String.Equals(operatorString, "log_"))
                            {
                                workingTokenList.Add(new LogToken());
                            }
                            else if (String.Equals(operatorString, "ln"))
                            {
                                workingTokenList.Add(new NaturalLogToken());
                            }
                            else if (String.Equals(operatorString, "sin"))
                            {
                                workingTokenList.Add(new SinToken());
                            }
                            else if (String.Equals(operatorString, "cos"))
                            {
                                workingTokenList.Add(new CosToken());
                            }
                            else if (String.Equals(operatorString, "tan"))
                            {
                                workingTokenList.Add(new TanToken());
                            }
                            else if (String.Equals(operatorString, "arcsin"))
                            {
                                workingTokenList.Add(new ArcsinToken());
                            }
                            else if (String.Equals(operatorString, "arccos"))
                            {
                                workingTokenList.Add(new ArccosToken());
                            }
                            else if (String.Equals(operatorString, "arctan"))
                            {
                                workingTokenList.Add(new ArctanToken());
                            }
                            index += operatorString.Length - 1;
                            // start the next token
                            tokenStart = index + 1;
                        }
                    }
                }

                if (singleOperators.Contains(_input[index]))
                {
                    // if the character is a single-character operator

                    // tokenise
                    switch (_input[index])
                    {
                        case '+':
                            workingTokenList.Add(new AdditionToken()); break;
                        case '-':
                            workingTokenList.Add(new SubtractionToken()); break;
                        case '*':
                        case '×':
                            workingTokenList.Add(new MultiplicationToken()); break;
                        case '/':
                        case '÷':
                            workingTokenList.Add(new DivisionToken()); break;
                        case '^':
                            workingTokenList.Add(new ExponentiationToken()); break;
                        case '(':
                            workingTokenList.Add(new OpenBracketToken()); break;
                        case ')':
                            workingTokenList.Add(new CloseBracketToken()); break;
                        case '~':
                            workingTokenList.Add(new NotToken()); break;
                        case '&':
                            workingTokenList.Add(new AndToken()); break;
                        case '∨':
                            workingTokenList.Add(new OrToken()); break;
                        case '⊕':
                            workingTokenList.Add(new XorToken()); break;
                        case '≪':
                            workingTokenList.Add(new ShiftLeftToken()); break;
                        case '≫':
                            workingTokenList.Add(new ShiftRightToken()); break;
                        case '|':
                            workingTokenList.Add(new ModulusToken()); break;
                        case '⫽':
                            workingTokenList.Add(new IntDivisionToken()); break;
                        case '%':
                            workingTokenList.Add(new ModuloDivisionToken()); break;
                        case '=':
                            workingTokenList.Add(new EqualsToken()); break;
                        default:
                            isValidExpression = false;
                            return;
                    }

                    // start the next token
                    tokenStart = index + 1;
                }
                else if (_input[index] == '.')
                {
                    // if the character is a decimal point
                    if (hasDecimal)
                    {
                        // ERROR
                        isValidExpression = false;
                        return;
                    }
                    else
                    {
                        hasDecimal = true;
                    }
                }
                if (Char.IsDigit(_input[index]) || (IsHexDigit(_input[index]) && CurrentBase == 16) || _input[index] == '.')
                {
                    // if the character is a decimal point or a digit
                    if (index == _input.Length - 1 || !(Char.IsDigit(_input[index + 1]) || (IsHexDigit(_input[index + 1]) && CurrentBase == 16) || (_input[index + 1] == '.')))
                    {
                        // end of the number
                        // numbers cant end with a decimal point
                        if (_input[index] == '.')
                        {
                            isValidExpression = false;
                            return;
                        }
                        tokenEnd = index;

                        // tokenise
                        string inputSubstring = _input.Substring(tokenStart, tokenEnd - tokenStart + 1);
                        float inputFloat;
                        if (!hasDecimal)
                        {
                            long integer = Convert.ToInt64(inputSubstring, (int)CurrentBase);
                            inputFloat = (float)integer;
                        }
                        else
                        {
                            string[] splitString = inputSubstring.Split('.');
                            if (String.IsNullOrEmpty(splitString[0]))
                            {
                                splitString[0] = "0";
                            }
                            int integerPart = Convert.ToInt32(splitString[0].Substring(0, Math.Min(splitString[0].Length,9)), (int)CurrentBase);
                            int nonIntPart = Convert.ToInt32(splitString[1].Substring(0, Math.Min(splitString[1].Length, 9)), (int)CurrentBase);
                            inputFloat = (float)integerPart + (float)nonIntPart / (float)Math.Pow(CurrentBase, splitString[1].Length);
                        }
                        workingTokenList.Add(new FloatToken(inputFloat));

                        // start the next token
                        tokenStart = index + 1;
                        hasDecimal = false;
                    }
                }
                else if (_input[index] == 'π')
                {
                    workingTokenList.Add(new PiToken());
                    // start the next token
                    tokenStart = index + 1;
                }
                else if (_input[index] == 'e')
                {
                    workingTokenList.Add(new EToken());
                    // start the next token
                    tokenStart = index + 1;
                }
                else if (_input[index] == 'x')
                {
                    workingTokenList.Add(new IntervalToken("x", new MultiInterval()));
                    // start the next token
                    tokenStart = index + 1;
                }
                else if (_input[index] == 'y')
                {
                    workingTokenList.Add(new IntervalToken("y", new MultiInterval()));
                    // start the next token
                    tokenStart = index + 1;
                }
            }
            _tokenList = workingTokenList;
        }

        private void CorrectTokens()
        {
            correctedTokenList = new List<Token>();
            foreach (Token token in _tokenList)
            {
                correctedTokenList.Add(token);
            }
            int index = 0;
            while (index < correctedTokenList.Count)
            {
                if (correctedTokenList[index] is ModulusToken)
                {
                    correctedTokenList.RemoveAt(index);
                    correctedTokenList.Insert(index, new OpenModulusToken());

                    // find the closing modulus
                    for (int subIndex = correctedTokenList.Count - 1; subIndex > index; subIndex--)
                    {
                        if (correctedTokenList[subIndex] is ModulusToken)
                        {
                            correctedTokenList.RemoveAt(subIndex);
                            correctedTokenList.Insert(subIndex, new CloseModulusToken());
                            break;
                        }
                    }
                }
                index++;
            }

            index = 0;
            while (index < correctedTokenList.Count)
            {
                if (correctedTokenList[index] is EqualsToken)
                {
                    hasAnEquals = true;
                    correctedTokenList[index] = new SubtractionToken();
                    correctedTokenList.Insert(index + 1, new OpenBracketToken());
                    correctedTokenList.Add(new CloseBracketToken());
                }

                if (correctedTokenList[index] is SubtractionToken)
                {
                    if (index == 0 || !(correctedTokenList[index - 1] is CloseBracketToken) && correctedTokenList[index - 1] is OperatorToken)
                    {
                        correctedTokenList.RemoveAt(index);
                        correctedTokenList.Insert(index, new MultiplicationToken());
                        correctedTokenList.Insert(index, new FloatToken(-1));
                    }
                }
                else if (correctedTokenList[index] is ConstantToken || correctedTokenList[index] is VariableToken || correctedTokenList[index] is IntervalToken)
                {
                    if (index != 0)
                    {
                        if (correctedTokenList[index - 1] is CloseBracketToken || !(correctedTokenList[index - 1] is OperatorToken))
                        {
                            // insert a mult token before the open bracket
                            correctedTokenList.Insert(index++, new MultiplicationToken());
                        }
                    }
                    if (index != correctedTokenList.Count - 1)
                    {
                        if (correctedTokenList[index + 1] is OpenBracketToken || !(correctedTokenList[index + 1] is OperatorToken))
                        {
                            // insert a mult token after the close bracket
                            correctedTokenList.Insert(++index, new MultiplicationToken());
                        }
                    }
                }
                else if (correctedTokenList[index] is OpenBracketToken)
                {
                    if (index != 0)
                    {
                        bool logCase = false;
                        if (index >= 2 && correctedTokenList[index - 1] is CloseBracketToken)
                        {
                            // loop back through until you find its associated open bracket
                            int bracketCount = 1;
                            int openBracketIndex = 0;
                            for (int subIndex = index - 2; subIndex > 0; subIndex--)
                            {
                                if (correctedTokenList[subIndex] is CloseBracketToken)
                                {
                                    bracketCount++;
                                }
                                else if (correctedTokenList[subIndex] is OpenBracketToken)
                                {
                                    bracketCount--;
                                    if (bracketCount == 0)
                                    {
                                        openBracketIndex = subIndex;
                                        break;
                                    }
                                }
                            }
                            if (openBracketIndex >= 1 && correctedTokenList[openBracketIndex - 1] is LogToken)
                            {
                                logCase = true;
                            }
                        }
                        else if (index >= 2 && correctedTokenList[index - 2] is LogToken)
                        {
                            logCase = true;
                        }

                        if ((correctedTokenList[index - 1] is CloseBracketToken || !(correctedTokenList[index - 1] is OperatorToken)) && !logCase)
                        {
                            // insert a mult token before the open bracket
                            correctedTokenList.Insert(index++, new MultiplicationToken());
                        }
                    }
                }
                else if (correctedTokenList[index] is CloseBracketToken)
                {
                    bool logCase = false;
                    if (index >= 2)
                    {
                        // loop back through until you find its associated open bracket
                        int bracketCount = 1;
                        int openBracketIndex = 0;
                        for (int subIndex = index - 1; subIndex > 0; subIndex--)
                        {
                            if (correctedTokenList[subIndex] is CloseBracketToken)
                            {
                                bracketCount++;
                            }
                            else if (correctedTokenList[subIndex] is OpenBracketToken)
                            {
                                bracketCount--;
                                if (bracketCount == 0)
                                {
                                    // open bracket found
                                    openBracketIndex = subIndex;
                                    break;
                                }
                            }
                        }
                        if (openBracketIndex >= 1 && correctedTokenList[openBracketIndex - 1] is LogToken)
                        {
                            logCase = true;
                        }
                    }

                    if (index != correctedTokenList.Count - 1)
                    {
                        if ((correctedTokenList[index + 1] is OpenBracketToken || !(correctedTokenList[index + 1] is OperatorToken)) && !logCase)
                        {
                            // insert a mult token after the close bracket
                            correctedTokenList.Insert(++index, new MultiplicationToken());
                        }
                    }
                }
                else if (correctedTokenList[index] is OperatorToken && ((OperatorToken)(correctedTokenList[index])).argumentCount == 1)
                {
                    if (index != 0)
                    {
                        if (correctedTokenList[index - 1] is CloseBracketToken || !(correctedTokenList[index - 1] is OperatorToken))
                        {
                            // insert a mult token before the unary operator
                            correctedTokenList.Insert(index++, new MultiplicationToken());
                        }
                    }
                }

                index++;
            }
        }

        private void ShuntingYard()
        {
            expression = new Queue<Token>();
            Stack<OperatorToken> operatorStack = new Stack<OperatorToken>();

            foreach (Token token in correctedTokenList)
            {
                if (!(token is OperatorToken operatorToken))
                {
                    // enqueue any floats, constants or variables to the queue
                    expression.Enqueue(token);
                }
                else if (token is CloseBracketToken || token is CloseModulusToken)
                {
                    // enqueue operators from the stack to the queue until the open token is reached
                    bool reachedOpenToken = false;
                    while (!reachedOpenToken)
                    {
                        if (operatorStack.Count == 0)
                        {
                            isValidExpression = false;
                            return;
                        }
                        Token opToken = operatorStack.Pop();
                        if (token is CloseBracketToken && opToken is OpenBracketToken || token is CloseModulusToken && opToken is OpenModulusToken)
                        {
                            reachedOpenToken = true;
                        }
                        else
                        {
                            expression.Enqueue(opToken);
                        }
                    }
                    if (token is CloseModulusToken)
                    {
                        expression.Enqueue(new ModulusToken());
                    }
                }
                else if (token is OpenBracketToken || token is OpenModulusToken)
                {
                    operatorStack.Push((OperatorToken)token);
                }
                else
                {
                    // enqueue operators from the stack to the queue until the stack's precedence is equal
                    bool shouldPop = true;
                    while (operatorStack.Count > 0 && shouldPop)
                    {
                        if (((OperatorToken)(operatorStack.Peek())).precedence <= operatorToken.precedence)
                        {
                            expression.Enqueue(operatorStack.Pop());
                        }
                        else
                        {
                            shouldPop = false;
                        }
                    }
                    operatorStack.Push((OperatorToken)token);
                }
            }

            // clear the remains of the operator stack
            while (operatorStack.Count > 0)
            {
                OperatorToken token = operatorStack.Pop();
                if (token is OpenBracketToken)
                {
                    isValidExpression = false;
                    return;
                }
                expression.Enqueue(token);
            }

            if (expression.Count > 1)
            {
                isValidExpression = true;
            }
        }

        public OperandToken Evaluate()
        {
            Queue<Token> workingExpression = new Queue<Token>(expression);
            Stack<OperandToken> workingStack = new Stack<OperandToken>();
            while (workingExpression.Count > 0)
            {
                if (workingExpression.Peek() is OperatorToken)
                {
                    OperatorToken opToken = (OperatorToken)workingExpression.Dequeue();
                    if (workingStack.Count < opToken.argumentCount)
                    {
                        isValidExpression = false;
                        return new FloatToken(0f);
                    }
                    if (opToken.argumentCount == 1)
                    {
                        OperandToken firstArg = workingStack.Pop();
                        workingStack.Push(opToken.Operate(firstArg, new FloatToken(0f)));
                    }
                    else
                    {
                        OperandToken secondArg = workingStack.Pop();
                        OperandToken firstArg = workingStack.Pop();
                        workingStack.Push(opToken.Operate(firstArg, secondArg));
                    }
                }
                else if (workingExpression.Peek() is VariableToken)
                {
                    isValidExpression = false;
                    return new FloatToken(0f);
                }
                else
                {
                    workingStack.Push((OperandToken)workingExpression.Dequeue());
                }
            }
            if (workingStack.Count == 0)
            {
                isValidExpression = false;
                return new FloatToken(0f);
            }
            if (workingStack.Peek() is FloatToken)
            {
                return ((FloatToken)workingStack.Peek());
            }
            else if (workingStack.Peek() is IntervalToken)
            {
                return ((IntervalToken)workingStack.Peek());
            }
            else
            {
                return new FloatToken(0f);
            }
        }

        public bool InsideBounds((double, double) xBounds, (double,double) yBounds)
        {
            bool hasAVariable = false;
            foreach (Token token in expression)
            {
                if (token is IntervalToken)
                {
                    if (String.Equals(((IntervalToken)token).identifier, "x"))
                    {
                        hasAVariable = true;
                        ((IntervalToken)token).interval = new Interval(xBounds.Item1, xBounds.Item2);
                    }
                    else if (String.Equals(((IntervalToken)token).identifier, "y"))
                    {
                        hasAVariable = true;
                        ((IntervalToken)token).interval = new Interval(yBounds.Item1, yBounds.Item2);
                    }
                }
            }
            if (hasAVariable && hasAnEquals && isValidExpression)
            {
                OperandToken result = Evaluate();
                if (result is FloatToken)
                {
                    double value = ((FloatToken)result).value;
                    if (value >= -Double.Epsilon && value <= Double.Epsilon)
                    {
                        return true;
                    }
                }
                else if (result is IntervalToken)
                {
                    if (((IntervalToken)result).interval.Contains(0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            string output = "";
            foreach (Token token in _tokenList)
            {
                if (token is OperatorToken)
                {
                    output += token.ToString();
                }
                else if (token is ConstantToken)
                {
                    output += ((ConstantToken)token).ToString();
                }
                else if (token is VariableToken)
                {
                    output += ((VariableToken)token).identifier;
                }
                else
                {
                    output += ((FloatToken)token).ToString((int)CurrentBase);
                }
            }
            return output;
        }

        public string ToString(int newBase)
        {
            string output = "";
            foreach (Token token in _tokenList)
            {
                if (token is OperatorToken)
                {
                    output += token.ToString();
                }
                else if (token is PiToken)
                {
                    output += ((PiToken)token).ToString();
                }
                else if (token is VariableToken)
                {
                    output += ((VariableToken)token).identifier;
                }
                else
                {
                    output += ((FloatToken)token).ToString(newBase);
                }
            }
            return output;
        }

        public bool IsHexDigit(char character)
        {
            if (Char.IsDigit(character) || 
                character == 'A' || character == 'a' ||
                character == 'B' || character == 'b' ||
                character == 'C' || character == 'c' ||
                character == 'D' || character == 'd' ||
                character == 'E' || // e is reserved for euler's number
                character == 'F' || character == 'f')
            {
                return true;
            }
            return false;
        }
    }

    public abstract class Token
    {

    }


    public abstract class OperandToken : Token
    {
        public static OperandToken operator +(OperandToken a) => a;
        public static OperandToken operator -(OperandToken a)
        {
            if (a is FloatToken)
            {
                return -(FloatToken)a;
            }
            else if (a is IntervalToken)
            {
                return -(IntervalToken)a;
            }
            return a;
        }
        public static OperandToken operator ~(OperandToken a)
        {
            if (a is FloatToken)
            {
                return ~(FloatToken)a;
            }
            else if (a is IntervalToken)
            {
                return ~(IntervalToken)a;
            }
            else
            {
                return a;
            }
        }
        public static OperandToken operator ++(OperandToken a)
        {
            if (a is FloatToken)
            {
                return (FloatToken)a++;
            }
            else if (a is IntervalToken)
            {
                return (IntervalToken)a++;
            }
            else
            {
                return a;
            }
        }
        public static OperandToken operator --(OperandToken a)
        {
            if (a is FloatToken)
            {
                return (FloatToken)a--;
            }
            else if (a is IntervalToken)
            {
                return (IntervalToken)a--;
            }
            else
            {
                return a;
            }
        }

        public static OperandToken operator +(OperandToken a, OperandToken b )
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a + (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b + (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a + (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a + (IntervalToken)b;
            }
        }
        public static OperandToken operator -(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a - (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b - (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a - (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a - (IntervalToken)b;
            }
        }
        public static OperandToken operator *(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a * (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b * (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a * (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a * (IntervalToken)b;
            }
        }
        public static OperandToken operator /(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a / (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return new IntervalToken(((IntervalToken)b).identifier, 
                    (Interval)((FloatToken)a).value / ((IntervalToken)b).interval);
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a / (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a / (IntervalToken)b;
            }
        }
        public static OperandToken operator %(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a % (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return b;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return a;
            }
            else
            {
                return a;
            }
        }
        public static OperandToken operator &(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a & (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b & (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a & (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a & (IntervalToken)b;
            }
        }
        public static OperandToken operator |(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a | (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b | (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a | (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a | (IntervalToken)b;
            }
        }
        public static OperandToken operator ^(OperandToken a, OperandToken b)
        {
            if (a is FloatToken && b is FloatToken)
            {
                return (FloatToken)a ^ (FloatToken)b;
            }
            else if (a is FloatToken && b is IntervalToken)
            {
                return (IntervalToken)b ^ (FloatToken)a;
            }
            else if (a is IntervalToken && b is FloatToken)
            {
                return (IntervalToken)a ^ (FloatToken)b;
            }
            else
            {
                return (IntervalToken)a ^ (IntervalToken)b;
            }
        }
    }

    public class VariableToken : OperandToken
    {
        public string identifier;
        public VariableToken(string newIdentifier)
        {
            identifier = newIdentifier;
        }
    }

    public class IntervalToken : OperandToken
    {
        public MultiInterval interval;
        public string identifier;
        public IntervalToken(string newIdentifier, MultiInterval newInterval)
        {
            identifier = newIdentifier;
            interval = newInterval;
        }

        public static IntervalToken operator +(IntervalToken a) => a;
        public static IntervalToken operator -(IntervalToken a) => new IntervalToken(a.identifier, -a.interval);
        public static IntervalToken operator ~(IntervalToken a) => new IntervalToken(a.identifier, ~a.interval);
        public static IntervalToken operator ++(IntervalToken a) => new IntervalToken(a.identifier, a.interval + (Interval)1.0);
        public static IntervalToken operator --(IntervalToken a) => new IntervalToken(a.identifier, a.interval - (Interval)1.0);

        public static IntervalToken operator +(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval + b.interval);
        public static IntervalToken operator -(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval - b.interval);
        public static IntervalToken operator *(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval * b.interval);
        public static IntervalToken operator /(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval / b.interval);
        public static IntervalToken operator %(IntervalToken a, IntervalToken b)
        {
            MessageBox.Show("Modulo division not yet implemented for variables.");
            return new IntervalToken(a.identifier, a.interval);
        }
        public static IntervalToken operator &(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval & b.interval);
        public static IntervalToken operator |(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval | b.interval);
        public static IntervalToken operator ^(IntervalToken a, IntervalToken b) => new IntervalToken(a.identifier, a.interval ^ b.interval);

        public static implicit operator IntervalToken(FloatToken a)
        {
            return new IntervalToken("", (Interval)a.value);
        }

        public override string ToString()
        {
            return interval.ToString();
        }
    }

    public class FloatToken : OperandToken
    {
        public double value;

        public FloatToken(double newValue)
        {
            value = newValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public string ToString(int newBase)
        {
            string output = "";
            double workingValue = value;
            if (value < 0)
            {
                workingValue *= -1;
                output += "-";
            }
            int integerPart = (int)workingValue;
            output += Convert.ToString(integerPart, newBase).ToUpper();
            if ((float)integerPart != workingValue)
            {
                output += ".";
                double decimalPart = (double)workingValue - (double)(int)(workingValue);
                int maxIterations = 5;
                int iteration = 0;
                double epsilon = Math.Pow(newBase, -maxIterations);
                while (iteration < maxIterations)
                {
                    int bitToRemove = (int)(decimalPart / Math.Pow(newBase, -1));
                    output += Convert.ToString(bitToRemove, newBase).ToUpper();
                    decimalPart -= bitToRemove * (float)Math.Pow(newBase, -1);
                    if (decimalPart <= epsilon)
                    {
                        break;
                    }
                    decimalPart *= newBase;
                    iteration++;
                }
            }
            return output;
        }

        public static FloatToken operator +(FloatToken a) => a;
        public static FloatToken operator -(FloatToken a) => new FloatToken(-a.value);
        public static FloatToken operator ~(FloatToken a) => new FloatToken(~(int)a.value);
        public static FloatToken operator ++(FloatToken a)
        {
            a.value++;
            return a;
        }
        public static FloatToken operator --(FloatToken a)
        {
            a.value--;
            return a;
        }

        public static FloatToken operator +(FloatToken a, FloatToken b) => new FloatToken(a.value + b.value);
        public static FloatToken operator -(FloatToken a, FloatToken b) => new FloatToken(a.value - b.value);
        public static FloatToken operator *(FloatToken a, FloatToken b) => new FloatToken(a.value * b.value);
        public static FloatToken operator /(FloatToken a, FloatToken b) => new FloatToken(a.value / b.value);
        public static FloatToken operator %(FloatToken a, FloatToken b) => new FloatToken(a.value % b.value);
        public static FloatToken operator &(FloatToken a, FloatToken b) => new FloatToken((int)a.value & (int)b.value);
        public static FloatToken operator |(FloatToken a, FloatToken b) => new FloatToken((int)a.value | (int)b.value);
        public static FloatToken operator ^(FloatToken a, FloatToken b) => new FloatToken((int)a.value ^ (int)b.value);
    }


    public abstract class ConstantToken : FloatToken
    {
        private string symbol;
        public ConstantToken(double value, string newSymbol) : base(value)
        {
            symbol = newSymbol;
        }

        public override string ToString()
        {
            return symbol;
        }
    }

    public class PiToken : ConstantToken
    {
        public PiToken() : base(Math.PI, "π")
        {

        }
    }

    public class EToken : ConstantToken
    {
        public EToken() : base(Math.E, "e")
        {

        }
    }


    public abstract class OperatorToken : Token
    {
        public int precedence;
        public int argumentCount;
        public abstract OperandToken Operate(OperandToken input1, OperandToken input2);
    }

    public class AdditionToken : OperatorToken
    {
        public AdditionToken()
        {
            precedence = 4;
            argumentCount = 2;
        }
        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 + input2;
        }

        public override string ToString()
        {
            return " + ";
        }
    }

    public class SubtractionToken : OperatorToken
    {
        public SubtractionToken()
        {
            precedence = 4;
            argumentCount = 2;
        }
        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 - input2;
        }

        public override string ToString()
        {
            return " - ";
        }
    }

    public class MultiplicationToken : OperatorToken
    {
        public MultiplicationToken()
        {
            precedence = 3;
            argumentCount = 2;
        }
        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 * input2;
        }

        public override string ToString()
        {
            return " * ";
        }
    }

    public class DivisionToken : OperatorToken
    {
        public DivisionToken()
        {
            precedence = 3;
            argumentCount = 2;
        }
        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 / input2;
        }

        public override string ToString()
        {
            return " / ";
        }
    }

    public class ExponentiationToken : OperatorToken
    {
        public ExponentiationToken()
        {
            precedence = 2;
            argumentCount = 2;
        }
        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken && input2 is FloatToken)
            {
                return new FloatToken(Math.Pow(((FloatToken)input1).value, ((FloatToken)input2).value));
            }
            else if (input1 is FloatToken && input2 is IntervalToken)
            {
                return new IntervalToken(((IntervalToken)input2).identifier, 
                    MultiInterval.Exponentiation((Interval)((FloatToken)input1).value, ((IntervalToken)input2).interval));
            }
            else if (input1 is IntervalToken && input2 is FloatToken)
            {
                return new IntervalToken(((IntervalToken)input1).identifier,
                    MultiInterval.Exponentiation(((IntervalToken)input1).interval, (Interval)((FloatToken)input2).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier,
                    MultiInterval.Exponentiation(((IntervalToken)input1).interval, ((IntervalToken)input2).interval));
            }
        }

        public override string ToString()
        {
            return " ^ ";
        }
    }

    public class OpenBracketToken : OperatorToken
    {
        public OpenBracketToken()
        {
            precedence = 999;
            argumentCount = 0;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "(";
        }
    }

    public class CloseBracketToken : OperatorToken
    {
        public CloseBracketToken()
        {
            precedence = 999;
            argumentCount = 0;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ")";
        }
    }

    public class ModulusToken : OperatorToken
    {
        public ModulusToken()
        {
            precedence = 1;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Abs(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Absolute(((IntervalToken)input1).interval));
            }
        }

        public override string ToString()
        {
            return "|";
        }
    }

    public class OpenModulusToken : OpenBracketToken
    {
        public OpenModulusToken() : base()
        {

        }

        public override string ToString()
        {
            return "[";
        }
    }

    public class CloseModulusToken : CloseBracketToken
    {
        public CloseModulusToken() : base()
        {

        }

        public override string ToString()
        {
            return "]";
        }
    }

    public class NotToken : OperatorToken
    {
        public NotToken()
        {
            precedence = 1;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return new FloatToken((~((int)((FloatToken)input1).value)));
        }

        public override string ToString()
        {
            return " ~";
        }
    }

    public class AndToken : OperatorToken
    {
        public AndToken()
        {
            precedence = 8;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 & input2;
        }

        public override string ToString()
        {
            return " & ";
        }
    }

    public class OrToken : OperatorToken
    {
        public OrToken()
        {
            precedence = 10;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 | input2;
        }

        public override string ToString()
        {
            return " ∨ ";
        }
    }

    public class XorToken : OperatorToken
    {
        public XorToken()
        {
            precedence = 9;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return input1 ^ input2;
        }

        public override string ToString()
        {
            return " ⊕ ";
        }
    }

    public class ShiftLeftToken : OperatorToken
    {
        public ShiftLeftToken()
        {
            precedence = 5;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return new FloatToken((int)((FloatToken)input1).value << (int)((FloatToken)input2).value);
        }

        public override string ToString()
        {
            return " ≪ ";
        }
    }

    public class ShiftRightToken : OperatorToken
    {
        public ShiftRightToken()
        {
            precedence = 5;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return new FloatToken((int)((FloatToken)input1).value >> (int)((FloatToken)input2).value);
        }

        public override string ToString()
        {
            return " ≫ ";
        }
    }

    public class IntDivisionToken : OperatorToken
    {
        public IntDivisionToken()
        {
            precedence = 3;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return new FloatToken(((int)((FloatToken)input1).value / (int)((FloatToken)input2).value));
        }

        public override string ToString()
        {
            return " ⫽ ";
        }
    }

    public class ModuloDivisionToken : OperatorToken
    {
        public ModuloDivisionToken()
        {
            precedence = 3;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            return new FloatToken(((int)((FloatToken)input1).value % (int)((FloatToken)input2).value));
        }

        public override string ToString()
        {
            return " % ";
        }
    }

    public class LogToken : OperatorToken
    {
        public LogToken()
        {
            precedence = 2;
            argumentCount = 2;
        }

        public override OperandToken Operate(OperandToken newBase, OperandToken value)
        {
            if (newBase is FloatToken && value is FloatToken)
            {
                return new FloatToken(Math.Log(((FloatToken)value).value, ((FloatToken)newBase).value));
            }
            else if (newBase is FloatToken && value is IntervalToken)
            {
                return new IntervalToken(((IntervalToken)value).identifier, MultiInterval.Logarithm(((IntervalToken)value).interval, (Interval)((FloatToken)newBase).value));
            }
            else if (newBase is IntervalToken && value is FloatToken)
            {
                return new IntervalToken(((IntervalToken)newBase).identifier, MultiInterval.Logarithm((Interval)((FloatToken)value).value, ((IntervalToken)newBase).interval));
            }
            else
            {
                return new IntervalToken(((IntervalToken)value).identifier, MultiInterval.Logarithm(((IntervalToken)value).interval, ((IntervalToken)newBase).interval));
            }
        }

        public override string ToString()
        {
            return "log_";
        }
    }

    public class NaturalLogToken : OperatorToken
    {
        public NaturalLogToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Log(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Logarithm(((IntervalToken)input1).interval, (Interval)Math.E));
            }
        }

        public override string ToString()
        {
            return "ln ";
        }
    }

    public class SinToken : OperatorToken
    {
        public SinToken()
        {
            precedence = 2;
            argumentCount= 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Sin(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Sin(((IntervalToken)input1).interval));
            }
        }
    }

    public class CosToken : OperatorToken
    {
        public CosToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Cos(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Cos(((IntervalToken)input1).interval));
            }
        }
    }

    public class TanToken : OperatorToken
    {
        public TanToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Tan(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Tan(((IntervalToken)input1).interval));
            }
        }
    }

    public class ArcsinToken : OperatorToken
    {
        public ArcsinToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Asin(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Arcsin(((IntervalToken)input1).interval));
            }
        }
    }

    public class ArccosToken : OperatorToken
    {
        public ArccosToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Acos(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Arccos(((IntervalToken)input1).interval));
            }
        }
    }

    public class ArctanToken : OperatorToken
    {
        public ArctanToken()
        {
            precedence = 2;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            if (input1 is FloatToken)
            {
                return new FloatToken(Math.Atan(((FloatToken)input1).value));
            }
            else
            {
                return new IntervalToken(((IntervalToken)input1).identifier, MultiInterval.Arctan(((IntervalToken)input1).interval));
            }
        }
    }

    public class EqualsToken : OperatorToken
    {
        public EqualsToken()
        {
            precedence = 999;
            argumentCount = 1;
        }

        public override OperandToken Operate(OperandToken input1, OperandToken input2)
        {
            throw new NotImplementedException();
        }
    }
}
