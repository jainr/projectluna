using Luna.RBAC.Clients;
using Luna.RBAC.Data.Entities;
using Luna.RBAC.Public.Client.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Luna.RBAC.Test
{
    [TestClass]
    public class RBACCache_UnitTest
    {
        private const string admin1 = "admin1";
        private const string admin2 = "admin2";
        private RoleAssignmentDb admin1RA;
        private RoleAssignmentDb admin2RA;

        private const string publisher1 = "publisher1";
        private const string publisher2 = "publisher2";
        private RoleAssignmentDb publisher1RA;
        private RoleAssignmentDb publisher2RA;

        private const string user1 = "user1";
        private const string user2 = "user2";

        private const string resource1 = "/applications/app2";
        private const string resource2 = "/applications/app1";

        private OwnershipDb ownership1_1;
        private OwnershipDb ownership1_2;
        private OwnershipDb ownership2_1;
        private OwnershipDb ownership2_2;

        [TestInitialize]
        public void TestInitialize()
        {
            admin1RA = new RoleAssignmentDb()
            {
                Uid = admin1,
                Role = RBACRole.SystemAdmin.ToString()
            };

            admin2RA = new RoleAssignmentDb()
            {
                Uid = admin2,
                Role = RBACRole.SystemAdmin.ToString()
            };

            publisher1RA = new RoleAssignmentDb()
            {
                Uid = publisher1,
                Role = RBACRole.Publisher.ToString()
            };

            publisher2RA = new RoleAssignmentDb()
            {
                Uid = publisher2,
                Role = RBACRole.Publisher.ToString()
            };

            ownership1_1 = new OwnershipDb()
            {
                Uid = user1,
                ResourceId = resource1
            };

            ownership1_2 = new OwnershipDb()
            {
                Uid = user1,
                ResourceId = resource2
            };

            ownership2_1 = new OwnershipDb()
            {
                Uid = user2,
                ResourceId = resource1
            };

            ownership2_2 = new OwnershipDb()
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
