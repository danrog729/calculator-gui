using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
                CorrectTokens();
                ShuntingYard();
            }
        }
        private List<Token> correctedTokenList;
        private Queue<Token> expression;
        private readonly HashSet<char> operators;
        public bool isValidExpression;
        public int numberBase;

        public FreeformCalculator()
        {
            _input = "";
            expression = new Queue<Token>();
            _tokenList = new List<Token>();
            isValidExpression = false;
            operators = new HashSet<char> { '+', '-', '*', '×', '/', '÷', '^', '(', ')', '~', '&', '|', '⊕' };
            numberBase = 10;
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
                if (operators.Contains(_input[index]))
                {
                    // if the character is an operator

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
                        case '|':
                            workingTokenList.Add(new OrToken()); break;
                        case '⊕':
                            workingTokenList.Add(new XorToken()); break;
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
                if (Char.IsDigit(_input[index]) || _input[index] == '.')
                {
                    // if the character is a decimal point or a digit
                    if (index == _input.Length - 1 || !(Char.IsDigit(_input[index + 1]) || (_input[index + 1] == '.')))
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
                            long integer = Convert.ToInt64(inputSubstring, numberBase);
                            inputFloat = (float)integer;
                        }
                        else
                        {
                            string[] splitString = inputSubstring.Split('.');
                            int integerPart = Convert.ToInt32(splitString[0], numberBase);
                            int nonIntPart = Convert.ToInt32(splitString[1], numberBase);
                            inputFloat = (float)integerPart + (float)nonIntPart / (float)Math.Pow(numberBase, splitString[1].Length);
                        }
                        workingTokenList.Add(new FloatToken(inputFloat));

                        // start the next token
                        tokenStart = index + 1;
                        hasDecimal = false;
                    }
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
                if (correctedTokenList[index] is SubtractionToken)
                {
                    if (index == 0 || correctedTokenList[index - 1] is OperatorToken & !(correctedTokenList[index - 1] is CloseBracketToken))
                    {
                        correctedTokenList.RemoveAt(index);
                        correctedTokenList.Insert(index, new MultiplicationToken());
                        correctedTokenList.Insert(index, new FloatToken(-1));
                    }
                }
                else if (correctedTokenList[index] is OpenBracketToken)
                {
                    if (index != 0)
                    {
                        if (correctedTokenList[index - 1] is CloseBracketToken || !(correctedTokenList[index - 1] is OperatorToken))
                        {
                            // insert a mult token before the open bracket
                            correctedTokenList.Insert(index++, new MultiplicationToken());
                        }
                    }
                }
                else if (correctedTokenList[index] is CloseBracketToken)
                {
                    if (index != correctedTokenList.Count - 1)
                    {
                        if (correctedTokenList[index + 1] is OpenBracketToken || !(correctedTokenList[index + 1] is OperatorToken))
                        {
                            // insert a mult token after the close bracket
                            correctedTokenList.Insert(++index, new MultiplicationToken());
                        }
                    }
                }

                index++;
            }
        }

        private void ShuntingYard()
        {
            expression = new Queue<Token>();
            Stack<Token> operatorStack = new Stack<Token>();

            foreach (Token token in correctedTokenList)
            {
                if (!(token is OperatorToken operatorToken))
                {
                    // enqueue any floats to the queue
                    expression.Enqueue(token);
                }
                else if (token is CloseBracketToken)
                {
                    // enqueue operators from the stack to the queue until an open bracket is reached
                    bool reachedOpenBracket = false;
                    while (!reachedOpenBracket)
                    {
                        Token opToken = operatorStack.Pop();
                        if (opToken is OpenBracketToken)
                        {
                            reachedOpenBracket = true;
                        }
                        else
                        {
                            expression.Enqueue(opToken);
                        }
                    }
                }
                else if (!(token is OpenBracketToken))
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
                    operatorStack.Push(token);
                }
                else
                {
                    operatorStack.Push(token);
                }
            }

            // clear the remains of the operator stack
            while (operatorStack.Count > 0)
            {
                expression.Enqueue(operatorStack.Pop());
            }

            if (expression.Count > 1)
            {
                isValidExpression = true;
            }
        }

        public double Evaluate()
        {
            Stack<FloatToken> workingStack = new Stack<FloatToken>();
            while (expression.Count > 0)
            {
                if (expression.Peek() is OperatorToken)
                {
                    OperatorToken opToken = (OperatorToken)expression.Dequeue();
                    if (workingStack.Count < 2)
                    {
                        isValidExpression = false;
                        return 0f;
                    }
                    if (opToken.argumentCount == 1)
                    {
                        double firstArg = workingStack.Pop().value;
                        workingStack.Push(new FloatToken(opToken.Operate(firstArg, 0f)));
                    }
                    else
                    {
                        double secondArg = workingStack.Pop().value;
                        double firstArg = workingStack.Pop().value;
                        workingStack.Push(new FloatToken(opToken.Operate(firstArg, secondArg)));
                    }
                }
                else
                {
                    workingStack.Push((FloatToken)expression.Dequeue());
                }
            }
            if (workingStack.Count == 0)
            {
                isValidExpression = false;
                return 0f;
            }
            return workingStack.Peek().value;
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
                else
                {
                    output += ((FloatToken)token).ToString(numberBase);
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
                else
                {
                    output += ((FloatToken)token).ToString(newBase);
                }
            }
            return output;
        }
    }

    public abstract class Token
    {

    }

    public class FloatToken : Token
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
            int integerPart = (int)value;
            output += Convert.ToString(integerPart, newBase).ToUpper();
            if ((float)integerPart != value)
            {
                output += ".";
            }
            double decimalPart = (double)value - (double)(int)(value);
            int maxIterations = 5;
            int iteration = 0;
            while (decimalPart > Single.Epsilon && iteration < maxIterations)
            {
                int bitToRemove = (int)(decimalPart / Math.Pow(newBase, -1));
                output += bitToRemove;
                decimalPart -= bitToRemove * (float)Math.Pow(newBase, -1);
                decimalPart *= newBase;
                iteration++;
            }
            return output;
        }
    }

    public abstract class OperatorToken : Token
    {
        public int precedence;
        public int argumentCount;
        public abstract double Operate(double value1, double value2);
    }

    public class AdditionToken : OperatorToken
    {
        public AdditionToken()
        {
            precedence = 4;
            argumentCount = 2;
        }
        public override double Operate(double value1, double value2)
        {
            return value1 + value2;
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
        public override double Operate(double value1, double value2)
        {
            return value1 - value2;
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
        public override double Operate(double value1, double value2)
        {
            return value1 * value2;
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
        public override double Operate(double value1, double value2)
        {
            return value1 / value2;
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
        public override double Operate(double value1, double value2)
        {
            return (float)Math.Pow(value1, value2);
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

        public override double Operate(double value1, double value2)
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

        public override double Operate(double value1, double value2)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ")";
        }
    }

    public class NotToken : OperatorToken
    {
        public NotToken()
        {
            precedence = 1;
            argumentCount = 1;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)(~((int)value1));
        }
    }

    public class AndToken : OperatorToken
    {
        public AndToken()
        {
            precedence = 8;
            argumentCount = 2;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)((int)value1 & (int)value2);
        }
    }

    public class OrToken : OperatorToken
    {
        public OrToken()
        {
            precedence = 10;
            argumentCount = 2;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)((int)value1 | (int)value2);
        }
    }

    public class XorToken : OperatorToken
    {
        public XorToken()
        {
            precedence = 9;
            argumentCount = 2;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)((int)value1 ^ (int)value2);
        }
    }

    public class ShiftLeftToken : OperatorToken
    {
        public ShiftLeftToken()
        {
            precedence = 5;
            argumentCount = 2;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)((int)value1 << (int)value2);
        }
    }

    public class ShiftRightToken : OperatorToken
    {
        public ShiftRightToken()
        {
            precedence = 5;
            argumentCount = 2;
        }

        public override double Operate(double value1, double value2)
        {
            return (float)((int)value1 >> (int)value2);
        }
    }
}
