
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
	public class SubExpressionUpdate
	{
		private ArrayList m_expressionList = new ArrayList();

		public SubExpressionUpdate(ICalculator calculator, IFunction callFunction, string val, int propIndex, IVariable relative)
		{
			if (propIndex == -1 && callFunction is IVariableReferencePropertyUsage)
			{
				if (val != relative.Name)
				{
					// Compute dependicy properties before returning the variable
					// in order to receive correct values.
					object var = calculator.ComputeVariable(val, null);
					string[] props = ((IVariableReferencePropertyUsage)callFunction).GetDependices(var.GetType());
					if (props != null)
					{
						foreach (string prop in props)
						{
							Expression exp = calculator.GetExpressionObject(val + "." + prop);
							if (exp != null)
								m_expressionList.Add(exp);
						}
					}
				}
			}
			else if (propIndex > 0)
			{
				// Compute property dependices before this.
				string name = val.Substring(0, propIndex);
				if (name != relative.Name)
				{
					object var = calculator.ComputeVariable(name, null);
					if (var is IOptimizable)
					{
						IOptimizable opt = (IOptimizable)var;
						PropertyDependicy[] deps = opt.PropertyDependices;
						string prop = val.Substring(propIndex + 1);
						if (deps != null)
						{
							Expression[] exps = new Expression[1];
							foreach (PropertyDependicy dep in deps)
							{
								if (dep.ParentProperty == prop)
								{
									Expression exp = calculator.GetExpressionObject(name + "." + dep.ChildProperty);
									if (exp != null)
										m_expressionList.Add(exp);
								}
							}
						}
					}
				}
			}
		} // SubExpressionUpdate.

		public void UpdateSubExpressions(ICalculator calculator)
		{
			if (this.m_expressionList.Count != 0)
				calculator.ComputeExpressions(m_expressionList);
		}
	}
}
