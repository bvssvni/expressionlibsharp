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
	//Contains exceptions.
	public class ExpressionExceptionCollection : ICollection
	{
		private ArrayList m_exceptions = new ArrayList();
		public void AddException(ExpressionException ex)
		{
			m_exceptions.Add(ex);
		}
		public void Clear()
		{
			m_exceptions.Clear();
		}

		public ExpressionExceptionCollection Copy()
		{
			ExpressionExceptionCollection newCollection = new ExpressionExceptionCollection();
			newCollection.m_exceptions = (ArrayList)m_exceptions.Clone();
			return newCollection;
		}

		#region "ICollection stuff"
		public void CopyTo(System.Array array, int index)
		{
			m_exceptions.CopyTo(array, index);
		}
		public int Count 
		{
			get 
			{
				return m_exceptions.Count;
			}
		}
		public bool IsSynchronized 
		{
			get 
			{
				return m_exceptions.IsSynchronized;
			}
		}
		public object SyncRoot 
		{
			get 
			{
				return m_exceptions.SyncRoot;
			}
		}
		public System.Collections.IEnumerator GetEnumerator()
		{
			return m_exceptions.GetEnumerator();
		}
		#endregion
	}
}