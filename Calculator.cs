
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

using System.Collections;
using System;

namespace ExpressionLib
{
	// Calculates properties.
	public class Calculator : VariableSet, ICalculator, IOperatorManager
	{
		public const char PROPERTY_CHAR = '.';
		public const string NAME_REGEX = "^[a-zA-Z]+[0-9]*$";
		private Hashtable m_expressions = new Hashtable(100);
		private FunctionSet m_functions = new FunctionSet();
		private string m_up = "$";
		private ExpressionExceptionCollection m_exceptions = new ExpressionExceptionCollection();
		//Event is raised when variables should be refreshed in gui.
		public event RefreshVariablesEventHandler RefreshVariables;
		public delegate void RefreshVariablesEventHandler(object sender, EventArgs evt);
		private OperatorCollection m_uniOperators = new OperatorCollection();
		private OperatorCollection m_biOperators = new OperatorCollection();

		public bool IsExpressionFree(string key)
		{
			if (this.GetExpression(key) != null)
			{
				return false;
			}
			string varname = VariableOfKey(key);
			if (varname == null) throw new Exception("Variable does not have property");
			string propname = PropertyOfKey(key);
			if (propname == null) throw new Exception("Property does not exist in the key");
			IVariable var = this[varname];
			if (var == null) throw new Exception("Variable is null");
			if (!(var is IOptimizable)) return true;
			IOptimizable optimizable = var as IOptimizable;
			if (optimizable.PropertyDependices == null) return true;
			foreach(PropertyDependicy dependicy in optimizable.PropertyDependices)
			{
				// Check each dependicy and see if an expression is linked to one of them.
				if (dependicy.ChildProperty == propname &&
				    this.GetExpression(varname + PROPERTY_CHAR + dependicy.ParentProperty) != null)
					return false;
			}
			return true;
		}
		
		public static string PropertyOfKey(string key)
		{
			int propIndex = key.IndexOf(PROPERTY_CHAR);
			if (propIndex == -1)
				return null;
			return key.Substring(propIndex + 1);
		}

		public static string VariableOfKey(string key)
		{
			int propIndex = key.IndexOf(PROPERTY_CHAR);
			if (propIndex == -1)
				return null;
			return key.Substring(0, propIndex);
		}

		//
		// If true, running expressions may change the variable properties.
		//
		private bool m_runExpressions = false;
		public bool RunExpressions
		{
			get
			{return m_runExpressions;}
			set
			{m_runExpressions = value;}
		}

		public Calculator()
		{
			
			this.m_uniOperators.Add(new ExpressionOperator("-", "Subtract", 1));
			this.m_uniOperators.Add(new ExpressionOperator("!", "Not", 2));
		
			m_biOperators.Add(new ExpressionOperator("^", "Power", 3));
			m_biOperators.Add(new ExpressionOperator("*", "Multiply", 2));
			m_biOperators.Add(new ExpressionOperator("/", "Divide", 2));
			m_biOperators.Add(new ExpressionOperator("-", "Subtract", 1));
			m_biOperators.Add(new ExpressionOperator("+", "Add", 1));
			m_biOperators.Add(new ExpressionOperator("%", "Mod", 1));
			m_biOperators.Add(new ExpressionOperator("<", "Less", 0));
			m_biOperators.Add(new ExpressionOperator(">", "More", 0));
			m_biOperators.Add(new ExpressionOperator("=", "Equal", 0));
			m_biOperators.Add(new ExpressionOperator("<>", "NotEqual", 0));
			m_biOperators.Add(new ExpressionOperator("!=", "NotEqual", 0));
			m_biOperators.Add(new ExpressionOperator("<=", "LessOrEqual", 0));
			m_biOperators.Add(new ExpressionOperator(">=", "MoreOrEqual", 0));
			m_biOperators.Add(new ExpressionOperator("==", "Equal", 0));
			m_biOperators.Add(new ExpressionOperator("&&", "And", -1));
			m_biOperators.Add(new ExpressionOperator("||", "Or", -2));
		
		}
		//Gets the exceptions collected under last run.
		public ExpressionExceptionCollection Exceptions 
		{
			get 
			{
				return m_exceptions;
			}
		}
		//The string that tells us to go up one level.
		public string Up 
		{
			get 
			{
				return m_up;
			}
			set 
			{
				m_up = value;
			}
		}
		public FunctionSet Functions 
		{
			get 
			{
				return m_functions;
			}
			set 
			{
				m_functions = value;
			}
		}

