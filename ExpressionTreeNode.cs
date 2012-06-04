/*
 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions are met: 
 
 1. Redistributions of source code must retain the above copyright notice, this
 list of conditions and the following disclaimer. 
 2. Redistributions in binary form must reproduce the above copyright notice,
 this list of conditions and the following disclaimer in the documentation
 and/or other materials provided with the distribution. 
 
 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
 The views and conclusions contained in the software and documentation are those
 of the authors and should not be interpreted as representing official policies, 
 either expressed or implied, of the FreeBSD Project.
 */

using System;
using System.Collections;

namespace ExpressionLib
{
	[System.ComponentModel.Description("Analyses and calculates expressions.")]
	public class ExpressionTreeNode
	{
		private IOperatorManager m_operatorManager;
//		private ExpressionTreeNode m_left;
//		private ExpressionTreeNode m_right;
		private ExpressionTreeNode[] m_subNodes;
		// (19.01.2007 15:21)
		// Use object for direct casting numbers.
		private object m_value;
		private NODE_TYPE m_type;
		//Which column node got in expression.
		private int m_column;
		//The length of expression.
		private int m_length;
		private IFunction m_function;
		private SubExpressionUpdate m_subExpressionUpdate = null;

		public const int MAX_DEPTH = 20;
		public enum NODE_TYPE : int
		{
			NUMBER,
			VARIABLE,
			FUNCTION,
			OPERATOR,
			TEXT
		}
		//Gets the type of node.
		public NODE_TYPE NodeType 
		{
			get 
			{
				return m_type;
			}
		}
		//Gets the value of node.
		public object Value 
		{
			get 
			{
				return m_value;
			}
			set 
			{
				m_value = value;
			}
		}

		//Gets the sub nodes of expression tree node.
		public ExpressionTreeNode[] SubNodes 
		{
			get 
			{
				return m_subNodes;
			}
		}
		//Analyses a cell and creates an expression tree by it.
		//Used internal to avoid circular references.
		public static ExpressionTreeNode Parse(string value, IOperatorManager operatorManager)
		{
			return new ExpressionTreeNode(value, 0, operatorManager);
		}
		//Calculates the value by expression tree.
		//Calculator - The object that contains functions, variables and expressions.
		//Relative - The relative variable.
		public object Calc(ICalculator calculator, IVariable relative, IFunction callFunction)
		{
			try 
			{
				if (m_type == NODE_TYPE.FUNCTION || m_type == NODE_TYPE.OPERATOR)
				{
					//Run function.
					IFunction func;
					//(14.11.2006 11:50)
					//Point directly to the function to increase speed.
					if (m_function != null)
					{
						func = m_function;
					}
					else
					{
						string val = (string)m_value;
						FunctionSet functions = calculator.Functions;
						if (functions.Contains(val))
						{
							func = functions.Item(val);
							m_function = func;
						}
						else
						{
							throw new Exception("Unknown function :" + val);
						}
					}

					if (func is IIntelligenceFunction)
					{
						return ((IIntelligenceFunction)func).Run(m_subNodes, calculator, relative);
					}

					object[] @params = new object[this.m_subNodes.Length];
					//The argument position.
					int i = 0;
					while (i < @params.Length) 
					{
						object obj = m_subNodes[i].Calc(calculator, relative, func);
						@params[i] = obj;
						i += 1;
					}

					return func.Run(@params);
				}
				else if (m_type == NODE_TYPE.TEXT)
					return m_value;
				else if (m_type == NODE_TYPE.NUMBER)
					return (float)m_value;
				else if (m_type == NODE_TYPE.VARIABLE)
				{
					string val = (string)m_value;
					int propIndex = val.IndexOf('.');
					
					#region "Sub Expression Updating"
					// Update sub expressions before calculating the value to make sure
					// the computation will be correct.
					if (calculator.RunExpressions && relative != null)
					{
						if (this.m_subExpressionUpdate == null)
							this.m_subExpressionUpdate = new SubExpressionUpdate(calculator, callFunction, val, propIndex, relative);
						this.m_subExpressionUpdate.UpdateSubExpressions(calculator);
						if (propIndex == 0)
							return relative.GetType().InvokeMember(val.Substring(propIndex + 1), System.Reflection.BindingFlags.GetProperty, null, relative, new object[] {});
						else
							return calculator.ComputeVariable(val, null);
					}
					#endregion

					// Don't use relative if there is none.
					if (relative == null || propIndex != 0 && (propIndex == -1 || val.Substring(0, propIndex) != relative.Name))
					{
						return calculator.ComputeVariable(val, relative);
					}
					// Get value directly from variable if relative is the variable.
					return relative.GetType().InvokeMember(val.Substring(propIndex + 1), System.Reflection.BindingFlags.GetProperty, null, relative, new object[] {});
				}
				
			}
			catch (Exception ex) 
			{
				if (!(ex is ExpressionException))
				{
					throw new ExpressionException(ex.Message, m_column, m_length);
				}
				else
				{
					throw ex;
				}
			}
			return null;
		}
		
