using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PagedList;


namespace QLVB.Models
{
    public class Tools
    {
        public static int MaDanhMucTin = 2;

        public List<SelectListItem> DMTinhTrang(int iSelect)
        {
            List<SelectListItem> lstTinhTrang = new List<SelectListItem>();
            // BEGIN
            //lstTinhTrang.Add(new SelectListItem { Text = "Chưa có hiệu lực", Value = "1", Selected = iSelect == 1 ? true : false });
            lstTinhTrang.Add(new SelectListItem { Text = "Dự thảo", Value = "1", Selected = iSelect == 1 ? true : false });
            // END
            lstTinhTrang.Add(new SelectListItem { Text = "Còn hiệu lực", Value = "2", Selected = iSelect == 2 ? true : false });
            lstTinhTrang.Add(new SelectListItem { Text = "Hết hiệu lực", Value = "3", Selected = iSelect == 3 ? true : false });
            return lstTinhTrang;
        }

        public List<SelectListItem> DMTinTuc(int iSelect)
        {
            List<SelectListItem> lstTinhTrang = new List<SelectListItem>();
            lstTinhTrang.Add(new SelectListItem { Text = "Văn bản", Value = "1", Selected = iSelect == 3 ? true : false });
            lstTinhTrang.Add(new SelectListItem { Text = "Hình ảnh", Value = "2", Selected = iSelect == 2 ? true : false });
            lstTinhTrang.Add(new SelectListItem { Text = "Video", Value = "3", Selected = iSelect == 3 ? true : false });
            return lstTinhTrang;
        }

        public static byte[] encryptData(string data)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashedBytes;
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(data));
            return hashedBytes;
        }

        public static string md5(string data)
        {
            return BitConverter.ToString(encryptData(data)).Replace("-", "").ToLower();
        }

        public static string NenChuoi(string text)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            System.IO.MemoryStream outStream = new System.IO.MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string GiaiNenChuoi(string compressedText)
        {
            if (!string.IsNullOrEmpty(compressedText))
            {
                byte[] gzBuffer = Convert.FromBase64String(compressedText);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                    ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                    byte[] buffer = new byte[msgLength];

                    ms.Position = 0;
                    using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                    {
                        zip.Read(buffer, 0, buffer.Length);
                    }

                    return System.Text.Encoding.UTF8.GetString(buffer);
                }
            }
            return "";
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            text = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            text = Regex.Replace(text, "[^0-9a-zA-Zđ]", "-");
            text = text.ToLower().Replace("đ", "d");

            return text;
        }

        public DateTime FirstDayOfWeek(DateTime date)
        {
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }

        public DateTime LastDayOfWeek(DateTime date)
        {
            DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
            return ldowDate;
        }

        public static string ThoiGianBinhLuan(DateTime dtThoiGian)
        {
            string sSoPhut = "1 phút";

            if (DateTime.Now.Date == dtThoiGian.Date)
            {
                int iThoiGian = 0;
                iThoiGian = DateTime.Now.Hour - dtThoiGian.Hour;
                if (iThoiGian == 0)
                {
                    iThoiGian = DateTime.Now.Minute - dtThoiGian.Minute;
                    if (iThoiGian != 0)
                        sSoPhut = iThoiGian + " phút";

                }
                else
                {
                    sSoPhut = iThoiGian + " giờ";
                }
            }
            else
            {
                sSoPhut = dtThoiGian.ToString("dd/MM/yyyy HH:mm");
            }

            return sSoPhut;
        }

        
        public void SendMail(string NoiDung, string NguoiNhan, string TieuDe)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };

                string sNguoiGui = System.Configuration.ConfigurationManager.AppSettings["MailSend"];
                string sPass = System.Configuration.ConfigurationManager.AppSettings["MailPass"];
                string sHost = System.Configuration.ConfigurationManager.AppSettings["MailHost"];
                string sPort = System.Configuration.ConfigurationManager.AppSettings["MailPort"];


                // gui mail
                var fromAddress = new MailAddress(sNguoiGui);
                string fromPassword = sPass;
                string subject = TieuDe;
                string body = NoiDung;

                var smtp = new SmtpClient
                {
                    Host = sHost,
                    Port = int.Parse(sPort),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                var toList = NguoiNhan.Split(';');

                foreach (var email in toList)
                {
                    var toAddress = new MailAddress(email);
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        IsBodyHtml = true,
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string RenderViewToString(ControllerContext context, string viewPath, object model = null, bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new System.IO.FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new System.IO.StringWriter())
            {
                var ctx = new ViewContext(context, view,
                                            context.Controller.ViewData,
                                            context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        public static void WriteLog(string module, string action, string description)
        {
            try
            {
                NhanVien user = (NhanVien)HttpContext.Current.Session["DangNhap"];
                if (user == null)
                {
                    user = new NhanVien();
                    user.TenDangNhap = "anonymous";
                }
                using (QuanLyVanBanEntities db = new QuanLyVanBanEntities())
                {
                    NhatKy item = new NhatKy();
                    item.DateTime = DateTime.Now;
                    item.Module = module;
                    item.Action = action;
                    item.Description = description;
                    item.Author = user.TenDangNhap;
                    item.IpAddress = GetClientIpAddress();

                    db.NhatKies.Add(item);
                    db.SaveChanges();

                    // SendMail
                    var config = db.CauHinhs.Where(x => x.MaCauHinh == "EMAIL-LOG").FirstOrDefault();
                    if (config != null)
                    {
                        string strEmail = config.DuLieu;
                        if (!string.IsNullOrEmpty(strEmail))
                        {
                            var tool = new Tools();
                            string strTitle = db.CauHinhs.Where(x => x.MaCauHinh == "EMAIL-LOG-TITLE").Select(x => x.DuLieu).FirstOrDefault();
                            string strHtml = db.CauHinhs.Where(x => x.MaCauHinh == "EMAIL-LOG-BODY").Select(x => x.DuLieu).FirstOrDefault();

                            strTitle = strTitle.Replace("{ACTION}", action);
                            strHtml = strHtml.Replace("{USERNAME}", user.TenDangNhap);
                            strHtml = strHtml.Replace("{FULLNAME}", user.HoTen);
                            strHtml = strHtml.Replace("{LOG}", description);

                            tool.SendMail(strHtml, strEmail, strTitle);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static string GetClientIpAddress()
        {
            try
            {
                HttpRequest contextRequest = HttpContext.Current.Request;
                string strIpList = contextRequest.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(strIpList))
                {
                    return strIpList.Split(',')[0];
                }
                return contextRequest.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}