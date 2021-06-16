
namespace Luna.Publish.Public.Client
{
    public class PublishQueryParameterConstants
    {
        public static string[] GetValidKeyNames()
        {
            return new string[] { PRIMARY_KEY_NAME, SECONDARY_KEY_NAME };
        }

        public const string KEY_NAME_QUERY_PARAMETER_NAME = "key-name";
        public const string PRIMARY_KEY_NAME = "primaryKey";
        public const string SECONDARY_KEY_NAME = "secondaryKey";
        public const string ROLE_QUERY_PARAMETER_NAME = "role";
        public const string ADMIN_ROLE_NAME = "admin";
    }
}
