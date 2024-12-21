using Data.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common;
using TGClothes.Models;
using OfficeOpenXml;
using System.IO;
using System.ComponentModel;

namespace TGClothes.Areas.Admin.Controllers
{
    public class RevenueStatisticController : BaseController
    {
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public RevenueStatisticController(IOrderDetailService orderDetailService, IOrderService orderService, IProductService productService)
        {
            _orderDetailService = orderDetailService;
            _orderService = orderService;
            _productService = productService;
        }
        // GET: Admin/RevenueStatistic
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OrderStatistic(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
            {

                var data = (from o in _orderService.GetAll()
                            join od in _orderDetailService.GetAll() on o.Id equals od.OrderId
                            where o.OrderDate.Month == DateTime.Now.Month && o.Status == (int)OrderStatus.SUCCESSFUL
                            group o by o.OrderDate.Date into g
                            select new
                            {
                                Date = g.Key,
                                TotalOrdersSold = g.Count()
                            }).ToList();

                ViewBag.TotalOrder = data.Sum(x => x.TotalOrdersSold);
                ViewBag.NumOfOrder = data.Select(x => x.TotalOrdersSold);
                ViewBag.DateOrder = data.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
                return View(data);
            }
            else
            {
                var data = (from o in _orderService.GetAll()
                            join od in _orderDetailService.GetAll() on o.Id equals od.OrderId
                            where o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == (int)OrderStatus.SUCCESSFUL
                            group o by o.OrderDate.Date into g
                            select new
                            {
                                Date = g.Key,
                                TotalOrdersSold = g.Count()
                            }).ToList();

                ViewBag.TotalOrder = data.Sum(x => x.TotalOrdersSold);
                ViewBag.NumOfOrder = data.Select(x => x.TotalOrdersSold);
                ViewBag.DateOrder = data.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
                return View(data);
            }
        }

        [HttpPost]
        public ActionResult ExportToExcel(string fromDate, string toDate)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Chuyển đổi tham số từ string sang DateTime
            if (!string.IsNullOrEmpty(fromDate))
                startDate = DateTime.Parse(fromDate);
            if (!string.IsNullOrEmpty(toDate))
                endDate = DateTime.Parse(toDate);

            // Truy vấn dữ liệu
            var data = (from o in _orderService.GetAll()
                        join od in _orderDetailService.GetAll() on o.Id equals od.OrderId
                        join p in _productService.GetAll() on od.ProductId equals p.Id
                        where (!startDate.HasValue || o.OrderDate >= startDate)
                              && (!endDate.HasValue || o.OrderDate <= endDate)
                              && o.Status == (int)OrderStatus.SUCCESSFUL
                        group new { od, p } by o.OrderDate.Date into g
                        select new
                        {
                            Date = g.Key,
                            Revenue = g.Sum(x => x.od.Price * x.od.Quantity),
                            Profit = g.Sum(x => (x.od.Price - x.p.OriginalPrice) * x.od.Quantity)
                        }).ToList();

            // Tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Revenue Statistics");
                worksheet.Cells[1, 1].Value = "Ngày";
                worksheet.Cells[1, 2].Value = "Doanh thu";
                worksheet.Cells[1, 3].Value = "Lợi nhuận";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.Date.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = item.Revenue;
                    worksheet.Cells[row, 3].Value = item.Profit;
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 3].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Thống Kê Doanh Thu {DateTime.Now.ToString("yyyyMMdd")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }


