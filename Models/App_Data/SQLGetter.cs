using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Net.NetworkInformation;

namespace DashboardApplication
{
    public class SQLGetter
    {
        //Connection string for databases
        private static string ConnectionStringSCCM = ConfigurationManager.ConnectionStrings["sccmConnection"].ConnectionString;
        private static string ConnectionStringEPO = ConfigurationManager.ConnectionStrings["epoConnection"].ConnectionString;
        private static string ConnectionStringSCSM = ConfigurationManager.ConnectionStrings["scsmConnection"].ConnectionString;
        //Final values to be rendered on search page
        public static string SCCMTableString = "";
        public static string EPOTableString = "";
        public static string SCSMTableString = "";
        //Variables to store databases on initial launch
        public static DataTable scsm_data;
        public static DataTable sccm_data;
        public static DataTable sccm_data_network_login;
        public static string collections_overview, collections_changes, collections_notifications, deployments_summary, app_deployments_summary, firefox_patches;
        public static DataTable epo_data;
        public static DataTable epo_data_last_update;
        public static DataTable epo_data_dat_version;
        //Final values to be rendered on dashboard page
        public static string activeJSONresult, threatJSONresult, agentcomplianceJSONResult, healthJSONresult, passfailJSONresult, detectionJSONresult, rebootJSONresult, rebootSeven, rebootFourteen, rebootMonth, rebootTMonth, rebootSMonth, reboot12Month, activepass, inactivepass, inactiveunknown, activeunknown, activefail, inactivefail, unhealthyMachines, agentString, threatString, detectionString, productJSONresult, productString;

        //Retrieve rows from system center service manager 
        public static void GetSCSM()
        {
            //gets hardware id to user id and timestamps for time added, last modified
            var table = new DataTable();
            String query = "SELECT BME1.DisplayName as ComputerName, BME2.Name as UserName, BME1.TimeAdded as TimeAdded, BME1.LastModified as LastModified FROM dbo.Relationship R JOIN dbo.BaseManagedEntity BME1 ON BME1.BaseManagedEntityId = R.SourceEntityId JOIN dbo.BaseManagedEntity BME2 ON BME2.BaseManagedEntityId = R.TargetEntityId WHERE R.IsDeleted = 0 AND R.RelationshipTypeId = '299133D6-D5EB-E4E8-DD84-BD303E0F48DA'";
            using (var da = new SqlDataAdapter(query, ConnectionStringSCSM))
            {
                da.Fill(table);
            }
            scsm_data = table;
        }

        //Retrieve rows to do with Deployments in SCCM
        public static string GetDeploymentData()
        {
            var table = new DataTable();
            String query = "select AssignmentID, CollectionID, CollectionName, SoftwareName, NumberSuccess, NumberUnknown, NumberErrors, SummarizationTime from v_DeploymentSummary order by CollectionID";
            using (var da = new SqlDataAdapter(query, ConnectionStringSCCM))
            {
                da.Fill(table);
            }
         
            deployments_summary = Utils.ConvertDataTableToHTML(table, "deployments_summary");

            var table2 = new DataTable();
            String query2 = "select AssignmentID, TargetCollectionID, Descript, ModificationTime, Success, Unknown, Error, RequirementsNotMet from v_AppDeploymentSummary order by TargetCollectionID";
            using (var da = new SqlDataAdapter(query2, ConnectionStringSCCM))
            {
                da.Fill(table2);
            }
           
            app_deployments_summary = Utils.ConvertDataTableToHTML(table2, "app_deployments_summary");

            var table3 = new DataTable(); 
            String query3 = "select distinct V_R_System.ResourceID, V_R_System.ResourceType, V_R_System.Name0, V_R_System.SMS_Unique_Identifier0, V_R_System.Resource_Domain_OR_Workgr0, V_R_System.Client0 from  V_R_System inner join v_GS_SoftwareProduct on v_GS_SoftwareProduct.ResourceID = V_R_System.ResourceId where v_GS_SoftwareProduct.ProductName like 'Firefox' and v_GS_SoftwareProduct.ProductVersion <= '63'";
            using (var da = new SqlDataAdapter(query3, ConnectionStringSCCM))
            {
                da.Fill(table3);
            }
            firefox_patches = Utils.ConvertDataTableToHTML(table3, "firefox_patches");

            return deployments_summary + "#" + app_deployments_summary + "#" + firefox_patches;
        }
        //Retrieve rows to do with Collections in SCCM
        public static string GetCollectionData()
        {
            var table = new DataTable();
            String query = "select Collections_G.SiteID, CollectionMemberCounts.CollectionID, CollectionMemberCounts.AssignedCount, CollectionMemberCounts.UnassignedCount, CollectionMemberCounts.LastChangeTime from Collections_G, CollectionMemberCounts where CollectionMemberCounts.CollectionID = Collections_G.CollectionID";
            using (var da = new SqlDataAdapter(query, ConnectionStringSCCM))
            {
                da.Fill(table);
            }
            collections_overview = Utils.ConvertDataTableToHTML(table, "collections_overview");

            var table2 = new DataTable();
            String query2 = "select CollectionMachineChanges.SiteID, V_R_System.Name0, CollectionMachineChanges.MachineID from CollectionMachineChanges, V_R_System where CollectionMachineChanges.MachineID = V_R_System.ResourceID";
            using (var da = new SqlDataAdapter(query2, ConnectionStringSCCM))
            {
                da.Fill(table2);
            }
            collections_changes = Utils.ConvertDataTableToHTML(table2, "collections_changes");

            var table3 = new DataTable();
            String query3 = "select CollectionNotifications.RecordID, V_R_System.Name0, CollectionNotifications.MachineID, CollectionNotifications.TableName, CollectionNotifications.ChangeTime  from CollectionNotifications, V_R_System where CollectionNotifications.MachineID = V_R_System.ResourceID";
            using (var da = new SqlDataAdapter(query3, ConnectionStringSCCM))
            {
                da.Fill(table3);
            }
            collections_notifications = Utils.ConvertDataTableToHTML(table3, "collections_notifications");

            return collections_overview + "#" + collections_changes + "#" + collections_notifications;
        }

