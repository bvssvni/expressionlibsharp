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

//Contains a group of functions.
//Used by ExpressionTree to execute and validate expressions.
using System.Collections;

namespace ExpressionLib
{
	public class FunctionSet : ICollection
	{
		private SortedList m_functions = new SortedList();
		
		// There is no way to toggle case sensitivity for static functions
		// in a sensible way, therefore this option should be removed and
		// functions should be case sensitive.
		// Before doing this, check if anything has case insensitivity.
		private bool m_caseSensitive = false;
		
		private static SortedList s_sharedFunctions = new SortedList();
		public bool CaseSensitive 
		{
			get 
			{
				return m_caseSensitive;
			}
			set 
			{
				m_caseSensitive = value;
			}
		}
		//Gets the item.
		public IFunction Item(string name)
		{
			// Check first with shared function.
			// If it contains the name, return it.
			if (s_sharedFunctions.Contains(name))
				return (IFunction)s_sharedFunctions[name];
			
			if (m_caseSensitive)
			{
				return (IFunction)m_functions[name];
			}
			else
			{
				return (IFunction)m_functions[name.ToLower()];
			}
		}
	
		//Gets the functions.
		public IFunction[] GetFunctions()
		{
			IFunction[] func = new IFunction[m_functions.Count];
			for (int i = 0; i <= m_functions.Count - 1; i++) 
			{
				func[i] = (IFunction)m_functions.GetByIndex(i);
			}
			return func;
		}
		//Adds a function to the function set.
		public void Add(IFunction func)
		{
			//(28.07.2006 12:36)
			//Use shared function to save memory.
			if (func is ISharedFunction)
			{
				if (s_sharedFunctions.Contains(func.Name))
				{
					func = (IFunction)s_sharedFunctions[func.Name];
				}
				else
				{
					s_sharedFunctions.Add(func.Name, func);
				}
			}
			if (m_caseSensitive)
			{
				m_functions.Add(func.Name, func);
			}
			else
			{
				m_functions.Add(func.Name.ToLower(), func);
			}
		}
		
		public void AddSharedFunctions(ISharedFunction[] arr)
		{
			foreach (var el in arr)
				s_sharedFunctions.Add(((IFunction)el).Name, el);
		}
		//Removes a function from the function set.
		public void Remove(IFunction func)
		{
			if (m_caseSensitive)
			{
				m_functions.Remove(func.Name);
			}
			else
			{
				m_functions.Remove(func.Name.ToLower());
			}
		}
		public bool Contains(string functionName)
		{
			if (s_sharedFunctions.Contains(functionName)) return true;
			
			if (m_caseSensitive)
				return m_functions.Contains(functionName);
			
			return m_functions.Contains(functionName.ToLower());
		}
		public void CopyTo(System.Array array, int index)
		{
			m_functions.CopyTo(array, index);
		}
		#region "ICollection stuff"
		public int Count 
		{
			get 
			{
				return m_functions.Count;
			}
		}
		public bool IsSynchronized 
		{
			get 
			{
				return m_functions.IsSynchronized;
			}
		}
		public object SyncRoot 
		{
			get 
			{
				return m_functions.SyncRoot;
			}
		}
		public System.Collections.IEnumerator GetEnumerator()
		{
			return m_functions.GetEnumerator();
		}
		#endregion
	}
}
