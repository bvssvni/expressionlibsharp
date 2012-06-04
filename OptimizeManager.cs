
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
	public class OptimizeManager
	{
		private Calculator m_calculator;
		private Expression[] m_notOptimizables;
		private Hashtable m_eventGraph;
		private ArrayList m_objectReferences;
		private ComputeList m_computeList;
		private FunctionSet m_functions;

		//Gets or sets the calculator.
		public Calculator Calculator 
		{
			get 
			{
				return m_calculator;
			}
			set 
			{
				m_calculator = value;
			}
		}
		public void Optimize()
		{
			//Contains dependices linked to expressions in calculator.
			Hashtable optimizables = new Hashtable();
			// If FindDependices returns false, the key is marked as not-optimizable.
			Hashtable notOptimizables = new Hashtable();
			//(28.07.2006 11:58)
			//Reduce unecessary calls to the property.
			m_functions = m_calculator.Functions;

			#region "Finding Dependicies"
			//Find all dependicies.
			Expression[] expressions = m_calculator.GetExpressions();
			foreach (Expression exp in expressions) 
			{
				ArrayList deps = new ArrayList();
				// Get all the dependices of expression.
				// Use the variable name of expression key as relative.
				if (!FindDependices(exp.Key, exp.Tree, deps, exp.Key.Substring(0, exp.Key.IndexOf(Calculator.PROPERTY_CHAR)), null, false))
				{
					// Mark as not optimizable.
					if (!notOptimizables.Contains(exp.Key))
					{
						notOptimizables.Add(exp.Key, exp.Key);
					}
				}
				else
				{
					// Check for direct property dependence.
					object expObject = exp.Variable;
					if (expObject is IOptimizable)
					{
						IOptimizable expOpt = (IOptimizable)expObject;
						PropertyDependicy[] propDeps = expOpt.PropertyDependices;
						if (propDeps != null)
						{
							foreach (PropertyDependicy propDep in propDeps) 
							{
								if (propDep.ParentProperty == exp.PropName)
								{
									deps.Add(exp.Variable.Name + Calculator.PROPERTY_CHAR + propDep.ChildProperty);
								}
							}
						}
					}
					if (deps.Count != 0)
					{
						DependicyInfo dependicy = new DependicyInfo();
						dependicy.Key = exp.Key;
						//Analyse each dependicy.
						string[] depsTable = new string[deps.Count];
						bool optimizable = true;
						for (int i = 0; i <= depsTable.Length - 1; i++) 
						{
							string nodeValue = (string)deps[i];
							if (!optimizables.Contains(nodeValue))
							{
								//Create dependicy info.
								DependicyInfo depNode = new DependicyInfo();
								depNode.Key = nodeValue;
								//Detect type.
								depNode.Type = DependicyType(nodeValue);
								//See if the dependicy is not optimizable.
								if (depNode.Type == DependicyInfo.DEPENDICY_TYPE_NOT_LEGAL)
								{
									optimizable = false;
									break;
								}
								else if (depNode.Type == DependicyInfo.DEPENDICY_TYPE_VARIABLE) 
								{
									//See if variable inherits from IOptimizable.
									object obj = m_calculator.Variable(depNode.Key);
									if (!(obj is IOptimizable))
									{
										optimizable = false;
										break;
									}
								}
								else if (depNode.Type == DependicyInfo.DEPENDICY_TYPE_PROPERTY) 
								{
									//See if variable inherits from IOptimizable.
									int dotIndex = depNode.Key.IndexOf(Calculator.PROPERTY_CHAR);
									string varName = depNode.Key.Substring(0, dotIndex);
									object obj = m_calculator.Variable(varName);
									if (!(obj is IOptimizable))
									{
										optimizable = false;
										break;
									}
								}
								//Add dependicy as optimizable.
								optimizables.Add(nodeValue, depNode);
								depsTable[i] = depNode.Key;
							}
							else
							{
								//Optimizable already exists.
								depsTable[i] = nodeValue;
							}
						}
						if (!optimizable)
						{
							notOptimizables.Add(exp.Key, exp.Key);
						}
						else
						{
							//Add as optimizable.
							dependicy.Type = DependicyInfo.DEPENDICY_TYPE_EXPRESSION;
							dependicy.Deps = depsTable;
							//Remove temporary optimizable.
							if (optimizables.Contains(exp.Key))
							{
								optimizables.Remove(exp.Key);
							}
							optimizables.Add(exp.Key, dependicy);
						}
					}
					else
					{
						// Expression depends on no other.
						DependicyInfo dep = new DependicyInfo();
						dep.Deps = new string[] {};
						dep.Type = DependicyInfo.DEPENDICY_TYPE_EXPRESSION;
						dep.Key = exp.Key;
						// Remove temporary optimizable.
						if (optimizables.Contains(exp.Key))
						{
							optimizables.Remove(exp.Key);
						}
						optimizables.Add(dep.Key, dep);
					}
				}
			}
			#endregion

			// Remove all not-optimizables from optimizables.
			foreach (string key in notOptimizables.Values) 
			{
				if (optimizables.Contains(key))
				{
					optimizables.Remove(key);
				}
			}
			//Remove all optimalizables that depends on not-optimalizables.
			Hashtable lastNotOptimizables = notOptimizables;
			//Continue to remove optimizables that depends on not-optimizables
			//until table of optimizables is clean.
			while (lastNotOptimizables.Count != 0) 
			{
				//Find all optimizables that depends on newly removed optimizables.
				Hashtable tempNotOptimizables = new Hashtable();
				foreach (DependicyInfo dep in optimizables.Values) 
				{
					if (dep.Type == DependicyInfo.DEPENDICY_TYPE_EXPRESSION)
					{
						foreach (string depName in dep.Deps) 
						{
							if (lastNotOptimizables[depName] != null)
							{
								tempNotOptimizables.Add(dep.Key, dep.Key);
								//Remove child property dependices of dependicy.
								int propIndex = depName.IndexOf(Calculator.PROPERTY_CHAR);
								string variableName = depName.Substring(0, propIndex);
								object obj = m_calculator.Variable(variableName);
								if (obj is IOptimizable)
								{
									IOptimizable opt = (IOptimizable)obj;
									if (opt.PropertyDependices != null)
									{
										foreach (PropertyDependicy propertyDep in opt.PropertyDependices) 
										{
											if (propertyDep.ParentProperty == depName.Substring(propIndex + 1))
											{
												string childKey = variableName + Calculator.PROPERTY_CHAR + propertyDep.ChildProperty;
												if (!tempNotOptimizables.Contains(childKey))
												{
													tempNotOptimizables.Add(childKey, childKey);
												}
											}
										}
									}
								}
								break;
							}
						}
					}
				}
				//Remove from optimizables in end to not get enumeration exception.
				foreach (string expName in tempNotOptimizables.Values) 
				{
					optimizables.Remove(expName);
					if (!notOptimizables.Contains(expName))
						notOptimizables.Add(expName, expName);
				}
				lastNotOptimizables = tempNotOptimizables;
			}
			//Remove recursive.
			foreach (DependicyInfo opt in optimizables.Values) 
			{
				SortedList history = new SortedList();
				Hashtable processed = new Hashtable(optimizables.Count);
				RemoveRecursive(opt, optimizables, notOptimizables, history, processed);
			}
			//Remove all not-optimizables from optimizables.
			foreach (string key in notOptimizables.Values) 
			{
				if (optimizables.Contains(key))
				{
					optimizables.Remove(key);
				}
			}

			IVariable[] variables = m_calculator.GetVariables();
			foreach (IVariable variable in variables)
			{
				if (variable is IOptimizable)
				{
					IOptimizable obj = (IOptimizable)variable;
					obj.PropertyChanged -= new PropertyChangedEventHandler(PropertyChanged);
				}
			}

			#region "Dependicy Reversing"
			//Build optimize graphs.
			//Reverse the dependices.
			Hashtable eventGraph = new Hashtable();
			Hashtable objectMap = new Hashtable();
			//(29.11.2006 12:15)
			Hashtable processedEvents = new Hashtable();
			foreach (DependicyInfo opt in optimizables.Values) 
			{
				if (opt.Deps != null)
				{
					AddToEventList(opt, opt.Deps, optimizables, eventGraph, objectMap, processedEvents);
					//An expression should update it self if it differ from it's previous value.
					if (opt.Type == DependicyInfo.DEPENDICY_TYPE_EXPRESSION)
					{
						string varName = opt.Key.Substring(0, opt.Key.IndexOf(Calculator.PROPERTY_CHAR));
						object obj = m_calculator.Variable(varName);
						if (obj is IOptimizable)
						{
							IOptimizable optObj = (IOptimizable)obj;
							if (!eventGraph.Contains(opt.Key))
							{
								if (!objectMap.Contains(optObj))
								{
									optObj.PropertyChanged += new PropertyChangedEventHandler(PropertyChanged);
									objectMap.Add(optObj, varName);
								}
								eventGraph.Add(opt.Key, new ArrayList());
							}
							//Add to event list.
							ArrayList events = (ArrayList)eventGraph[opt.Key];
							if (!events.Contains(opt.Key))
							{
								events.Add(m_calculator.GetExpressionObject(opt.Key));
							}
						}
					}
				}
			}
			#endregion

			m_computeList = new ComputeList();
			m_eventGraph = eventGraph;

			#region "Single Object Reference"
			//Single expressions using object references, are not necessary to compute.
			ArrayList objectReferences = new ArrayList();
			foreach (string key in notOptimizables.Values) 
			{
				string exp = m_calculator.GetExpression(key);
				if (exp != null && System.Text.RegularExpressions.Regex.IsMatch(exp.Trim(), Calculator.NAME_REGEX))
				{
					objectReferences.Add(key);
				}
			}
			foreach (string key in objectReferences) 
			{
				notOptimizables.Remove(key);
			}
			//Remember the object references for later use.
			m_objectReferences = objectReferences;
			//Create list of not optimizable expressions.
			ArrayList expressionList = new ArrayList();
			foreach (string key in notOptimizables.Values) 
			{
				Expression exp = m_calculator.GetExpressionObject(key);
				if (exp != null)
				{
					expressionList.Add(exp);
				}
			}
			#endregion
		
			m_notOptimizables = new Expression[expressionList.Count];
			expressionList.CopyTo(m_notOptimizables);
		}
		//(10.05.2006 08:05)
		//Puts expressions with indirect property usage last in list.
		private class ComputeList
		{
			private Hashtable m_computing = new Hashtable(100);
			private Queue m_direct = new Queue();

			//Creates a compute list with reference of indirect info to this list.
			public ComputeList Clone()
			{
				ComputeList computeList = new ComputeList();
				return computeList;
			}
			public void Clear()
			{
				//(16.10.2006 09:02)
				//Make thread safe.
				lock (this) 
				{
					m_computing.Clear();
					//(09.10.2006 14:14)
					//Check whether the direct and indirect list 
					//is empty before clearing.
					if (m_direct.Count != 0)
					{
						m_direct.Clear();
					}
				}
			}
			public int Count 
			{
				get 
				{
					return m_direct.Count;
				}
			}
			public Expression[] GetExpressions()
			{
				//(16.10.2006 09:02)
				//Make thread safe.
				lock (this) 
				{
					//Create a list with indirect expressions last.
					int directCount = m_direct.Count;
					Expression[] exps = new Expression[directCount];
					m_direct.CopyTo(exps, 0);
					return exps;
				}
			}
			public void Add(string key, Expression exp)
			{
				//(16.10.2006 09:02)
				//Make thread safe.
				lock (this) 
				{
					if (!m_computing.Contains(key))
					{
						m_computing.Add(key, key);
						m_direct.Enqueue(exp);
						//Reset the buffer before computing.
						exp.ResetBuffer();
					}
				}
			}
		}

		private void AddToEventList(DependicyInfo opt, string[] deps, Hashtable optimizables, Hashtable eventGraph, Hashtable objectMap, Hashtable processed)
		{
			// (03.01.2007 21:06)
//			//(29.11.2006 12:15)
//			if (processed.Contains(opt))
//			{
//				return;
//			}
			foreach (string depName in deps) 
			{
				DependicyInfo depOpt = (DependicyInfo)optimizables[depName];
				//Only properties on object that implements IOptimizable can give messages.
				//If depOpt.Type = DependicyInfo.DEPENDICY_TYPE.PROPERTY Then
				//Add to event graph.
				if (!eventGraph.Contains(depName))
				{
					string varName = depName.Substring(0, depName.IndexOf(Calculator.PROPERTY_CHAR));
					IOptimizable optObj = (IOptimizable)m_calculator.Variable(varName);
					if (!objectMap.Contains(optObj))
					{
						//(06.05.2006 20:44)
						//Add event if no events added earlier.
						// TODO: Check that this works.
						optObj.PropertyChanged += new PropertyChangedEventHandler(PropertyChanged);
						objectMap.Add(optObj, varName);
					}
					eventGraph.Add(depName, new ArrayList());
				}
				//Add to event list.
				ArrayList events = (ArrayList)eventGraph[depName];
				if (!events.Contains(opt.Key))
				{
					events.Add(m_calculator.GetExpressionObject(opt.Key));
				}
				//ElseIf depOpt.Type = DependicyInfo.DEPENDICY_TYPE.EXPRESSION Then
				if (depOpt.Type == DependicyInfo.DEPENDICY_TYPE_EXPRESSION)
				{
					//An expression depends on another expressions.
					AddToEventList(opt, depOpt.Deps, optimizables, eventGraph, objectMap, processed);
				}
			}
			//Tell the dependicy is processed.
			processed[opt] = true;
		}
		//Removes recursive.
		//Returns true if this is recursive.
		private bool RemoveRecursive(DependicyInfo opt, Hashtable optimized, Hashtable notOptimized, SortedList history, Hashtable processed)
		{
			//(29.11.2006 12:15)
			if (processed.Contains(opt))
			{
				return (bool)processed[opt];
			}
			if (notOptimized[opt.Key] == null)
			{
				if (opt.Deps == null)
				{
					processed[opt] = false;
					return false;
				}
				bool recursive = false;
				//Check each dependicy if it is recursive.
				foreach (string depName in opt.Deps) 
				{
					string historyKey = opt.Key + " " + depName;
					if (history[historyKey] != null)
					{
						//Recursive.
						//(B001)
						notOptimized[depName] = depName;
						recursive = true;
					}
					else
					{
						//Check with property dependicy.
						int propIndex = depName.IndexOf(Calculator.PROPERTY_CHAR);
						string variableName = depName.Substring(0, propIndex);
						object depObj = m_calculator.Variable(variableName);
						if (depObj is IOptimizable)
						{
							IOptimizable depOpt = (IOptimizable)depObj;
							if (depOpt.PropertyDependices != null)
							{
								foreach (PropertyDependicy propertyDep in depOpt.PropertyDependices) 
								{
									if (propertyDep.ChildProperty == depName.Substring(propIndex + 1))
									{
										string parentKey = variableName + Calculator.PROPERTY_CHAR + propertyDep.ParentProperty;
										if (history[opt.Key + " " + parentKey] != null)
										{
											//Recursive.
											//(B001)
											notOptimized[depName] = depName;
											recursive = true;
										}
									}
								}
							}
						}
					}
				}
				//Search in each not-recursive dependicy.
				foreach (string depName in opt.Deps) 
				{
					string historyKey = opt.Key + " " + depName;
					if (notOptimized[depName] == null)
					{
						history.Add(historyKey, true);
						if (this.RemoveRecursive((DependicyInfo)optimized[depName], optimized, notOptimized, history, processed))
						{
							recursive = true;
						}
						else
						{
							history.Remove(historyKey);
						}
					}
				}
				if (recursive)
				{
					if (notOptimized[opt.Key] == null)
					{
						notOptimized.Add(opt.Key, opt.Key);
					}
				}
				processed[opt] = recursive;
				return recursive;
			}
			else
			{
				processed[opt] = true;
				return true;
			}
		}
		private int DependicyType(string key)
		{
			if (m_calculator.GetExpression(key) != null)
			{
				//There is an expression for this.
				return DependicyInfo.DEPENDICY_TYPE_EXPRESSION;
			}
			else if (key.IndexOf(Calculator.PROPERTY_CHAR) == -1) 
			{
				if (!ExpressionTreeNode.IsFunction(key) && !ExpressionTreeNode.IsText(key))
				{
					//It is a variable.
					return DependicyInfo.DEPENDICY_TYPE_VARIABLE;
				}
				else
				{
					return DependicyInfo.DEPENDICY_TYPE_NOT_LEGAL;
				}
			}
			else if (!ExpressionTreeNode.IsFunction(key) && !ExpressionTreeNode.IsText(key)) 
			{
				//Don't allow more than sub-properties.
				int count = 0;
				for (int i = 0; i <= key.Length - 1; i++) 
				{
					char c = key[i];
					if (c == Calculator.PROPERTY_CHAR)
					{
						count += 1;
						if (count > 2)
						{
							return DependicyInfo.DEPENDICY_TYPE_NOT_LEGAL;
						}
					}
				}
				if (count == 1)
				{
					return DependicyInfo.DEPENDICY_TYPE_PROPERTY;
				}
			}
			return DependicyInfo.DEPENDICY_TYPE_NOT_LEGAL;
		}
		//Search for all value nodes in expression.
		//Returns true if it is optimizable.
		//Return false if it is not optimizable.
		private bool FindDependices(string expKey, ExpressionTreeNode node, ArrayList coll, string relative, ExpressionTreeNode parent, bool slack)
		{
			if (node == null)
			{
				return true;
			}
			switch (node.NodeType) 
			{
				case ExpressionTreeNode.NODE_TYPE.VARIABLE:
					//Detect relative dependicy.
					string key = (string)node.Value;
					//Check for relative usage of properties.
					//Etc. dependicy is 'line1.FromPoint' using '.ToPoint'.
					int propIndex = key.IndexOf(Calculator.PROPERTY_CHAR);
					if (propIndex != -1)
					{
						if (key.Substring(0, propIndex) == "")
						{
							//Set new key using full name.
							key = relative + key;
							propIndex += relative.Length;
						}
						//Check for slack.
						if (key == expKey)
						{
							//Recursive on 1 level.
							if (!slack)
							{
								return false;
							}
							else
							{
								//Accept slack.
								return true;
							}
						}
						//Treat sub-properties as properties.
						int nextPropIndex = key.IndexOf(Calculator.PROPERTY_CHAR, propIndex + 1);
						if (nextPropIndex != -1)
						{
							//Sub-property.
							//Treat as property.
							key = key.Substring(0, nextPropIndex);
						}
					}
					else
					{
						// The variable does not use a property.
						if (parent != null && parent.NodeType == ExpressionTreeNode.NODE_TYPE.FUNCTION)
						{
							IFunction func = m_functions.Item((string)parent.Value);
							//Check whether function has information
							//about which properties of variable that is used.
							if (func is IVariableReferencePropertyUsage)
							{
								IVariable var = this.m_calculator.Variable((string)node.Value);
								if (var != null)
								{
									IVariableReferencePropertyUsage propFunc = (IVariableReferencePropertyUsage)func;
									string[] propertyDependices = propFunc.GetDependices(var.GetType());
									if (propertyDependices == null)
									{
										return false;
									}
									foreach (string prop in propertyDependices) 
									{
										coll.Add(node.Value + "." + prop);

										// (04.05.2007 10:43)
										// Add child property dependices.
										if (var is IOptimizable)
										{
											PropertyDependicy[] deps = ((IOptimizable)var).PropertyDependices;
											if (deps != null)
											{
												foreach (PropertyDependicy dep in deps)
												{
													if (dep.ParentProperty.Equals(prop))
														coll.Add(node.Value + "." + dep.ChildProperty);
												}
											}
										}
									}
									return true;
								}
							}
						}


						// Object reference. Not optimizable.
						return false;
					}

					//Detect property dependices.
					if (propIndex != -1)
					{
						string variableName = key.Substring(0, propIndex);
						IVariable var = m_calculator.Variable(variableName);
						if (var == null)
						{
							return false;
						}
						object obj = var;
						if (obj is IOptimizable)
						{
							IOptimizable opt = (IOptimizable)obj;
							string propName = key.Substring(propIndex + 1);
							if (opt.PropertyDependices != null)
							{
								foreach (PropertyDependicy propertyDep in opt.PropertyDependices) 
								{
									if (propertyDep.ChildProperty == propName)
									{
										//Add dependicy for property dependicy.
										coll.Add(variableName + Calculator.PROPERTY_CHAR + propertyDep.ParentProperty);
									}
								}
							}
						}
					}

					coll.Add(key);
					return true;
				case ExpressionTreeNode.NODE_TYPE.FUNCTION:
				case ExpressionTreeNode.NODE_TYPE.OPERATOR:
					IFunction nodeFunction = m_functions.Item((string)node.Value);
					if (nodeFunction is IRandomFunction)
					{
						//Not optimizable.
						return false;
					}

					//Check if function is slack.
					bool isSlack = false;
					if (nodeFunction is ISlackFunction)
					{
						isSlack = true;
					}

					bool optimizable = true;
					bool childOptimizable = false;
					foreach (ExpressionTreeNode child in node.SubNodes) 
					{
						childOptimizable = FindDependices(expKey, child, coll, relative, node, isSlack);
						optimizable = optimizable & childOptimizable;
					}

					return optimizable;
				default:
					return true;
//				default:
//					//Operator.
//					bool isOptimizable = true;
//					isOptimizable = isOptimizable & FindDependices(expKey, node.Left, coll, relative, null, false);
//					isOptimizable = isOptimizable & FindDependices(expKey, node.Right, coll, relative, null, false);
//					return isOptimizable;
			}
		}
		//Gets the compute list of expressions.
		private ArrayList GetComputeList()
		{
			Expression[] expressions = m_computeList.GetExpressions();
			ArrayList coll = new ArrayList(expressions.Length + m_notOptimizables.Length);
			if (m_notOptimizables.Length != 0)
			{
				coll.AddRange(this.m_notOptimizables);
			}
			if (expressions.Length != 0)
			{
				coll.AddRange(expressions);
			}
			return coll;
		}

		bool computing = false;
		//Computes the expressions.
		//Returns the expressions necessary to compute
		//that was optimizable.
		public int Compute()
		{
			// ElementoLib.Development.StartTimer(ElementoLib.Development.TIMER_OPTIMIZEMANAGER_COMPUTE);
			//Resets all buffers in expressions that are not optimizable.
			if (m_notOptimizables != null)
			{
				int n = m_notOptimizables.Length;
				for (int i = 0; i < n; i++)
					m_notOptimizables[i].ResetBuffer();
			}
			computing = true;
			int newComputeCount = m_computeList.Count;
			m_calculator.ComputeExpressions(GetComputeList());
			m_computeList.Clear();
			computing = false;
			// ElementoLib.Development.StopTimer(ElementoLib.Development.TIMER_OPTIMIZEMANAGER_COMPUTE);
			return newComputeCount;
		}
		//Computes all expressions in a such way that
		//a correct result will appear after 1 computation.
		public int ComputeAll()
		{
			//(22.07.2006 23:43)
			//Compute all object references first.
			Expression exp;
			foreach (string expressionName in this.m_objectReferences) 
			{
				exp = m_calculator.GetExpressionObject(expressionName);
				m_computeList.Add(expressionName, exp);
			}
			int n;
			foreach (DictionaryEntry entry in this.m_eventGraph) 
			{
				ArrayList expressionList = (ArrayList)m_eventGraph[entry.Key];
				n = expressionList.Count;
				for (int i = 0; i < n; i++)
				{
					exp = (Expression)expressionList[i];
					m_computeList.Add(exp.Key, exp);
				}
			}
			return Compute();
		}

		public void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs evt)
		{
			if (computing)
			{
				return;
			}
			if (m_eventGraph != null)
			{
				// Map to key, and call event graph.
				string varName = ((IVariable)sender).Name;
				string key = varName + Calculator.PROPERTY_CHAR + evt.PropertyName;
				if (m_eventGraph.Contains(key))
				{
					ArrayList expressionList = (ArrayList)m_eventGraph[key];
					foreach (Expression exp in expressionList)
					{
						m_computeList.Add(exp.Key, exp);
					}
				}
			}
		}
		//Copies the optimize info to another optimize manager so
		//optimzing is not necessary. Expressions should be exactly the same.
		public void CopyInfoTo(OptimizeManager optimizeManager)
		{
			//Set the expressions that is not optimizable.
			Expression[] notOptimizables = new Expression[this.m_notOptimizables.Length];
			Calculator calc = optimizeManager.Calculator;
			for (int i = 0; i < this.m_notOptimizables.Length; i++) 
			{
				notOptimizables[i] = calc.GetExpressionObject(this.m_notOptimizables[i].Key);
			}
			optimizeManager.m_notOptimizables = notOptimizables;
			
			optimizeManager.m_eventGraph = new Hashtable();
			foreach (DictionaryEntry entry in this.m_eventGraph)
			{
				ArrayList events = (ArrayList)entry.Value;
				ArrayList newEvents = new ArrayList();
				foreach (Expression exp in events)
					newEvents.Add(calc.GetExpressionObject(exp.Key));
				optimizeManager.m_eventGraph[entry.Key] = newEvents;
			}

			//Set the object map.
			IVariable[] variables = optimizeManager.Calculator.GetVariables();
			foreach (IVariable variable in variables)
			{
				if (variable is IOptimizable)
					((IOptimizable)variable).PropertyChanged += new PropertyChangedEventHandler(optimizeManager.PropertyChanged);
			}

			//Copy object references.
			optimizeManager.m_objectReferences = new ArrayList();
			foreach (string objectReference in this.m_objectReferences)
				optimizeManager.m_objectReferences.Add(objectReference);

			//Clone compute list.
			//Shares indirection info, which makes computation faster in the start.
			// optimizeManager.m_computeList = this.m_computeList.Clone();
			optimizeManager.m_computeList = new ComputeList();
		}
	}
}
