using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLVB.Models
{
    public class SchedulerTaskService : ISchedulerEventService<TaskViewModel>
    {
        private static bool UpdateDatabase = true;
        private QuanLyVanBanEntities db;

        public SchedulerTaskService(QuanLyVanBanEntities context)
        {
            db = context;
        }

        public SchedulerTaskService()
            : this(new QuanLyVanBanEntities())
        {
        }

        public virtual IList<TaskViewModel> GetAll()
        {
            bool IsWebApiRequest = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api");
            IList<TaskViewModel> result = null;

            if (!IsWebApiRequest)
            {
                result = HttpContext.Current.Session["SchedulerTasks"] as IList<TaskViewModel>;
            }

            if (result == null || UpdateDatabase)
            {
                result = db.LichHops.Where(n=>n.Status==true).ToList().Select(task => new TaskViewModel
                {
                    TaskID = task.ID,
                    Title = task.Subject,
                    Start = DateTime.SpecifyKind(task.Start.Value, DateTimeKind.Utc),
                    End = DateTime.SpecifyKind(task.End.Value, DateTimeKind.Utc),
                    // StartTimezone = task.StartTimezone,
                    //  EndTimezone = task.EndTimezone,
                    Description = task.Description,
                    // IsAllDay = task.IsAllDay,
                    RecurrenceRule = task.RecurrenceRule,
                   // RecurrenceException = task.RecurrenceException,
                    RecurrenceID = task.RecurrenceParentId,
                    OwnerID = task.RoomId,
                    CreatedDate = DateTime.SpecifyKind(task.CreatedDate.Value, DateTimeKind.Utc),
                    CreatedUserId = task.CreatedUserId,
                    UpdatedDate = DateTime.SpecifyKind(task.UpdatedDate.Value, DateTimeKind.Utc),
                    UpdatedUserId = task.UpdatedUserId,
                    Status = task.Status
                }).ToList();

                if (!IsWebApiRequest)
                {
                    HttpContext.Current.Session["SchedulerTasks"] = result;
                }
            }

            return result;
        }

        public virtual void Insert(TaskViewModel task, ModelStateDictionary modelState)
        {
            if (ValidateModel(task, modelState))
            {
                if (!UpdateDatabase)
                {
                    var first = GetAll().OrderByDescending(e => e.TaskID).FirstOrDefault();
                    var id = (first != null) ? first.TaskID : 0;

                    task.TaskID = id + 1;

                    GetAll().Insert(0, task);
                }
                else
                {
                    if (string.IsNullOrEmpty(task.Title))
                    {
                        task.Title = "";
                    }

                    var entity = task.ToEntity();

                    db.LichHops.Add(entity);
                    db.SaveChanges();

                    task.TaskID = entity.ID;
                }
            }
        }

        public virtual void Update(TaskViewModel task, ModelStateDictionary modelState)
        {
            if (ValidateModel(task, modelState))
            {
                if (!UpdateDatabase)
                {
                    var target = One(e => e.TaskID == task.TaskID);

                    if (target != null)
                    {
                        target.Title = task.Title;
                        target.Start = task.Start;
                        target.End = task.End;
                        target.StartTimezone = task.StartTimezone;
                        target.EndTimezone = task.EndTimezone;
                        target.Description = task.Description;
                        target.IsAllDay = task.IsAllDay;
                        target.RecurrenceRule = task.RecurrenceRule;
                        target.RecurrenceException = task.RecurrenceException;
                        target.RecurrenceID = task.RecurrenceID;
                        target.OwnerID = task.OwnerID;
                        target.CreatedDate = task.CreatedDate;
                        target.CreatedUserId = task.CreatedUserId;
                        target.UpdatedDate = task.UpdatedDate;
                        target.UpdatedUserId = task.UpdatedUserId;
                        target.Status = task.Status;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(task.Title))
                    {
                        task.Title = "";
                    }

                    var entity = task.ToEntity();
                    db.LichHops.Attach(entity);
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public virtual void Delete(TaskViewModel task, ModelStateDictionary modelState)
        {
            if (!UpdateDatabase)
            {
                var target = One(p => p.TaskID == task.TaskID);
                if (target != null)
                {
                    GetAll().Remove(target);
                    string itaskID = task.TaskID.ToString();
                    var recurrenceExceptions = GetAll().Where(m => m.RecurrenceID == itaskID).ToList();

                    foreach (var recurrenceException in recurrenceExceptions)
                    {
                        GetAll().Remove(recurrenceException);
                    }
                }
            }
            else
            {
                var entity = task.ToEntity();
                db.LichHops.Attach(entity);
                 string itaskID = task.TaskID.ToString();
                 var recurrenceExceptions = db.LichHops.Where(t => t.RecurrenceParentId == itaskID);

                foreach (var recurrenceException in recurrenceExceptions)
                {
                    db.LichHops.Remove(recurrenceException);
                }

                db.LichHops.Remove(entity);
                db.SaveChanges();
            }
        }

        private bool ValidateModel(TaskViewModel appointment, ModelStateDictionary modelState)
        {
            if (appointment.Start > appointment.End)
            {
                modelState.AddModelError("errors", "End date must be greater or equal to Start date.");
                return false;
            }

            return true;
        }

        public TaskViewModel One(Func<TaskViewModel, bool> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}