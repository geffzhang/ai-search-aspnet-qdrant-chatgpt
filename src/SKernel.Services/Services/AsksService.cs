using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SKernel.Contract.Services;
using SKernel.Factory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SKernel.Service.Services
{
    public class AsksService : ServiceBase, IAsksService
    {
        private SemanticKernelFactory semanticKernelFactory;
        private IHttpContextAccessor contextAccessor;
        private IPlanExecutor planExecutor;

        public AsksService(SemanticKernelFactory factory, IHttpContextAccessor contextAccessor, IPlanExecutor planExecutor)
        {
            this.semanticKernelFactory = factory;
            this.contextAccessor = contextAccessor;
            this.planExecutor = planExecutor;
            RouteOptions.DisableAutoMapRoute = true;//当前服务禁用自动注册路由
            App.MapPost("/api/asks", PostAsync);
        }

        public async Task<IResult> PostAsync([FromQuery(Name = "iterations")] int? iterations, Message message)
        {
            var httpRequest = this.contextAccessor?.HttpContext?.Request;

            return httpRequest.TryGetKernel(semanticKernelFactory, out var kernel)
                ? (message.Pipeline == null || message.Pipeline.Count == 0
                ? await planExecutor.Execute(kernel!, message, iterations ?? 10)
                : await kernel!.InvokePipedFunctions(message)).ToResult(message.Skills)
            : Results.BadRequest("API config is not valid");
        }
    }
}
