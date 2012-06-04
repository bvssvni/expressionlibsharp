
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
	public class DependicyInfo
	{
		private string m_key;
		private string[] m_deps;
		private int m_type;
		
		public const int DEPENDICY_TYPE_VARIABLE = 0;
		public const int DEPENDICY_TYPE_PROPERTY = 1;
		public const int DEPENDICY_TYPE_EXPRESSION = 2;
		public const int DEPENDICY_TYPE_NOT_LEGAL = 3;
		
		//Gets or sets the dependices.
		public string[] Deps 
		{
			get 
			{
				return m_deps;
			}
			set 
			{
				m_deps = value;
			}
		}
		//Gets or sets the key of dependicy.
		public string Key 
		{
			get 
			{
				return m_key;
			}
			set 
			{
				m_key = value;
			}
		}
		//Gets or sets the type of dependicy.
		//VARIABLE - Dependicy is a reference to a variable.
		//PROPERTY - Dependicy is a reference to a property of variable.
		//SUB_PROPERTY - Dependicy is a reference to a property of property of variable.
		//EXPRESSION - Dependicy is a reference to another expression.
		//NOT_LEGAL - Dependicy reference is not legal.
		public int Type 
		{
			get 
			{
				return m_type;
			}
			set 
			{
				m_type = value;
			}
		}
	}
}
