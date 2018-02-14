using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Resources;

namespace MHU_First_Run_Assistant
{
    public class ADInterface
    {
        public ADInterface()
        {

        }

        public List<string> GetOrgUnits(string root, List<string> badOU)
        {
            //create object to hold OU's
            List<string> Results = new List<string>();
            
            //Make Directory connection to AD and search for OU's at argument provided search root
            DirectoryEntry SearchRoot = new DirectoryEntry(root);

            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);
            searcher.Filter = "(objectCategory=organizationalUnit)";

            //get name of each result
            foreach (SearchResult res in searcher.FindAll())
            {
                var OU = res.GetDirectoryEntry();

                Results.Add(OU.Name);
            }

            //clean up list for presentation
            for (int i = 0; i < Results.Count; i++)
            {
                Results[i] = Results[i].Replace("OU=", "");
            }

            //remove banned OU Groups
            foreach (string bad in badOU)
            {
                Results.Remove(bad);
            }

            //place list in alphabetical order
            Results.Sort();

            //Clean up after dyn memeory and connections
            SearchRoot.Dispose();
            searcher.Dispose();

           //return list for presentation
            return Results;
        }           // Finds Available OU's under a given search root while excluding blacklisted OU names

        private bool VerifyOU(string destination, string root)
        {
            //create object to hold OU's
            List<string> Results = new List<string>();

            //Make Directory connection to AD and search for OU's at argument provided search root

            DirectoryEntry SearchRoot = new DirectoryEntry(root);

            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);
            searcher.Filter = "(objectCategory=organizationalUnit)";

            //get name of each result
            foreach (SearchResult res in searcher.FindAll())
            {
                var OU = res.GetDirectoryEntry();

                Results.Add(OU.Name);
            }

            //clean up given destination to include AD formating prefix for comparison to results in AD structure format.
            destination = "OU=" + destination;

            if (Results.Contains(destination))
            {
                return true;
            }
            else
            {
                return false;
            }
        }                     // Verifies a given OU Exists under a given search root

        public void MoveOrgUnit(string destination, string root)
        {
            //Verify function provides valid destinations
            if (VerifyOU(destination, root))
            {
                /*
                try
                {
                */
                    //Locate Machine LDAP location and create LDAP prinipal object for the computer
                    PrincipalContext Computer_Search_Context = new PrincipalContext(ContextType.Domain);
                    ComputerPrincipal Local_Computer = new ComputerPrincipal(Computer_Search_Context);
                    Local_Computer.Name = System.Environment.MachineName;

                    PrincipalSearcher Search_Results = new PrincipalSearcher(Local_Computer);

                    var Local_Computer_Principal = Search_Results.FindOne();


                    //Open a directory connection for the current machine obejct
                    DirectoryEntry Computers_Current_Location = new DirectoryEntry("LDAP://" + Local_Computer_Principal.DistinguishedName, Properties.Resources.ServiceAccountUserName, Properties.Resources.ServiceAccountPassword);

                    //Set up destination OU based on user suppled input.
                    // Complex line provides formating adjustments for converting user perferences into proper format for DirectoryEntry.
                    DirectoryEntry Destination_OU = new DirectoryEntry("LDAP://" + ("OU=" + destination + ", " + root.Replace("LDAP://", "")).Replace(", ", ","), Properties.Resources.ServiceAccountUserName, Properties.Resources.ServiceAccountPassword);

                    //Move the current computer object to the destination OU
                    Computers_Current_Location.MoveTo(Destination_OU);
                //}
                
                /*
                catch(System.DirectoryServices.DirectoryServicesCOMException)
                {
                    throw new System.Security.Authentication.AuthenticationException("Service Account Credential are Invalid");
                }
                */
            }
            else
            {
                throw new System.ArgumentException("Destination target is not found under the search root");
            }
            return;
        }                   // Moves a Computer from one OU to another

        public void UpdateDescription(string building, string room, string type)
        {
            try
            {
                //Locate Machine LDAP location and create LDAP prinipal object for the computer
                PrincipalContext Computer_Search_Context = new PrincipalContext(ContextType.Domain);
                ComputerPrincipal Local_Computer = new ComputerPrincipal(Computer_Search_Context);
                Local_Computer.Name = System.Environment.MachineName;

                PrincipalSearcher Search_Results = new PrincipalSearcher(Local_Computer);

                var Local_Computer_Principal = Search_Results.FindOne();


                //Open a directory connection for the current machine obejct
                DirectoryEntry Computers_Current_Location = new DirectoryEntry("LDAP://" + Local_Computer_Principal.DistinguishedName, Properties.Resources.ServiceAccountUserName, Properties.Resources.ServiceAccountPassword);
                Computers_Current_Location.UsePropertyCache = false;    //insures changes to machine description are commited immdately.

                //Build new Description for the computer from supplied arguments
                string NewDecsription = building + "," + room + "," + type;

                //perform changes to machine description
                Computers_Current_Location.Properties["description"].Value = NewDecsription;

             }
            catch
             {
                 throw new SystemException("Could not SET Machine Description in AD");
             }
             



        }   // Updates a Computer Objects Description with the building, room, and type argument information
    }
}
