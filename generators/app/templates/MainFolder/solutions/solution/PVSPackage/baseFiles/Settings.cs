// -----------------------------------------------------------------------
// <copyright file="Settings.cs" company="MicrosoftCorporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Microsoft.PowerApps.<%= solutionName %>.PVSPackage
{
    internal class Constants
    {
        // Application user property names
        public static readonly string RoleId = "roleid"; // Id of the (custom) role to be assigned to application user.
                                                         //Represents Id of the application for which a user needs to be create in CRM. Application service principal should be present same AD as CRM.
        public static readonly string ExportRoleId = "exportroleid";
        public static readonly string ApplicationId = "applicationid";
        // Represents First name of the application user.
        public static readonly string FirstName = "firstname";
        //Represents Last name of the application user.
        public static readonly string LastName = "lastname";
        //Represents Email address for the application user. Should be unique.
        public static readonly string InternalEmailAddress = "internalemailaddress";

        public class DefaultAppUser
        {
            public static readonly string FirstName = "";
            public static readonly string LastName = "<%= creators %>";
            public static readonly string InternalEmailAddress = "<%= creators %>";
            public static readonly Guid RoleId = Guid.Parse("E05075EB-9D74-E811-A95C-000D3A1C53E4");
            public static readonly Guid ExportRoleId = Guid.Parse("4931681D-8163-E811-A965-000D3A11FE32");
            // TODO: Update with first-party application ID
            public static readonly Guid ApplicationId = Guid.Parse("297753d2-2c78-44ca-b63c-4c5168bd2864");

        }
    }

    public sealed class ApplicationUser
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InternalEmailAddress { get; set; }
        public Guid ExportRoleId { get; set; }
    }

    public static class Settings
    {
        private static Dictionary<string, object> AppUserAttributes = new Dictionary<string, object> {
            { Constants.ApplicationId, Constants.DefaultAppUser.ApplicationId },
            { Constants.RoleId, Constants.DefaultAppUser.RoleId},
            { Constants.FirstName, Constants.DefaultAppUser.FirstName},
            { Constants.LastName, Constants.DefaultAppUser.LastName},
            { Constants.InternalEmailAddress, Constants.DefaultAppUser.InternalEmailAddress},
            { Constants.ExportRoleId, Constants.DefaultAppUser.ExportRoleId}
        };

        internal static void AddRuntimeSettings(Dictionary<string, Object> runtimeSettings)
        {
            // Assuming that only 1 application user is passed as part of runtime settings.
            // If more than 1 user needs to be passed. Following code would need modification.
            // The format of RuntimeSetting is:
            // ApplicationId=[GUID OF APP]|RoleId=[Role Id]|FirstName=[first name]|....
            foreach (var setting in runtimeSettings.Keys)
            {
                if (AppUserAttributes.ContainsKey(setting.ToLowerInvariant()))
                {
                    AppUserAttributes[setting.ToLowerInvariant()] = runtimeSettings[setting];
                }
            }

        }

        internal static ApplicationUser GetApplicationUser()
        {
            return new ApplicationUser
            {
                Id = Guid.Parse(AppUserAttributes[Constants.ApplicationId].ToString()),
                RoleId = Guid.Parse(AppUserAttributes[Constants.RoleId].ToString()),
                FirstName = (string)AppUserAttributes[Constants.FirstName],
                LastName = (string)AppUserAttributes[Constants.LastName],
                InternalEmailAddress = (string)AppUserAttributes[Constants.InternalEmailAddress],
                ExportRoleId = Guid.Parse(AppUserAttributes[Constants.ExportRoleId].ToString()),
            };
        }
    }
}