		private static System.Text.RegularExpressions.Regex regText = new System.Text.RegularExpressions.Regex("^\"([^\"]|(\"\"))*\"$", System.Text.RegularExpressions.RegexOptions.Singleline);
		
		//Finds whether value is text.
		public static bool IsText(string value)
		{
			return regText.Match(value).Success;
		}
		public static string GetText(string value)
		{
			value = value.Substring(1, value.Length - 2);
			value = value.Replace("\"\"", "\"");
			return value;
		}
		
		private static System.Text.RegularExpressions.Regex regFunc = new System.Text.RegularExpressions.Regex("^[^;]+\\(.*\\)$", System.Text.RegularExpressions.RegexOptions.Singleline);
		
		//Finds whether value is function.
		public static bool IsFunction(string @value)
		{
			bool functionReturnValue = false;
			if (regFunc.Match(@value).Success)
			{
				int countP = 0;
				int pos = @value.IndexOf('(');
				int endPos = 0;
				for (int i = pos; i <= @value.Length - 1; i++) 
				{
					char ch = @value[i];
					if (ch == '(')
					{
						countP += 1;
					}
					else if (ch == ')') 
					{
						countP -= 1;
						if (countP == 0)
						{
							endPos = i;
							break; // TODO: might not be correct. Was : Exit For
						}
					}
				}
				return !(endPos == 0 || endPos < value.Length - 1);
			}
			return functionReturnValue;
		}
		public static OperatorMatch FindOperators(string text, IOperatorManager operatorManager)
		{
			char c;
			int depth = 0;
			OperatorMatch topOperator = null;
			string op = "";
			ArrayList matches = new ArrayList();
			for (int i = 0; i <= text.Length - 1; i++) 
			{
				c = text[i];
				if (c == '(')
				{
					depth += 1;
				}
				else if (c == ')') 
				{
					depth -= 1;
				}
				if (depth == 0 && c != ';' && !char.IsLetter(c) && !char.IsNumber(c) && !char.IsWhiteSpace(c) && c != '(' && c != ')' && c != '.')
				{
					//Found beginning of operator.
					op += c;
				}
				else if (op != "") 
				{
					matches.Add(new OperatorMatch(i - op.Length, op.Length));
					op = "";
				}
			}
			if (matches.Count > 0)
			{
				//Find the operators.
				int endIndex = 0;
				OperatorMatch firstMatch = (OperatorMatch)matches[0];
				if (firstMatch.StartIndex == 0)
				{
					op = text.Substring(firstMatch.StartIndex, firstMatch.Length);
					firstMatch.ExpressionOperator = operatorManager.UniOperators[op];
					firstMatch.Unary = true;
					endIndex = 1;
				}
				for (int i = matches.Count - 1; i >= endIndex; i += -1) 
				{
					OperatorMatch match = (OperatorMatch)matches[i];
					op = text.Substring(match.StartIndex, match.Length);
					match.ExpressionOperator = operatorManager.BiOperators[op];
					if (match.ExpressionOperator == null)
					{
						//Could be two operators beside each other.
						//Check for unary operator.
						ExpressionOperator[] ops = operatorManager.UniOperators.GetOperators();
						foreach (ExpressionOperator uniOperator in ops) 
						{
							if (op.EndsWith(uniOperator.Identifier))
							{
								match.ExpressionOperator = operatorManager.BiOperators[op.Substring(0, op.Length - uniOperator.Identifier.Length)];
								match.Length -= uniOperator.Identifier.Length;
								if (match.ExpressionOperator != null)
								{
									break;
								}
							}
						}
					}
					if (match.ExpressionOperator != null)
					{
						if (topOperator == null || topOperator.ExpressionOperator.Presedence > match.ExpressionOperator.Presedence)
						{
							topOperator = match;
						}
					}
				}
				if (topOperator == null)
				{
					//Use unary operator if no other matches.
					topOperator = firstMatch;
				}
				if (topOperator.ExpressionOperator == null)
				{
					return null;
				}
			}
			return topOperator;
		}
		private ExpressionTreeNode(string @value, int column, IOperatorManager operatorManager)
		{
			m_operatorManager = operatorManager;
			//Remove whitespace.
			int whiteSpace = CountWhiteSpace(@value);
			m_column = column + whiteSpace;
			int oldLength = @value.Length;
			@value = @value.Trim();
			m_length = @value.Length;
			SetValue(@value);
		}
		//Counts whitespace at beginning of text.
		private int CountWhiteSpace(string text)
		{
			char c;
			int count = 0;
			for (int i = 0; i <= text.Length - 1; i++) 
			{
				c = text[i];
				if (char.IsWhiteSpace(c))
				{
					count += 1;
				}
				else
				{
					return count;
				}
			}
			return count;
		}
		
