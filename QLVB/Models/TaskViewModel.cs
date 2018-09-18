using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLVB.Models
{
    public class TaskViewModel : ISchedulerEvent
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        private DateTime start;
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value.ToUniversalTime();
            }
        }

        public string StartTimezone { get; set; }

        private DateTime end;
        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                end = value.ToUniversalTime();
            }
        }

        public string EndTimezone { get; set; }

        public string RecurrenceRule { get; set; }
        public string RecurrenceID { get; set; }
        public string RecurrenceException { get; set; }
        public bool IsAllDay { get; set; }
        public int? OwnerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedUserId { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? UpdatedUserId { get; set; }

        public bool? Status { get; set; }

        public LichHop ToEntity()
        {
            return new LichHop
            {
                ID = TaskID,
                Subject = Title,
                Start = Start,
                //  StartTimezone = StartTimezone,
                End = End,
                // EndTimezone = EndTimezone,
                Description = Description,
                RecurrenceRule = RecurrenceRule,
                //RecurrenceException = RecurrenceException,
                RecurrenceParentId = RecurrenceID,
                //IsAllDay = IsAllDay,
                RoomId = OwnerID,
                CreatedDate = CreatedDate,
                CreatedUserId = CreatedUserId,
                UpdatedDate = UpdatedDate,
                UpdatedUserId = UpdatedUserId,
                Status = Status
            };
        }

        public LichHop_BLD ToEntityBLD()
        {
            return new LichHop_BLD
            {
                ID = TaskID,
                Subject = Title,
                Start = Start,
                End = End,
                Description = Description,
                RecurrenceRule = RecurrenceRule,
                RecurrenceParentId = RecurrenceID,
                OwnerId = OwnerID,
                CreatedDate = CreatedDate,
                CreatedUserId = CreatedUserId,
                UpdatedDate = UpdatedDate,
                UpdatedUserId = UpdatedUserId,
                Status = Status
            };
        }
    }
}