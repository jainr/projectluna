

namespace Luna.Publish.Public.Client
{
    public class PipelineEndpointAPIVersionProp : BaseAPIVersionProp
    {
        public PipelineEndpointAPIVersionProp(PipelineEndpointAPIVersionType type)
            :base(type.ToString())
        {
        }

    }
}
