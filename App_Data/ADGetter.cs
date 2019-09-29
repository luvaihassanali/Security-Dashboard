using System;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace DashboardApplication
{
    public class ADGetter
    {

        //function to determine whether query is username or hostname 
        static public string parseData(string query)
        {
            //check for numeric characters in query
            bool isCharDigit = query.Any(char.IsDigit);
            if (isCharDigit)
            {
                return GetComputer(query);
            }
            else
            {
                return GetUser(query);
            }
        }

        //search Active directory for computers
        static public string GetComputer(string name)
        {
            //domain connection
            using (var pc = new PrincipalContext(ContextType.Domain, "Domain Name"))
            {
                //search through all computer objects in ad for matching name
                ComputerPrincipal computer = ComputerPrincipal.FindByIdentity(pc, IdentityType.Name, name);
                if (computer != null)
                {
                    DataTable tempTable = new DataTable();
                    //save computer name & last logon for table
                    tempTable.Columns.Add("Name", typeof(string));
                    tempTable.Columns.Add("LastLogon", typeof(DateTime));
                    //convert time to est 
                    DateTime easternTime = Utils.ConvertUTCToEST(computer.LastLogon.Value);
                    tempTable.Rows.Add(computer.Name, easternTime);
                    //finalize string for webpage
                    return Utils.ConvertDataTableToHTML(tempTable, "adComputer");
                }
                else
                {
                    return "<br>Computer not found in AD<br><br>";
                }
            }
        }

        //search Active directory for usernames
        static public string GetUser(string name)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, "Domain Name"))
            {
                //search through all user objects in ad for a match
                UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, name);

                if (user != null)
                {
                    //convert time value to est
                    DateTime easternTime = Utils.ConvertUTCToEST(user.LastLogon.Value);
                    DataTable tempTable = new DataTable();
                    //save sAMAccountName & last logon attribute for table
                    tempTable.Columns.Add("sAMAccountName", typeof(string));
                    tempTable.Columns.Add("LastLogon", typeof(DateTime));
                    tempTable.Rows.Add(user.SamAccountName, easternTime);
                    //final string to be rendered on search page
                    return Utils.ConvertDataTableToHTML(tempTable, "adUser");
                }
                else
                {
                    return "<br>User not found in AD<br><br>";
                }
            }
        }     
    }
}