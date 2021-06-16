
namespace Luna.Publish.Public.Client
{
    public class APIVersion
    {
        public APIVersion(string name, BaseAPIVersionProp properties)
        {
            this.Name = name;
            this.Properties = properties;
        }

        public string Name { get; set; }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