		//Gets or sets the collection of uneari operators.
		public OperatorCollection UniOperators 
		{
			get 
			{
				return m_uniOperators;
			}
			set 
			{
				m_uniOperators = value;
			}
		}
		//Gets or sets the binear operators.
		public OperatorCollection BiOperators 
		{
			get 
			{
				return m_biOperators;
			}
			set 
			{
				m_biOperators = value;
			}
		}
		//Returns true whether a name is a valid variable name.
		public bool IsValidName(string name)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(name, NAME_REGEX);
		}
	
		public object ComputeVariable(string varName)
		{
			return ComputeVariable(varName, null);
		}
	
		public object ComputeVariable(string varName, IVariable relative) 
		{
			if (relative != null)
			{
				if (varName.StartsWith(PROPERTY_CHAR.ToString()))
				{
					//Use property directly.
					varName = relative.Name + varName;
				}
			}
			if (m_expressions.Contains(varName))
			{
				//Return calculated variable.
				Expression expr = (Expression)m_expressions[varName];
				return expr.Calc(this);
			}
			else
			{
				int propertyIndex = varName.LastIndexOf(PROPERTY_CHAR);
				if (propertyIndex == -1)
				{
					//No property.
					IVariable var = base.Variable(varName);
					if (var == null)
					{
						throw new VariableDoesNotExistsException("Variable '" + varName + "' does not exist");
					}
					else
					{
						return var;
					}
				}
				else
				{
					string prop = varName.Substring(propertyIndex + 1);
					string propVar = varName.Substring(0, propertyIndex);
					//Return property value.
					object obj;
					obj = base.Variable(propVar);
					if (obj == null)
					{
						throw new VariableDoesNotExistsException("Variable '" + varName + "' does not exist");
					}
					else
					{
						return obj.GetType().InvokeMember(prop, System.Reflection.BindingFlags.GetProperty, null, obj, new object[] {});
					}
				}
			}
		}
	
		public object Compute(string expression)
		{
			return Compute(expression, null);
		}
	
		//Computes an expression.
		//Not relative to anything.
		public object Compute(string expression, IVariable relative)
		{
			ExpressionTreeNode n = ExpressionTreeNode.Parse(expression, this);
			return n.Calc(this, relative, null);
		}
		//Add a new expression.
		public void AddExpression(string key, string expression)
		{
			Expression expr = new Expression();
			expr.SetExpression(key, expression, this);
			this.m_expressions.Add(key, expr);
		}
		//Remove expression.
		public void RemoveExpression(string key)
		{
			this.m_expressions.Remove(key);
		}
		//Check if expression exists.
		public bool ContainsExpression(string key)
		{
			return m_expressions.Contains(key);
		}
		//Gets the expression by key.
		public string GetExpression(string key)
		{
			Expression exp = (Expression)m_expressions[key];
			if (exp == null)
			{
				//Expression doesn't exist in calculator. Return nothing.
				return null;
			}
			else
			{
				return exp.Formula;
			}
		}
		//Gets the expression object of key.
		public Expression GetExpressionObject(string key)
		{
			Expression exp = (Expression)m_expressions[key];
			return exp;
		}
		//Get a table with all expressions.
		public Expression[] GetExpressions()
		{
			Expression[] expressions = new Expression[m_expressions.Count];
			m_expressions.Values.CopyTo(expressions, 0);
			return expressions;
		}
		//Clear everything.
		public override void Clear()
		{
			base.Clear();
			this.m_expressions.Clear();
		}
		//Reset all expression buffers.
		public void ResetBuffer()
		{
			ICollection expressions = m_expressions.Values;
			foreach (Expression expr in expressions) 
			{
				expr.ResetBuffer();
			}
		}
		//Computes the value and puts the result to property.
		public void Assign(string key, string expression)
		{
			string name = key.Substring(0, key.LastIndexOf(PROPERTY_CHAR));
			string prop = key.Substring(key.LastIndexOf(PROPERTY_CHAR) + 1);
			IVariable var = base.Variable(name);
			try 
			{
				System.Reflection.PropertyInfo propertyInfo = var.GetType().GetProperty(prop);
				Type type = propertyInfo.PropertyType;
				object calcValue = this.Compute(expression, var);
				if (object.ReferenceEquals(type, typeof(float)))
				{
					calcValue = Convert.ToSingle(calcValue);
				}
				else if (object.ReferenceEquals(type, typeof(int))) 
				{
					calcValue = Convert.ToInt32(calcValue);
				}
				else if (!object.ReferenceEquals(type, typeof(string)) && calcValue is string) 
				{
					calcValue = Convert.ToString(calcValue);
				}
				var.GetType().InvokeMember(prop, System.Reflection.BindingFlags.SetProperty, null, var, new object[] {calcValue});
			}
			catch (Exception ex) 
			{
				//Collection exceptions.
				if (ex is ExpressionException)
				{
					m_exceptions.AddException((ExpressionException)ex);
				}
				else if (ex is MissingMemberException) 
				{
					//Incompatible datatype between property and expression.
					ExpressionException expressionException = new ExpressionException("Expression result is not compatible with property", 0, expression.Length);
					Expression exp = new Expression();
					exp.SetExpression(key, expression, this);
					expressionException.Expression = exp;
					m_exceptions.AddException(expressionException);
				}
			}
		}

