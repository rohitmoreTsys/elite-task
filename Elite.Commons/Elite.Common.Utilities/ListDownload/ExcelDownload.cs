using ClosedXML.Excel;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.ListDownload
{
    public class ExcelDownload
    {
        public async Task<MemoryStream> DownloadsSpreadsheet(List<ListToDownloadDto> items, string tablename, List<string> ColumnNames, bool isEliteClassic = false)
        {


            DataTable dt = new DataTable();
            dt.TableName = tablename;
            foreach (var name in ColumnNames)
                dt.Columns.Add(name);

            if (tablename.Equals("Topic List"))
            {
                foreach (var topic in items)
                {
                    string responsible = (string.Join(Environment.NewLine, topic.TopicResponsibleJson.Select(x => x.DisplayName.ToString()).ToArray()));
                    string speaker = topic.TopicSpeakerJson != null ? (string.Join(Environment.NewLine, topic.TopicSpeakerJson.Select(x => x.DisplayName.ToString()).ToArray())) : null;
                    string guest = topic.TopicGuestJson != null ? (string.Join(Environment.NewLine, topic.TopicGuestJson.Select(x => x.DisplayName.ToString()).ToArray())) : null;


                    var status = ((topic.Status == "Open") ? (((topic.TopicAction == 2) || (topic.TopicAction == 9)) ? ((topic.TopicAction == 2) ? topic.Status + "(" + "RFS" + ")" : topic.Status + "(" + "RFD" + ")") : topic.Status) : topic.Status);

                    dt.Rows.Add(topic.Title, responsible, speaker, guest, topic.CommitteeName, status, topic.DueDate, topic.ScheduledDate, topic.MeetingName, topic.Description);
                }
            }
            else if (tablename.Equals("Task List"))
            {
                foreach (var task in items)
                {
                    string coresponsible = (string.Join(Environment.NewLine, task.CoResponsibleJson.Select(x => x.DisplayName.ToString()).ToArray()));
                    if (task.taskDueStatus.Length > 0)
                        task.Status = task.Status + "(" + task.taskDueStatus + ")";
                    string allComments = task.TaskComments != null
                        ? string.Join(Environment.NewLine, task.TaskComments.Select(c => c.Comment ?? string.Empty))
                        : string.Empty;

                    string allCommentedBy = task.TaskComments != null
                        ? string.Join(Environment.NewLine, task.TaskComments.Select(c => c.CreatedBy ?? string.Empty))
                        : string.Empty;

                    string allCommentedOn = task.TaskComments != null
                        ? string.Join(Environment.NewLine, task.TaskComments.Select(c => c.CreatedDate.Value.ToString("dd.MM.yyyy")))
                        : string.Empty;
                    string meetingDate = task.MeetingDate.HasValue ? task.MeetingDate.Value.ToString("dd.MM.yyyy") : "";

                    dt.Rows.Add(task.Title, HtmlAgilityUtility.ConvertHTMLToString(task.Description), task.ResponsibleJson.DisplayName, coresponsible, task.CommitteeName, task.Status, task.DueDate, task.Filelink, HtmlAgilityUtility.ConvertHTMLToString(task.TaskClosureComment), allComments, allCommentedBy, allCommentedOn, meetingDate, task.ResponsibleDivision);
                }
            }
            else if (tablename.Equals("Committee List"))
            {
                foreach (var committee in items)
                {
                    dt.Rows.Add(committee.Id, committee.CommitteeName, committee.CreatedBy, committee.CreatedDate, committee.CommitteeManagers, committee.CoreMembers, committee.Users);
                }
            }
            else if (tablename.Equals("User List"))
            {
                foreach (var user in items)
                {
                    dt.Rows.Add(user.Id, user.Uid, user.FirstName, user.LastName, user.DisplayName, user.Email, user.Department, user.Phone, user.CreatedDate);
                }
            }
            else if (tablename.Equals("New Committee Requests"))
            {
                foreach (var item in items)
                {
                    dt.Rows.Add(item.CommitteeName, item.Id, item.Requestor, item.Department, item.CommitteeType, item.DisplayName);
                }
            }
            else if (tablename.Equals("Topic History List"))
            {
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.AdditionalInfo))
                        item.AdditionalInfo = item.AdditionalInfo.ToString().Replace("&nbsp", " ");
                    if (isEliteClassic)
                        dt.Rows.Add(item.ResponsibleJson.DisplayName, item.CreatedDate, item.Status, item.TopicComments, HtmlAgilityUtility.ConvertHTMLToString(item.AdditionalInfo), HtmlAgilityUtility.ConvertHTMLToString(item.TaskClosureComment));
                    else
                        dt.Rows.Add(item.ResponsibleJson.DisplayName, item.CreatedDate, item.Status, item.TopicComments, HtmlAgilityUtility.ConvertHTMLToString(item.AdditionalInfo));
                }
            }
            else if (tablename.Equals("Decision List"))
            {
                foreach (var item in items)
                {

                    dt.Rows.Add(item.Title, item.CommitteeType, item.CommitteeName, item.ResponsibleJson.DisplayName, item.MeetingName, Convert.ToDateTime(item.MeetingDate).ToString("dd/MM/yyyy"), item.Description);
                }
            }
            else if (tablename.Equals("Participant List"))
            {
                foreach (var meeting in items)
                {
                    DataRow firstRow = dt.NewRow();
                    firstRow[0] = meeting.MeetingName ?? string.Empty;
                    firstRow[2] = meeting.Organizer?.DisplayName ?? string.Empty;

                    var participants = meeting.MeetingParticipantJson;
                    firstRow[3] = FormatPersonCollection(meeting.MeetingParticipantJson);

                    dt.Rows.Add(firstRow);
                    if (meeting.MeetingAgenda?.Any() == true)
                    {
                        foreach (var agendaItem in meeting.MeetingAgenda)
                        {
                            DataRow row = dt.NewRow();
                            row[1] = agendaItem.Title ?? string.Empty; // Agenda Topic
                            row[4] = FormatPersonCollection(agendaItem.TopicResponsibleJson); // Responsible
                            row[5] = FormatPersonCollection(agendaItem.TopicSpeakerJson); // Speaker
                            row[6] = FormatPersonCollection(agendaItem.TopicGuestJson); // Guest

                            dt.Rows.Add(row);
                        }
                    }
                }
            }

            dt.AcceptChanges();
            XLWorkbook workbook = new XLWorkbook();
            var wb = workbook.Worksheets.Add(dt);



            wb.Columns().AdjustToContents();
            wb.Rows().AdjustToContents();

            if (tablename.Equals("Participant List"))
            {
                wb.Cells().Style.Alignment.WrapText = true;


                foreach (var col in wb.ColumnsUsed())
                {
                    col.AdjustToContents();
                    col.Width = Math.Min(col.Width, 50);
                }
                foreach (var row in wb.RowsUsed())
                {
                    row.AdjustToContents();
                    row.ClearHeight();
                }
            }

            for (var i = 1; i <= ColumnNames.Count; i++)
            {
                wb.Cell(1, i).Style.Fill.BackgroundColor = XLColor.BeauBlue;
                wb.Cell(1, i).Style.Font.Bold = true;
            }
            wb.Table("Table1").Theme = XLTableTheme.None;
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;

        }
        private string FormatPersonCollection(IEnumerable<PersonDto> persons)
        {
            if (persons == null)
                return string.Empty;

            return string.Join("; ", persons
                .Select(p => p?.DisplayName)
                .Where(name => !string.IsNullOrEmpty(name)));
        }
    }
}
