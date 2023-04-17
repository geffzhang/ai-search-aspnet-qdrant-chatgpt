using Microsoft.AspNetCore.Builder;
using SKernel.Contract.Services;

namespace SKernel.Service.Services
{
    public class SkillsService : ServiceBase, ISkillsService
    {
        public SkillsService()
        {
            RouteOptions.DisableAutoMapRoute = true;//当前服务禁用自动注册路由
            App.MapGet("/api/skills/{skill}/{function}", GetSkillFunctionAsync);
            App.MapGet("/api/skills", GetSkillsAsync);
        }


        public Task<List<string>> GetSkillFunctionAsync(string skill, string function)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<Dictionary<string, object>>>> GetSkillsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
