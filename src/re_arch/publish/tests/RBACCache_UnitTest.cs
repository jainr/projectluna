using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Luna.RBAC.Test
{
    [TestClass]
    public class RBACCache_UnitTest
    {
        private const string user1_user = "user1";
        private const string user2_user = "user2";

        private const string application_wildcard_scope = "/applications/*";
        private const string application_app1_scope = "/applications/app1";
        private const string application_app2_scope = "/applications/app2";

        private const string get_action = "GET";
        private const string put_action = "PUT";

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestMethod]
        public void AddWildcardRBACRuleAndValidate()
        {
        }
    }
}
