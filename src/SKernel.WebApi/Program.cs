using Masa.BuildingBlocks.Service.MinimalAPIs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using SKernel;
using SKernel.Factory.Config;
using System.Reflection;

namespace SKernel.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var skills = builder.Configuration.GetSection("SKConfig:Skills").Get<string[]>() ?? new[] { "./skills" };

            foreach (var folder in skills)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddConsoleLogger(builder.Configuration);
            builder.Services.AddSemanticKernelFactory(builder.Configuration);
            builder.WebHost.UseUrls($"http://*:{builder.Configuration["Port"] ?? "5000"}");

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SK API",
                    Version = "v1",
                    Contact = new OpenApiContact { Name = "SK", }
                });

                foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml"))
                {
                    c.IncludeXmlComments(item, true);
                }
                c.DocInclusionPredicate((docName, action) => true);
            });

            builder.Services.AddMasaMinimalAPIs(opt =>
            {
                opt.PluralizeServiceName = false;  
                var loadMinimalApiAssembies = opt.Assemblies.ToList();
                loadMinimalApiAssembies.Add(Assembly.Load("SKernel.Service")); //业务层程序集
                opt.Assemblies = loadMinimalApiAssembies;
            });
            builder.Services.AddSemanticKernelFactory(builder.Configuration);
            builder.Services.AddAutoInject();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseMasaExceptionHandler();
            app.MapMasaMinimalAPIs();
            app.UseExceptionHandler(handler =>
            handler.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()!.Error;
                switch (exception)
                {
                    case KernelException
                    {
                        ErrorCode: KernelException.ErrorCodes.FunctionNotAvailable
                    }
                    kernelException:
                        await Results.BadRequest(kernelException.Message).ExecuteAsync(context);
                        break;
                    default:
                        await Results.Problem(exception.Message).ExecuteAsync(context);
                        break;
                }
                }));
            app.Run();
        }
    }
}