        //Retrieve rows from mcafee epolicy orchestrator
        public static void GetEPO()
        {
            //get current DAT version of systems
            var table3 = new DataTable();
            String query3 = "SELECT EPOLeafNodeMT.NodeName, EPOProdPropsView_THREATPREVENTION.verDAT32Major FROM EPOLeafNodeMT INNER JOIN EPOProdPropsView_THREATPREVENTION ON EPOLeafNodeMT.AutoID = EPOProdPropsView_THREATPREVENTION.LeafNodeID";
            using (var da = new SqlDataAdapter(query3, ConnectionStringEPO))
            {
                da.Fill(table3);
            }
            epo_data_dat_version = table3;

            //gets agent last communication time with system
            var table2 = new DataTable();
            String query2 = "select NodeName, LastUpdate from EPOLeafNode";
            using (var da = new SqlDataAdapter(query2, ConnectionStringEPO))
            {
                da.Fill(table2);
            }

            epo_data_last_update = table2;

            //general system info
            var table = new DataTable();
            String query = "select ComputerName, UserName, IPAddress from EPOComputerProperties;";
            using (var da = new SqlDataAdapter(query, ConnectionStringEPO))
            {
                da.Fill(table);
            }          
            epo_data = table;
        }

        //Retrieve rows from system center configuration manager
        public static void GetSCCM()
        {
            //retrieve network logins
            var table1 = new DataTable();
            String query = "SELECT vSMS_R_System.Name0, vSMS_R_System.User_Name0, v_GS_NETWORK_ADAPTER_CONFIGURATION.IPAddress0, vSMS_R_System.Last_Logon_Timestamp0 FROM vSMS_R_System INNER JOIN v_GS_NETWORK_ADAPTER_CONFIGURATION ON vSMS_R_System.Name0 = v_GS_NETWORK_ADAPTER_CONFIGURATION.DNSHostName0";
            using (var da = new SqlDataAdapter(query, ConnectionStringSCCM))
            {
                da.Fill(table1);
            }

            sccm_data = table1;

            //general system info
            var table2 = new DataTable();
            String query2 = "SELECT vSMS_R_System.Name0, vSMS_R_System.User_Name0, vSMS_R_System.Last_Logon_Timestamp0, vSMS_R_System.lastLogon0 FROM vSMS_R_System";
            using (var da = new SqlDataAdapter(query2, ConnectionStringSCCM))
            {
                da.Fill(table2);
            }
            sccm_data_network_login = table2;
        }

        //Retrieves sccm last network login for given username
        public static DateTime getLastUpdate(string username)
        {
            DataTable tempTable = new DataTable();
            string query = "SELECT TOP 1 LastLogon0 FROM v_GS_NETWORK_LOGIN_PROFILE WHERE Caption0 = '" + username + "' ORDER BY LastLogon0 desc";
            DateTime lastUpdate = DateTime.MinValue;
            using (var da = new SqlDataAdapter(query, ConnectionStringSCCM))
            {
                da.Fill(tempTable);
            }
            if (tempTable.Rows.Count != 0)
            {
                if (!Convert.IsDBNull(tempTable.Rows[0]["LastLogon0"]))
                {
                    lastUpdate = Convert.ToDateTime(tempTable.Rows[0]["LastLogon0"]);
                }
            }
            return lastUpdate;
        }

