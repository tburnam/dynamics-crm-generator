// -----------------------------------------------------------------------
// <copyright file="PackageTemplate.cs" company="MicrosoftCorporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Uii.Common.Entities;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace Microsoft.PowerApps.<%= solutionName %>.PVSPackage
{
    /// <summary>
    /// Import package starter frame. 
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension
    {
        public int DefaultMonths = 2;
        public int DailyInterval = 1;
        public string jobName = "Analysis Results Cleanup Job";

        /// <summary>
        /// Folder Name for the Package data. 
        /// </summary>
        public override string GetImportPackageDataFolderName
        {
            get
            {
                return "msdyn_<%= solutionName %>";
            }
        }

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText
        {
            get { return "This solution is for running Solution Checker on solutions to analyze for quality defects."; }
        }

        /// <summary>
        /// Long name of the Import Package. 
        /// </summary>
        public override string GetLongNameOfImport
        {
            get { return "Solution Checker Solution"; }
        }

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        /// <returns>Name of the Import Package</returns>
        public override string GetNameOfImport(bool plural)
        {
            return "<%= solutionName %>";
        }

        /// <summary>
        /// Called When the package is initialized. 
        /// </summary>
        public override void InitializeCustomExtension()
        {
        }

        /// <summary>
        /// Called Before Import Completes. 
        /// </summary>
        /// <returns>BeforeImportStage value</returns>
        public override bool BeforeImportStage()
        {
            return true;
        }

        /// <summary>
        /// Called for each UII record imported into the system
        /// This is UII Specific and is not generally used by Package Developers
        /// </summary>
        /// <param name="app">App Record</param>
        /// <returns>The Application record</returns>
        public override ApplicationRecord BeforeApplicationRecordImport(ApplicationRecord app)
        {
            return app;  // do nothing here. 
        }

        /// <summary>
        /// Called during a solution upgrade while both solutions are present in the target CRM instance. 
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring. 
        /// </summary>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }

        /// <summary>
        /// Called after Import completes. 
        /// </summary>
        /// <returns>After primary import value</returns>
        public override bool AfterPrimaryImport()
        {
            PackageLog.Log("AfterPrimaryImport: Start");

            try
            {
                if (RuntimeSettings != null)
                {
                    PackageLog.Log("AfterPrimaryImport: RuntimeSettings is not null");
                    Settings.AddRuntimeSettings(RuntimeSettings);
                }

                var appUser = Settings.GetApplicationUser();

                // If the application user Id isn't passed in as parameter, it will be Guid.Empty. In that case we shouldn't proceed further.
                if (appUser.Id != Guid.Empty)
                {
                    PackageLog.Log(string.Format("AfterPrimaryImport: Application user Id is: {0}", appUser.Id));
                    
                    // TODO: Handle First-party application AAD provisioning (auth + single call to graphAPI?)
                    
                    //1.Create application user if it doesn't exist. Get its Id if one exists.
                    var systemUserId = CreateOrGetApplicationUser(appUser);

                    //2.Associate role with user.
                    if (systemUserId != Guid.Empty)
                    {
                        PackageLog.Log(string.Format("AfterPrimaryImport: System user Id is: {0}", systemUserId));
                        AssignRoleToUser(systemUserId, appUser.RoleId);
                        AssignRoleToUser(systemUserId, appUser.ExportRoleId);
                    }
                }
                PackageLog.Log("AfterPrimaryImport: Creating Bulk Delete Job");
                CreateBulkDeleteJob(DefaultMonths);

                PackageLog.Log("AfterPrimaryImport: Completed");
            }
            catch (Exception ex)
            {
                PackageLog.Log(string.Format("AfterPrimaryImport: Exception: {0}", ex.ToString()));
                PackageLog.Log(string.Format("AfterPrimaryImport: StackTrace: {0}", ex.StackTrace));

                return false;
            }

            return true;
        }

        private Guid CreateOrGetApplicationUser(ApplicationUser appUser)
        {
            QueryExpression appUserQuery = new QueryExpression
            {
                EntityName = "systemuser",
                ColumnSet = new ColumnSet("systemuserid"),
                Criteria =
                {
                    Conditions =    {
                                        new ConditionExpression("applicationid", ConditionOperator.Equal, appUser.Id)
                                    }
                }
            };

            EntityCollection appUsers = CrmSvc.RetrieveMultiple(appUserQuery);

            if (appUsers != null && appUsers.Entities.Count > 0)
            {
                return appUsers.Entities[0].Id;
            }

            return CreateUser(appUser);
        }

        private Guid CreateUser(ApplicationUser appUser)
        {
            QueryExpression businessUnitQuery = new QueryExpression
            {
                EntityName = "businessunit",
                ColumnSet = new ColumnSet("businessunitid"),
                Criteria =
                                {
                    Conditions =
                                    {
                        new ConditionExpression("parentbusinessunitid",
                            ConditionOperator.Null)
                                    }
                }
            };


            EntityCollection defaultBusinessUnits = CrmSvc.RetrieveMultiple(businessUnitQuery);
            var defaultBusinessUnit = defaultBusinessUnits.Entities[0] ?? null;

            if (defaultBusinessUnit != null)
            {
                Entity systemUser = new Entity("systemuser");
                systemUser["firstname"] = appUser.FirstName;
                systemUser["lastname"] = appUser.LastName;
                systemUser["internalemailaddress"] = appUser.InternalEmailAddress;
                systemUser["applicationid"] = appUser.Id;
                systemUser["businessunitid"] = new EntityReference
                {
                    LogicalName = "businessunit",
                    Name = "businessunit",
                    Id = defaultBusinessUnit.Id
                };

                return CrmSvc.Create(systemUser);
            }

            return Guid.Empty;
        }

        private void AssignRoleToUser(Guid appUserId, Guid roleId)
        {
            // Associate the user with the role if the role isn't already assigned to the user
            if (roleId != Guid.Empty && appUserId != Guid.Empty && !IsRoleAssignedToUser(appUserId, roleId))
            {
                CrmSvc.Associate(
                            "systemuser",
                            appUserId,
                            new Relationship("systemuserroles_association"),
                            new EntityReferenceCollection() { new EntityReference("role", roleId) });

            }
        }

        private bool IsRoleAssignedToUser(Guid sytemUserId, Guid roleId)
        {
            try
            {
                // Establish a SystemUser link for a query.
                LinkEntity systemUserLink = new LinkEntity()
                {
                    LinkFromEntityName = "systemuserroles",
                    LinkFromAttributeName = "systemuserid",
                    LinkToEntityName = "systemuserroles",
                    LinkToAttributeName = "systemuserid",
                    LinkCriteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            "systemuserid", ConditionOperator.Equal, sytemUserId)
                    }
                }
                };

                // Build the query.
                QueryExpression linkQuery = new QueryExpression()
                {
                    EntityName = "role",
                    ColumnSet = new ColumnSet("roleid"),
                    LinkEntities =
                {
                    new LinkEntity()
                    {
                        LinkFromEntityName = "role",
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = "systemuserroles",
                        LinkToAttributeName = "roleid",
                        LinkEntities = {systemUserLink}
                    }
                },
                    Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("roleid", ConditionOperator.Equal, roleId)
                    }
                }
                };

                // Retrieve matching roles.
                EntityCollection matchEntities = CrmSvc.RetrieveMultiple(linkQuery);

                return (matchEntities.Entities.Count > 0);
            }
            catch
            {
                return false;
            }
        }

        private void CreateBulkDeleteJob(int months)
        {
            if (!BulkDeleteJobExists())
            {
                string frequency =
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                  "FREQ=DAILY;INTERVAL={0};", DailyInterval);


                QueryExpression query = new QueryExpression
                {
                    EntityName = "msdyn_analysisresult",
                    ColumnSet = new ColumnSet("msdyn_name"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                                {
                                    new ConditionExpression
                                    {
                                        AttributeName = "createdon",
                                        Operator = ConditionOperator.OlderThanXMonths,
                                        Values = { months }
                                    }
                                }
                    }
                };

                BulkDeleteRequest bulkDeleteRequest =
                    new BulkDeleteRequest
                    {
                        JobName = jobName,
                        QuerySet = new QueryExpression[] { query },
                        StartDateTime = DateTime.Now.Date,
                        RecurrencePattern = frequency,
                        SendEmailNotification = false,
                        ToRecipients = new Guid[] { },
                        CCRecipients = new Guid[] { }
                    };

                CrmSvc.Execute(bulkDeleteRequest);
            }
        }

        private bool BulkDeleteJobExists()
        {
            QueryExpression bulkExpression = new QueryExpression()
            {
                EntityName = "asyncoperation",
                ColumnSet = new ColumnSet(new string[] { "asyncoperationid", "name" }),
                Criteria ={
                           Conditions=
                           {
                              new ConditionExpression("operationtype", ConditionOperator.Equal, 13),
                              new ConditionExpression("name", ConditionOperator.Equal, jobName),

                           }
                       }
            };

            return CrmSvc.RetrieveMultiple(bulkExpression).Entities.Count>0;
        }
    }
}