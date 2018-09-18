using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QLVB.Models;
using System.Globalization;
using System.Text;

namespace QLVB.Models
{
    public class DuyetLichHop
    {
        public string TieuDe { get; set; }
        public int MaPhong { get; set; }
        public DateTime BatDau { get; set; }
        public DateTime KetThuc { get; set; }
        public DateTime NgayDinhKy { get; set; }
        public bool DinhKy { get; set; }

        public DuyetLichHop(string sTieuDe, int iMaPhong, DateTime dBatDau, DateTime dKetThuc, DateTime dDinhKy, bool iDinhKy)
        {
            this.TieuDe = sTieuDe;
            this.MaPhong = iMaPhong;
            this.BatDau = dBatDau;
            this.KetThuc = dKetThuc;
            this.NgayDinhKy = dDinhKy;
            this.DinhKy = iDinhKy;
        }

        public static List<DuyetLichHop> ThemLichDinhKy(DateTime dStart, DateTime dEnd)
        {
            using (QuanLyVanBanEntities db = new QuanLyVanBanEntities())
            {
                List<DuyetLichHop> lstDuyetLich = new List<DuyetLichHop>();
                foreach (var item in db.LichHops.Where(n =>n.Status==true && n.RecurrenceRule != null))
                {
                   
                    if (dStart.Date <= dEnd.Date)
                        KiemTraDinhKy(dStart, dEnd, item.RecurrenceRule, lstDuyetLich, item);

                }
                return lstDuyetLich;
            }
        }

        public static void KiemTraDinhKy(DateTime dtStart, DateTime dtEnd, string rRule, List<DuyetLichHop> lstDuyetLich, LichHop item)
        {
            string parsed = string.Empty;//set up the return string
            string calType = string.Empty; //need a scratchpad
            Dictionary<string, string> rruleElems = new Dictionary<string, string>();

            parsed = getElemValue(rRule);
            string[] elements = parsed.Split(';');

            for (int i = 0; i < elements.Length; i++)
            {
                string[] tmp = elements[i].Split('=');
                rruleElems.Add(tmp[0], tmp[1]);
            }

            string abc = rruleElems["FREQ"];
            int timeToAdd = 0;
            try
            {
                //timeToAdd = Convert.ToInt32(rruleElems["COUNT"]);
            }
            catch
            {
                timeToAdd = 0;
            }
            switch (rruleElems["FREQ"].ToLower())
            {
                case "daily":
                    string[] days = rruleElems["BYDAY"].Split(',');
                    dtEnd = dtEnd.AddDays(timeToAdd);
                    calType = "days";
                    break;
                case "weekly":
                    dtEnd = dtEnd.AddDays(timeToAdd * 7);
                    try
                    {
                        days = rruleElems["BYDAY"].Split(',');
                        parseDayNames(days, dtStart, dtEnd, lstDuyetLich, item);
                    }
                    catch
                    {
                        //just in case we missed something on this one
                        throw new Exception("Error while processing Recurrence Rule");
                    }
                    break;
                case "monthly":
                    calType = "months";
                    dtEnd = dtEnd.AddMonths(timeToAdd);
                    //see if it's positional
                    try
                    {
                        string bsp = getDayEnding(rruleElems["BYSETPOS"]);
                    }
                    catch
                    {
                        //Ok, no BYSETPOS, let's go for BYMONTHDAY
                      //  string bsp = getDayEnding(rruleElems["BYMONTHDAY"]);
                    }
                    break;
                case "yearly":
                    calType = "years";
                    dtEnd = dtEnd.AddYears(timeToAdd);
                    //looks a lot like monthly....
                    string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(rruleElems["BYMONTH"]));
                    //see if it's positional
                    try
                    {
                        string bsp = getDayEnding(rruleElems["BYSETPOS"]);
                    }
                    catch
                    {
                        //Ok, no BYSETPOS, let's go for BYMONTHDAY
                        //string bsp = getDayEnding(rruleElems["BYMONTHDAY"]);
                    }
                    break;

                default:
                    break;

            }

        }
        private static string getElemValue(string elem)
        {
            //just easier than writing split all over the place
            string[] elems = elem.Split(':');
            return elems[0].Trim();
        }
        private static string getDayName(string day)
        {
            //pretty self explanatory
            switch (day)
            {
                case "MO":
                    return "Monday";
                case "TU":
                    return "Tuesday";
                case "WE":
                    return "Wednesday";
                case "TH":
                    return "Thursday";
                case "FR":
                    return "Friday";
                case "SA":
                    return "Saturday";
                case "SU":
                    return "Sunday";
                default:
                    return "";
            }
        }
        private static void parseDayNames(string[] days, DateTime dtStart, DateTime dtEnd, List<DuyetLichHop> lstDuyetLich, LichHop item)
        {

            if (days.Length < 7)
            {
                for (int d = 0; d < days.Length; d++)
                {
                    switch (getDayName(days[d]))
                    {
                        case "Monday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Monday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Tuesday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Tuesday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Wednesday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Wednesday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Thursday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Thursday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Friday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Friday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Saturday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Saturday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        case "Sunday":
                            {
                                for (DateTime i = dtStart; i.Date <= dtEnd.Date; i = i.AddDays(1))
                                {
                                    if (i.DayOfWeek.ToString() == "Sunday")
                                    {
                                        DuyetLichHop lich = new DuyetLichHop(item.Subject, item.RoomId.Value, item.Start.Value, item.End.Value, i, true);
                                        lstDuyetLich.Add(lich);
                                    }
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

        }
        private static string getDayEnding(string d)
        {
            //tried to avoid a big ugly if statement
            //handle the events on the "n"th day of the month
            if (d.EndsWith("1") && d != "11")
            {
                d += "st";
            }
            if (d.EndsWith("2") && d != "12")
            {
                d += "nd";
            }
            if (d.EndsWith("3") && d != "13")
            {
                d += "rd";
            }
            if (d.Length < 3)//hasn't been appended yet
            {
                d += "th";
            }
            return d;
        }


    }
}