        //Temporary function to test networking issues...
        public static void Search2(string query)
        {
            DataTable table = new DataTable();
            DataTable table2 = new DataTable();
            String x = ConfigurationManager.ConnectionStrings["epoConnection"].ConnectionString;
            String y = ConfigurationManager.ConnectionStrings["sccmConnection"].ConnectionString;
            String z = ConfigurationManager.ConnectionStrings["scsmConnection"].ConnectionString;
            String query2 = "select top 2 NodeName, LastUpdate from EPOLeafNode";
            using (var da = new SqlDataAdapter(query2, x))
            {
                da.Fill(table2);
            }
            EPOTableString = Utils.ConvertDataTableToHTML(table2, "epoResult");
   
            String query4 = "SELECT top 2 BME1.DisplayName as ComputerName, BME2.Name as UserName, BME1.TimeAdded as TimeAdded, BME1.LastModified as LastModified FROM dbo.Relationship R JOIN dbo.BaseManagedEntity BME1 ON BME1.BaseManagedEntityId = R.SourceEntityId JOIN dbo.BaseManagedEntity BME2 ON BME2.BaseManagedEntityId = R.TargetEntityId WHERE R.IsDeleted = 0 AND R.RelationshipTypeId = '299133D6-D5EB-E4E8-DD84-BD303E0F48DA'";
            using (var da = new SqlDataAdapter(query4, z))
            {
                da.Fill(table);
            }
            SCSMTableString = Utils.ConvertDataTableToHTML(table, "scsmResult");

            /*String query3 = "select top 2 * from VSMS_R_System";
            using (var da = new SqlDataAdapter(query3, y))
            {
                da.Fill(table);
            }*/
            SCCMTableString = "<br>"; //Utils.ConvertDataTableToHTML(table, "sccmResult"); 

        }
        //Main search function: searches sccm, epo, and scsm for given query 
        //TO-DO: clean up this piece of shit
        public static void Search(string query)
        {
            String searchString = query;
            bool anyCharactersInStringAreDigits = searchString.Any(char.IsDigit);
            /* ----------------------------------------------------------------------------- QUERY IS HOSTNAME ----------------------------------------------------------------- */
            if (anyCharactersInStringAreDigits)
            {
                /* --------------------- SEARCH SCCM --------------------- */
                //search through sccm first for hostname
                DataRow[] dataRows3 = sccm_data.Select("Name0 LIKE '%" + searchString + "%'");
                if (dataRows3.Length != 0)
                {
                    DataTable sccmResultComp = sccm_data.Select("Name0 LIKE '%" + searchString + "%'").CopyToDataTable();
                    //for all found rows
                    for (int i = 0; i < sccmResultComp.Rows.Count; i++)
                    {
                        string long_ip = Convert.ToString(sccmResultComp.Rows[i]["IPAddress0"]);
                        //remove garbage at suffix of ip
                        var split_ip = long_ip.Split(',');
                        sccmResultComp.Rows[i]["IPAddress0"] = split_ip[0];
                        //if no valid last logon timestamp then search network login table
                        string value = Convert.ToString(sccmResultComp.Rows[i]["Last_Logon_Timestamp0"]);
                        if (value.Equals(""))
                        {
                            String name = Convert.ToString(sccmResultComp.Rows[i]["User_Name0"]);
                            sccmResultComp.Rows[i]["Last_Logon_Timestamp0"] = getLastUpdate(name);
                        }
                    }
                    //convert time from utc to est
                    for (int i = 0; i < sccmResultComp.Rows.Count; i++)
                    {
                        DateTime easternTime = Utils.ConvertUTCToEST(Convert.ToDateTime(sccmResultComp.Rows[i]["Last_Logon_Timestamp0"]));
                        sccmResultComp.Rows[i]["Last_Logon_Timestamp0"] = easternTime;
                    }
                    //finalize table string to be sent to search page
                    if (sccmResultComp != null)
                    {
                        SCCMTableString = Utils.ConvertDataTableToHTML(sccmResultComp, "sccmResultComp");
                    }
                }
                else
                {
                    SCCMTableString = "<br>No computers found in SCCM<br><br>";
                }
                /* --------------------- SEARCH EPO --------------------- */
                //search through epo for hostname
                DataRow[] dataRows4 = epo_data.Select("ComputerName LIKE '%" + searchString + "%'");
                if (dataRows4.Length != 0)
                {
                    DataTable epoResultComp = epo_data.Select("ComputerName LIKE '%" + searchString + "%'").CopyToDataTable();
                    epoResultComp.Columns.Add("verDAT32Major", typeof(int));
                    epoResultComp.Columns.Add("LastCommunication", typeof(DateTime));
                    //for all rows found
                    for (int i = 0; i < epoResultComp.Rows.Count; i++)
                    {
                        //edit hostname to create valid query
                        string compName = epoResultComp.Rows[i]["ComputerName"].ToString();
                        if (compName.Contains("\'"))
                        {
                            var index = compName.IndexOf("\'");
                            compName = compName.Insert(index, "\'");
                        }
                        //retrieve DAT version
                        DataRow[] d = epo_data_dat_version.Select("NodeName LIKE '%" + compName + "%'");
                        int temp = Convert.ToInt32(d[0]["verDAT32Major"]);
                        epoResultComp.Rows[i]["verDAT32Major"] = temp;
                        //retrieve last communication time
                        DataRow[] d2 = epo_data_last_update.Select("NodeName LIKE '%" + compName + "%'");
                        if (Convert.IsDBNull(d2[0]["LastUpdate"])) { }
                        else
                        {
                            //convert time from utc to est
                            DateTime temp2 = Convert.ToDateTime(d2[0]["LastUpdate"]);
                            epoResultComp.Rows[i]["LastCommunication"] = temp2;
                            DateTime est = Utils.ConvertUTCToEST(Convert.ToDateTime((epoResultComp.Rows[i]["LastCommunication"])));
                            epoResultComp.Rows[i]["LastCommunication"] = est;
                        }
                    }
                    //finalize string to be rendered on search page
                    if (epoResultComp != null)
                    {
                        EPOTableString = Utils.ConvertDataTableToHTML(epoResultComp, "epoResultComp");
                    }
                }
                else
                {
                    EPOTableString = "<br>No computers found in EPO<br><br>";
                }
                //remove alphabet characters for querying scsm
                var noDigitSearchString = Utils.GetNumbers(searchString);
                /* --------------------- SEARCH SCSM --------------------- */
                //search scsm for hostname
                DataRow[] dataRows5 = scsm_data.Select("ComputerName LIKE '%" + noDigitSearchString + "%'");
                if (dataRows5.Length != 0)
                {
                    DataTable scsmResult = scsm_data.Select("ComputerName LIKE '%" + noDigitSearchString + "%'").CopyToDataTable();
                    //for all rows found convert time values to est
                    for (int i = 0; i < scsmResult.Rows.Count; i++)
                    {
                        DateTime easternTime = Utils.ConvertUTCToEST(Convert.ToDateTime((scsmResult.Rows[i]["TimeAdded"])));
                        scsmResult.Rows[i]["TimeAdded"] = easternTime;
                        DateTime easternTime2 = Utils.ConvertUTCToEST(Convert.ToDateTime((scsmResult.Rows[i]["LastModified"])));
                        scsmResult.Rows[i]["LastModified"] = easternTime2;
                    }
                    //finalize string for rendering to search page
                    if (scsmResult != null)
                    {
                        SCSMTableString = Utils.ConvertDataTableToHTML(scsmResult, "scsmResult");
                    }
                }
                else
                {
                    SCSMTableString = "<br>No computers found in SCSM<br><br>";
                }
            }
            /* -------------------------------------------------------------------------------- QUERY IS USERNAME ----------------------------------------------------------------------------*/
            else
            {
                DataRow[] dataRows = sccm_data.Select("User_Name0 LIKE '%" + searchString + "%'");
                if (dataRows.Length != 0)
                {
                    /* --------------------- SEARCH SCCM --------------------- */ 
                    DataTable sccmResult = sccm_data.Select("User_Name0 LIKE '%" + searchString + "%'").CopyToDataTable();
                    //for all rows found
                    for (int i = 0; i < sccmResult.Rows.Count; i++)
                    {
                        //remove garbage at end of ip
                        string value = Convert.ToString(sccmResult.Rows[i]["Last_Logon_Timestamp0"]);
                        string long_ip = Convert.ToString(sccmResult.Rows[i]["IPAddress0"]);
                        var split_ip = long_ip.Split(',');
                        sccmResult.Rows[i]["IPAddress0"] = split_ip[0];
                        if (value.Equals(""))
                        {
                            //if no last logon time search network login table
                            String name = Convert.ToString(sccmResult.Rows[i]["User_Name0"]);
                            sccmResult.Rows[i]["Last_Logon_Timestamp0"] = getLastUpdate(name);
                        }
                    }
                    for (int i = 0; i < sccmResult.Rows.Count; i++)
                    {
                        //convert time to est
                        DateTime easternTime = Utils.ConvertUTCToEST(Convert.ToDateTime(sccmResult.Rows[i]["Last_Logon_Timestamp0"]));
                        sccmResult.Rows[i]["Last_Logon_Timestamp0"] = easternTime;
                    }
                    //finalize string for rendering
                    if (sccmResult != null)
                    {
                        SCCMTableString = Utils.ConvertDataTableToHTML(sccmResult, "sccmResult");
                    }
                }
                else
                {
                    SCCMTableString = "<br>No users found in SCCM<br><br>";
                }
                /* --------------------- SEARCH EPO --------------------- */
                //search through epo with username
                DataRow[] dataRows2 = epo_data.Select("UserName LIKE '%" + searchString + "%'");
                if (dataRows2.Length != 0)
                {
                    DataTable epoResult = epo_data.Select("UserName LIKE '%" + searchString + "%'").CopyToDataTable();
                    epoResult.Columns.Add("verDAT32Major", typeof(int));
                    epoResult.Columns.Add("LastCommunication", typeof(DateTime));
                    //for all rows found
                    for (int i = 0; i < epoResult.Rows.Count; i++)
                    {
                        string compName = epoResult.Rows[i]["ComputerName"].ToString();
                        if (compName.Contains("\'"))
                        {
                            //edit hostname for further querying
                            var index = compName.IndexOf("\'");
                            compName = compName.Insert(index, "\'");
                        }
                        //retrieve DAT version
                        DataRow[] d3 = epo_data_dat_version.Select("NodeName LIKE '%" + compName + "%'");
                        int temp = Convert.ToInt32(d3[0]["verDAT32Major"]);
                        epoResult.Rows[i]["verDAT32Major"] = temp;
                        //retrieve last communication 
                        DataRow[] d2 = epo_data_last_update.Select("NodeName LIKE '%" + compName + "%'");
                        if (Convert.IsDBNull(d2[0]["LastUpdate"]))
                        {                     
                        }
                        else
                        {
                            //convert times to est
                            DateTime temp2 = Convert.ToDateTime(d2[0]["LastUpdate"]);
                            epoResult.Rows[i]["LastCommunication"] = temp2;
                            DateTime est = Utils.ConvertUTCToEST(Convert.ToDateTime((epoResult.Rows[i]["LastCommunication"])));
                            epoResult.Rows[i]["LastCommunication"] = est;
                        }
                    }
                    //finalize string for rendering on search page
                    if (epoResult != null)
                    {
                        EPOTableString = Utils.ConvertDataTableToHTML(epoResult, "epoResult");
                    }
                }
                else
                {
                    EPOTableString = "<br>No users found in EPO<br><br>";
                }
                /* --------------------- SEARCH SCSM --------------------- */
                //search through scsm with username
                DataRow[] dataRows3 = scsm_data.Select("UserName LIKE '%" + searchString + "%'");
                if (dataRows3.Length != 0)
                {
                    DataTable scsmResult = scsm_data.Select("UserName LIKE '%" + searchString + "%'").CopyToDataTable();
                    //for all rows found
                    for (int i = 0; i < scsmResult.Rows.Count; i++)
                    {
                        //convert time from utc to est
                        DateTime easternTime = Utils.ConvertUTCToEST(Convert.ToDateTime((scsmResult.Rows[i]["TimeAdded"])));
                        scsmResult.Rows[i]["TimeAdded"] = easternTime;
                        DateTime easternTime2 = Utils.ConvertUTCToEST(Convert.ToDateTime((scsmResult.Rows[i]["LastModified"])));
                        scsmResult.Rows[i]["LastModified"] = easternTime2;
                    }
                    //finalize string for rendering on search page
                    if (scsmResult != null)
                    {
                        SCSMTableString = Utils.ConvertDataTableToHTML(scsmResult, "scsmResult");
                    }
                }
                else
                {
                    SCSMTableString = "<br>No users found in SCSM<br><br>";
                }
            }
        }

