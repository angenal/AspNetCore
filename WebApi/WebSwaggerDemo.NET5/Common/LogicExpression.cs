using System;
using System.Linq;

namespace WebSwaggerDemo.NET5.Common
{
    public class LogicExpression
    {
        /// <summary>
        /// 分析计算表达式
        /// </summary>
        /// <param name="expression">表达式如: (A || B)</param>
        /// <param name="data">数据如: [A,B,C]</param>
        /// <returns></returns>
        public static bool Eval(string expression, string[] data)
        {
            //去除空格括号等
            string temp = expression.Replace(" ", "").Replace("(", "").Replace(")", "");

            //将逻辑运算符用逗号替换
            string variable = temp.Replace("&&", ",").Replace("||", ",");

            //将逻辑表达式中的所有条件名称取出
            string[] variables = variable.Split(',');

            //条件名称替换成其对应的bool值
            for (int i = 0; i < variables.Length; i++)
            {
                string item = variables[i];
                if (item.StartsWith("!"))
                {
                    string str = item.Substring(1);
                    bool value = data.Any(v => v.Equals(str, StringComparison.OrdinalIgnoreCase));
                    expression = expression.Replace(item, (!value).ToString());
                }
                else
                {
                    bool value = data.Any(v => v.Equals(item, StringComparison.OrdinalIgnoreCase));
                    expression = expression.Replace(item, value.ToString());
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
                bool logicResult = LogicOperate(logicExpression);
                expression = expression.Replace("(" + logicExpression + ")", logicResult.ToString());
            }
            return LogicOperate(expression);
        }

        /// <summary>
        /// 逻辑运算
        /// </summary>
        private static bool LogicOperate(string expression)
        {
            //获取所有的条件的bool值
            string[] arrLogicValue = expression.Split(',');

            //获取表达式的逻辑运算符
            string logicExpressionOperator = expression
                .Replace("True", ",", StringComparison.OrdinalIgnoreCase)
                .Replace("False", ",", StringComparison.OrdinalIgnoreCase)
                .Remove(0, 1);
            logicExpressionOperator = logicExpressionOperator.Remove(logicExpressionOperator.Length - 1, 1);
            string[] arrOperator = logicExpressionOperator.Split(',');

            //最终运算结果
            bool result = Convert.ToBoolean(arrLogicValue[0]);

            //更具逻辑运算符的数量通过循环进行运算(不包含!)
            for (int i = 0; i < arrOperator.Length; i++)
            {
                if (arrOperator[i] == "&&")
                {
                    result = result && Convert.ToBoolean(arrLogicValue[i + 1]);
                }
                else if (arrOperator[i] == "||")
                {
                    result = result || Convert.ToBoolean(arrLogicValue[i + 1]);
                }
            }
            return result;
        }
    }
}
