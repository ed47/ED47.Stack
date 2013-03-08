// /************************************
// * Created by Essence1
// **************************************/
#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#endregion

namespace ED47.Stack.Web.Template
{
    [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public class Evaluator
    {
        public Template Tpl;
        private System.Collections.Hashtable constTable = new System.Collections.Hashtable();
        private System.Collections.Hashtable varTable = new System.Collections.Hashtable();
        private System.Collections.Hashtable opTable = new System.Collections.Hashtable();
        private System.Collections.Hashtable funcTable = new System.Collections.Hashtable();

        private System.Collections.ArrayList tokenList = new System.Collections.ArrayList();
        private System.Collections.Stack postfixStack = new System.Collections.Stack();
        private System.Collections.Stack operatorStack = new System.Collections.Stack();
        private System.Collections.Stack operandStack = new System.Collections.Stack();
        private System.Collections.ArrayList argumentList = new System.Collections.ArrayList();

        private System.String delimiters = "";
        private System.String lexTokenAlphabet = "";
        private System.String lastError = "";
        private Operand lastResult = new Operand(OperandType.LONG, 0);

        public Evaluator()
        {
            delimiters = "+-*/^&|!=,%<>";
            lexTokenAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789_";

            constTable.Add("pi", new Operand(OperandType.DOUBLE, System.Math.PI));
            constTable.Add("e", new Operand(OperandType.DOUBLE, System.Math.E));
            constTable.Add("true", new Operand(OperandType.BOOLEAN, true));
            constTable.Add("false", new Operand(OperandType.BOOLEAN, false));

            opTable.Add("+", new Operator(OperatorType.BINARY, OperatorSemantics.Add, 6));
            opTable.Add("-", new Operator(OperatorType.BINARY, OperatorSemantics.Subtract, 6));
            opTable.Add("*", new Operator(OperatorType.BINARY, OperatorSemantics.Multiply, 5));
            opTable.Add("/", new Operator(OperatorType.BINARY, OperatorSemantics.Divide, 5));
            opTable.Add("%", new Operator(OperatorType.BINARY, OperatorSemantics.Modulo, 5));
            opTable.Add("^", new Operator(OperatorType.BINARY, OperatorSemantics.Power, 4));
            opTable.Add("&&", new Operator(OperatorType.BINARY, OperatorSemantics.And, 13));
            opTable.Add("||", new Operator(OperatorType.BINARY, OperatorSemantics.Or, 14));
            opTable.Add("!", new Operator(OperatorType.UNARY, OperatorSemantics.Not, 3));
            opTable.Add("==", new Operator(OperatorType.BINARY, OperatorSemantics.Equals, 9));
            opTable.Add("!=", new Operator(OperatorType.BINARY, OperatorSemantics.NotEquals, 9));
            opTable.Add("===", new Operator(OperatorType.BINARY, OperatorSemantics.EqualsType, 9));
            opTable.Add("<", new Operator(OperatorType.BINARY, OperatorSemantics.Less, 8));
            opTable.Add(">", new Operator(OperatorType.BINARY, OperatorSemantics.Greater, 8));


            opTable.Add("<=", new Operator(OperatorType.BINARY, OperatorSemantics.LessEqual, 8));
            opTable.Add(">=", new Operator(OperatorType.BINARY, OperatorSemantics.GreaterEqual, 8));
            opTable.Add("(", new Operator(OperatorType.LPAREN, null, 0));
            opTable.Add(")", new Operator(OperatorType.RPAREN, null, 0));
            opTable.Add(",", new Operator(OperatorType.ARGSEP, null, 0));

            funcTable.Add("sqrt", new FunctionCall(FunctionDefinitions.Sqrt, 1));
            funcTable.Add("cbrt", new FunctionCall(FunctionDefinitions.Cbrt, 1));
            funcTable.Add("sin", new FunctionCall(FunctionDefinitions.Sin, 1));
            funcTable.Add("cos", new FunctionCall(FunctionDefinitions.Cos, 1));
            funcTable.Add("sinh", new FunctionCall(FunctionDefinitions.Sinh, 1));
            funcTable.Add("cosh", new FunctionCall(FunctionDefinitions.Cosh, 1));
            funcTable.Add("asin", new FunctionCall(FunctionDefinitions.Asin, 1));
            funcTable.Add("acos", new FunctionCall(FunctionDefinitions.Acos, 1));
            funcTable.Add("tan", new FunctionCall(FunctionDefinitions.Tan, 1));
            funcTable.Add("tanh", new FunctionCall(FunctionDefinitions.Tanh, 1));
            funcTable.Add("atan", new FunctionCall(FunctionDefinitions.Atan, 1));
            funcTable.Add("atan2", new FunctionCall(FunctionDefinitions.Atan2, 2));
            funcTable.Add("rad", new FunctionCall(FunctionDefinitions.Rad, 1));
            funcTable.Add("deg", new FunctionCall(FunctionDefinitions.Deg, 1));
            funcTable.Add("log", new FunctionCall(FunctionDefinitions.Log, 1));
            funcTable.Add("log2", new FunctionCall(FunctionDefinitions.Log2, 1));
            funcTable.Add("log10", new FunctionCall(FunctionDefinitions.Log10, 1));
            funcTable.Add("hypot", new FunctionCall(FunctionDefinitions.Hypot, 2));
            funcTable.Add("ceil", new FunctionCall(FunctionDefinitions.Ceil, 1));
            funcTable.Add("floor", new FunctionCall(FunctionDefinitions.Floor, 1));
            funcTable.Add("round", new FunctionCall(FunctionDefinitions.Round, 1));
            funcTable.Add("trunc", new FunctionCall(FunctionDefinitions.Trunc, 1));
            funcTable.Add("sgn", new FunctionCall(FunctionDefinitions.Sgn, 1));
            funcTable.Add("neg", new FunctionCall(FunctionDefinitions.Neg, 1));
            funcTable.Add("abs", new FunctionCall(FunctionDefinitions.Abs, 1));
            funcTable.Add("fact", new FunctionCall(FunctionDefinitions.Fact, 1));
            funcTable.Add("exp", new FunctionCall(FunctionDefinitions.Exp, 1));
            funcTable.Add("pow", new FunctionCall(FunctionDefinitions.Pow, 2));
            funcTable.Add("int", new FunctionCall(FunctionDefinitions.Long, 1));
            funcTable.Add("long", new FunctionCall(FunctionDefinitions.Long, 1));
            funcTable.Add("double", new FunctionCall(FunctionDefinitions.Double, 1));
            funcTable.Add("float", new FunctionCall(FunctionDefinitions.Double, 1));
            funcTable.Add("bool", new FunctionCall(FunctionDefinitions.Bool, 1));
            funcTable.Add("boolean", new FunctionCall(FunctionDefinitions.Bool, 1));
            funcTable.Add("if", new FunctionCall(FunctionDefinitions.If, 3));
            funcTable.Add("rand", new FunctionCall(FunctionDefinitions.Rand, 2));
        }

        public bool Evaluate(System.String expression)
        {
            if (Tokenize(expression) && ToPostfix() && EvaluatePostfix())
            {
                return true;
            }

            return false;
        }

        public Operand Result()
        {
            return lastResult;
        }

        public System.String Error()
        {
            return lastError;
        }

        public System.Collections.Hashtable Constants()
        {
            return constTable;
        }

        public System.Collections.Hashtable Variables()
        {
            return varTable;
        }

        public System.Collections.Hashtable Functions()
        {
            return funcTable;
        }

        public bool AddConstant(System.String key, Operand constant)
        {
            if (!LexTokenValid(key))
            {
                return false;
            }

            RemoveKey(key);
            constTable.Add(key, constant);
            return true;
        }

        public bool AddVariable(System.String key, Operand variable)
        {
            if (!LexTokenValid(key))
            {
                return false;
            }

            RemoveKey(key);
            varTable.Add(key, variable);
            return true;
        }

        public bool AddFunction(System.String key, FunctionCall function)
        {
            if (!LexTokenValid(key))
            {
                return false;
            }

            RemoveKey(key);
            funcTable.Add(key, function);
            return true;
        }

        private bool LexTokenValid(System.String token)
        {
            for (int i = 0; i < token.Length; i++)
            {
                if (token[i].ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture).IndexOfAny(lexTokenAlphabet.ToCharArray()) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        private void RemoveKey(System.String key)
        {
            constTable.Remove(key);
            varTable.Remove(key);
            funcTable.Remove(key);
        }
        
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool EvaluatePostfix()
        {
            operandStack.Clear();
            Operand num;
            System.Array postfix = postfixStack.ToArray();
            System.Array.Reverse(postfix);

            foreach (var token in postfix)
            {
                if (token.GetType() == typeof(Operator))
                {
                    num = new Operand();

                    switch (((Operator)token).type)
                    {
                        case OperatorType.BINARY:
                            if (operandStack.Count < 2)
                            {
                                lastError = "Evaluation error, a binary operator needs 2 operands";
                                return false;
                            }

                            operandStack.Push(((Operator)token).semantic(ref num,
                                ((Operand)operandStack.Pop()), ((Operand)operandStack.Pop())));
                            break;

                        case OperatorType.UNARY:
                            if (operandStack.Count < 1)
                            {
                                lastError = "Evaluation error, an unary operator needs 1 operand";
                                return false;
                            }

                            operandStack.Push(((Operator)token).semantic(ref num,
                                ((Operand)operandStack.Pop())));
                            break;
                    }
                }
                else if (token.GetType() == typeof(Operand))
                {
                    operandStack.Push(token);
                }
                else if (token.GetType() == typeof(FunctionCall))
                {
                    num = new Operand();
                    argumentList.Clear();

                    for (int i = 0; i < ((FunctionCall)token).args; i++)
                    {
                        if (operandStack.Count != 0)
                        {
                            argumentList.Add(operandStack.Pop());
                        }
                        else
                        {
                            lastError = "Evaluation error, wrong argument count";
                            return false;
                        }
                    }

                    operandStack.Push(((FunctionCall)token).definition(ref num,
                        (Operand[])argumentList.ToArray(typeof(Operand))));
                }
            }

            if (operandStack.Count != 1)
            {
                lastError = "Evaluation error, unstacked operands";
                return false;
            }

            lastResult = ((Operand)operandStack.Peek());
            varTable.Remove("ans");
            varTable.Add("ans", lastResult);
            return true;
        }
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool ToPostfix()
        {
            System.Object prevTok = null;
            System.Object currTok;
            bool end = false;
            postfixStack.Clear();
            operatorStack.Clear();

            for (int i = 0; i < tokenList.Count; i++)
            {
                currTok = tokenList[i];

                if (currTok.GetType() == typeof(Operator))
                {
                    switch (((Operator)currTok).type)
                    {
                        case OperatorType.LPAREN:
                            if (prevTok != null)
                            {
                                if ((prevTok.GetType() == typeof(Operator) && (
                                    (Operator)prevTok).type == OperatorType.RPAREN) ||
                                    prevTok.GetType() == typeof(Operand))
                                {

                                    lastError = "Parse error, unexpected left parenthesis";
                                    return false;
                                }
                                else if (prevTok.GetType() != typeof(FunctionCall))
                                {
                                    operatorStack.Push(currTok);
                                }
                            }
                            else
                            {
                                operatorStack.Push(currTok);
                            }
                            end = false;
                            break;

                        case OperatorType.ARGSEP:
                        case OperatorType.RPAREN:
                            if (prevTok != null && (
                                prevTok.GetType() == typeof(Operand) || (
                                prevTok.GetType() == typeof(Operator) &&
                                ((Operator)prevTok).type == OperatorType.RPAREN)))
                            {

                                while (operatorStack.Count != 0 &&
                                    operatorStack.Peek().GetType() != typeof(FunctionCall) &&
                                    operatorStack.Peek().GetType() == typeof(Operator) &&
                                    ((Operator)operatorStack.Peek()).type != OperatorType.LPAREN)
                                {

                                    postfixStack.Push(operatorStack.Pop());
                                }

                                if (((Operator)currTok).type == OperatorType.RPAREN)
                                {
                                    if (operatorStack.Count != 0 &&
                                        operatorStack.Peek().GetType() == typeof(Operator) &&
                                        ((Operator)operatorStack.Peek()).type == OperatorType.LPAREN)
                                    {

                                        operatorStack.Pop();
                                    }
                                    else if (operatorStack.Count != 0 && operatorStack.Peek().GetType() == typeof(FunctionCall))
                                    {
                                        postfixStack.Push(operatorStack.Pop());
                                    }
                                    else
                                    {
                                        lastError = "Parse error, unbalanced parenthesis";
                                        return false;
                                    }

                                    while (operatorStack.Count != 0 &&
                                        operatorStack.Peek().GetType() == typeof(Operator) &&
                                        ((Operator)operatorStack.Peek()).type == OperatorType.UNARY)
                                    {

                                        postfixStack.Push(operatorStack.Pop());
                                    }
                                }
                                else if (operatorStack.Count == 0 || operatorStack.Peek().GetType() != typeof(FunctionCall))
                                {
                                    lastError = "Parse error, no matching function to argument separator";
                                    return false;
                                }
                            }
                            else
                            {
                                lastError = ((Operator)currTok).type == OperatorType.RPAREN ?
                                    "Parse error, unexpected right parenthesis" :
                                    "Parse error, unexpected argument separator";

                                return false;
                            }
                            end = true;
                            break;

                        case OperatorType.BINARY:
                            if (((Operator)currTok).semantic == OperatorSemantics.Subtract && (prevTok == null || (
                                prevTok.GetType() == typeof(Operator) &&
                                ((Operator)prevTok).type != OperatorType.RPAREN)))
                            {

                                currTok = new Operator(OperatorType.UNARY, FunctionDefinitions.Neg, 0);
                                goto UnaryContext;
                            }

                            if (prevTok == null || (prevTok.GetType() == typeof(FunctionCall) || (
                                prevTok.GetType() == typeof(Operator) &&
                                ((Operator)prevTok).type != OperatorType.RPAREN)))
                            {

                                lastError = "Parse error, unexpected binary operator";
                                return false;
                            }

                            while (operatorStack.Count != 0 &&
                                operatorStack.Peek().GetType() == typeof(Operator) &&
                                ((Operator)operatorStack.Peek()).type != OperatorType.LPAREN &&
                                ((Operator)operatorStack.Peek()).precedence <= ((Operator)currTok).precedence)
                            {

                                postfixStack.Push(operatorStack.Pop());
                            }

                            operatorStack.Push(currTok);
                            end = false;
                            break;

                        case OperatorType.UNARY:
                        UnaryContext:

                            if (prevTok != null && (prevTok.GetType() == typeof(Operand) || (
                                prevTok.GetType() == typeof(Operator) &&
                                ((Operator)prevTok).type == OperatorType.RPAREN)))
                            {

                                lastError = "Parse error, unexpected unary operator";
                                return false;
                            }

                            operatorStack.Push(currTok);
                            end = false;
                            break;
                    }
                }
                else if (currTok.GetType() == typeof(Operand))
                {
                    if (prevTok != null && (
                        prevTok.GetType() == typeof(Operand) || (
                        prevTok.GetType() == typeof(Operator) && ((Operator)prevTok).type == OperatorType.RPAREN)))
                    {

                        lastError = "Parse error, unexpected operand";
                        return false;
                    }

                    postfixStack.Push(currTok);

                    while (operatorStack.Count != 0 &&
                        operatorStack.Peek().GetType() == typeof(Operator) &&
                        ((Operator)operatorStack.Peek()).type == OperatorType.UNARY)
                    {

                        postfixStack.Push(operatorStack.Pop());
                    }

                    end = true;
                }
                else if (currTok.GetType() == typeof(FunctionCall))
                {
                    if (prevTok != null && (
                        prevTok.GetType() == typeof(Operand) || (
                        prevTok.GetType() == typeof(Operator) && ((Operator)prevTok).type == OperatorType.RPAREN)))
                    {

                        lastError = "Parse error, unexpected function";
                        return false;
                    }

                    operatorStack.Push(currTok);
                    end = false;
                }

                prevTok = currTok;
            }

            if (!end)
            {
                lastError = "Parse error, Unexpected end of expression";
                return false;
            }

            while (operatorStack.Count != 0)
            {
                if ((operatorStack.Peek().GetType() == typeof(Operator) && (
                    (Operator)operatorStack.Peek()).type == OperatorType.LPAREN) ||
                    operatorStack.Peek().GetType() == typeof(FunctionCall))
                {

                    lastError = "Parse error, unbalanced parenthesis at end of expression";
                    return false;
                }
                else
                {
                    postfixStack.Push(operatorStack.Pop());
                }
            }

            return true;
        }

        private bool Tokenize(System.String expression)
        {
            System.String lexToken = "";
            System.String opToken;
            long longVal;
            double doubleVal;
            int scan = 0;
            expression += " ";
            tokenList.Clear();

            while (scan < expression.Length)
            {
                if (expression[scan] == ' ' || expression[scan].ToString(CultureInfo.InvariantCulture).IndexOfAny((delimiters + "()").ToCharArray()) > -1)
                {
                    if (lexToken.Length != 0)
                    {
                        if (constTable.ContainsKey(lexToken))
                        {
                            tokenList.Add(new Operand(((Operand)constTable[lexToken]).type, ((Operand)constTable[lexToken]).value));
                        }
                        else if (funcTable.ContainsKey(lexToken))
                        {
                            tokenList.Add(funcTable[lexToken]);
                        }
                        else if (varTable.ContainsKey(lexToken))
                        {
                            tokenList.Add(new Operand(((Operand)varTable[lexToken]).type, ((Operand)varTable[lexToken]).value));
                        }
                        else
                        {
                            if (long.TryParse(lexToken,
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.NumberFormatInfo.InvariantInfo, out longVal))
                            {

                                tokenList.Add(new Operand(OperandType.LONG, longVal));
                            }
                            else if (double.TryParse(lexToken,
                              System.Globalization.NumberStyles.Any,
                              System.Globalization.NumberFormatInfo.InvariantInfo, out doubleVal))
                            {

                                tokenList.Add(new Operand(OperandType.DOUBLE, doubleVal));
                            }
                            else
                            {
                                object val = Template.GetValue(Tpl, Tpl.Current, lexToken);
                                OperandType t;
                                if (val == null) val = "";
                                switch (val.GetType().FullName)
                                {
                                    case "System.Int32":
                                        t = OperandType.LONG;
                                        break;
                                    case "System.Double": t = OperandType.DOUBLE; break;
                                    case "System.Boolean": t = OperandType.BOOLEAN; break;
                                    case "System.String": t = OperandType.STRING; break;
                                    default: t = OperandType.STRING; break;
                                }

                                tokenList.Add(new Operand(t, val));
                            }
                        }
                    }

                    if (expression[scan].ToString(CultureInfo.InvariantCulture).IndexOfAny("()".ToCharArray()) > -1)
                    {
                        tokenList.Add(opTable[expression[scan].ToString(CultureInfo.InvariantCulture)]);
                        scan++;
                    }
                    else if (expression[scan] != ' ')
                    {
                        opToken = "";

                        do
                        {
                            opToken += expression[scan];
                            scan++;
                        } while (scan < expression.Length && expression[scan].ToString(CultureInfo.InvariantCulture).IndexOfAny(delimiters.ToCharArray()) > -1);

                        if (opTable.ContainsKey(opToken))
                        {
                            tokenList.Add(opTable[opToken]);
                        }
                        else
                        {
                            lastError = "Tokenize error, unrecognized operator '" +
                                opToken + "' at position " + scan;

                            return false;
                        }
                    }
                    else
                    {
                        scan++;
                    }

                    lexToken = "";
                }
                else
                {
                    lexToken += expression[scan++];
                }
            }

            return true;
        }
    }

    public enum OperandType
    {
        DOUBLE,
        LONG,
        BOOLEAN,
        STRING
    };

    public class Operand
    {
        public OperandType type;
        public System.Object value;

        public Operand(OperandType type, System.Object value)
        {
            this.type = type;
            this.value = value;
        }

        public Operand()
        {
        }

        public double ToDouble()
        {
            if (type == OperandType.LONG)
            {
                value = Convert.ToDouble(value);
                type = OperandType.DOUBLE;
            }
            else if (type == OperandType.BOOLEAN)
            {
                value = ((bool)value ? 1.0 : 0.0);
                type = OperandType.DOUBLE;
            }
            return Convert.ToDouble(value);
        }

        public long ToLong()
        {
            if (type == OperandType.DOUBLE)
            {
                value = (long)((double)value);
                type = OperandType.LONG;
            }
            else if (type == OperandType.BOOLEAN)
            {
                value = (long)(((bool)value) ? 1 : 0);
                type = OperandType.LONG;
            }
            return Convert.ToInt64(value);
        }

        public bool ToBoolean()
        {
            if (type == OperandType.LONG)
            {
                value = ((long)value == 0 ? false : true);
                type = OperandType.BOOLEAN;
            }
            else if (type == OperandType.DOUBLE)
            {
                value = ((double)value == 0.0 ? false : true);
                type = OperandType.BOOLEAN;
            }
            return (bool)value;
        }
    }

    public enum OperatorType
    {
        UNARY,
        BINARY,
        LPAREN,
        RPAREN,
        ARGSEP,
    };

    public class Operator
    {
        public OperatorType type;
        public OperatorSemantic semantic;
        public byte precedence;

        public Operator(OperatorType type, OperatorSemantic semantic, byte precedence)
        {
            this.type = type;
            this.semantic = semantic;
            this.precedence = precedence;
        }
    }

    public delegate Operand OperatorSemantic(ref Operand result, params Operand[] operands);

    static class OperatorSemantics
    {
        public static Operand Add(ref Operand result, params Operand[] operands)
        {
            if (operands[1].type == OperandType.DOUBLE || operands[0].type == OperandType.DOUBLE)
            {
                result.value = operands[1].ToDouble() + operands[0].ToDouble();
                result.type = OperandType.DOUBLE;
            }
            else
            {
                result.value = operands[1].ToLong() + operands[0].ToLong();
                result.type = OperandType.LONG;
            }
            return result;
        }

        public static Operand Subtract(ref Operand result, params Operand[] operands)
        {
            if (operands[1].type == OperandType.DOUBLE || operands[0].type == OperandType.DOUBLE)
            {
                result.value = operands[1].ToDouble() - operands[0].ToDouble();
                result.type = OperandType.DOUBLE;
            }
            else
            {
                result.value = operands[1].ToLong() - operands[0].ToLong();
                result.type = OperandType.LONG;
            }
            return result;
        }

        public static Operand Multiply(ref Operand result, params Operand[] operands)
        {
            if (operands[1].type == OperandType.DOUBLE || operands[0].type == OperandType.DOUBLE)
            {
                result.value = operands[1].ToDouble() * operands[0].ToDouble();
                result.type = OperandType.DOUBLE;
            }
            else
            {
                result.value = operands[1].ToLong() * operands[0].ToLong();
                result.type = OperandType.LONG;
            }
            return result;
        }

        public static Operand Divide(ref Operand result, params Operand[] operands)
        {
            if (operands[1].type == OperandType.DOUBLE || operands[0].type == OperandType.DOUBLE)
            {
                try
                {
                    result.value = operands[1].ToDouble() / operands[0].ToDouble();
                }
                catch (System.DivideByZeroException)
                {
                    System.Console.WriteLine("Exception: Division by zero!");
                }
                result.type = OperandType.DOUBLE;
            }
            else
            {
                try
                {
                    result.value = operands[1].ToLong() / operands[0].ToLong();
                }
                catch (System.DivideByZeroException)
                {
                    System.Console.WriteLine("Exception: Division by zero!");
                }
                result.type = OperandType.LONG;
            }
            return result;
        }

        public static Operand Modulo(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToLong() % operands[0].ToLong();
            result.type = OperandType.LONG;
            return result;
        }

        public static Operand Power(ref Operand result, params Operand[] operands)
        {
            if (operands[1].type == OperandType.DOUBLE || operands[0].type == OperandType.DOUBLE)
            {
                result.value = System.Math.Pow(operands[1].ToDouble(), operands[0].ToDouble());
                result.type = OperandType.DOUBLE;
            }
            else
            {
                result.value = (long)System.Math.Pow(operands[1].ToLong(), operands[0].ToLong());
                result.type = OperandType.LONG;
            }
            return result;
        }

        public static Operand And(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToBoolean() && operands[0].ToBoolean();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand Or(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToBoolean() || operands[0].ToBoolean();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand Not(ref Operand result, params Operand[] operands)
        {
            result.value = !operands[0].ToBoolean();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand Equals(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() == operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand NotEquals(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() != operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand EqualsType(ref Operand result, params Operand[] operands)
        {
            result.value = (operands[1].type == operands[0].type) && (operands[1].ToDouble() == operands[0].ToDouble());
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand Less(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() < operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand Greater(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() > operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand LessEqual(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() <= operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand GreaterEqual(ref Operand result, params Operand[] operands)
        {
            result.value = operands[1].ToDouble() >= operands[0].ToDouble();
            result.type = OperandType.BOOLEAN;
            return result;
        }
    }
    [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public class FunctionCall
    {
        public FunctionDefinition definition;
        public byte args;

        public FunctionCall(FunctionDefinition definition, byte args)
        {
            this.definition = definition;
            this.args = args;
        }
    }

    public delegate Operand FunctionDefinition(ref Operand result, params Operand[] arguments);

    static class FunctionDefinitions
    {
        public static Operand Sqrt(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Sqrt(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Cbrt(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Pow(arguments[0].ToDouble(), 1.0 / 3.0);
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Sin(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Sin(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Cos(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Cos(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Sinh(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Sinh(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Cosh(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Cosh(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Asin(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Asin(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Acos(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Acos(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Tan(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Tan(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Tanh(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Tanh(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Atan(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Atan(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Atan2(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Atan2(arguments[1].ToDouble(), arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Rad(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.PI * arguments[0].ToDouble() / 180;
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Deg(ref Operand result, params Operand[] arguments)
        {
            result.value = arguments[0].ToDouble() * 180 / System.Math.PI;
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Log(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Log(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Log2(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Log(arguments[0].ToDouble()) / System.Math.Log(2);
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Log10(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Log10(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Hypot(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Sqrt(
                System.Math.Pow(arguments[1].ToDouble(), 2) +
                System.Math.Pow(arguments[0].ToDouble(), 2));

            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Ceil(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Ceiling(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Floor(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Floor(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Round(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Round(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Trunc(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Truncate(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Sgn(ref Operand result, params Operand[] arguments)
        {
            result.value = (long)System.Math.Sign(arguments[0].ToDouble());
            result.type = OperandType.LONG;
            return result;
        }

        public static Operand Neg(ref Operand result, params Operand[] operands)
        {
            if (operands[0].type == OperandType.DOUBLE)
            {
                result.value = -operands[0].ToDouble();
                result.type = OperandType.DOUBLE;
            }
            else if (operands[0].type == OperandType.LONG)
            {
                result.value = -operands[0].ToLong();
                result.type = OperandType.LONG;
            }
            else
            {
                result.value = !operands[0].ToBoolean();
                result.type = OperandType.BOOLEAN;
            }
            return result;
        }

        public static Operand Abs(ref Operand result, params Operand[] arguments)
        {
            switch (arguments[0].type)
            {
                case OperandType.BOOLEAN:
                    result.value = arguments[0].value;
                    result.type = OperandType.BOOLEAN;
                    break;

                case OperandType.DOUBLE:
                    result.value = System.Math.Abs((double)arguments[0].value);
                    result.type = OperandType.DOUBLE;
                    break;

                case OperandType.LONG:
                    result.value = System.Math.Abs((long)arguments[0].value);
                    result.type = OperandType.LONG;
                    break;
            }
            return result;
        }

        public static Operand Fact(ref Operand result, params Operand[] arguments)
        {
            result.value = (long)1;

            for (long i = 2; i <= arguments[0].ToLong(); i++)
            {
                result.value = (long)result.value * i;
            }

            result.type = OperandType.LONG;
            return result;
        }

        public static Operand Exp(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Exp(arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Pow(ref Operand result, params Operand[] arguments)
        {
            result.value = System.Math.Pow(arguments[1].ToDouble(), arguments[0].ToDouble());
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Long(ref Operand result, params Operand[] arguments)
        {
            result.value = arguments[0].ToLong();
            result.type = OperandType.LONG;
            return result;
        }

        public static Operand Double(ref Operand result, params Operand[] arguments)
        {
            result.value = arguments[0].ToDouble();
            result.type = OperandType.DOUBLE;
            return result;
        }

        public static Operand Bool(ref Operand result, params Operand[] arguments)
        {
            result.value = arguments[0].ToBoolean();
            result.type = OperandType.BOOLEAN;
            return result;
        }

        public static Operand If(ref Operand result, params Operand[] arguments)
        {
            if (arguments[2].ToBoolean())
            {
                result.value = arguments[1].value;
                result.type = arguments[1].type;
            }
            else
            {
                result.value = arguments[0].value;
                result.type = arguments[0].type;
            }
            return result;
        }

        public static Operand Rand(ref Operand result, params Operand[] arguments)
        {
            var rand = new System.Random();
            result.value = (long)rand.Next((int)arguments[1].ToLong(), (int)arguments[0].ToLong());
            result.type = OperandType.LONG;
            return result;
        }

    }
}