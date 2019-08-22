using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Net.Mail;
using System.Net;

namespace QLVB.Controllers
{
    [Authorize]
    public class LichHopController : Controller
    { 
         private SchedulerTaskService taskService;

         QuanLyVanBanEntities db = new QuanLyVanBanEntities();

        public bool KiemTraSession()
        {
            if (Session["DangNhap"] == null)
            {
                return true;
            }
            return false;
        }
        public LichHopController()
        {
            this.taskService = new SchedulerTaskService();
        }

        public ActionResult DangKy()
        {
             if (KiemTraSession() == true)
               return RedirectToAction("DangNhap", "QuanTri");

            IEnumerable<PhongHop> lstPhongHop = db.PhongHops.Where(n => n.Status == true).OrderBy(n => n.Ordering);
            ViewBag.lstPhongHop = lstPhongHop;
            Session["PhongHop"] = lstPhongHop.FirstOrDefault().Id;

            return View();
        }

        public virtual JsonResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(taskService.GetAll().ToDataSourceResult(request));
        }

        public virtual JsonResult Destroy([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            NhanVien nv = (NhanVien)Session["DangNhap"];

            if (ModelState.IsValid && (task.CreatedUserId == nv.Id || HttpContext.User.IsInRole("UPDATE-LICH")))
            {
               //taskService.Delete(task, ModelState);

                task.Status = false;
                taskService.Update(task, ModelState);
            }

            string sNoiDung = " <p>Dear Ms. Đoan,</p><p>Hệ thống vừa ghi nhận thao tác hủy sử dụng phòng họp của Ông/Bà <b>" + nv.HoTen + "</b> như sau:</p>";
            LichHop lh = db.LichHops.SingleOrDefault(n => n.ID == task.TaskID);
            sNoiDung += LayNoiDungLich(lh);
            sNoiDung += "<p>Trân trọng.</p>";
            MailLich(sNoiDung);

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public virtual ActionResult Create([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
            if(Session["PhongHop"]==null)
                return RedirectToAction("DangKy", "LichHop");

            NhanVien nv = (NhanVien)Session["DangNhap"];
            task.CreatedUserId = nv.Id;

            task.CreatedDate = DateTime.Now;
            task.OwnerID = int.Parse(Session["PhongHop"].ToString());
            task.Status = true;

            if (ModelState.IsValid && KiemTraLichTrung(task.Start.ToString("d/M/yyyy H:m"),task.End.ToString("d/M/yyyy H:m"), "0", task.RecurrenceRule)=="1")
            {

                taskService.Insert(task, ModelState);
            }

            string sNoiDung = " <p>Dear Ms. Đoan,</p><p>  <b>"+nv.HoTen+"</b> như sau:</p>";
            LichHop lh = db.LichHops.SingleOrDefault(n => n.ID == task.TaskID);
            sNoiDung += LayNoiDungLich(lh);
            sNoiDung += "<p>Trân trọng.</p>";
            MailLich(sNoiDung);

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));

        }

        public virtual ActionResult Update([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
            if (Session["PhongHop"] == null)
                return RedirectToAction("DangKy", "LichHop");

            NhanVien nv = (NhanVien)Session["DangNhap"];
            task.OwnerID = int.Parse(Session["PhongHop"].ToString());

            //gui mail lay noi dung cu 
            string sNoiDung = " <p>Dear Ms. Đoan,</p><p>Hệ thống vừa ghi thao tác chỉnh sửa lịch họp của Ông/Bà <b>" + nv.HoTen + "</b> như sau:</p> <p>Lịch họp cũ: </p>";
            LichHop lh = db.LichHops.SingleOrDefault(n => n.ID == task.TaskID);
            sNoiDung += LayNoiDungLich(lh);

            if (ModelState.IsValid && (task.CreatedUserId==nv.Id || HttpContext.User.IsInRole("UPDATE-LICH")) && KiemTraLichTrung(task.Start.ToString("d/M/yyyy H:m"), task.End.ToString("d/M/yyyy H:m"), task.TaskID.ToString(),task.RecurrenceRule) == "1")
            {
                task.UpdatedDate = DateTime.Now;
                taskService.Update(task, ModelState);
            }

            //gui mail lay moi dung moi

            lh.Subject = task.Title;
            lh.Start = task.Start;
            lh.End = task.End;
            lh.RecurrenceRule = task.RecurrenceRule;

            sNoiDung += "<p>Lịch họp mới: </p>";
            sNoiDung += LayNoiDungLich(lh);
            sNoiDung += "<p>Trân trọng.</p>";
            MailLich(sNoiDung);
            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult luu_ss_phong_ajax(string phong)
        {
            Session["PhongHop"] = phong;
            return Content("1");
        }

        public ActionResult LichHopTuan()
        {
             if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            DateTime dtFirst = tool.FirstDayOfWeek(DateTime.Now);
            DateTime dtLast = tool.LastDayOfWeek(DateTime.Now);
            //xu ly lich hop
            ViewBag.lstPhong = db.PhongHops.Where(n => n.Status == true).OrderBy(n => n.Ordering);
            IEnumerable<LichHop> lstLichHop = db.LichHops.Where(n =>n.Status==true && n.Start.Value >= dtFirst && n.End.Value <= dtLast);

            List<DuyetLichHop> lstDuyetLich = new List<DuyetLichHop>();
            lstDuyetLich = DuyetLichHop.ThemLichDinhKy(dtFirst, dtLast);

            foreach (var item in lstLichHop.Where(n => n.RecurrenceRule == null))
            {
                DuyetLichHop lichhop = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, item.Start.Value, false);
                lstDuyetLich.Add(lichhop);

            }

            ViewBag.lstLichHop = lstDuyetLich;

            return View();
        }

        public string KiemTraLichTrung(string start, string end, string lichID,string recurrenceRule)
        {

            DateTime dtstart = DateTime.ParseExact(start, "d/M/yyyy H:m", null);
            DateTime dtend = DateTime.ParseExact(end, "d/M/yyyy H:m", null);
            int ilichID = int.Parse(lichID);
            int iKT = 1;

            //lich khong dinh ky
            if (string.IsNullOrEmpty(recurrenceRule))
            {
                IEnumerable<LichHop> lstLichHop = db.LichHops.Where(n => n.Status == true && n.ID != ilichID && n.Start.Value.Day == dtstart.Day && n.Start.Value.Month == dtstart.Month && n.Start.Value.Year == dtstart.Year);
                List<DuyetLichHop> lstDuyetLich = new List<DuyetLichHop>();
                lstDuyetLich = DuyetLichHop.ThemLichDinhKy(dtstart, dtend);

                foreach (var item in lstLichHop.Where(n => n.RecurrenceRule == null))
                {
                    DuyetLichHop lichhop = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, item.Start.Value, false);
                    lstDuyetLich.Add(lichhop);

                }

                //loc theo phong
                int iRoomID = int.Parse(Session["PhongHop"].ToString());
                lstDuyetLich = lstDuyetLich.Where(n => n.MaPhong == iRoomID).ToList();


                foreach (var item in lstDuyetLich)
                {
                    if (item.BatDau.TimeOfDay <= dtstart.TimeOfDay && item.KetThuc.TimeOfDay > dtstart.TimeOfDay)
                        iKT = 0;

                    if (item.BatDau.TimeOfDay < dtend.TimeOfDay && item.KetThuc.TimeOfDay >= dtend.TimeOfDay)
                        iKT = 0;

                    if (item.BatDau.TimeOfDay > dtstart.TimeOfDay && item.KetThuc.TimeOfDay < dtend.TimeOfDay)
                        iKT = 0;

                    if(iKT==0)
                    return item.NgayDinhKy.ToString("dd/MM/yyyy") + " lúc " + item.BatDau.ToString("HH:mm") + " đến " + item.KetThuc.ToString("HH:mm") + "\n" + item.TieuDe;
                }
            }
            else
            {
               
                Tools tool = new Tools();
                DateTime dtFirst = tool.FirstDayOfWeek(dtstart);
                DateTime dtLast = tool.LastDayOfWeek(dtstart);

                //them tam lich tuan moi
                LichHop lichTuan = new LichHop();
                lichTuan.Subject = "";
                lichTuan.RoomId = int.Parse(Session["PhongHop"].ToString());
                lichTuan.Start = dtstart;
                lichTuan.End = dtend;

                List<DuyetLichHop> lstLichTuan = new List<DuyetLichHop>();
                DuyetLichHop.KiemTraDinhKy(dtFirst, dtLast, recurrenceRule, lstLichTuan, lichTuan);

                //lay lich tuan
                ViewBag.lstPhong = db.PhongHops.Where(n => n.Status == true).OrderBy(n => n.Ordering);
                IEnumerable<LichHop> lstLichHop = db.LichHops.Where(n => n.Status == true && n.ID != ilichID && n.Start.Value >= dtFirst && n.End.Value <= dtLast);

                List<DuyetLichHop> lstDuyetLich = new List<DuyetLichHop>();

                foreach (var item in db.LichHops.Where(n => n.Status == true && n.ID!= ilichID &&n.RecurrenceRule != null))
                {
                        DuyetLichHop.KiemTraDinhKy(dtFirst, dtLast, item.RecurrenceRule, lstDuyetLich, item);

                }

                foreach (var item in lstLichHop.Where(n => n.RecurrenceRule == null))
                {
                    DuyetLichHop lichhop = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, item.Start.Value, false);
                    lstDuyetLich.Add(lichhop);

                }
                //loc theo phong
                int iRoomID = int.Parse(Session["PhongHop"].ToString());
                lstDuyetLich = lstDuyetLich.Where(n => n.MaPhong == iRoomID).ToList();

                foreach (var item in lstDuyetLich)
                {
                    foreach (var itemTuan in lstLichTuan)
                    {
                        if (item.NgayDinhKy.Date == itemTuan.NgayDinhKy.Date && item.BatDau.TimeOfDay <= dtstart.TimeOfDay && item.KetThuc.TimeOfDay > dtstart.TimeOfDay)
                            iKT = 0;

                        if (item.NgayDinhKy.Date == itemTuan.NgayDinhKy.Date && item.BatDau.TimeOfDay < dtend.TimeOfDay && item.KetThuc.TimeOfDay >= dtend.TimeOfDay)
                            iKT = 0;

                        if (item.NgayDinhKy.Date == itemTuan.NgayDinhKy.Date && item.BatDau.TimeOfDay > dtstart.TimeOfDay && item.KetThuc.TimeOfDay < dtend.TimeOfDay)
                            iKT = 0;

                        if (iKT == 0)
                            return item.NgayDinhKy.ToString("dd/MM/yyyy") + " lúc " + item.BatDau.ToString("HH:mm") + " đến " + item.KetThuc.ToString("HH:mm") + "\n" + item.TieuDe;
                    }
                }

            }



            return "1";
        }

