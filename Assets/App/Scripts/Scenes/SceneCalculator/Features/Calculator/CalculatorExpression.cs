
using System;
using System.Collections.Generic;

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


       
        private readonly Dictionary<string, string> _expressions = new(); // Сохраненные выражения
        private readonly Dictionary<string, float> _values = new(); // Константы

        public float Execute(string expression)
        {
            try
            {
                var tokens = Tokenize(ReplaceVariables(expression));
                var rpn = ConvertToRPN(tokens);
                return EvaluateRPN(rpn);
            }
            catch (Exception e)
            {
                throw new ExceptionExecuteExpression($"Ошибка при вычислении: {expression}. {e.Message}");
            }
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
            if (!_expressions.TryGetValue(expressionKey, out string expression))
                throw new ExceptionExecuteExpression($"Выражение '{expressionKey}' не найдено.");

            return Execute(expression);
        }

        private string ReplaceVariables(string expression)
        {
            foreach (var kvp in _values)
            {
                expression = expression.Replace(kvp.Key, kvp.Value.ToString(), StringComparison.Ordinal);
            }
            foreach (var kvp in _expressions)
            {
                expression = expression.Replace(kvp.Key, $"({kvp.Value})", StringComparison.Ordinal);
            }
            return expression;
        }

        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            int length = expression.Length;

            for (int i = 0; i < length; i++)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                    continue;

                if (char.IsDigit(c) || c == '.')
                {
                    int start = i;
                    while (i + 1 < length && (char.IsDigit(expression[i + 1]) || expression[i + 1] == '.'))
                        i++;
                    tokens.Add(expression.Substring(start, i - start + 1));
                }
                else if (char.IsLetter(c))
                {
                    int start = i;
                    while (i + 1 < length && char.IsLetterOrDigit(expression[i + 1]))
                        i++;
                    string varName = expression.Substring(start, i - start + 1);
                    if (!_values.ContainsKey(varName) && !_expressions.ContainsKey(varName))
                        throw new ExceptionExecuteExpression($"Неизвестная переменная '{varName}'");
                    tokens.Add(varName);
                }
                else if ("+-*/()".Contains(c))
                {
                    tokens.Add(c.ToString());
                }
                else
                {
                    throw new ExceptionExecuteExpression($"Неверный символ '{c}' в выражении.");
                }
            }
            return tokens;
        }

        private List<string> ConvertToRPN(List<string> tokens)
        {
            var output = new List<string>();
            var operators = new Stack<string>();

            Dictionary<string, int> precedence = new()
        {
            { "+", 1 }, { "-", 1 },
            { "*", 2 }, { "/", 2 }
        };

            foreach (var token in tokens)
            {
                if (float.TryParse(token, out _))
                {
                    output.Add(token);
                }
                else if (_values.ContainsKey(token))
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
                        output.Add(operators.Pop());
                    if (operators.Count == 0 || operators.Pop() != "(")
                        throw new ExceptionExecuteExpression("Несовпадающие скобки.");
                }
                else if (precedence.ContainsKey(token))
                {
                    while (operators.Count > 0 && precedence.ContainsKey(operators.Peek()) &&
                           precedence[operators.Peek()] >= precedence[token])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
                else
                {
                    throw new ExceptionExecuteExpression($"Неизвестный токен '{token}'.");
                }
            }

            while (operators.Count > 0)
            {
                string op = operators.Pop();
                if (op == "(" || op == ")")
                    throw new ExceptionExecuteExpression("Несовпадающие скобки.");
                output.Add(op);
            }

            return output;
        }

        private float EvaluateRPN(List<string> rpn)
        {
            var stack = new Stack<float>();

            foreach (var token in rpn)
            {
                if (float.TryParse(token, out float number))
                {
                    stack.Push(number);
                }
                else if ("+-*/".Contains(token))
                {
                    if (stack.Count < 2)
                        throw new ExceptionExecuteExpression("Недостаточно операндов для оператора.");

                    float b = stack.Pop();
                    float a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/":
                            if (b == 0) throw new ExceptionExecuteExpression("Деление на ноль.");
                            stack.Push(a / b);
                            break;
                    }
                }
                else
                {
                    throw new ExceptionExecuteExpression($"Неизвестный токен '{token}' при вычислении.");
                }
            }

            if (stack.Count != 1)
                throw new ExceptionExecuteExpression("Некорректная структура выражения.");

            return stack.Pop();
        }
    }
}