		public static bool IsNumeric(object text)
		{
			if (text is int || text is float || text is double || 
				text is Decimal ||
				text is byte || text is short || text is long)
				return true;
			if (!(text is string)) return false;
			string str = (string)text;
			if (str.Length == 0) return false;
			if (char.IsNumber(str[0]))
				return true;
			else
				return false;
		}
		
		public void SetValue(string value)
		{
			//(04.07.2006 10:32)
			//Check for text first.
			if (IsText(value))
			{
				this.m_value = GetText(value);
				this.m_type = NODE_TYPE.TEXT;
				return;
			}
			//Check for operators first.
			OperatorMatch opMatch = FindOperators(value, m_operatorManager);
			if (opMatch != null)
			{
				m_type = NODE_TYPE.OPERATOR;
				string leftParam = value.Substring(0, opMatch.StartIndex);
				string rightParam = value.Substring(opMatch.StartIndex + opMatch.Length);
				if (leftParam != "")
					m_subNodes = new ExpressionTreeNode[]{new ExpressionTreeNode(leftParam, m_column, m_operatorManager),
						new ExpressionTreeNode(rightParam, m_column, m_operatorManager)};
				else
					m_subNodes = new ExpressionTreeNode[]{new ExpressionTreeNode(rightParam, m_column, m_operatorManager)};
				m_value = opMatch.ExpressionOperator.FunctionName;
			}
			else if (value.IndexOf("(") != -1) 
			{
				int paranStart = value.IndexOf('(');
				string functionName = value.Substring(0, paranStart);
				if (value.Length - paranStart - 2 < 0)
				{
					throw new ExpressionException("Missing ')'", m_column, value.Length);
				}
				string param = value.Substring(paranStart + 1, value.Length - paranStart - 2);
				if (functionName == "")
				{
					m_column += 1;
					SetValue(param);
					return;
				}
				if (param != "")
				{
					ArrayList @params = new ArrayList();
					int col = m_column + paranStart + 1;
					int depth = 0;
					string par = "";
					//(27.07.2006 16:24)
					//Detects whether we are inside a text or not.
					bool insideText = false;
					for (int i = 0; i <= param.Length - 1; i++) 
					{
						char c = param[i];
						if (c == '(')
						{
							depth += 1;
						}
						else if (c == ')') 
						{
							depth -= 1;
						}
						else if (c == '"') 
						{
							insideText = !insideText;
						}
						if (depth == 0 && c == ';' && !insideText)
						{
							@params.Add(par);
							par = "";
						}
						else
						{
							par += c;
						}
					}
					if (par != "")
					{
						@params.Add(par);
					}
				
					m_subNodes = new ExpressionTreeNode[@params.Count];
					for (int i = 0; i < @params.Count; i++) 
					{
						m_subNodes[i] = new ExpressionTreeNode((string)@params[i], col, m_operatorManager);
						col += ((string)@params[i]).Length + 1;
					}
					// (23.02.2007 15:15)
					if (value.IndexOf(')') == -1 || depth > 0)
						throw new ExpressionException("Missing ')'", m_column, value.Length);
					else if (depth < 0)
						throw new ExpressionException("One ')' to much", m_column, value.Length);
				}
				else
				{
					m_subNodes = new ExpressionTreeNode[] {};
				}
				m_value = functionName;
				m_type = NODE_TYPE.FUNCTION;
			}
			else
			{
				//Number or post data.
				m_value = value.Trim();
				if (IsNumeric(m_value))
				{
					m_type = NODE_TYPE.NUMBER;
					m_value = float.Parse((string)m_value);
				}
				else
				{
					m_type = NODE_TYPE.VARIABLE;
				}
			}
		}
	}
}