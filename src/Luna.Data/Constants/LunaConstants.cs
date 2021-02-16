using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.Constants
{
    public class LunaConstants
    {
        public const string SECRET_NOT_CHANGED_VALUE = "notchanged";
        public const string AML_SECRET_NAME_FORMAT = "amlkey-{0}";
        public const string SYNAPSE_SECRET_NAME_FORMAT = "synapse-{0}";
        public const string ADB_SECRET_NAME_FORMAT = "adbkey-{0}";
        public const string GIT_SECRET_NAME_FORMAT = "gitkey-{0}";
        public const string ENDPOINT_AUTH_SECRET_NAME_FORMAT = "epkey-{0}";
        public const string PRIMARY_KEY_SECRET_NAME_FORMAT = "primarykey-{0}";
        public const string SECONDARY_KEY_SECRET_NAME_FORMAT = "secondary-{0}";
        public const string GITHUB_REPO_HOST = "github.com";
        public const string AZURE_DEVOPS_REPO_HOST_SUFFIX = ".visualstudio.com";
        public const string PUBLISHER_TAG_KEY = "publisher";
    }
}
