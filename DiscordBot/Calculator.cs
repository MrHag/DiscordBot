using System;
using System.Collections.Generic;

namespace CSharpCALC
{
    internal static class Calculator
    {
        private static Calculator.number cal(List<Calculator.number> g, int index)
        {
            Calculator.number res = new Calculator.number();
            res.type = Calculator.number.types.Int;
            if (index - 1 < 0 || index + 1 >= g.Count)
            {
                throw new Exception("![]");
            }
            if (g[index - 1].type == Calculator.number.types.String)
            {
                g[index - 1] = Calculator.calculate(g[index - 1].val);
                if (g[index - 1].type == Calculator.number.types.Error)
                {
                    return g[index - 1];
                }
            }
            if (g[index + 1].type == Calculator.number.types.String)
            {
                g[index + 1] = Calculator.calculate(g[index + 1].val);
                if (g[index + 1].type == Calculator.number.types.Error)
                {
                    return g[index + 1];
                }
            }
            double num = double.Parse(g[index - 1].val);
            double num2 = double.Parse(g[index + 1].val);
            char c = g[index].val[0];
            switch (c)
            {
                case '*':
                    res.val = (num * num2).ToString();
                    break;
                case '+':
                    res.val = (num + num2).ToString();
                    break;
                case ',':
                case '.':
                    break;
                case '-':
                    res.val = (num - num2).ToString();
                    break;
                case '/':
                    if (num2 == 0.0)
                    {
                        throw new Exception("/0");
                    }
                    res.val = (num / num2).ToString();
                    break;
                default:
                    if (c == '^')
                    {
                        res.val = Math.Pow(num, num2).ToString();
                    }
                    break;
            }
            return res;
        }

