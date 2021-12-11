using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebSwaggerDemo.NET5.Common
{
    public class LogicExpression
    {
        /// <summary>
        /// 分析计算表达式 https://github.com/dynamicexpresso/DynamicExpresso
        /// </summary>
        /// <param name="expression">表达式如: (A || B)</param>
        /// <param name="data">数据如: [A,B,C]</param>
        /// <returns></returns>
        public static bool Parse(string expression, string[] data)
        {
            var target = new Interpreter();
            var parameters = new List<Parameter>();
            string temp = Regex.Replace(expression.Replace(" ", ""), @"\W+", ",");
            string[] variables = temp.Split(',');
            foreach (string variable in variables)
            {
                if (variable.Length == 0 || !char.IsLetter(variable[0]))
                    return false;
                bool value = data.Any(v => v.Equals(variable, StringComparison.OrdinalIgnoreCase));
                parameters.Add(new Parameter(variable, value.GetType(), value));
            }
            return target.Eval<bool>(expression, parameters.ToArray());
        }

        /// <summary>
        /// 分析计算表达式
        /// </summary>
        /// <param name="expression">表达式如: (A || B)</param>
        /// <param name="data">数据如: [A,B,C]</param>
        /// <returns></returns>
        public static bool Eval(string expression, string[] data)
        {
            if (expression.Contains("&") && !expression.Contains("&&"))
                expression = expression.Replace("&", "&&");
            if (expression.Contains("|") && !expression.Contains("||"))
                expression = expression.Replace("|", "||");

            //去除空格括号等
            string temp = expression.Replace(" ", "").Replace("(", "").Replace(")", "");

            //将逻辑运算符用逗号替换
            string variable = temp.Replace("&&", ",").Replace("||", ",");

            //将逻辑表达式中的所有条件名称取出
            string[] variables = variable.Split(',');

            //条件名称替换成其对应的值
            for (int i = 0; i < variables.Length; i++)
            {
                string item = variables[i];
                if (item.StartsWith("!"))
                {
                    string str = item.Substring(1);
                    bool value = data.Any(v => v.Equals(str, StringComparison.OrdinalIgnoreCase));
                    expression = expression.Replace(item, !value ? "1" : "0");
                }
                else
                {
                    bool value = data.Any(v => v.Equals(item, StringComparison.OrdinalIgnoreCase));
                    expression = expression.Replace(item, value ? "1" : "0");
                }
            }

            return DealBrackets(expression);
        }

        /// <summary>
        /// 处理括号
        /// </summary>
        private static bool DealBrackets(string expression)
        {
            while (expression.Contains("("))
            {
                //最后一个左括号
                int lasttLeftBracketIndex = -1;
                //与最后第一个左括号对应的右括号
                int firstRightBracketIndex = -1;
                //找到最后一个左括号
                for (int i = 0; i < expression.Length; i++)
                {
                    //获取字符串中的第i个字符
                    string tempChar = expression.Substring(i, 1);
                    //如果是左括号，则将该字符的索引号给lasttLeftBracketIndex直到最后一个
                    if (tempChar == "(") lasttLeftBracketIndex = i;
                }
                //找到与最后第一个左括号对应的右括号
                for (int i = lasttLeftBracketIndex; i < expression.Length; i++)
                {
                    //获取字符串中的第i个字符
                    string tempChar = expression.Substring(i, 1);
                    if (tempChar == ")" && firstRightBracketIndex == -1) firstRightBracketIndex = i;
                }

                string logicExpression = expression.Substring(lasttLeftBracketIndex + 1, firstRightBracketIndex - lasttLeftBracketIndex - 1);
                bool result = LogicOperate(logicExpression);
                expression = expression.Replace("(" + logicExpression + ")", result ? "1" : "0");
            }
            return LogicOperate(expression);
        }

        /// <summary>
        /// 逻辑运算
        /// </summary>
        private static bool LogicOperate(string expression)
        {
            //去除空格
            string temp = expression.Replace(" ", "");

            //获取所有的条件的值
            string[] arrLogicValue = temp.Replace("&&", ",").Replace("||", ",").Split(',');

            //获取表达式的逻辑运算符
            string logicExpressionOperator = temp.Replace("0", ",").Replace("1", ",").Remove(0, 1);
            string[] arrOperator = logicExpressionOperator.Remove(logicExpressionOperator.Length - 1, 1).Split(',');

            //最终运算结果
            bool result = arrLogicValue[0] == "1";

            //更具逻辑运算符的数量通过循环进行运算
            for (int i = 0; i < arrOperator.Length; i++)
            {
                if (arrOperator[i] == "&&")
                {
                    result = result && arrLogicValue[i + 1] == "1";
                }
                else if (arrOperator[i] == "||")
                {
                    result = result || arrLogicValue[i + 1] == "1";
                }
            }
            return result;
        }
    }
}
