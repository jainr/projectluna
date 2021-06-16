using Luna.Publish.Public.Client;

namespace Luna.Partner.Public.Client
{
    public class RealtimeEndpoint : BaseMLComponent
    {

        public RealtimeEndpoint(string id, string name) :
            base(id, name, LunaAPIType.Realtime)
        {

        }
    }
}
