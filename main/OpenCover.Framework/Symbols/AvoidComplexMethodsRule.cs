// Decompiled with JetBrains decompiler
// Type: Gendarme.Rules.Maintainability.AvoidComplexMethodsRule
// Assembly: Gendarme.Rules.Maintainability, Version=2.11.0.0, Culture=neutral, PublicKeyToken=b07ccf3a9f1ab3c9
// MVID: 1304D328-3EE1-499C-8754-E2E28008DB85
// Assembly location: C:\Projects\opencover.git\working\tools\GendarmeSigned\Gendarme.Rules.Maintainability.dll

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using OpenCover.Framework;

namespace Gendarme.Rules.Maintainability
{
    [ExcludeFromCoverage("3rd Party - imported due to Roslyn intellisense error CS0012")]
    [Problem("Methods with a large cyclomatic complexity are hard to understand and maintain.")]
    [FxCopCompatibility("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [Solution("Simplify the method using refactors like Extract Method.")]
    [EngineDependency(typeof(OpCodeEngine))]
    internal class AvoidComplexMethodsRule : Rule, IMethodRule, IRule
    {
        private static OpCodeBitmask ld = new OpCodeBitmask(68718871548UL, 1946399463954251744UL, 70373055920128UL, 3552UL);
        private static List<Instruction> targets = new List<Instruction>();
        private const int DefaultSuccessThreshold = 25;

        [DefaultValue(25)]
        [Description("The cyclomatic complexity at which defects are reported.")]
        public int SuccessThreshold { get; set; }

        [Description("Methods with cyclomatic complexity less than this will be reported as low severity.")]
        [DefaultValue(0)]
        public int LowThreshold { get; set; }

        [Description("Methods with cyclomatic complexity less than this will be reported as medium severity.")]
        [DefaultValue(0)]
        public int MediumThreshold { get; set; }

        [Description("Methods with cyclomatic complexity less than this will be reported as high severity.")]
        [DefaultValue(0)]
        public int HighThreshold { get; set; }

        public AvoidComplexMethodsRule()
        {
            this.SuccessThreshold = 25;
        }

        public override void Initialize(IRunner runner)
        {
            base.Initialize(runner);
            if (this.LowThreshold == 0)
                this.LowThreshold = this.SuccessThreshold * 2;
            if (this.MediumThreshold == 0)
                this.MediumThreshold = this.SuccessThreshold * 3;
            if (this.HighThreshold != 0)
                return;
            this.HighThreshold = this.SuccessThreshold * 4;
        }

        public RuleResult CheckMethod(MethodDefinition method)
        {
            if (!method.HasBody || MethodRocks.IsGeneratedCode((MethodReference)method) || method.IsCompilerControlled)
                return RuleResult.DoesNotApply;
            if (method.Body.Instructions.Count < this.SuccessThreshold)
                return RuleResult.Success;
            int cyclomaticComplexity = AvoidComplexMethodsRule.GetCyclomaticComplexity(method);
            if (cyclomaticComplexity < this.SuccessThreshold)
                return RuleResult.Success;
            Severity complexitySeverity = this.GetCyclomaticComplexitySeverity(cyclomaticComplexity);
            string message = string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Method's cyclomatic complexity : {0}.", new object[1]
            {
        (object) cyclomaticComplexity
            });
            this.Runner.Report((IMetadataTokenProvider)method, complexitySeverity, Confidence.High, message);
            return RuleResult.Failure;
        }

        public Severity GetCyclomaticComplexitySeverity(int cc)
        {
            if (cc < this.LowThreshold)
                return Severity.Low;
            if (cc < this.MediumThreshold)
                return Severity.Medium;
            return cc < this.HighThreshold ? Severity.High : Severity.Critical;
        }

        public static int GetCyclomaticComplexity(MethodDefinition method)
        {
            if (method == null || !method.HasBody)
                return 1;
            if (OpCodeEngine.GetBitmask(method).Get(Code.Switch))
                return AvoidComplexMethodsRule.GetSwitchCyclomaticComplexity(method);
            return AvoidComplexMethodsRule.GetFastCyclomaticComplexity(method);
        }

        private static int GetFastCyclomaticComplexity(MethodDefinition method)
        {
            int num = 1;
            foreach (Instruction instruction in method.Body.Instructions)
            {
                switch (instruction.OpCode.FlowControl)
                {
                    case FlowControl.Branch:
                        Instruction previous = instruction.Previous;
                        if (previous != null && AvoidComplexMethodsRule.ld.Get(previous.OpCode.Code))
                        {
                            ++num;
                            continue;
                        }
                        continue;
                    case FlowControl.Cond_Branch:
                        ++num;
                        continue;
                    default:
                        continue;
                }
            }
            return num;
        }

        private static int GetSwitchCyclomaticComplexity(MethodDefinition method)
        {
            Instruction instruction1 = (Instruction)null;
            int num1 = 1;
            foreach (Instruction ins in method.Body.Instructions)
            {
                switch (ins.OpCode.FlowControl)
                {
                    case FlowControl.Branch:
                        if (instruction1 != null)
                        {
                            instruction1 = ins.Previous;
                            if (AvoidComplexMethodsRule.ld.Get(instruction1.OpCode.Code))
                                ++num1;
                            if (instruction1.OpCode.FlowControl == FlowControl.Cond_Branch)
                            {
                                Instruction instruction2 = instruction1.Operand as Instruction;
                                if (instruction2 != null && AvoidComplexMethodsRule.targets.Contains(instruction2))
                                {
                                    CollectionRocks.AddIfNew<Instruction>((ICollection<Instruction>)AvoidComplexMethodsRule.targets, ins);
                                    continue;
                                }
                                continue;
                            }
                            continue;
                        }
                        continue;
                    case FlowControl.Cond_Branch:
                        if (ins.OpCode.Code == Code.Switch)
                        {
                            AvoidComplexMethodsRule.AccumulateSwitchTargets(ins);
                            continue;
                        }
                        Instruction instruction3 = ins.Operand as Instruction;
                        instruction1 = instruction3.Previous;
                        if (instruction1 != null && !InstructionRocks.Is(instruction1.Previous, Code.Switch) && !AvoidComplexMethodsRule.targets.Contains(instruction3))
                        {
                            ++num1;
                            continue;
                        }
                        continue;
                    default:
                        continue;
                }
            }
            int num2 = num1 + AvoidComplexMethodsRule.targets.Count;
            AvoidComplexMethodsRule.targets.Clear();
            return num2;
        }

        private static void AccumulateSwitchTargets(Instruction ins)
        {
            Instruction[] instructionArray = (Instruction[])ins.Operand;
            foreach (Instruction instruction in instructionArray)
            {
                if (instruction != ins.Next)
                    CollectionRocks.AddIfNew<Instruction>((ICollection<Instruction>)AvoidComplexMethodsRule.targets, instruction);
            }
            Instruction next = ins.Next;
            if (next.OpCode.FlowControl != FlowControl.Branch || AvoidComplexMethodsRule.FindFirstUnconditionalBranchTarget(instructionArray[0]) == next.Operand)
                return;
            CollectionRocks.AddIfNew<Instruction>((ICollection<Instruction>)AvoidComplexMethodsRule.targets, next.Operand as Instruction);
        }

        private static Instruction FindFirstUnconditionalBranchTarget(Instruction ins)
        {
            for (; ins != null; ins = ins.Next)
            {
                if (ins.OpCode.FlowControl == FlowControl.Branch)
                    return (Instruction)ins.Operand;
            }
            return (Instruction)null;
        }
    }
}