        /* 
        Function to retrieve data for dashboard page
        Queries represent pie chart data
        Notice that queries for epo contain timestamps 
        */
        public static string GetDashData()
        {
            //retrieves active/inactive computer from sccm
            var table = new DataTable();
            String query = "Declare @CollectionID as Varchar(8) Declare @TotalClientInstalled as Numeric(8) Declare @ClientActive as Numeric(8) Declare @ClientInActive as Numeric(8) Set @CollectionID = 'SMSDM003' select @TotalClientInstalled = ( select COUNT(*) as 'Count' from v_FullCollectionMembership where CollectionID = @CollectionID and v_FullCollectionMembership.ResourceID in ( Select Vrs.ResourceID from v_R_System Vrs inner join v_CH_ClientSummary Ch on Vrs.ResourceID = ch.ResourceID where (Ch.ClientActiveStatus = 1 or Ch.ClientActiveStatus = 0) and Vrs.Operating_System_Name_and0 like '%Workstation%')) select @ClientActive = ( select COUNT(*) as 'Count' from v_FullCollectionMembership where CollectionID = @CollectionID and v_FullCollectionMembership.ResourceID in (Select Vrs.ResourceID from v_R_System Vrs inner join v_CH_ClientSummary Ch on Vrs.ResourceID = ch.ResourceID where (Ch.ClientActiveStatus = 1) and Vrs.Operating_System_Name_and0 like '%Workstation%')) select @ClientInActive = ( select COUNT(*) as 'Count' from v_FullCollectionMembership where CollectionID = @CollectionID and v_FullCollectionMembership.ResourceID in ( Select Vrs.ResourceID from v_R_System Vrs inner join v_CH_ClientSummary Ch on Vrs.ResourceID = ch.ResourceID where (Ch.ClientActiveStatus = 0) and Vrs.Operating_System_Name_and0 like '%Workstation%') ) select @TotalClientInstalled as 'TotalClientInstalled', @ClientActive as 'ClientActive', @ClientInActive as 'ClientInActive', case when (@TotalClientInstalled = 0) or (@TotalClientInstalled is null) Then '100' Else (round(@ClientActive/ convert (float,@TotalClientInstalled)*100,2)) End as 'ClientActivePercent';";
            using (var da = new SqlDataAdapter(query, ConnectionStringSCCM))
            {
                da.Fill(table);
            }
            activeJSONresult = JsonConvert.SerializeObject(table);

            //Time stamps used to adjust for epo queries
            DateTime dt = DateTime.Now;
            DateTime yt = dt.AddDays(-1);
            yt = yt.AddHours(4.0);
            String yesterday_date = String.Format("{0:s}", yt);

            //retrieves 360 top threats within 24 hours information from epo
            var table2 = new DataTable();
            String query2 = "select top 360 count(*) as 'count', [EPOEventFilterDesc].[Name], [EPOEventFilterDesc].[Name] from [EPOEvents] left join [EPOEventFilterDesc] on [EPOEvents].[ThreatEventID] = [EPOEventFilterDesc].[EventId] and (EPOEventFilterDesc.Language='0409') where ( ( [EPOEvents].[DetectedUTC] >= '" + yesterday_date + "' ) and ( not [EPOEvents].[ThreatCategory] LIKE 'ops%' ) ) group by [EPOEventFilterDesc].[Name] order by 'count' desc, [EPOEventFilterDesc].[Name] asc";
            using (var da = new SqlDataAdapter(query2, ConnectionStringEPO))
            {
                da.Fill(table2);
            }
            threatJSONresult = JsonConvert.SerializeObject(table2);

            //retrieves agent compliance summary from epo
            var table3 = new DataTable();
            String query3 = "select count(*) as 'count', [BooleanPieChart_Alias].[ChartColor], [BooleanPieChart_Alias].[ChartColor] from ( select ( case when ( ( [EPOLeafNode].[LastUpdate] >= '" + yesterday_date + "' ) and ( ( [EPOProdPropsView_EPOAGENT].[verProductMajor] >= 1 ) ) ) then 1 when ( not ( ( [EPOLeafNode].[LastUpdate] >= '" + yesterday_date + "' ) and ( ( [EPOProdPropsView_EPOAGENT].[verProductMajor] >= 1 ) ) ) ) then 0 else -1 end ) as ChartColor from [EPOLeafNode] left join [EPOProdPropsView_EPOAGENT] on [EPOLeafNode].[AutoID] = [EPOProdPropsView_EPOAGENT].[LeafNodeID] ) as BooleanPieChart_Alias group by [BooleanPieChart_Alias].[ChartColor] order by [BooleanPieChart_Alias].[ChartColor] desc";
            using (var da = new SqlDataAdapter(query3, ConnectionStringEPO))
            {
                da.Fill(table3);
            }
            agentcomplianceJSONResult = JsonConvert.SerializeObject(table3);

            //retrieves unhealthy/healthy machines from sccm
            var table4 = new DataTable();
            String query4 = "Declare @CollectionID as Varchar(8) Declare @TotalMachines as Numeric(5) Declare @Healthy as Numeric(5) Declare @UnHealthy as Numeric(5) Set @CollectionID = 'SMS00001' select @TotalMachines = (select COUNT(*) from v_FullCollectionMembership where CollectionID = @CollectionID) select @Healthy = ( select COUNT(*) from v_FullCollectionMembership where CollectionID = @CollectionID and IsAssigned = 1 and IsActive = 1 and IsObsolete != 1 and IsClient = 1 ) select @UnHealthy = ( select COUNT(*) from v_FullCollectionMembership where CollectionID = @CollectionID and ResourceID Not in (select ResourceID from v_FullCollectionMembership where CollectionID = @CollectionID and IsAssigned = 1 and IsActive = 1 and IsObsolete != 1 and IsClient = 1 ) ) select @TotalMachines as 'TotalMachines', @Healthy as 'Healthy', @UnHealthy as 'UnHealthy', (Select(@Healthy/@TotalMachines)*100) as 'HealthyPercent'";
            using (var da = new SqlDataAdapter(query4, ConnectionStringSCCM))
            {
                da.Fill(table4);
            }
            healthJSONresult = JsonConvert.SerializeObject(table4);

            //client health status from sccm
            var table5 = new DataTable();
            String query5 = "SELECT ClientstateDescription AS [ClientState], COUNT(ResourceID) AS [NumberofClients], CONVERT(varchar, 100 * count(*) / tot,1) as 'Percent' FROM v_CH_ClientSummary, (SELECT COUNT(*) as tot FROM v_CH_ClientSummary) x GROUP BY ClientstateDescription, tot ORDER BY [NumberofClients] DESC";
            using (var da = new SqlDataAdapter(query5, ConnectionStringSCCM))
            {
                da.Fill(table5);
            }
            passfailJSONresult = JsonConvert.SerializeObject(table5);

            //product updates within 24 hours from epo
            var table6 = new DataTable();
            String query6 = "select count(*) as 'count', [BooleanPieChart_Alias].[ChartColor], [BooleanPieChart_Alias].[ChartColor] from ( select ( case when ( [EPOProductEvents].[TVDEventID] = 2401 ) then 1 when ( not ( [EPOProductEvents].[TVDEventID] = 2401 ) ) then 0 else -1 end ) as ChartColor from [EPOProductEvents] where ( ( ( [EPOProductEvents].[TVDEventID] = 2401 ) or ( [EPOProductEvents].[TVDEventID] = 2402 ) ) and ( [EPOProductEvents].[DetectedUTC] >= '" + yesterday_date + "' ) ) ) as BooleanPieChart_Alias group by [BooleanPieChart_Alias].[ChartColor] order by [BooleanPieChart_Alias].[ChartColor] desc";
            using (var da = new SqlDataAdapter(query6, ConnectionStringEPO))
            {
                da.Fill(table6);
            }
            productJSONresult = JsonConvert.SerializeObject(table6);

            //detection per product from epo
            var table7 = new DataTable();
            String query7 = "select count(*) as 'count', [EPOEvents].[AnalyzerName], [EPOEvents].[AnalyzerName] from [EPOEvents] where ( ( [EPOEvents].[DetectedUTC] >= '" + yesterday_date + "' ) and ( not [EPOEvents].[ThreatCategory] LIKE 'ops%' ) ) group by [EPOEvents].[AnalyzerName] order by 'count' desc, [EPOEvents].[AnalyzerName] asc";
            using (var da = new SqlDataAdapter(query7, ConnectionStringEPO))
            {
                da.Fill(table7);
            }
            detectionJSONresult = JsonConvert.SerializeObject(table7);

            //total machine reboot statistics from sccm
            var table8 = new DataTable();
            String query8 = "Declare @CollectionID as Varchar(8) Set @CollectionID = 'SMS00001' select 'Last Reboot within 7 days' as TimePeriod,Count(sys.Name0) as 'Count',1 SortOrder from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(day,-7, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Last Reboot within 14 days' as TimePeriod,Count(sys.Name0) as 'Count',2 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(day,-14, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Last Reboot within 1 month' as TimePeriod,Count(sys.Name0) as 'Count',3 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(month,-1, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Last Reboot within 3 months' as TimePeriod,Count(sys.Name0) as 'Count',4 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(month,-3, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Last Reboot within 6 months' as TimePeriod,Count(sys.Name0) as 'Count',5 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(month,-6, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Last Reboot within 12 months' as TimePeriod,Count(sys.Name0) as 'Count',6 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where os.LastBootUpTime0 < DATEADD(month,-12, GETDATE()) and ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID UNION select 'Total Machines Count' as TimePeriod,Count(sys.Name0) as 'Count',7 from v_R_System sys inner join v_GS_OPERATING_SYSTEM os on os.ResourceId = sys.ResourceId inner join v_FullCollectionMembership Vf on sys.ResourceID = Vf.ResourceID inner join v_CH_ClientSummary ch on ch.ResourceID = sys.ResourceID where ch.ClientActiveStatus = 1 and sys.Operating_System_Name_and0 like '%workstation%' and Vf.CollectionID = @CollectionID Order By SortOrder";
            using (var da = new SqlDataAdapter(query8, ConnectionStringSCCM))
            {
                da.Fill(table8);
            }
            rebootJSONresult = JsonConvert.SerializeObject(table8);

            //list of systems rebooted within 1 week
            var table11 = new DataTable();
            String query11 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(day,-7, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query11, ConnectionStringSCCM))
            {
                da.Fill(table11);
            }
            rebootSeven = Utils.ConvertDataTableToHTML(table11, "rebootSeven");

