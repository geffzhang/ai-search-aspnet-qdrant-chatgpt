using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.Threading.Tasks;

namespace SKernel.Factory
{
    public class DefaultPlanExecutor : IPlanExecutor
    {
        public async Task<SKContext> Execute(IKernel kernel, Message message, int iterations)
        {
            SKContext plan = await kernel.RunAsync(message.Variables.ToContext(), kernel.CreatePlan());
            var iteration = 0;
            var executePlan = kernel.ExecutePlan();

            var result = await kernel.RunAsync(plan.Variables, executePlan);

            while (!result.Variables.ToPlan().IsComplete && result.Variables.ToPlan().IsSuccessful &&
                   iteration < iterations - 1)
            {
                result = await kernel.RunAsync(result.Variables, executePlan);
                iteration++;
            }

            return result;
        }
    }
}
