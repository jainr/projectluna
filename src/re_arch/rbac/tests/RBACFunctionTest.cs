using Luna.Common.Utils;
using Luna.RBAC.Clients;
using Luna.RBAC.Data;
using Luna.RBAC.Public.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.RBAC.Test
{
    [TestClass]
    public class RBACFunctionTest
    {
        private LunaRequestHeaders _headers;
        private ILogger<RBACFunctionsImpl> _logger;

        private const string admin1 = "admin1";
        private const string admin2 = "admin2";
        private RoleAssignmentRequest admin1RA;
        private RoleAssignmentRequest admin2RA;


        private const string publisher1 = "publisher1";
        private const string publisher2 = "publisher2";
        private RoleAssignmentRequest publisher1RA;
        private RoleAssignmentRequest publisher2RA;

        private const string resource1 = "/applications/app2";
        private const string resource2 = "/applications/app1";

        private OwnershipRequest ownership1_1;
        private OwnershipRequest ownership1_2;
        private OwnershipRequest ownership2_1;
        private OwnershipRequest ownership2_2;

        private RBACQueryRequest query_admin1_resource1;
        private RBACQueryRequest query_admin2_resource1;
        private RBACQueryRequest query_publisher1_resource1;
        private RBACQueryRequest query_publisher1_resource2;
        private RBACQueryRequest query_publisher2_resource1;

        private RBACQueryRequest query_publisher1_create_app;
        private RBACQueryRequest query_publisher2_create_app;

        [TestInitialize]
        public void TestInitialize()
        {
            _headers = new LunaRequestHeaders();

            var mock = new Mock<ILogger<RBACFunctionsImpl>>();
            this._logger = mock.Object;

            admin1RA = new RoleAssignmentRequest()
            {
                UserName = admin1,
                Uid = admin1,
                Role = RBACRole.SystemAdmin.ToString()
            };

            admin2RA = new RoleAssignmentRequest()
            {
                UserName = admin2,
                Uid = admin2,
                Role = RBACRole.SystemAdmin.ToString()
            };

            publisher1RA = new RoleAssignmentRequest()
            {
                UserName = publisher1,
                Uid = publisher1,
                Role = RBACRole.Publisher.ToString()
            };

            publisher2RA = new RoleAssignmentRequest()
            {
                UserName = publisher2,
                Uid = publisher2,
                Role = RBACRole.Publisher.ToString()
            };

            ownership1_1 = new OwnershipRequest()
            {
                Uid = publisher1,
                ResourceId = resource1
            };

            ownership1_2 = new OwnershipRequest()
            {
                Uid = publisher1,
                ResourceId = resource2
            };

            ownership2_1 = new OwnershipRequest()
            {
                Uid = publisher2,
                ResourceId = resource1
            };

            ownership2_2 = new OwnershipRequest()
            {
                Uid = publisher2,
                ResourceId = resource2
            };

            query_admin1_resource1 = new RBACQueryRequest
            {
                Uid = admin1,
                ResourceId = resource1
            };

            query_admin2_resource1 = new RBACQueryRequest
            {
                Uid = admin2,
                ResourceId = resource1
            };

            query_publisher1_resource1 = new RBACQueryRequest
            {
                Uid = publisher1,
                ResourceId = resource1
            };

            query_publisher1_resource2 = new RBACQueryRequest
            {
                Uid = publisher1,
                ResourceId = resource2
            };

            query_publisher2_resource1 = new RBACQueryRequest
            {
                Uid = publisher2,
                ResourceId = resource1
            };

            query_publisher1_create_app = new RBACQueryRequest
            {
                Uid = publisher1,
                Action = RBACActions.CREATE_NEW_APPLICATION
            };

            query_publisher2_create_app = new RBACQueryRequest
            {
                Uid = publisher2,
                Action = RBACActions.CREATE_NEW_APPLICATION
            };
        }

        [TestMethod]
        public async Task AddAndRemoveSystemAdmin()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IRBACFunctionsImpl function = new RBACFunctionsImpl(context, this._logger, new RoleAssignmentMapper(), new OwnershipMapper());
                
                // Add admin 1
                var response = await function.AddRoleAssignmentAsync(admin1RA, this._headers);
                Assert.IsInstanceOfType(response, typeof(RoleAssignmentResponse));
                Assert.AreEqual(response.Role, admin1RA.Role);
                Assert.AreEqual(response.Uid, admin1RA.Uid);
                Assert.AreEqual(response.UserName, admin1RA.UserName);

                var queryResponse = await function.CanAccessAsync(query_admin1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                // Add admin 1 again, expect conflict exception
                await Assert.ThrowsExceptionAsync<LunaConflictUserException>(async () =>
                {
                    await function.AddRoleAssignmentAsync(admin1RA, this._headers);
                });

                queryResponse = await function.CanAccessAsync(query_admin1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                // Add admin 2
                response = await function.AddRoleAssignmentAsync(admin2RA, this._headers);
                Assert.IsInstanceOfType(response, typeof(RoleAssignmentResponse));
                Assert.AreEqual(response.Role, admin2RA.Role);
                Assert.AreEqual(response.Uid, admin2RA.Uid);
                Assert.AreEqual(response.UserName, admin2RA.UserName);

                queryResponse = await function.CanAccessAsync(query_admin1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_admin2_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                // Remove admin 1
                await function.RemoveRoleAssignmentAsync(admin1RA, this._headers);

                queryResponse = await function.CanAccessAsync(query_admin1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);
            }

        }

        [TestMethod]
        public async Task AddAndRemovePublisher()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IRBACFunctionsImpl function = new RBACFunctionsImpl(context, this._logger, new RoleAssignmentMapper(), new OwnershipMapper());

                // Add publisher 1
                var response = await function.AddRoleAssignmentAsync(publisher1RA, this._headers);
                Assert.IsInstanceOfType(response, typeof(RoleAssignmentResponse));
                Assert.AreEqual(response.Role, publisher1RA.Role);
                Assert.AreEqual(response.Uid, publisher1RA.Uid);
                Assert.AreEqual(response.UserName, publisher1RA.UserName);

                var queryResponse = await function.CanAccessAsync(query_publisher1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);

                // Publisher 1 own resource 1
                var ownerResponse = await function.AssignOwnershipAsync(ownership1_1, this._headers);
                Assert.IsInstanceOfType(ownerResponse, typeof(OwnershipResponse));
                Assert.AreEqual(ownerResponse.ResourceId, ownership1_1.ResourceId);
                Assert.AreEqual(ownerResponse.Uid, ownership1_1.Uid);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource2, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_publisher2_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);

                // Add publisher 2
                response = await function.AddRoleAssignmentAsync(publisher2RA, this._headers);
                Assert.IsInstanceOfType(response, typeof(RoleAssignmentResponse));
                Assert.AreEqual(response.Role, publisher2RA.Role);
                Assert.AreEqual(response.Uid, publisher2RA.Uid);
                Assert.AreEqual(response.UserName, publisher2RA.UserName);

                // Publisher 2 own resource 1
                ownerResponse = await function.AssignOwnershipAsync(ownership2_1, this._headers);
                Assert.IsInstanceOfType(ownerResponse, typeof(OwnershipResponse));
                Assert.AreEqual(ownerResponse.ResourceId, ownership2_1.ResourceId);
                Assert.AreEqual(ownerResponse.Uid, ownership2_1.Uid);

                queryResponse = await function.CanAccessAsync(query_publisher2_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                // Publisher 1 own resource 2
                ownerResponse = await function.AssignOwnershipAsync(ownership1_2, this._headers);
                Assert.IsInstanceOfType(ownerResponse, typeof(OwnershipResponse));
                Assert.AreEqual(ownerResponse.ResourceId, ownership1_2.ResourceId);
                Assert.AreEqual(ownerResponse.Uid, ownership1_2.Uid);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource2, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                // Remove publisher 1
                await function.RemoveOwnershipAsync(ownership1_1, this._headers);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource1, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_publisher1_resource2, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

            }
        }

        [TestMethod]
        public async Task PublisherCreateApp()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IRBACFunctionsImpl function = new RBACFunctionsImpl(context, this._logger, new RoleAssignmentMapper(), new OwnershipMapper());

                // Add publisher 1
                var response = await function.AddRoleAssignmentAsync(publisher1RA, this._headers);
                Assert.IsInstanceOfType(response, typeof(RoleAssignmentResponse));
                Assert.AreEqual(response.Role, publisher1RA.Role);
                Assert.AreEqual(response.Uid, publisher1RA.Uid);
                Assert.AreEqual(response.UserName, publisher1RA.UserName);

                var queryResponse = await function.CanAccessAsync(query_publisher1_create_app, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsTrue(queryResponse.CanAccess);

                queryResponse = await function.CanAccessAsync(query_publisher2_create_app, this._headers);
                Assert.IsInstanceOfType(queryResponse, typeof(RBACQueryResultResponse));
                Assert.IsFalse(queryResponse.CanAccess);

            }
        }
    }
}
