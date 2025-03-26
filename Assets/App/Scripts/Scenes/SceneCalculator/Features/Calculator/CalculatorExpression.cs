
using System.Collections.Generic;
using System.Globalization;



namespace App.Scripts.Scenes.SceneCalculator.Features.Calculator
{
    public class CalculatorExpression : ICalculatorExpression
    {

        //public float Execute(string expression)
        //{
        //    return 8;
        //}

        //public void SetExpression(string expressionKey, string expression)
        //{
        //}

        //public void SetValue(string key, float value)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public float Get(string expressionKey)
        //{
        //    return 6;
        //}
        private Dictionary<string, float> constantValues = new Dictionary<string, float>();
        private Dictionary<string, string> expressions = new Dictionary<string, string>();

        public float Execute(string expression)
        {
            return Evaluate(ExpressionToRPN(expression));
        }

        public void SetExpression(string expressionKey, string expression)
        {
            expressions[expressionKey] = expression;
        }

        public void SetValue(string key, float value)
        {
            constantValues[key] = value;
        }

        public float Get(string expressionKey)
        {
            if (expressions.TryGetValue(expressionKey, out var expression))
            {
                return Execute(expression);
            }
            throw new ExceptionExecuteExpression($"Expression '{expressionKey}' not found.");
        }

        private List<string> ExpressionToRPN(string expression)
        {
            var output = new List<string>();
            var operators = new Stack<string>();
            var tokens = Tokenize(expression);

            foreach (var token in tokens)
            {
                if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    output.Add(token);
                }
                else if (constantValues.ContainsKey(token))
                {
                    output.Add(constantValues[token].ToString(CultureInfo.InvariantCulture));
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(token))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    if (operators.Count == 0)
                    {
                        throw new ExceptionExecuteExpression("Mismatched parentheses.");
                    }
                    operators.Pop(); // pop the '('
                }
                else
                {
                    throw new ExceptionExecuteExpression($"Unknown token: {token}");
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output;
        }

        private float Evaluate(List<string> rpn)
        {
            var stack = new Stack<float>();

            foreach (var token in rpn)
            {
                if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    stack.Push(number);
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2)
                    {
                        throw new ExceptionExecuteExpression("Insufficient values in expression.");
                    }
                    var right = stack.Pop();
                    var left = stack.Pop();
                    var result = ApplyOperation(token, left, right);
                    stack.Push(result);
                }
            }

            if (stack.Count != 1)
            {
                throw new ExceptionExecuteExpression("The user input has too many values.");
            }

            return stack.Pop();
        }

        private float ApplyOperation(string op, float left, float right)
        {
            return op switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                _ => throw new ExceptionExecuteExpression($"Unknown operator: {op}")
            };
        }

        private bool IsOperator(string token) => token == "+" || token == "-" || token == "*" || token == "/";

        private int Precedence(string op) => op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };

        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var currentNumber = string.Empty;

            foreach (char c in expression)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentNumber))
                    {
                        tokens.Add(currentNumber);
                        currentNumber = string.Empty;
                    }
                    tokens.Add(c.ToString());
                }
            }

            if (!string.IsNullOrEmpty(currentNumber))
            {
                tokens.Add(currentNumber);
            }

            return tokens;
        }
    }
}