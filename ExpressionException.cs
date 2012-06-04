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

namespace ExpressionLib
{
	//Used to tell where in expression exception occured.
	public class ExpressionException : Exception
	{
		private int m_column;
		private int m_length;
		private IVariable m_variable;
		private Expression m_expression;
		public int Column 
		{
			get 
			{
				return m_column;
			}
		}
		public int Length 
		{
			get 
			{
				return m_length;
			}
		}
		public IVariable Variable 
		{
			get 
			{
				return m_variable;
			}
		}
		//The expression error happened in.
		public Expression Expression 
		{
			get 
			{
				return m_expression;
			}
			set 
			{
				m_expression = value;
			}
		}
	
		public ExpressionException(string message, int column, int length) : base(message)
		{
			m_column = column;
			m_length = length;	
			m_variable = null;
		}
		public ExpressionException(string message, int column, int length, IVariable variable) : base(message)
		{
			m_column = column;
			m_length = length;
			m_variable = variable;
		}
		public override string ToString()
		{
			if (this.m_expression == null)
			{
				return Message;
			}
			else
			{
				return this.m_expression.Key + " (" + this.m_column + "," + this.m_length + "): " + this.Message;
			}
		}
	}
}