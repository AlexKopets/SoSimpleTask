using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


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
        private Dictionary<string, string> _expressions = new Dictionary<string, string>();
        private Dictionary<string, float> _values = new Dictionary<string, float>();

        public float Execute(string expression)
        {
            return Evaluate(expression);
        }

        public void SetExpression(string expressionKey, string expression)
        {
            _expressions[expressionKey] = expression;
        }

        public void SetValue(string key, float value)
        {
            _values[key] = value;
        }

        public float Get(string expressionKey)
        {
            if (_expressions.TryGetValue(expressionKey, out string expression))
            {
                return Evaluate(expression);
            }
            throw new ExceptionExecuteExpression($"Expression with key '{expressionKey}' not found.");
        }

        private float Evaluate(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ConvertToRPN(tokens);
            return CalculateRPN(rpn);
        }

        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var regex = new Regex(@"(\d+(\.\d+)?)|([a-zA-Z]+)|([\+\-\*/\(\)])"); // Regex для чисел, переменных и операторов
            var matches = regex.Matches(expression);

            foreach (Match match in matches)
            {
                tokens.Add(match.Value);
            }

            return tokens;
        }

        private List<string> ConvertToRPN(List<string> tokens)
        {
            var output = new List<string>();
            var operators = new Stack<string>();
            Dictionary<string, int> precedence = new Dictionary<string, int>
        {
            { "+", 1 },
            { "-", 1 },
            { "*", 2 },
            { "/", 2 },
            { "(", 0 }
        };

            foreach (var token in tokens)
            {
                if (float.TryParse(token, out _)) // Если токен является числом
                {
                    output.Add(token);
                }
                else if (_values.ContainsKey(token)) // Если токен является переменной
                {
                    output.Add(_values[token].ToString());
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
                    operators.Pop(); // Удаляем '('
                }
                else if (precedence.ContainsKey(token)) // Если токен это оператор
                {
                    while (operators.Count > 0 && precedence[operators.Peek()] >= precedence[token])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
                else
                {
                    throw new ExceptionExecuteExpression($"Unknown token '{token}'.");
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output;
        }

        private float CalculateRPN(List<string> rpn)
        {
            var stack = new Stack<float>();

            foreach (var token in rpn)
            {
                if (float.TryParse(token, out float value))
                {
                    stack.Push(value);
                }
                else
                {
                    if (stack.Count < 2)
                    {
                        throw new ExceptionExecuteExpression("Insufficient values in the expression.");
                    }

                    float b = stack.Pop();
                    float a = stack.Pop();

                    switch (token)
                    {
                        case "+":
                            stack.Push(a + b);
                            break;
                        case "-":
                            stack.Push(a - b);
                            break;
                        case "*":
                            stack.Push(a * b);
                            break;
                        case "/":
                            if (b == 0)
                            {
                                throw new ExceptionExecuteExpression("Division by zero.");
                            }
                            stack.Push(a / b);
                            break;
                        default:
                            throw new ExceptionExecuteExpression($"Unknown operator '{token}'.");
                    }
                }
            }

            if (stack.Count != 1)
            {
                throw new ExceptionExecuteExpression("Malformed expression.");
            }

            return stack.Pop();
        }
    }
}