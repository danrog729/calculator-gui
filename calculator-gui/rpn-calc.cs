using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private List<Token> tokenList;
        private List<Token> correctedTokenList;
        private Queue<Token> expression;
        private readonly HashSet<char> operators;
        public bool isValidExpression;

        public FreeformCalculator()
        {
            _input = "";
            expression = new Queue<Token>();
            tokenList = new List<Token>();
            isValidExpression = false;
            operators = new HashSet<char> { '+', '-', '*', '/', '^', '(', ')' };
        }

        private void Tokenise()
        {
            tokenList = new List<Token>();

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
                            tokenList.Add(new AdditionToken()); break;
                        case '-':
                            tokenList.Add(new SubtractionToken()); break;
                        case '*':
                            tokenList.Add(new MultiplicationToken()); break;
                        case '/':
                            tokenList.Add(new DivisionToken()); break;
                        case '^':
                            tokenList.Add(new ExponentiationToken()); break;
                        case '(':
                            tokenList.Add(new OpenBracketToken()); break;
                        case ')':
                            tokenList.Add(new CloseBracketToken()); break;
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
                        if (!Single.TryParse(inputSubstring, out float inputFloat))
                        {
                            isValidExpression = false;
                            return;
                        }
                        tokenList.Add(new FloatToken(inputFloat));

                        // start the next token
                        tokenStart = index + 1;
                        hasDecimal = false;
                    }
                }
            }
        }

        private void CorrectTokens()
        {
            correctedTokenList = new List<Token>();
            foreach (Token token in tokenList)
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

        public float Evaluate()
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
                    float secondArg = workingStack.Pop().value;
                    float firstArg = workingStack.Pop().value;
                    workingStack.Push(new FloatToken(opToken.Operate(firstArg, secondArg)));
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
    }

    public abstract class Token
    {

    }

    public class FloatToken : Token
    {
        public float value;

        public FloatToken(float newValue)
        {
            value = newValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public abstract class OperatorToken : Token
    {
        public int precedence;
        public abstract float Operate(float value1, float value2);
    }

    public class AdditionToken : OperatorToken
    {
        public AdditionToken()
        {
            precedence = 3;
        }
        public override float Operate(float value1, float value2)
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
            precedence = 3;
        }
        public override float Operate(float value1, float value2)
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
            precedence = 2;
        }
        public override float Operate(float value1, float value2)
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
            precedence = 2;
        }
        public override float Operate(float value1, float value2)
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
            precedence = 1;
        }
        public override float Operate(float value1, float value2)
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
        }

        public override float Operate(float value1, float value2)
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
        }

        public override float Operate(float value1, float value2)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ")";
        }
    }
}