            //list of systems rebooted within 2 weeks
            var table12 = new DataTable();
            String query12 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(day,-14, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query12, ConnectionStringSCCM))
            {
                da.Fill(table12);
            }
            rebootFourteen = Utils.ConvertDataTableToHTML(table12, "rebootFourteen");

            //list of systems rebooted within 1 month
            var table13 = new DataTable();
            String query13 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(month,-1, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query13, ConnectionStringSCCM))
            {
                da.Fill(table13);
            }
            rebootMonth = Utils.ConvertDataTableToHTML(table13, "rebootMonth");

            //list of systems rebooted within 3 months
            var table14 = new DataTable();
            String query14 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(month,-3, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query14, ConnectionStringSCCM))
            {
                da.Fill(table14);
            }
            rebootTMonth = Utils.ConvertDataTableToHTML(table14, "rebootTMonth");

            //list of systems rebooted within 6 months
            var table15 = new DataTable();
            String query15 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(month,-6, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query15, ConnectionStringSCCM))
            {
                da.Fill(table15);
            }
            rebootSMonth = Utils.ConvertDataTableToHTML(table15, "rebootSMonth");

            //list of systems rebooted within 1 year
            var table16 = new DataTable();
            String query16 = "SELECT Netbios_Name0, User_Name0, LastBootUpTime0 FROM v_CH_ClientSummary INNER JOIN vSMS_R_System ON v_CH_ClientSummary.ResourceID = vSMS_R_System.ItemKey INNER JOIN v_GS_OPERATING_SYSTEM ON v_CH_ClientSummary.ResourceID = v_GS_OPERATING_SYSTEM.ResourceID where LastBootUpTime0 < DATEADD(month,-12, GETDATE()) and ClientActiveStatus = 1";
            using (var da = new SqlDataAdapter(query16, ConnectionStringSCCM))
            {
                da.Fill(table16);
            }
            reboot12Month = Utils.ConvertDataTableToHTML(table16, "reboot12Month");

            //list of active-pass systems from sccm
            var table17 = new DataTable();
            String query17 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Active/Pass'";
            using (var da = new SqlDataAdapter(query17, ConnectionStringSCCM))
            {
                da.Fill(table17);
            }
            activepass = Utils.ConvertDataTableToHTML(table17, "activepass");

            //list of inactive-pass systems
            var table18 = new DataTable();
            String query18 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Inactive/Pass'";
            using (var da = new SqlDataAdapter(query18, ConnectionStringSCCM))
            {
                da.Fill(table18);
            }
            inactivepass = Utils.ConvertDataTableToHTML(table18, "inactivepass");

            //list of inactive-unknown systems
            var table19 = new DataTable();
            String query19 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Inactive/Unknown'";
            using (var da = new SqlDataAdapter(query19, ConnectionStringSCCM))
            {
                da.Fill(table19);
            }
            inactiveunknown = Utils.ConvertDataTableToHTML(table19, "inactiveunknown");

            //list of active-unknown systems
            var table20 = new DataTable();
            String query20 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Active/Unknown'";
            using (var da = new SqlDataAdapter(query20, ConnectionStringSCCM))
            {
                da.Fill(table20);
            }
            activeunknown = Utils.ConvertDataTableToHTML(table20, "activeunknown");

            //list of active-fail systems
            var table21 = new DataTable();
            String query21 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Active/Fail'";
            using (var da = new SqlDataAdapter(query21, ConnectionStringSCCM))
            {
                da.Fill(table21);
            }
            activefail = Utils.ConvertDataTableToHTML(table21, "activefail");

            //list of inactive-fail systems 
            var table22 = new DataTable();
            String query22 = "SELECT LastHealthEvaluation, ClientStateDescription, Netbios_Name0, User_Name0 FROM vSMS_R_System INNER JOIN v_CH_ClientSummary ON vSMS_R_System.ItemKey = v_CH_ClientSummary.ResourceID WHERE ClientStateDescription = 'Inactive/Fail'";
            using (var da = new SqlDataAdapter(query22, ConnectionStringSCCM))
            {
                da.Fill(table22);
            }
            inactivefail = Utils.ConvertDataTableToHTML(table22, "inactivefail");

            //list of unhealthy macchines from sccm
            var table23 = new DataTable();
            String query23 = "Declare @CollectionID as Varchar(8) Set @CollectionID = 'SMS00001' select Netbios_Name0, User_Name0, Last_Logon_Timestamp0, Hardware_ID0 from v_R_system where V_R_System.ResourceID in ( select ResourceID from v_FullCollectionMembership where CollectionID = @CollectionID and ResourceID Not in (select ResourceID from v_FullCollectionMembership where CollectionID = @CollectionID and IsAssigned = 1 and IsActive = 1 and IsObsolete != 1 and IsClient = 1))";
            using (var da = new SqlDataAdapter(query23, ConnectionStringSCCM))
            {
                da.Fill(table23);
            }
            unhealthyMachines = Utils.ConvertDataTableToHTML(table23, "unhealthyMachines");

            //agent communication summary from epo 
            var table24 = new DataTable();
            String query24 = "select NodeName, LastUpdate, verProductMajor from [EPOLeafNode] left join [EPOProdPropsView_EPOAGENT] on [EPOLeafNode].[AutoID] = [EPOProdPropsView_EPOAGENT].[LeafNodeID] where EPOLeafNode.LastUpdate <= '" + yesterday_date + "' or EPOProdPropsView_EPOAGENT.verProductMajor <= 1";
            using (var da = new SqlDataAdapter(query24, ConnectionStringEPO))
            {
                da.Fill(table24);
            }
            agentString = Utils.ConvertDataTableToHTML(table24, "agentString");

            //list of top 360 threats within 24 hours from epo
            var table25 = new DataTable();
            String query25 = "select top 360 Name, DetectedUTC, TargetHostName, TargetUserName, ThreatCategory, ThreatName, ThreatActionTaken from [EPOEvents] left join [EPOEventFilterDesc] on [EPOEvents].[ThreatEventID] = [EPOEventFilterDesc].[EventId] and (EPOEventFilterDesc.Language='0409') where ( ( [EPOEvents].[DetectedUTC] >= '2018-07-09T12:58:25' ) and ( not [EPOEvents].[ThreatCategory] LIKE 'ops%' ) ) order by name desc;";
            using (var da = new SqlDataAdapter(query25, ConnectionStringEPO))
            {
                da.Fill(table25);
            }
            threatString = Utils.ConvertDataTableToHTML(table25, "threatString");

            //list of detections per product from epo
            var table26 = new DataTable();
            String query26 = "select DetectedUTC, AnalyzerName, AnalyzerHostName, ThreatSeverity, ThreatType, ThreatCategory, ThreatName, ThreatActionTaken, ThreatHandled from [EPOEvents] where ( ( [EPOEvents].[DetectedUTC] >= '" + yesterday_date + "' ) and ( not [EPOEvents].[ThreatCategory] LIKE 'ops%' ) )";
            using (var da = new SqlDataAdapter(query26, ConnectionStringEPO))
            {
                da.Fill(table26);
            }
            detectionString = Utils.ConvertDataTableToHTML(table26, "detectionString");

            //list of failed product updates epo
            var table27 = new DataTable();
            String query27 = "select TVDEventID, TVDSeverity, DetectedUTC, HostName, UserName, ProductCode from EPOProductEvents where DetectedUTC >= '" + yesterday_date + "' and TVDEventID = 2402";
            using (var da = new SqlDataAdapter(query27, ConnectionStringEPO))
            {
                da.Fill(table27);
            }
            productString = Utils.ConvertDataTableToHTML(table27, "productString");

            return activeJSONresult + "#" + threatJSONresult + "#" + agentcomplianceJSONResult + "#" + healthJSONresult + "#" + passfailJSONresult + "#" + productJSONresult + "#" + detectionJSONresult + "#" + rebootJSONresult + "#" + rebootSeven + "#" + rebootFourteen + "#" + rebootMonth + "#" + rebootTMonth + "#" + rebootSMonth + "#" + reboot12Month + "#" + inactivepass + "#" + inactiveunknown + "#" + activeunknown + "#" + activefail + "#" + inactivefail + "#" + unhealthyMachines + "#" + agentString + "#" + threatString + "#" + detectionString + "#" + productString;
        }
    }
}

