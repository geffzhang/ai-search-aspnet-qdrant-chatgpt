using Microsoft.SemanticKernel.Orchestration;
using System.Collections.Generic;
using System.Linq;

namespace SKernel
{
    public class Message
    {
        public IEnumerable<KeyValuePair<string, string>> Variables { get; init; } = new ContextVariables();
        public IList<FunctionRef>? Pipeline { get; init; } = new List<FunctionRef>();
        public IList<string>? Skills { get; init; } = new List<string>();

        public class FunctionRef
        {
            public string Skill { get; init; } = "PlannerSkill";
            public string Name { get; init; } = "CreatePlan";
        }

        public static Message Ask(string question, ContextVariables context)
        {
            return new Message
            {
                Variables = context.Clone().Update(question),
                Pipeline = new List<FunctionRef> { new() { Skill = "PlannerSkill", Name = "ExecutePlan" } }
            };
        }

        public static Message Ask(ContextVariables context, params FunctionRef[] pipeline)
        {
            return new Message()
            {
                Variables = new ContextVariables().Update(context),
                Pipeline = pipeline.ToList()
            };
        }
    }
}
