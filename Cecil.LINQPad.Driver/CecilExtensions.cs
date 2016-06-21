﻿/*
 * Copyright [2016] [Adriano Carlos Verona]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Cecil.LINQPad.Driver
{
	public static class CecilExtensions
	{
		public static bool Implements(this TypeDefinition type, TypeReference itf)
		{
			if (!type.HasInterfaces)
				return false;

			return type.Interfaces.Contains(itf);
		}

		public static PropertyDefinition Property(this TypeDefinition self, string name)
		{
			if (!self.HasProperties)
				return null;

			return self.Properties.Single(p => p.Name == name);
		}

		public static FieldDefinition Field(this TypeDefinition self, string name)
		{
			if (!self.HasFields)
				return null;

			return self.Fields.Single(f => f.Name == name);
		}

		public static EventDefinition Event(this TypeDefinition self, string name)
		{
			if (!self.HasEvents)
				return null;

			return self.Events.Single(e => e.Name == name);
		}

		public static bool References(this MethodDefinition self, TypeReference type)
		{
			if (!self.HasBody)
				return false;

			return self.Body.Instructions.Any(i => (i.Operand as TypeReference)?.FullName == type.FullName);
		}

		public static bool Calls(this MethodDefinition self, MethodReference method)
		{
			if (!self.HasBody)
				return false;
			
			return self.Body.Instructions.Any(i => IsCallTo(i, method));
		}

		public static bool Calls(this MethodDefinition self, MethodInfo method)
		{
			var importedMethod = self.Module.Import(method);
			if (importedMethod == null)
			{
				//TODO: Log failure.
				return false;
			}

			return Calls(self, importedMethod);
		}

		public static string AsString(this MethodDefinition self)
		{
			return Formatter.FormatMethodBody(self);
		}
		
		public static string AsString(this Mono.Cecil.Cil.MethodBody self)
		{
			if (self == null)
				return "Method has no body";

			return Formatter.FormatMethodBody(self.Method);
		}
		
		private static bool IsCallTo(Instruction instruction, MethodReference method)
		{
			if (instruction.OpCode != OpCodes.Callvirt
			    && instruction.OpCode != OpCodes.Call
			    && instruction.OpCode != OpCodes.Calli)
				return false;

			var operand = instruction.Operand as MethodReference;
			if (operand == null)
				return false;

			return operand.FullName == method.FullName;
		}
	}
}