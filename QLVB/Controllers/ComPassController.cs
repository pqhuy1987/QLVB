using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;
using System.Net;
using System.Net.Mail;

namespace QLVB.Controllers
{
    public class ComPassController : Controller
    {
        //
        // GET: /Compass/

        QuanLyVanBanEntities db = new QuanLyVanBanEntities();
        public ActionResult Index()
        {
            DateTime dtNow= DateTime.Now;
           IEnumerable<TinTuc> lstTin = db.TinTucs.Where(n => n.NgayDang <= dtNow && n.TrangThai == true).OrderByDescending(n=>n.NgayDang);
            ViewBag.lstMucTin = db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin);
            ViewBag.lstBanner = db.Banners.OrderBy(n => n.Order);

           return View(lstTin);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
	}
}