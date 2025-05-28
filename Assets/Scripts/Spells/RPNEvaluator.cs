using System;
using System.Collections.Generic;
using UnityEngine;

public static class RPNEvaluator
{
    public static float Evaluate(string expression, Dictionary<string, float> variables)
    {
        if (string.IsNullOrEmpty(expression)) return 0f;

        Stack<float> stack = new Stack<float>();
        string[] tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else if (float.TryParse(token, out float number))
            {
                stack.Push(number);
            }
            else
            {
                if (stack.Count < 2)
                {
                    Debug.LogError($"RPN Evaluation Error: Not enough operands for operator '{token}'");
                    return 0;
                }

                float b = stack.Pop();
                float a = stack.Pop();

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/": stack.Push(a / b); break;
                    case "%": stack.Push(a % b); break;
                    default:
                        Debug.LogError($"RPN Evaluation Error: Unknown token '{token}'");
                        return 0;
                }
            }
        }

        return stack.Count == 1 ? stack.Pop() : 0f;
    }
}