        public static Calculator.number calculate(string data)
        {
            Calculator.number result;
            try
            {
                data = data.Replace(" ", "").Replace('.', ',');
                int last_num = -1;
                bool open = false;
                int opens_count = 0;
                bool num = false;
                bool default_num = true;
                List<Calculator.number> arr = new List<Calculator.number>();
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == '+' || data[i] == '-' || data[i] == '*' || data[i] == '/' || data[i] == '^')
                    {
                        if (num && !open)
                        {
                            num = false;
                            int last_num_lenght = i - last_num;
                            Calculator.number.types tmp;
                            if (default_num)
                            {
                                tmp = Calculator.number.types.Int;
                            }
                            else
                            {
                                tmp = Calculator.number.types.String;
                            }
                            arr.Add(new Calculator.number(data.Substring(last_num, last_num_lenght), tmp));
                            default_num = true;
                            arr.Add(new Calculator.number(data[i].ToString(), Calculator.number.types.Do));
                        }
                        if (open)
                        {
                            default_num = false;
                        }
                        if (!num)
                        {
                            if (i > 0)
                            {
                                if (data[i - 1] == '+' || data[i - 1] == '-' || data[i - 1] == '*' || data[i - 1] == '/' || data[i - 1] == '^')
                                {
                                    throw new Exception("+-");
                                }
                            }
                            else
                            {
                                if (data[i] != '-')
                                {
                                    throw new Exception("+-");
                                }
                                num = true;
                                last_num = i;
                            }
                        }
                    }
                    else
                    {
                        if (data[i] == '(')
                        {
                            opens_count++;
                        }
                        if (data[i] == ')')
                        {
                            opens_count--;
                        }
                        if (opens_count < 0)
                        {
                            throw new Exception("!()");
                        }
                        if (!num)
                        {
                            if (data[i] == '(')
                            {
                                open = true;
                                num = true;
                                last_num = i + 1;
                            }
                            else if (int.Parse(data[i].ToString()) != -1)
                            {
                                num = true;
                                last_num = i;
                                if (i == data.Length - 1)
                                {
                                    num = false;
                                    int last_num_lenght = i + 1 - last_num;
                                    Calculator.number.types tmp2;
                                    if (default_num)
                                    {
                                        tmp2 = Calculator.number.types.Int;
                                    }
                                    else
                                    {
                                        tmp2 = Calculator.number.types.String;
                                    }
                                    arr.Add(new Calculator.number(data.Substring(last_num, last_num_lenght), tmp2));
                                    default_num = true;
                                }
                            }
                        }
                        else if (open)
                        {
                            if (data[i] == ')' && opens_count == 0)
                            {
                                open = false;
                                num = false;
                                int last_num_lenght = i - last_num;
                                Calculator.number.types tmp3;
                                if (default_num)
                                {
                                    tmp3 = Calculator.number.types.Int;
                                }
                                else
                                {
                                    tmp3 = Calculator.number.types.String;
                                }
                                arr.Add(new Calculator.number(data.Substring(last_num, last_num_lenght), tmp3));
                                if (i != data.Length - 1)
                                {
                                    arr.Add(new Calculator.number(data[i + 1].ToString(), Calculator.number.types.Do));
                                }
                                default_num = true;
                            }
                        }
                        else if (i == data.Length - 1)
                        {
                            num = false;
                            int last_num_lenght = i + 1 - last_num;
                            Calculator.number.types tmp4;
                            if (default_num)
                            {
                                tmp4 = Calculator.number.types.Int;
                            }
                            else
                            {
                                tmp4 = Calculator.number.types.String;
                            }
                            arr.Add(new Calculator.number(data.Substring(last_num, last_num_lenght), tmp4));
                            default_num = true;
                        }
                    }
                }
                Calculator.number res = new Calculator.number();
                string do_arr = new string("*/+-^".ToCharArray());
                if (arr.Count == 1)
                {
                    if (arr[0].type == Calculator.number.types.String)
                    {
                        arr.Add(new Calculator.number("*", Calculator.number.types.Do));
                        arr.Add(new Calculator.number("1", Calculator.number.types.Int));
                    }
                    else
                    {
                        if (arr[0].val != "")
                        {
                            return arr[0];
                        }
                        throw new Exception("undefined");
                    }
                }
                for (int j = 0; j < do_arr.Length; j += 2)
                {
                    for (int k = 0; k < arr.Count; k++)
                    {
                        if (arr[k].type == Calculator.number.types.Do)
                        {
                            res.type = Calculator.number.types.Int;
                            if (arr[k].val == do_arr[j].ToString() || arr[k].val == do_arr[j + 1].ToString())
                            {
                                arr[k - 1] = Calculator.cal(arr, k);
                                if (arr[k - 1].type == Calculator.number.types.Error)
                                {
                                    return arr[k - 1];
                                }
                                arr.RemoveAt(k);
                                arr.RemoveAt(k);
                                k--;
                            }
                            res.val = arr[k].val;
                        }
                    }
                }
                result = res;
            }
            catch (Exception error)
            {
                Calculator.number re = new Calculator.number();
                re.type = Calculator.number.types.Error;
                if (error.Message == "/0")
                {
                    re.val = "Деление 0";
                    result = re;
                }
                else if (error.Message == "!()")
                {
                    re.val = "Не коректно раставлены скобки ()";
                    result = re;
                }
                else if (error.Message == "+-")
                {
                    re.val = "Не коректно раставлены знаки действий";
                    result = re;
                }
                else if (error.Message == "undefined")
                {
                    re.val = "Не возможно определить число";
                    result = re;
                }
                else if (error.Message == "![]")
                {
                    re.val = "Не коректно раставлены елементов примера";
                    result = re;
                }
                else
                {
                    re.val = error.Message;
                    result = re;
                }
            }
            return result;
        }

        public class number
        {
            public string val { get; set; }
            public Calculator.number.types type { get; set; }

            public number(string v, Calculator.number.types t)
            {
                this.val = v;
                this.type = t;
            }

            public number()
            {
                this.val = "";
                this.type = Calculator.number.types.NULL;
            }

            public enum types
            {
                NULL,
                Int,
                String,
                Do,
                Error
            }
        }
    }
}
