using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExcelDataReader;

namespace MetricsData.Controllers
{
    public class MetricsController : Controller
    {
        // GET: Metrics
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UploadToServer()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadToServer(HttpPostedFileBase[] files)
        {
            ViewBag.Message = null;
            string _FileName = null;
            string _path = null;
            int j;

            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    _FileName = Path.GetFileName(files[i].FileName);
                    _path = Path.Combine(Server.MapPath("~/UploadFiles/"), _FileName);
                    files[i].SaveAs(_path);
                }

            }
            catch (Exception ErrorMessage)
            {
                ViewBag.Message = ErrorMessage.Message;
                return View();
            }
            finally
            {

                _path = null;
                for (int i = 0; i < files.Length; i++)
                {
                    _FileName = files[i].FileName;
                    _path = Path.Combine(Server.MapPath("~/UploadFiles/"), _FileName);
                    string PersonName = GetFileName(_FileName);
                    if (ViewBag.Message == null)
                    {
                        double sumOfTotalCatw = 0;
                        FileStream stream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Read);

                        var reader = ExcelReaderFactory.CreateReader(stream);
                        #region Feb-2018
                        if (true)//this leads to sheet Feb-2018
                        {
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            // SqlConnection Connection = new SqlConnection("Server=WIN-P2S8E7IH0S7\\SQLEXPRESS;Integrated Security=sspi;database=FileAnalysis");
                            SqlConnection con = new SqlConnection("Server=HIB30BWAX2; Initial Catalog = metrics; User ID = sa; Password = Passw0rd@12;");
                            while (reader.Read())
                            {

                                con.Open();
                                if (reader.GetString(0) == null || reader.GetString(0) == "")
                                {
                                    break;
                                }
                                else
                                {
                                    SqlCommand cmd = new SqlCommand("InsertIntoEmployeeDetails @EmployeeName,@ApplicationName,@TaskDescription,@TaskClassification,@AssignedDate,@CompletedDate,@EffortHours", con);
                                    cmd.Parameters.AddWithValue("@EmployeeName", PersonName);
                                    cmd.Parameters.AddWithValue("@ApplicationName", reader.GetString(0));
                                    cmd.Parameters.AddWithValue("@TaskDescription", reader.GetString(1));
                                    cmd.Parameters.AddWithValue("@TaskClassification", reader.GetString(2));
                                    cmd.Parameters.AddWithValue("@AssignedDate", reader.GetDateTime(3));
                                    cmd.Parameters.AddWithValue("@CompletedDate", reader.GetDateTime(4));
                                    cmd.Parameters.AddWithValue("@EffortHours", reader.GetDouble(5));
                                    sumOfTotalCatw = sumOfTotalCatw + reader.GetDouble(5);
                                    j = cmd.ExecuteNonQuery();
                                }
                                con.Close();
                            }
                        }
                        FillHoursTable(PersonName, sumOfTotalCatw);
                        #endregion

                        reader.NextResult();
                        reader.NextResult();

                        #region Vacation Dates
                        if (reader.Name == "Vacation Dates")
                        {
                            reader.Read();
                            SqlConnection con = new SqlConnection("Server=HIB30BWAX2; Initial Catalog = metrics; User ID = sa; Password = Passw0rd@12;");
                            while (reader.Read())
                            {

                                con.Open();

                                bool noRows = false;
                                try
                                {
                                    DateTime? d = reader.GetDateTime(1);
                                }
                                catch
                                {
                                    noRows = true;
                                }
                                if (noRows)
                                {
                                    break;
                                }
                                else
                                {
                                    SqlCommand cmd = new SqlCommand("insertintovacationDates @EmployeeName,@StartDate,@EndDate,@NumberOfDates,@Weekend", con);
                                    cmd.Parameters.AddWithValue("@EmployeeName", PersonName);
                                    cmd.Parameters.AddWithValue("@StartDate", reader.GetDateTime(1));
                                    cmd.Parameters.AddWithValue("@EndDate", reader.GetDateTime(2));
                                    string totalNumberofWeekend = Weekend(((reader.GetDateTime(1)).Day), ((reader.GetDateTime(2)).Day));
                                    int Number = (((reader.GetDateTime(2)).Day) + 1) - (reader.GetDateTime(1)).Day;
                                    int totalHolidays = TotalHolidays(((reader.GetDateTime(1)).Day), ((reader.GetDateTime(2)).Day));
                                    cmd.Parameters.AddWithValue("@NumberOfDates", Number - totalHolidays);
                                    cmd.Parameters.AddWithValue("@Weekend", totalNumberofWeekend);
                                    j = cmd.ExecuteNonQuery();
                                }
                                con.Close();
                            }
                        }
                        #endregion

                        reader.NextResult();

                        #region Time Sheet
                        if (reader.Name == "Timesheet")
                        {
                            reader.Read();
                            SqlConnection con = new SqlConnection("Server=HIB30BWAX2; Initial Catalog = metrics; User ID = sa; Password = Passw0rd@12;");
                            while (reader.Read())
                            {

                                con.Open();
                              
                                if (reader.GetString(0) == null || reader.GetString(0) == "")
                                {
                                    //SqlCommand cmd = new SqlCommand("insertintoCatwHours @EmployeeName,@TotalCatwHours", con);
                                    //cmd.Parameters.AddWithValue("@EmployeeName", PersonName);
                                    //cmd.Parameters.AddWithValue("@TotalCatwHours", 0);
                                    //j = cmd.ExecuteNonQuery();
                                    break;
                                }
                                else
                                {
                                    SqlCommand cmd = new SqlCommand("UpdateFillHoursTable @EmployeeName,@CatwHours", con);
                                    cmd.Parameters.AddWithValue("@EmployeeName", PersonName);
                                    cmd.Parameters.AddWithValue("@CatwHours", reader.GetDouble(1));
                                    j = cmd.ExecuteNonQuery();
                                }
                                con.Close();
                            }

                        }
                        #endregion
                    }
                }

            }
            return View();
        }
        public void FillHoursTable(string fileName, double sumOfTotalCatw)
        {
            int i;
            SqlConnection con = new SqlConnection("Server=HIB30BWAX2; Initial Catalog = metrics; User ID = sa; Password = Passw0rd@12;");
            con.Open();
            SqlCommand cmd = new SqlCommand("insertintoFillHoursTable @EmployeeName,@MetricsHours,@CatwHours", con);
            cmd.Parameters.AddWithValue("@EmployeeName", fileName);
            cmd.Parameters.AddWithValue("@MetricsHours", sumOfTotalCatw);
            cmd.Parameters.AddWithValue("@CatwHours",0);
            i = cmd.ExecuteNonQuery();
            con.Close();
        }
        public ActionResult Delete()
        {
            int i;
            SqlConnection con = new SqlConnection("Server=HIB30BWAX2; Initial Catalog = metrics; User ID = sa; Password = Passw0rd@12;");
            con.Open();
            SqlCommand cmd = new SqlCommand("DeleteMetrics", con);
            i = cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("UploadToServer");
        }
        public ActionResult Test()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Test(HttpPostedFileBase[] files)
        {
            string _path = null;
            for (int i = 0; i < files.Length; i++)
            {
                string _FileName = Path.GetFileName(files[i].FileName);
                _path = Path.Combine(Server.MapPath("~/Test/"), _FileName);
                files[i].SaveAs(_path);
            }
            string path = Server.MapPath("~/Test/");


            return View();
        }
        public string Weekend(int startDate, int EndDate)
        {
            int[] Dates = new int[] { 3, 4, 10, 11, 17, 18, 24, 25 };
            int sd = startDate;
            int tsd = startDate;
            int ed = EndDate;
            int ted = EndDate;
            string totalNumberofWeekend = null;
            List<int> absentDates = new List<int>();

            for (int i = sd; i < ed + 1;)
            {
                absentDates.Add(tsd);
                tsd++;
                ed--;
            }
            foreach (int item in absentDates)
            {
                if (Dates.Contains(item))
                {
                    totalNumberofWeekend = totalNumberofWeekend  + item + " ";
                }
            }
            if (totalNumberofWeekend != null)
            {
                totalNumberofWeekend = totalNumberofWeekend + "are weekend days";
            }
            else
            {
                totalNumberofWeekend = "No Weekend";
            }
            return totalNumberofWeekend;
        }
        public int TotalHolidays(int startDate, int EndDate)
        {
            int[] Dates = new int[] { 5, 6, 12, 13, 19, 20, 26, 27 };
            int sd = startDate;
            int tsd = startDate;
            int ed = EndDate;
            int ted = EndDate;
            int totalNumberofWeekend = 0;
            List<int> absentDates = new List<int>();

            for (int i = sd; i < ed + 1;)
            {
                absentDates.Add(tsd);
                tsd++;
                ed--;
            }
            foreach (int item in absentDates)
            {
                if (Dates.Contains(item))
                {
                    totalNumberofWeekend++;
                }
            }
            return totalNumberofWeekend;
        }
        public string GetFileName(string FileName)
        {
            string Name = null;
            int position=0;
            bool found = false;
            for (int i = 0; i < FileName.Length; i++)
            {
                if (Convert.ToChar(FileName[i])=='-')
                {
                    position = i;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                Name = FileName.Remove(position);
            }
            if (Name!=null)
            {
                return Name;
            }
            else
            {
                return "Name cant be Caluclated";
            }
           
        }
    }
}