
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
	public class OperatorMatch
	{
		private int m_startIndex;
		private int m_length;
		private ExpressionOperator m_operator;
		private bool m_unary = false;
		public int StartIndex 
		{
			get 
			{
				return m_startIndex;
			}
			set 
			{
				m_startIndex = value;
			}
		}
		public int Length 
		{
			get 
			{
				return m_length;
			}
			set 
			{
				m_length = value;
			}
		}
		public ExpressionOperator ExpressionOperator 
		{
			get 
			{
				return m_operator;
			}
			set 
			{
				m_operator = value;
			}
		}
		public bool Unary 
		{
			get 
			{
				return m_unary;
			}
			set 
			{
				m_unary = value;
			}
		}
		public OperatorMatch(int startIndex, int length)
		{
			m_startIndex = startIndex;
			m_length = length;
		}
	}
}
