using Luna.Publish.Public.Client;

namespace Luna.Partner.Public.Client
{
    public class PipelineEndpoint : BaseMLComponent
    {
        public PipelineEndpoint(string id, string name) :
            base(id, name, LunaAPIType.Pipeline)
        {

        }
    }
}
