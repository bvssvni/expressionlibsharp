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

namespace ExpressionLib
{

	public class Expression
	{
		private string m_key;
		private string m_expression;
		private object m_buffer;
		private ExpressionTreeNode m_tree;
		private bool m_bufferSet;
		private IVariable m_variable;
		private string m_propName;
		//Key of expression.
		public string Key 
		{
			get 
			{
				return m_key;
			}
		}
		public ExpressionTreeNode Tree 
		{
			get 
			{
				return m_tree;
			}
		}
		public string Formula 
		{
			get 
			{
				return m_expression;
			}
		}
		public IVariable Variable 
		{
			get 
			{
				return m_variable;
			}
		}
		public string PropName 
		{
			get 
			{
				return m_propName;
			}
		}
		public void SetExpression(string key, string value, Calculator calc)
		{
			m_key = key;
			m_expression = value;
			if (calc != null)
			{
				m_tree = ExpressionTreeNode.Parse(m_expression, calc);
			}
			m_bufferSet = false;
			//Find variable on forehand to make calculation faster.
			int propIndex = m_key.LastIndexOf(Calculator.PROPERTY_CHAR);
			string varName;
			if (propIndex != -1)
			{
				varName = m_key.Substring(0, propIndex);
			}
			else
			{
				varName = m_key;
			}
			if (calc != null)
			{
				m_variable = calc.Variable(varName);
				if (m_variable == null)
					throw new System.Exception("Invalid variable key: " + key);
			}
			m_propName = m_key.Substring(propIndex + 1);
		}
		//Calculate value of expression.
		public object Calc(ICalculator calculator) 
		{
			if (!m_bufferSet)
			{
				try 
				{
					//Calculate value.
					m_buffer = m_tree.Calc(calculator, m_variable, null);
					m_bufferSet = true;
				}
				catch (ExpressionException ex) 
				{
					if (ex.Expression == null)
					{
						//Tell which expression error happened in.
						ex.Expression = this;
					}
					throw ex;
				}
			}
			return m_buffer;
		}
		//Reset buffer.
		public void ResetBuffer()
		{
			this.m_bufferSet = false;
		}
	}
}