        [HttpPost]
        public ActionResult KiemTraTrung_ajax(string start, string end,string lichID, string recurrenceRule)
        {

            return Content(KiemTraLichTrung(start, end, lichID, recurrenceRule));
            
        }

        public void MailLich(string NoiDung)
        {
            string sMailGui = System.Configuration.ConfigurationManager.AppSettings["MailSend"];
            string sPass = System.Configuration.ConfigurationManager.AppSettings["MailPass"];
            string sHost = System.Configuration.ConfigurationManager.AppSettings["MailHost"];
            string sPort = System.Configuration.ConfigurationManager.AppSettings["MailPort"];
            string sTieuDe = System.Configuration.ConfigurationManager.AppSettings["ScheduleSubject"];
            string sMailTo = System.Configuration.ConfigurationManager.AppSettings["schedulerman"];
            
            // gui mail
            sMailTo = "quanghuy.pham@newtecons.vn";
            var fromAddress = new MailAddress(sMailGui);
            var toAddress = new MailAddress(sMailTo);
            string fromPassword = sPass;
            string subject = sTieuDe;
            string body = NoiDung;

            var smtp = new SmtpClient
            {
                Host = sHost,
                Port = int.Parse(sPort),
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            //var smtp = new SmtpClient();

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                IsBodyHtml = true,
                Subject = subject,
                Body = body
            })
            {
                    try
                    {
                         smtp.Send(message);
                         smtp.Dispose();
                    }
                    catch (SmtpFailedRecipientsException ex)
                    {
                        for (int i = 0; i < ex.InnerExceptions.Length; i++)
                        {
                            SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                            if (status == SmtpStatusCode.MailboxBusy || status == SmtpStatusCode.MailboxUnavailable)
                            {
                                // Console.WriteLine("Delivery failed - retrying in 5 seconds.");
                                System.Threading.Thread.Sleep(5000);
                                smtp.Send(message);
                            }
                            else
                            {
                                //  Console.WriteLine("Failed to deliver message to {0}", ex.InnerExceptions[i].FailedRecipient);
                                throw ex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //  Console.WriteLine("Exception caught in RetryIfBusy(): {0}",ex.ToString());
                        throw ex;
                    }
                    finally
                    {
                        smtp.Dispose();
                    }
            }

        }
        public string LayNoiDungLich(LichHop lh)
        {
            string sDinhKy = "";
            if (lh.RecurrenceRule != null)
                sDinhKy = " (Định kỳ)";
            string sNoiDung = "";
            sNoiDung += "<ul>";
            sNoiDung += "<li>Cuộc họp: "+lh.Subject+"</li>";
            sNoiDung += "<li>Thời gian: "+lh.Start.Value.ToString("HH:mm")+" - "+ lh.End.Value.ToString("HH:mm") + "</li>";
            sNoiDung += "<li>Ngày: " + lh.Start.Value.DayOfWeek + " " + lh.Start.Value.ToString("dd/MM/yyyy") + sDinhKy + "</li>";
            sNoiDung += "<li>Tại phòng họp: "+db.PhongHops.SingleOrDefault(n=>n.Id==lh.RoomId).Name+"</li>";
            sNoiDung += "</ul>";

            return sNoiDung;
        }

        protected override void Dispose(bool disposing)
        {
            taskService.Dispose();
            base.Dispose(disposing);
        }
	}
}