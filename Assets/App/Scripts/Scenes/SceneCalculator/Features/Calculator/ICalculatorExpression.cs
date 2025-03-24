using System;
using System.Collections.Generic;
using System.Text;

namespace App.Scripts.Scenes.SceneCalculator.Features.Calculator
{


    public interface ICalculatorExpression
    {
        /// <summary>
        /// выполняет выражение, если в нем есть переменные пробует их подставить
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        float Execute(string expression);

        /// <summary>
        /// устанавливает переменную и выражение для нее внутри калькулятора, при обращении по этому ключу
        /// будет вычисляться это выражение
        /// </summary>
        /// <param name="expressionKey"></param>
        /// <param name="expression"></param>
        void SetExpression(string expressionKey, string expression);


        /// <summary>
        /// устанавливает для заданного ключа значение
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, float value);

        /// <summary>
        /// запрашиваем выражение по ключу и выполняем его
        /// </summary>
        /// <param name="expressionKey"></param>
        /// <returns></returns>
        float Get(string expressionKey);
    }
    public class Calculator : ICalculatorExpression
    {
        private Dictionary<string, string> expressions = new Dictionary<string, string>();
        private Dictionary<string, float> values = new Dictionary<string, float>();

        public float Execute(string expression)
        {
            try
            {
                return Evaluate(expression);
            }
            catch (Exception ex)
            {
                throw new ExceptionExecuteExpression("Error executing expression: " + ex.Message);
            }
        }

        public void SetExpression(string expressionKey, string expression)
        {
            expressions[expressionKey] = expression;
        }

        public void SetValue(string key, float value)
        {
            values[key] = value;
        }

        public float Get(string expressionKey)
        {
            if (!expressions.ContainsKey(expressionKey))
            {
                throw new ExceptionExecuteExpression($"Expression with key '{expressionKey}' not found.");
            }

            try
            {
                return Evaluate(expressions[expressionKey]);
            }
            catch (Exception ex)
            {
                throw new ExceptionExecuteExpression($"Error executing expression '{expressionKey}': " + ex.Message);
            }
        }

        private float Evaluate(string expression)
        {
            return ParseExpression(new StringPtr(expression));
        }

        private float ParseExpression(StringPtr ptr)
        {
            float left = ParseTerm(ptr);

            while (!ptr.IsEnd && (ptr.Current == '+' || ptr.Current == '-'))
            {
                char op = ptr.Current;
                ptr.MoveNext();
                float right = ParseTerm(ptr);

                if (op == '+')
                {
                    left += right;
                }
                else
                {
                    left -= right;
                }
            }

            return left;
        }

        private float ParseTerm(StringPtr ptr)
        {
            float left = ParseFactor(ptr);

            while (!ptr.IsEnd && (ptr.Current == '*' || ptr.Current == '/'))
            {
                char op = ptr.Current;
                ptr.MoveNext();
                float right = ParseFactor(ptr);

                if (op == '*')
                {
                    left *= right;
                }
                else
                {
                    if (right == 0)
                    {
                        throw new DivideByZeroException("Division by zero.");
                    }
                    left /= right;
                }
            }

            return left;
        }

        private float ParseFactor(StringPtr ptr)
        {
            if (ptr.IsEnd)
            {
                throw new Exception("Unexpected end of expression.");
            }

            if (ptr.Current == '(')
            {
                ptr.MoveNext();
                float result = ParseExpression(ptr);

                if (ptr.IsEnd || ptr.Current != ')')
                {
                    throw new Exception("Missing closing parenthesis.");
                }

                ptr.MoveNext();
                return result;
            }
            else if (char.IsLetter(ptr.Current))
            {
                string variableName = ParseVariableName(ptr);
                if (values.TryGetValue(variableName, out float value))
                {
                    return value;
                }
                else
                {
                    throw new Exception($"Variable '{variableName}' not found.");
                }
            }
            else if (char.IsDigit(ptr.Current) || ptr.Current == '-')
            {
                return ParseNumber(ptr);
            }
            else
            {
                throw new Exception($"Unexpected character '{ptr.Current}'.");
            }
        }

        private string ParseVariableName(StringPtr ptr)
        {
            string name = "";
            while (!ptr.IsEnd && char.IsLetterOrDigit(ptr.Current))
            {
                name += ptr.Current;
                ptr.MoveNext();
            }
            return name;
        }

        private float ParseNumber(StringPtr ptr)
        {
            string numberStr = "";
            bool hasDecimal = false;

            if (ptr.Current == '-')
            {
                numberStr += ptr.Current;
                ptr.MoveNext();
            }

            while (!ptr.IsEnd && (char.IsDigit(ptr.Current) || ptr.Current == '.'))
            {
                if (ptr.Current == '.')
                {
                    if (hasDecimal)
                    {
                        throw new Exception("Invalid number format.");
                    }
                    hasDecimal = true;
                }
                numberStr += ptr.Current;
                ptr.MoveNext();
            }

            if (string.IsNullOrEmpty(numberStr) || numberStr == "-" || (numberStr.Length == 1 && numberStr[0] == '.'))
            {
                throw new Exception("Invalid number format.");
            }

            if (float.TryParse(numberStr, out float result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Invalid number: '{numberStr}'.");
            }
        }

        // Helper class to avoid string allocations
        private class StringPtr
        {
            private string str;
            private int position;

            public StringPtr(string str)
            {
                this.str = str.Replace(" ", ""); // Remove spaces
                this.position = 0;
            }

            public char Current
            {
                get { return str[position]; }
            }

            public bool IsEnd
            {
                get { return position >= str.Length; }
            }

            public void MoveNext()
            {
                position++;
            }
        }
    }
}