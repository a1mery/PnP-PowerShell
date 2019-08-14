﻿using OfficeDevPnP.Core.Entities;
using OfficeDevPnP.Core.Framework.Graph;
using OfficeDevPnP.Core.Utilities;
using SharePointPnP.PowerShell.CmdletHelpAttributes;
using SharePointPnP.PowerShell.Commands.Base;
using SharePointPnP.PowerShell.Commands.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.PowerShell.Commands.Graph
{
    [Cmdlet(VerbsCommon.New, "PnPUnifiedGroup")]
    [CmdletHelp("Creates a new Office 365 Group (aka Unified Group). Requires the Azure Active Directory application permission 'Group.ReadWrite.All'.",
        Category = CmdletHelpCategory.Graph,
        SupportedPlatform = CmdletSupportedPlatform.Online)]
    [CmdletExample(
       Code = "PS:> New-PnPUnifiedGroup -DisplayName $displayName -Description $description -MailNickname $nickname",
       Remarks = "Creates a public Office 365 Group with all the required properties",
       SortOrder = 1)]
    [CmdletExample(
       Code = "PS:> New-PnPUnifiedGroup -DisplayName $displayName -Description $description -MailNickname $nickname -Owners $arrayOfOwners -Members $arrayOfMembers",
       Remarks = "Creates a public Office 365 Group with all the required properties, and with a custom list of Owners and a custom list of Members",
       SortOrder = 2)]
    [CmdletExample(
       Code = "PS:> New-PnPUnifiedGroup -DisplayName $displayName -Description $description -MailNickname $nickname -IsPrivate",
       Remarks = "Creates a private Office 365 Group with all the required properties",
       SortOrder = 3)]
    [CmdletExample(
       Code = "PS:> New-PnPUnifiedGroup -DisplayName $displayName -Description $description -MailNickname $nickname -Owners $arrayOfOwners -Members $arrayOfMembers -IsPrivate",
       Remarks = "Creates a private Office 365 Group with all the required properties, and with a custom list of Owners and a custom list of Members",
       SortOrder = 4)]
    public class NewPnPUnifiedGroup : PnPGraphCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "The Display Name of the Office 365 Group.")]
        public String DisplayName;

        [Parameter(Mandatory = true, HelpMessage = "The Description of the Office 365 Group.")]
        public String Description;

        [Parameter(Mandatory = true, HelpMessage = "The Mail Nickname of the Office 365 Group. Cannot contain spaces.")]
        public String MailNickname;

        [Parameter(Mandatory = false, HelpMessage = "The array UPN values of the group's owners.")]
        public String[] Owners;

        [Parameter(Mandatory = false, HelpMessage = "The array UPN values of the group's members.")]
        public String[] Members;

        [Parameter(Mandatory = false, HelpMessage = "Makes the group private when selected.")]
        public SwitchParameter IsPrivate;

        [Parameter(Mandatory = false, HelpMessage = "The path to the logo file of to set.")]
        public string GroupLogoPath;

        [Parameter(Mandatory = false, HelpMessage = "Creates a MS Teams team associated with created group.")]
        public SwitchParameter CreateTeam;

        [Parameter(Mandatory = false, HelpMessage = "Specifying the Force parameter will skip the confirmation question.")]
        public SwitchParameter Force;

        protected override void ExecuteCmdlet()
        {
            if (MailNickname.Contains(" "))
            {
                throw new ArgumentException("MailNickname cannot contain spaces.");
            }
            bool forceCreation;

            if (!Force)
            {
                var candidates = UnifiedGroupsUtility.ListUnifiedGroups(AccessToken,
                    mailNickname: MailNickname,
                    endIndex: 1);
                // ListUnifiedGroups retrieves groups with starts-with, so need another check
                var existingGroup = candidates.Any(g => g.MailNickname.Equals(MailNickname, StringComparison.CurrentCultureIgnoreCase));

                forceCreation = !existingGroup || ShouldContinue(string.Format(Resources.ForceCreationOfExistingGroup0, MailNickname), Resources.Confirm);
            }
            else
            {
                forceCreation = true;
            }

            if (forceCreation)
            {
                var group = UnifiedGroupsUtility.CreateUnifiedGroup(
                    displayName: DisplayName,
                    description: Description,
                    mailNickname: MailNickname,
                    accessToken: AccessToken,
                    owners: Owners,
                    members: Members,
                    groupLogoPath: GroupLogoPath,
                    isPrivate: IsPrivate,
                    createTeam: CreateTeam);

                WriteObject(group);
            }
        }
    }
}