using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;

namespace LAWPullADGroupMembers
{
    class Program
    {

        static void Main(string[] args)
        {

            //Using List of AD Group ObjectGUIDs Since AD Admins Like to Move Groups or Rename OUs
            List<string> lLAWGrpGUIDs = new List<string>();
            lLAWGrpGUIDs.Add("6e0788a7-8221-4d22-adfb-f18afcfc1d05");

            PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "OU", "DC=OU,DC=AD3,DC=UCDAVIS,DC=EDU");

            foreach (string lawGrpGUID in lLAWGrpGUIDs)
            {
                //Var for Group's LDAP Path Based Upon AD GUID
                string grpLDAPPath = "LDAP://ad3.ucdavis.edu/<GUID=" + lawGrpGUID + ">";

                //Check to See Group Exists in AD
                if (DirectoryEntry.Exists(grpLDAPPath))
                {
                    //Pull Directory Entry of AD Group
                    DirectoryEntry deADGroup = new DirectoryEntry(grpLDAPPath);

                    //Var for Group's DN Value
                    string uADGrpDN = deADGroup.Properties["distinguishedname"][0].ToString();

                    //AD Group Principal
                    GroupPrincipal grpPrincipal = GroupPrincipal.FindByIdentity(prctxOU, IdentityType.DistinguishedName, uADGrpDN);

                    //Check Current Group Membership
                    if (grpPrincipal.Members.Count > 0)
                    {

                        //Loop Through Group Membership (Not Nested) For Nested Change to True
                        foreach (var crntMbr in grpPrincipal.GetMembers(false))
                        {
                            //Pull Directory Entry for Group Member
                            DirectoryEntry deMember = crntMbr.GetUnderlyingObject() as DirectoryEntry;

                            //Null Check on Users DN
                            if (deMember != null && deMember.Properties["distinguishedName"].Count > 0)
                            {

                                //Load Only AD3 Users
                                if (deMember.Properties["objectClass"].Contains("user") == true && deMember.Properties["objectClass"].Contains("computer") == false
                                    && deMember.Properties["distinguishedName"][0].ToString().ToLower().Contains(",dc=ou,dc=ad3,dc=ucdavis,dc=edu") == false)
                                {
                                    string LawMbrLogin = string.Empty;
                                    string LawMbrDisplayName = string.Empty;
                                    string LawMbrEmailAddress = string.Empty;

                                    //Load User ID
                                    LawMbrLogin = deMember.Properties["samAccountName"][0].ToString().ToLower();

                                    //Load Display Name
                                    int nGMDisplayNameCount = deMember.Properties["displayName"].Count;
                                    if (nGMDisplayNameCount > 0)
                                    {
                                        LawMbrDisplayName = deMember.Properties["displayName"][0].ToString();
                                    }

                                    //Mail Address Check
                                    int nGMMailCount = deMember.Properties["mail"].Count;
                                    if (nGMMailCount > 0)
                                    {
                                        LawMbrEmailAddress = deMember.Properties["mail"][0].ToString().ToLower();
                                    }

                                    //Proxy Address Check (for Primary SMTP Address)
                                    int nGMProxyAddrCount = deMember.Properties["proxyAddresses"].Count;
                                    if (nGMProxyAddrCount > 0)
                                    {
                                        for (int x = 0; x <= nGMProxyAddrCount - 1; x++)
                                        {

                                            if (deMember.Properties["proxyAddresses"][x].ToString().StartsWith("SMTP:"))
                                            {
                                                LawMbrEmailAddress = deMember.Properties["proxyAddresses"][x].ToString().ToLower().Replace("smtp:", "");
                                            }

                                        }

                                    }//End of Proxy Address Check


                                    Console.WriteLine("Login: " + LawMbrLogin);
                                    Console.WriteLine("Display Name: " + LawMbrDisplayName);
                                    Console.WriteLine("Email Address: " + LawMbrEmailAddress);
                                    Console.WriteLine(" ");

                                }//End of Only AD3 Users Checks

                            }//End of deMember Null\Empty Check on distinguishedName

                        }//End of Get Members Foreach

                    }//End of Membership Count

                }//End of Directory Entry Check (Sometimes Things Get Deleted)

            }//End of lLAWGrpGUIDs Foreach


            Console.WriteLine(" ");
            Console.WriteLine("-----End of Program-----");
            Console.ReadLine();


        }
    }
}
