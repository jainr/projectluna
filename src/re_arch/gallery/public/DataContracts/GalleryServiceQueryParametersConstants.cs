using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class GalleryServiceQueryParametersConstants
    {
        public static string[] GetValidKeyNames()
        {
            return new string[] { SUBCRIPTION_PRIMARY_KEY_VALUE, SUBCRIPTION_SECONDARY_KEY_VALUE };
        }

        public const string SUBCRIPTION_KEY_NAME_PARAM_NAME = "key-name";
        public const string SUBCRIPTION_PRIMARY_KEY_VALUE = "PrimaryKey";
        public const string SUBCRIPTION_SECONDARY_KEY_VALUE = "SecondaryKey";
    }
}
