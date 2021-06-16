using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client
{
    public class RBACActions
    {
        public const string CREATE_NEW_APPLICATION = @"\applications\create";
        public const string CREATE_MARKETPLACE_OFFER = @"\offers\create";
        public const string LIST_MARKETPLACE_OFFER = @"\offers\list";
        public const string LIST_APPLICATIONS = @"\applications\list";
        public const string READ_PARTNER_SERVICES = @"\partnerservices\read";

        public static string[] PublisherAllowedActions = new string[] { CREATE_NEW_APPLICATION, LIST_APPLICATIONS, READ_PARTNER_SERVICES };

        public static string[] ValidActions = new string[] 
        { 
            CREATE_NEW_APPLICATION, 
            LIST_APPLICATIONS, 
            READ_PARTNER_SERVICES ,
            CREATE_MARKETPLACE_OFFER,
            LIST_MARKETPLACE_OFFER
        };
    }
}
