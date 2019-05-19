using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KL.HttpScheduler.Api.Common
{
    public class BasePathDocumentFilter : IDocumentFilter
    {
        private string BasePath { get; }
        public BasePathDocumentFilter(string basePath)
        {
            BasePath = basePath;
        }
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.BasePath = BasePath;
        }
    }
}
