using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace DashboardApplication
{
    public class Utils
    {
        //this function takes a dataTable and id value and returns a valid html table
        public static string ConvertDataTableToHTML(DataTable dt, string id)
        {
            string html = "<table id=\"" + id + "\" class=\"compact, stripe, cell-border\">";
            html += "<thead><tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr></thead>";
            html += "<tbody>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</tbody></table>";
            return html;
        }

        //convert DateTime object from UTC to EST format
        public static DateTime ConvertUTCToEST(DateTime utc)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(utc, easternZone);
            return easternTime;
        }

        //extract numeric characters from string
        public static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
    }
}