		public void AssignValue(string key, object val)
		{
			string name = key.Substring(0, key.LastIndexOf(PROPERTY_CHAR));
			string prop = key.Substring(key.LastIndexOf(PROPERTY_CHAR) + 1);
			IVariable var = base.Variable(name);

			System.Reflection.PropertyInfo propertyInfo = var.GetType().GetProperty(prop);
			Type type = propertyInfo.PropertyType;
			object calcValue = val;
			if (object.ReferenceEquals(type, typeof(float)))
			{
				calcValue = Convert.ToSingle(calcValue);
			}
			else if (object.ReferenceEquals(type, typeof(int))) 
			{
				calcValue = Convert.ToInt32(calcValue);
			}
			else if (!object.ReferenceEquals(type, typeof(string)) && calcValue is string) 
			{
				calcValue = Convert.ToString(calcValue);
			}
			var.GetType().InvokeMember(prop, System.Reflection.BindingFlags.SetProperty, null, var, new object[] {calcValue});
		}

		// Finds whether a expression is recursive.
		public bool IsRecursive(string key, string expression)
		{
			ExpressionTreeNode expr = ExpressionTreeNode.Parse(expression, this);
			Stack st = new Stack();
			st.Push(expr);
			while (st.Count > 0) 
			{
				expr = (ExpressionTreeNode)st.Pop();
				
				if (expr.SubNodes != null) 
				{
					foreach (ExpressionTreeNode subNode in expr.SubNodes) 
					{
						st.Push(subNode);
					}
				}
				else if (expr.NodeType == ExpressionTreeNode.NODE_TYPE.VARIABLE) 
				{
					if (this.ContainsExpression((string)expr.Value))
					{
						st.Push(this.GetExpressionObject((string)expr.Value).Tree);
					}
				}
				if (expr.NodeType == ExpressionTreeNode.NODE_TYPE.VARIABLE && (string)expr.Value == key)
				{
					return true;
				}
			}
			return false;
		}
		//Computes all values.
		public void ComputeExpressions(ICollection expressions)
		{
			if (expressions == null)
			{
				expressions = m_expressions.Values;
			}
			IVariable var;
			object[] param = new object[1];
			foreach (Expression expr in expressions) 
			{
				//It was a property.
				string name = expr.Key.Substring(0, expr.Key.LastIndexOf(PROPERTY_CHAR));
				string prop = expr.Key.Substring(expr.Key.LastIndexOf(PROPERTY_CHAR) + 1);
				var = base.Variable(name);
				try 
				{
					System.Reflection.PropertyInfo propertyInfo = var.GetType().GetProperty(prop);
					Type type = propertyInfo.PropertyType;
					// This code need to be synchronized with ExpressionValidationHelper.ConvertValue.
					object calcValue = ComputeVariable(expr.Key, var);
					if (object.ReferenceEquals(type, typeof(float)))
					{
						calcValue = Convert.ToSingle(calcValue);
					}
					else if (object.ReferenceEquals(type, typeof(int))) 
					{
						calcValue = Convert.ToInt32(calcValue);
					}
					else if (object.ReferenceEquals(type, typeof(bool))) 
					{
						calcValue = Convert.ToBoolean(calcValue);
					}
					/*
					 * 01.01.2012 If this code is not required, remove it.
					else if (!object.ReferenceEquals(type, typeof(string)) && calcValue is string) 
					{
						calcValue = Convert.ToString(calcValue);
					}
					*/
					else if (object.ReferenceEquals(type, typeof(string)))
					{
						calcValue = Convert.ToString(calcValue);
					}
					param[0] = calcValue;
					var.GetType().InvokeMember(prop, System.Reflection.BindingFlags.SetProperty, null, var, param);
				}
				catch (Exception ex) 
				{
					//Collection exceptions.
					if (ex is ExpressionException)
					{
						m_exceptions.AddException((ExpressionException)ex);
					}
					else if (ex is MissingMemberException) 
					{
						//Incompatible datatype between property and expression.
						ExpressionException expressionException = new ExpressionException("Expression result is not compatible with property", 0, expr.Formula.Length);
						expressionException.Expression = expr;
						m_exceptions.AddException(expressionException);
					}
				}
			}
		}
		public void ProcessRefreshVariables()
		{
			if (RefreshVariables != null) 
			{
				RefreshVariables(this, new EventArgs());
			}
		}
	}
}
