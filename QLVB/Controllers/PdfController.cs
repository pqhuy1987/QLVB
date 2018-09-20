using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Cyotek.GhostScript;
using Cyotek.GhostScript.PdfConversion;
using QLVB.Models;
using Ghostscript.NET;

namespace QLVB.Controllers
{
    public class PdfController : Controller
    {
        QuanLyVanBanEntities db = new QuanLyVanBanEntities();

        public ActionResult ConvertPdf(string path)
        {
            return View();
        }

        public ActionResult Preview(int id)
        {
            var lstImage = db.PdfPages.Where(x => x.DocumentId == id).ToList();
            if (lstImage == null || (lstImage != null && lstImage.Count == 0))
            {
                if (!ConvertDocumentPdf(id))
                {
                    System.Threading.Thread.Sleep(2000);
                    ConvertDocumentPdf(id);
                }
            }
            lstImage = db.PdfPages.Where(x => x.DocumentId == id).ToList();
            return View(lstImage);
        }

        private bool ConvertDocumentPdf(int id)
        {
            TaiLieu document = db.TaiLieux.Where(x => x.MaTaiLieu == id).FirstOrDefault();
            if (document != null)
            {
                string strPdfPath = Path.Combine(Server.MapPath("~/Upload/TaiLieuGoc/"), document.fileGOC);
                if (System.IO.File.Exists(strPdfPath) && strPdfPath.ToLower().EndsWith(".pdf"))
                {
                    Pdf2ImageSettings convertSetting = new Pdf2ImageSettings();
                    convertSetting.Dpi = 150;

                    Pdf2Image pdf = new Pdf2Image(strPdfPath, convertSetting);
                    if (pdf.PageCount > 0)
                    {
                        List<PdfPage> lstPage = new List<PdfPage>();

                        for (int j = 1; j <= pdf.PageCount; j++)
                        {
                            try
                            {
                                System.Drawing.Bitmap bitmap = pdf.GetImage(j);
                                //MemoryStream ms = new MemoryStream();
                                //bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                                string strFileName = id + "-" + j + ".png";
                                string strFilePath = Path.Combine(Server.MapPath("~/Upload/Image/"), strFileName);
                                if (System.IO.File.Exists(strFilePath))
                                {
                                    System.IO.File.Delete(strFilePath);
                                }
                                bitmap.Save(strFilePath, System.Drawing.Imaging.ImageFormat.Png);

                                PdfPage page = new PdfPage();
                                page.DocumentId = id;
                                page.Page = j;
                                page.ImageUrl = "/Upload/Image/" + strFileName;
                                lstPage.Add(page);

                            }
                            catch (IndexOutOfRangeException)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                using (StreamWriter writer = new StreamWriter(Path.Combine(Server.MapPath("~/Upload/Image/"), "Log.txt"), true, System.Text.UTF8Encoding.Unicode))
                                {
                                    writer.WriteLine(ex.ToString());
                                }
                                break;
                            }
                        }

                        if (lstPage.Count > 0)
                        {
                            using (TransactionScope scope = new TransactionScope())
                            {
                                var lstOld = db.PdfPages.Where(x => x.DocumentId == id).ToList();
                                if (lstOld != null)
                                {
                                    foreach (var item in lstOld)
                                    {
                                        db.PdfPages.Remove(item);
                                    }
                                }

                                db.PdfPages.AddRange(lstPage);
                                db.SaveChanges();

                                scope.Complete();
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
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