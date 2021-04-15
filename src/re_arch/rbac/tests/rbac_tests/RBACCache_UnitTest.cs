using Luna.RBAC.Data.DataContracts;
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
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app2_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, put_action));
        }

        [TestMethod]
        public void AddDuplicateWildcardRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsFalse(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app2_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, put_action));
        }

        [TestMethod]
        public void RemoveWildcardRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));

            Assert.IsTrue(cache.RemoveRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, put_action));

            Assert.IsFalse(cache.RemoveRBACRule(user1_user, application_wildcard_scope, get_action));
        }

        [TestMethod]
        public void AddExactRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));
        }

        [TestMethod]
        public void AddDuplicatedExactRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.AddRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));
        }

        [TestMethod]
        public void RemoveExactRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.RemoveRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));
        }

        [TestMethod]
        public void AddOverlapWildcardAndExactRBACRuleAndValidate()
        {
            RBACCache cache = new RBACCache();
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));

            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));

            Assert.IsTrue(cache.RemoveRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));

            Assert.IsTrue(cache.AddRBACRule(user1_user, application_wildcard_scope, get_action));
            Assert.IsTrue(cache.RemoveRBACRule(user1_user, application_app1_scope, get_action));

            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsTrue(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));
        }

        [TestMethod]
        public void ValidateWithEmptyCache()
        {
            RBACCache cache = new RBACCache();

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app2_scope, get_action));
            Assert.IsFalse(cache.HasRBACRule(user2_user, application_app1_scope, get_action));

            Assert.IsFalse(cache.HasRBACRule(user1_user, application_app1_scope, put_action));
        }
    }
}
