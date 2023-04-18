using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel;
using SKernel.Factory.Config;
using SKernel.Factory;
using Microsoft.AspNetCore.Http;

namespace SKernel.Service
{
    public static class Extensions
    {
        public static ApiKey ToApiKeyConfig(this HttpRequest request)
        {
            var apiConfig = new ApiKey();

            if (request.Headers.TryGetValue(Headers.TextCompletionKey, out var textKey))
                apiConfig.Text = textKey.First()!;

            apiConfig.Embedding = request.Headers.TryGetValue(Headers.EmbeddingKey, out var embeddingKey)
                ? embeddingKey.First()!
                : apiConfig.Text;
            apiConfig.Chat = request.Headers.TryGetValue(Headers.ChatCompletionKey, out var chatKey)
                ? chatKey.First()!
                : apiConfig.Text;

            return apiConfig;
        }

        public static bool TryGetKernel(this HttpRequest request, SemanticKernelFactory factory,
            out IKernel? kernel, IList<string>? selected = null)
        {
            var api = request.ToApiKeyConfig();
            kernel = api is { Text: { }, Embedding: { } } ? factory.Create(api, selected) : null;
            return kernel != null;
        }

        public static IResult ToResult(this SKContext result, IList<string>? skills) => (result.ErrorOccurred)
            ? Results.BadRequest(result.LastErrorDescription)
            : Results.Ok(new Message { Variables = result.Variables, Skills = skills ?? new List<string>() });
    }
}
