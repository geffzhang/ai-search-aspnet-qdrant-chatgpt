using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using SKernel.Contract.Services;

namespace SKernel.Service.Services
{
    public class AsksService : ServiceBase, IAsksService
    {
        public AsksService()
        {
            RouteOptions.DisableAutoMapRoute = true;//当前服务禁用自动注册路由
            App.MapPost("/api/asks", PostAsync);
        }

        public Task<IList<string>> PostAsync([FromQuery(Name = "iterations")] int? iterations, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
