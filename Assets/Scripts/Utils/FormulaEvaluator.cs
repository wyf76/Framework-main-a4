using UnityEngine;
using System.Collections.Generic;

public static class FormulaEvaluator
{
    public static int Evaluate(string expr, int wave)
    {
        var tokens = expr.Split(' ');
        var stack  = new Stack<float>();
        foreach (var t in tokens)
        {
            if (float.TryParse(t, out float num))
            {
                stack.Push(num);
            }
            else if (t == "wave")
            {
                stack.Push(wave);
            }
            else if (t == "+")
            {
                var b = stack.Pop();
                var a = stack.Pop();
                stack.Push(a + b);
            }
            else if (t == "-")
            {
                var b = stack.Pop();
                var a = stack.Pop();
                stack.Push(a - b);
            }
            else if (t == "*")
            {
                var b = stack.Pop();
                var a = stack.Pop();
                stack.Push(a * b);
            }
            else if (t == "/")
            {
                var b = stack.Pop();
                var a = stack.Pop();
                stack.Push(a / b);
            }
            else
            {
                Debug.LogError($"Unknown token in formula: {t}");
            }
        }
        return Mathf.RoundToInt(stack.Pop());
    }
}
