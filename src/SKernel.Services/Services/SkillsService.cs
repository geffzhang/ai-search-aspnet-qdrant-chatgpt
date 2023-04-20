using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using SKernel.Contract.Services;
using SKernel.Factory;

namespace SKernel.Service.Services
{
    public class SkillsService : ServiceBase, ISkillsService
    {
        private SemanticKernelFactory semanticKernelFactory;
        private IHttpContextAccessor contextAccessor;

        public SkillsService(SemanticKernelFactory factory, IHttpContextAccessor contextAccessor)
        {
            this.semanticKernelFactory = factory;
            this.contextAccessor = contextAccessor;
            RouteOptions.DisableAutoMapRoute = true;//当前服务禁用自动注册路由
            App.MapGet("/api/skills/{skill}/{function}", GetSkillFunctionAsync);
            App.MapGet("/api/skills", GetSkillsAsync);
        }


        public async Task<IResult> GetSkillFunctionAsync(string skill, string function)
        {
            var httpRequest = this.contextAccessor?.HttpContext?.Request;

            return httpRequest.TryGetKernel(semanticKernelFactory, out var kernel)
                 ? kernel!.Skills.HasFunction(skill, function)
                 ? Results.Ok(kernel.Skills.GetFunction(skill, function).Describe())
             : Results.NotFound()
             : Results.BadRequest("API config is not valid");
        }

        public async Task<IResult> GetSkillsAsync()
        {
            var httpRequest = this.contextAccessor?.HttpContext?.Request;

            return  httpRequest.TryGetKernel(semanticKernelFactory, out var kernel)
                ? Results.Ok(
                    new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["skills"] = (from function in kernel!.ToSkills()
                              select new Dictionary<string, object>
                              {
                                  ["skill"] = function.SkillName,
                                  ["function"] = function.Name,
                                  ["_links"] = new Dictionary<string, object>
                                  {
                                      ["self"] = new Dictionary<string, object>
                                      {
                                          ["href"] = ($"/api/skills/{function.SkillName}/{function.Name}".ToLower())
                                      }
                                  }
                              })
                    .ToList()
            })
                : Results.BadRequest("API config is not valid");


        }
    }
}
