using Luna.RBAC.Clients;
using Luna.RBAC.Data.Entities;
using Luna.RBAC.Data.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Luna.RBAC.Test
{
    [TestClass]
    public class RBACCache_UnitTest
    {
        private const string admin1 = "admin1";
        private const string admin2 = "admin2";
        private RoleAssignment admin1RA;
        private RoleAssignment admin2RA;

        private const string publisher1 = "publisher1";
        private const string publisher2 = "publisher2";
        private RoleAssignment publisher1RA;
        private RoleAssignment publisher2RA;

        private const string user1 = "user1";
        private const string user2 = "user2";

        private const string resource1 = "/applications/app2";
        private const string resource2 = "/applications/app1";

        private Ownership ownership1_1;
        private Ownership ownership1_2;
        private Ownership ownership2_1;
        private Ownership ownership2_2;

        [TestInitialize]
        public void TestInitialize()
        {
            admin1RA = new RoleAssignment()
            {
                Uid = admin1,
                Role = RBACRoles.SystemAdmin.ToString()
            };

            admin2RA = new RoleAssignment()
            {
                Uid = admin2,
                Role = RBACRoles.SystemAdmin.ToString()
            };

            publisher1RA = new RoleAssignment()
            {
                Uid = publisher1,
                Role = RBACRoles.Publisher.ToString()
            };

            publisher2RA = new RoleAssignment()
            {
                Uid = publisher2,
                Role = RBACRoles.Publisher.ToString()
            };

            ownership1_1 = new Ownership()
            {
                Uid = user1,
                ResourceId = resource1
            };

            ownership1_2 = new Ownership()
            {
                Uid = user1,
                ResourceId = resource2
            };

            ownership2_1 = new Ownership()
            {
                Uid = user2,
                ResourceId = resource1
            };

            ownership2_2 = new Ownership()
            {
                Uid = user2,
                ResourceId = resource2
            };
        }

        [TestMethod]
        public void AddAndRemoveSystemAdmin()
        {
            IRBACCacheClient client = new RBACCacheClient();
            Assert.IsFalse(client.IsSystemAdmin(admin1));
            Assert.IsFalse(client.IsSystemAdmin(admin2));
            Assert.IsTrue(client.AddRoleAssignment(admin1RA));
            Assert.IsTrue(client.IsSystemAdmin(admin1));
            Assert.IsFalse(client.IsSystemAdmin(admin2));
            Assert.IsTrue(client.AddRoleAssignment(admin2RA));
            Assert.IsTrue(client.IsSystemAdmin(admin2));
            Assert.IsFalse(client.AddRoleAssignment(admin1RA));
            Assert.IsTrue(client.RemoveRoleAssignment(admin1RA));
            Assert.IsTrue(client.RemoveRoleAssignment(admin2RA));
            Assert.IsFalse(client.RemoveRoleAssignment(admin1RA));
            Assert.IsFalse(client.IsSystemAdmin(admin1));
            Assert.IsFalse(client.IsSystemAdmin(admin2));
        }

        [TestMethod]
        public void AddAndRemovePublisher()
        {
            IRBACCacheClient client = new RBACCacheClient();
            Assert.IsFalse(client.IsPublisher(publisher1));
            Assert.IsFalse(client.IsPublisher(publisher2));
            Assert.IsTrue(client.AddRoleAssignment(publisher1RA));
            Assert.IsTrue(client.IsPublisher(publisher1));
            Assert.IsFalse(client.IsPublisher(publisher2));
            Assert.IsTrue(client.AddRoleAssignment(publisher2RA));
            Assert.IsTrue(client.IsPublisher(publisher2));
            Assert.IsFalse(client.AddRoleAssignment(publisher1RA));
            Assert.IsTrue(client.RemoveRoleAssignment(publisher1RA));
            Assert.IsTrue(client.RemoveRoleAssignment(publisher2RA));
            Assert.IsFalse(client.RemoveRoleAssignment(publisher1RA));
            Assert.IsFalse(client.IsPublisher(publisher1));
            Assert.IsFalse(client.IsPublisher(publisher2));
        }

        [TestMethod]
        public void AddAndRemoveOwnership()
        {
            IRBACCacheClient client = new RBACCacheClient();
            Assert.IsFalse(client.IsOwnedBy(user1, resource1));
            Assert.IsFalse(client.IsOwnedBy(user2, resource2));
            Assert.IsTrue(client.AssignOwnership(ownership1_1));
            Assert.IsTrue(client.AssignOwnership(ownership2_2));
            Assert.IsTrue(client.IsOwnedBy(user1, resource1));
            Assert.IsTrue(client.IsOwnedBy(user2, resource2));
            Assert.IsFalse(client.IsOwnedBy(user1, resource2));
            Assert.IsFalse(client.IsOwnedBy(user2, resource1));
            Assert.IsTrue(client.AssignOwnership(ownership1_2));
            Assert.IsTrue(client.IsOwnedBy(user1, resource2));
            Assert.IsFalse(client.IsOwnedBy(user2, resource1));
            Assert.IsFalse(client.AssignOwnership(ownership1_1));
            Assert.IsFalse(client.AssignOwnership(ownership2_2));
            Assert.IsTrue(client.RemoveOwnership(ownership1_1));
            Assert.IsFalse(client.RemoveOwnership(ownership1_1));
            Assert.IsFalse(client.IsOwnedBy(user1, resource1));
            Assert.IsTrue(client.IsOwnedBy(user1, resource2));
            Assert.IsTrue(client.RemoveOwnership(ownership1_2));
            Assert.IsFalse(client.IsOwnedBy(user1, resource1));
            Assert.IsFalse(client.IsOwnedBy(user1, resource2));
            Assert.IsTrue(client.IsOwnedBy(user2, resource2));
            Assert.IsFalse(client.IsOwnedBy(user2, resource1));
        }
    }
}
