using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public ActionResult Index() { return View(); }
    [HttpPost]
    public ActionResult Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            if (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (Stream stream = file.OpenReadStream())
                {
                    IExcelDataReader reader;
                    if (file.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    DataTable dt = CreateDataTable(reader);
                    return PartialView("_DataTablePartialView", dt);
                }
            }
            else { ModelState.AddModelError("File", "This file format is not supported"); }
        }
        else { ModelState.AddModelError("File", "Please upload your file"); }
        // Handle validation errors
        return BadRequest(ModelState);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private DataTable CreateDataTable(IExcelDataReader reader)
    {
        DataTable dt = new();
        DataRow row;
        DataTable dt_ = new();
        var Date = GetPersianDate();

        try
        {
            dt_ = reader.AsDataSet().Tables[0];
            dt.Columns.Add("PerDate");
            for (int i = 0; i < dt_.Columns.Count; i++)
            {
                dt.Columns.Add(dt_.Rows[0][i].ToString());
            }
            for (int row_ = 1; row_ < dt_.Rows.Count; row_++)
            {
                row = dt.NewRow();
                row["PerDate"] = Date;

                for (int col = 0; col < dt_.Columns.Count; col++)
                {
                    row[col + 1] = dt_.Rows[row_][col].ToString();
                }
                dt.Rows.Add(row);
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("File", "Unable to upload file!");
        }
        reader.Close();
        reader.Dispose();
        return dt;
    }



    public static string GetPersianDate()
    {
        System.Globalization.PersianCalendar persianCalandar = new();
        var dateTime = DateTime.Now;
        string year = persianCalandar.GetYear(dateTime).ToString().Substring(2, 2);
        string month = persianCalandar.GetMonth(dateTime).ToString("0#");
        //int day = persianCalandar.GetDayOfMonth(dateTime);
        return $"{year}{month}";
    }
}