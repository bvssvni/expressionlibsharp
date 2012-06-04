
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

namespace ExpressionLib
{
	public class OperatorCollection
	{
		private Hashtable m_operators = new Hashtable(50);
		public void Add(ExpressionOperator op)
		{
			m_operators[op.Identifier] = op;
		}
		public void Remove(ExpressionOperator op)
		{
			m_operators.Remove(op.FunctionName);
		}
		public void Clear(ExpressionOperator op)
		{
			m_operators.Clear();
		}
		//Gets or sets the operator.
		public ExpressionOperator this[string op] 
		{
			get 
			{
				return (ExpressionOperator)m_operators[op];
			}
			set 
			{
				m_operators[op] = value;
			}
		}
		//Gets the function name of operator.
		public string FunctionName(string op)
		{
			return ((ExpressionOperator)m_operators[op]).FunctionName;
		}
		public int Presedence(string op)
		{
			return ((ExpressionOperator)m_operators[op]).Presedence;
		}
		public ExpressionOperator[] GetOperators()
		{
			ExpressionOperator[] ops = new ExpressionOperator[m_operators.Count];
			int i = 0;
			foreach (ExpressionOperator op in m_operators.Values) 
			{
				ops[i] = op;
				i += 1;
			}
			return ops;
		}
	}
}