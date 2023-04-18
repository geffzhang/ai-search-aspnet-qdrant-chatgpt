using Masa.BuildingBlocks.Service.MinimalAPIs;
using Microsoft.OpenApi.Models;
using SKernel;
using System.Reflection;

namespace SKernel.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

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
                loadMinimalApiAssembies.Add(Assembly.Load("SKernel.Service")); //ҵ������
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

            app.Run();
        }
    }
}