        public ActionResult RevenueStatistic(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
            {

                var data = (from o in _orderService.GetAll()
                            join od in _orderDetailService.GetAll() on o.Id equals od.OrderId
                            join p in _productService.GetAll() on od.ProductId equals p.Id
                            where o.OrderDate.Month == DateTime.Now.Month 
                          && o.OrderDate.Year == DateTime.Now.Year
                          && o.Status == (int)OrderStatus.SUCCESSFUL
                            group new { od, p } by o.OrderDate.Date into g
                            select new
                            {
                                Date = g.Key,
                                Revenue = g.Sum(x => x.od.Price * x.od.Quantity),
                                Profit = g.Sum(x => (x.od.Price - x.p.OriginalPrice) * x.od.Quantity)
                            }).ToList();

                ViewBag.TotalRevenue = data.Sum(x => x.Revenue);
                ViewBag.TotalProfit = data.Sum(x => x.Profit);
                ViewBag.RevenueOfDay = data.Select(x => x.Revenue);
                ViewBag.ProfitOfDay = data.Select(x => x.Profit);
                ViewBag.DateOrder = data.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
                return View(data);
            }
            else
            {
                var data = (from o in _orderService.GetAll()
                            join od in _orderDetailService.GetAll() on o.Id equals od.OrderId
                            join p in _productService.GetAll() on od.ProductId equals p.Id
                            where o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == (int)OrderStatus.SUCCESSFUL
                            group new { od, p } by o.OrderDate.Date into g
                            select new
                            {
                                Date = g.Key,
                                Revenue = g.Sum(x => x.od.Price * x.od.Quantity),
                                Profit = g.Sum(x => (x.od.Price - x.p.OriginalPrice) * x.od.Quantity)
                            }).ToList();

                ViewBag.TotalRevenue = data.Sum(x => x.Revenue);
                ViewBag.TotalProfit = data.Sum(x => x.Profit);
                ViewBag.RevenueOfDay = data.Select(x => x.Revenue);
                ViewBag.ProfitOfDay = data.Select(x => x.Profit);
                ViewBag.DateOrder = data.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
                return View(data);
            }
        }

        public ActionResult ProductStatistic(DateTime? fromDate, DateTime? toDate, string filterType, int page = 1, int pageSize = 10)
        {
            // Dữ liệu mặc định nếu không có filterType
            var productsQuery = _orderService.GetAll()
                .Join(_orderDetailService.GetAll(), o => o.Id, od => od.OrderId, (o, od) => new { o, od })
                .Join(_productService.GetAll(), x => x.od.ProductId, p => p.Id, (x, p) => new { x.o, x.od, p })
                .Where(x => x.o.Status == (int)OrderStatus.SUCCESSFUL);

            // Nếu có chọn ngày
            if (fromDate.HasValue && toDate.HasValue)
            {
                productsQuery = productsQuery.Where(x => x.o.OrderDate >= fromDate && x.o.OrderDate <= toDate);
            }

            // Lọc dữ liệu theo filterType
            IEnumerable<ProductStatistic> result = new List<ProductStatistic>();

            switch (filterType)
            {
                case "Top5":
                    result = (from x in productsQuery
                              group x by x.od.ProductId into g
                              select new ProductStatistic()
                              {
                                  ProductSold = g.Sum(x => x.od.Quantity),
                                  Product = _productService.GetProductById(g.Key),
                                  Price = g.First().od.Price.Value
                              }).OrderByDescending(x => x.ProductSold).Take(5);
                    break;

                case "Bottom5":
                    result = (from x in productsQuery
                              group x by x.od.ProductId into g
                              select new ProductStatistic()
                              {
                                  ProductSold = g.Sum(x => x.od.Quantity),
                                  Product = _productService.GetProductById(g.Key),
                                  Price = g.First().od.Price.Value
                              }).OrderBy(x => x.ProductSold).Take(5);
                    break;

                case "All":
                default:
                    result = (from x in productsQuery
                              group x by x.od.ProductId into g
                              select new ProductStatistic()
                              {
                                  ProductSold = g.Sum(x => x.od.Quantity),
                                  Product = _productService.GetProductById(g.Key),
                                  Price = g.First().od.Price.Value
                              }).OrderByDescending(x => x.ProductSold);
                    break;
            }

            // Paginate the result
            var data = result.ToPagedList(page, pageSize);

            ViewBag.FilterType = filterType;
            // Tính tổng số lượng sản phẩm bán
            ViewBag.TotalProductSold = result.Sum(x => x.ProductSold);

            return View(data);
        }




    }
}