
using Elite.Common.Utilities.CommonType;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesTaskListDto
    {

        public long Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public QueriesPersonDto ResponsibleJson { get; set; }
        public List<QueriesGroupDto> CoResponsibleJson { get; set; }

        public TaskStatus Status { get; set; }

        public TaskDueStatus? TaskDueStatus { get; set; }

        public bool HasSubTask { get; set; }

        public long SubTaskCount { get; set; }

        public bool HasMeetingTask { get; set; }

        public long? MeetingId { get; set; }

        public string CommitteeName { get; set; }
        public long? CommitteeId { get; set; }

        public int? MeetingStatus { get; set; }

        public long? AgendaId { get; set; }

        public QueriesPersonDto CreatedByJson { get; set; }
        public List<EntityAction> Actions { get; set; }
        public string FileLink { get; set; }

        public QueriesJiraTicketInfoDto JiraTicketInfo { get; set; }

        public bool? IsPublishedToJira { get; set; }
        public string ClosureComment { get; set; } = String.Empty;
        public List<QueriesGroupDto> CoResponsibleEmailRecipientJson { get; set; }
        public QueriesPersonDto ResponsibleEmailRecipientJson { get; set; }
        public bool? IsCustomEmailRecipient { get; set; } = false;
        public DateTime? MeetingDate { get; set; }
        public string ResponsibleDivision { get; set; }
        public DateTime? CompletionDate { get; set; }

    }

    public class GlobalSearchTaskDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int? Committee { get; set; }
        public string Description { get; set; }
        public string DescriptionWithoutHtml { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public DateTime? DueDate { get; set; }

        public string ResponsibleJson { get; set; }
        public string CoResponsibleJson { get; set; }
        public long? ParentId { get; set; }
        public string ResponsibleDivision { get; set; }
        public string CoResponsibleDivisions { get; set; }
        public string ClosureComment { get; set; }
        public DateTime? CompletionDate { get; set; }

        public double? RelevanceScore { get; set; }
        public string MatchTypes { get; set; }
        public int? TotalMatches { get; set; }
        public int? CommentMatches { get; set; }
        public int? AttachmentMatches { get; set; }

        public string HighlightedTitle { get; set; }
        public string HighlightedDescription { get; set; }
        public string HighlightedResponsibleJson { get; set; }
        public string HighlightedCoResponsibleJson { get; set; }
        public string HighlightedClosureComment { get; set; }

        public string MatchedCommentsPreview { get; set; }
        public string MatchedAttachmentsPreview { get; set; }

        public string LanguageDetected { get; set; }

        public long? MeetingId { get; set; }
        public long? AgendaId { get; set; }

        public int RowsCount { get; set; }

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
        public bool? HasNextPage { get; set; }
        public bool? HasPreviousPage { get; set; }

        [NotMapped]
        public int TotalTaskCount { get; set; }

        [NotMapped]
        public List<QueriesPersonDto> ResponsibleList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ResponsibleJson))
                    return new List<QueriesPersonDto>();

                if (ResponsibleJson.Trim().StartsWith("{"))
                {
                    var single = JsonConvert.DeserializeObject<QueriesPersonDto>(ResponsibleJson);
                    return new List<QueriesPersonDto> { single };
                }

                return JsonConvert.DeserializeObject<List<QueriesPersonDto>>(ResponsibleJson);
            }
        }

        [NotMapped]
        public List<QueriesPersonDto> CoResponsibleList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CoResponsibleJson))
                    return new List<QueriesPersonDto>();

                if (CoResponsibleJson.Trim().StartsWith("{"))
                {
                    var single = JsonConvert.DeserializeObject<QueriesPersonDto>(CoResponsibleJson);
                    return new List<QueriesPersonDto> { single };
                }

                return JsonConvert.DeserializeObject<List<QueriesPersonDto>>(CoResponsibleJson);
            }
        }

        public string HighlightedDescriptionWithoutHtml { get; set; }
        public string HighlightedResponsibleDivision { get; set; }
        public string HighlightedCoResponsibleDivisions { get; set; }
        public string HighlightedDueDate { get; set; }
    }

    public class GlobalSearchTaskEntity
    {
        [Key]
        public long task_id { get; set; }
        public long? meeting_id { get; set; }
        public long? agenda_id { get; set; }
        public int? committee_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string description_without_html { get; set; }

        public string status { get; set; }
        public string action { get; set; }

        public DateTime? due_date { get; set; }

        public string responsible_json { get; set; }
        public string co_responsibles_json { get; set; }

        public long? parent_id { get; set; }

        public string responsible_division { get; set; }
        public string co_responsible_divisions { get; set; }

        public string closure_comment { get; set; }
        public DateTime? completion_date { get; set; }

        public double? relevance_score { get; set; }
        public string match_types { get; set; }
        public int? total_matches { get; set; }
        public int? comment_matches { get; set; }
        public int? attachment_matches { get; set; }

        public string highlighted_title { get; set; }
        public string highlighted_description { get; set; }
        public string highlighted_responsible_json { get; set; }
        public string highlighted_co_responsibles_json { get; set; }
        public string highlighted_closure_comment { get; set; }

        public string matched_comments_preview { get; set; }
        public string matched_attachments_preview { get; set; }

        public string language_detected { get; set; }

        public int total_results { get; set; }
        public int? page_number { get; set; }
        public int? page_size { get; set; }
        public int? total_pages { get; set; }
        public bool? has_next_page { get; set; }
        public bool? has_previous_page { get; set; }
        public string highlighted_description_without_html { get; set; }
        public string highlighted_responsible_division { get; set; }
        public string highlighted_co_responsible_divisions { get; set; }
        public string highlighted_due_date { get; set; }
    }
}
