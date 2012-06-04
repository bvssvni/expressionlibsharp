
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
	public abstract class VariableSet
	{
		private Hashtable m_children = new Hashtable();
		private Hashtable m_names = new Hashtable();
		public event VariableAddedEventHandler VariableAdded;
		public delegate void VariableAddedEventHandler(object sender, VariableEventArgs evt);
		public event VariableRemovedEventHandler VariableRemoved;
		public delegate void VariableRemovedEventHandler(object sender, VariableEventArgs evt);
		
		//Gets a variable using a name.
		public IVariable this[string name] 
		{
			get
			{
				if (name == null)
				{
					return null;
				}
				int dotIndex = name.IndexOf(Calculator.PROPERTY_CHAR);
				if (dotIndex == -1)
				{
					return (IVariable)m_children[name];
				}
				else
				{
					return (IVariable)m_children[name.Substring(0, dotIndex)];
				}
			}
		}
		
		//Gets a variable using a name.
		public IVariable Variable(string name) 
		{
			if (name == null)
			{
				return null;
			}
			int dotIndex = name.IndexOf(Calculator.PROPERTY_CHAR);
			if (dotIndex == -1)
			{
				return (IVariable)m_children[name];
			}
			else
			{
				return (IVariable)m_children[name.Substring(0, dotIndex)];
			}
		}
		
		public void AddNew(IVariable value)
		{
			if (m_children.Contains(value.Name))
			{
				//Variable already exist in variable set.
				throw new VariableAlreadyExistsException("Variable already exists");
			}
			else if (value != null) 
			{
				//Add variable.
				m_children.Add(value.Name, value);
				m_names.Add(value, value.Name);
				//Tell that variable was added.
				if (VariableAdded != null) 
				{
					VariableAdded(this, new VariableEventArgs(value));
				}
			}
		}

		// TODO: Rename to Remove instead of RemoveChild.
		public void RemoveChild(IVariable value)
		{
			m_children.Remove(value.Name);
			m_names.Remove(value);
			//Tell that variable was removed.
			if (VariableRemoved != null) 
			{
				VariableRemoved(this, new VariableEventArgs(value));
			}
		}
		public virtual void Clear()
		{
			m_children.Clear();
		}

		//Gets the children.
		public IVariable[] GetVariables()
		{
			IVariable[] var = new IVariable[m_children.Count];
			int i = 0;
			foreach (IVariable v in m_children.Values) 
			{
				var[i] = v;
				i += 1;
			}
			return var;
		}
		//Generates a new name on variable.
		public string GenerateNewName(string type)
		{
			int i = 1;
			bool isTrue = true;
			while (isTrue) 
			{
				bool exists = false;
				if (m_children.Contains(type + i))
				{
					exists = true;
				}
				if (!exists)
				{
					return type + i;
				}
				i += 1;
			}
			return null;
		}
		//Used to send info about variable.
		public class VariableEventArgs : EventArgs
		{
			private IVariable m_variable;
			public IVariable Variable 
			{
				get 
				{
					return m_variable;
				}
			}
			public VariableEventArgs(IVariable variable)
			{
				m_variable = variable;
			}
		}
	}
}