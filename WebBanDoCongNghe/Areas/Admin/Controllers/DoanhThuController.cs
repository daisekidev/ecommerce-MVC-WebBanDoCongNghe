using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanDoCongNghe.Models;
using System.Data.Entity;
using PagedList;

namespace WebBanDoCongNghe.Areas.Admin.Controllers
{

    public class DoanhThuController : Controller
    {
        DBQuanLyBanDoCongNgheEntities db = new DBQuanLyBanDoCongNgheEntities();


        public ActionResult DoanhThuCustomer(DateTime? fromDate, DateTime? toDate, int? employeeId, int? page, string customerUsername)
        {
                int pageNumber = (page ?? 1);
                int pageSize = 5;
            // Kiểm tra nếu ngày bắt đầu không được chọn, sử dụng ngày đầu tiên của tháng hiện tại
            if (!fromDate.HasValue)
            {
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            // Kiểm tra nếu ngày kết thúc không được chọn, sử dụng ngày cuối của tháng hiện tại
            if (!toDate.HasValue)
            {
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            }

            // Lấy danh sách nhân viên từ database
            var employeeList = db.tb_NhanVien.ToList();
            ViewBag.EmployeeList = new SelectList(employeeList, "MaNhanVien", "TenNhanVien");

            // Tra cứu các đơn hàng đã hoàn thành trong khoảng ngày từ fromDate đến toDate
            var completedOrders = db.tb_Order.Include(o => o.tb_NhanVien)
                .Where(o => o.UpdatedDate >= fromDate && o.UpdatedDate <= toDate && o.IsHoanThanh == true);

            // Kiểm tra nếu có giá trị của employeeId, thêm điều kiện lọc theo nhân viên
            if (employeeId.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.tb_NhanVien.MaNhanVien == employeeId.Value);
            }
            if (!string.IsNullOrEmpty(customerUsername))
            {
                completedOrders = completedOrders.Where(o => o.tb_Customer.TaiKhoan == customerUsername);
            }
            // Chuyển kết quả về danh sách
            var ordersList = completedOrders.ToList();

            // Tính toán doanh thu từ các đơn hàng đã hoàn thành
            decimal totalRevenue = ordersList.Sum(o => Convert.ToDecimal(o.TotalPayment));
            string formattedTotalRevenue = totalRevenue.ToString("N0");
            var dailyRevenue = ordersList.GroupBy(o => o.UpdatedDate.HasValue ? o.UpdatedDate.Value.Date : (DateTime?)null).Select(group => new
            {
                Date = group.Key,
                Revenue = group.Sum(o => Convert.ToDecimal(o.TotalPayment))
            }).OrderBy(x => x.Date).ToList();
            var ordersPagedList = ordersList.ToPagedList(pageNumber, pageSize);

            //Truyền dữ liệu đến view hoặc thực hiện các thao tác khác tùy thuộc vào yêu cầu của
            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.FromDate = fromDate.Value;
            ViewBag.ToDate = toDate.Value;
            ViewBag.FormattedTotalRevenue = formattedTotalRevenue;

            return View(ordersPagedList);

        }
        public ActionResult DoanhThuTraveler(DateTime? fromDate, DateTime? toDate, int? employeeId, string phoneCus, int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 5;

            if (!fromDate.HasValue)
            {
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            if (!toDate.HasValue)
            {
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            }

            var employeeList = db.tb_NhanVien.ToList();
            ViewBag.EmployeeList = new SelectList(employeeList, "MaNhanVien", "TenNhanVien");

            var completedOrders = db.tb_Traveler.Include(o => o.tb_NhanVien)
                .Where(o => o.UpdatedDate >= fromDate && o.UpdatedDate <= toDate && o.IsHoanThanh == true);

            if (employeeId.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.tb_NhanVien.MaNhanVien == employeeId.Value);
            }

            if (!string.IsNullOrEmpty(phoneCus))
            {
                completedOrders = completedOrders.Where(o => o.PhoneNumber1.Contains(phoneCus));
            }

            var ordersList = completedOrders.ToList();
            decimal totalRevenue = ordersList.Sum(o => Convert.ToDecimal(o.TotalPayment));
            string formattedTotalRevenue = totalRevenue.ToString("N0");

            var dailyRevenue = ordersList.GroupBy(o => o.UpdatedDate.HasValue ? o.UpdatedDate.Value.Date : (DateTime?)null).Select(group => new
            {
                Date = group.Key,
                Revenue = group.Sum(o => Convert.ToDecimal(o.TotalPayment))
            }).OrderBy(x => x.Date).ToList();

            var ordersPagedList = ordersList.ToPagedList(pageNumber, pageSize);

            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.FromDate = fromDate.Value;
            ViewBag.ToDate = toDate.Value;
            ViewBag.FormattedTotalRevenue = formattedTotalRevenue;

            return View(ordersPagedList);
        }
    }
}