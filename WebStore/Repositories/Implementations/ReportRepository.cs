using Microsoft.EntityFrameworkCore;
using WebStore.Data;
using WebStore.Models;
using WebStore.Repositories.Interfaces;
using WebStore.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace WebStore.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public ReportRepository(AppDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<ReportDTO> GetDailyEarningsReport(DateTime date)
        {
            var cacheKey = $"daily_report_{date:yyyyMMdd}";

            
            if (_memoryCache.TryGetValue(cacheKey, out ReportDTO cachedReport))
            {
                return cachedReport; 
            }

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var totalEarnings = await _context.Orders
                .Where(o => o.OrderDate.Date == utcDate.Date && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderItems)
                .SumAsync(oi => oi.Quantity * oi.UnitPrice);

            var mostSellingProduct = await _context.OrderItems
                .Where(oi => oi.Order.OrderDate.Date == utcDate.Date && oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Product)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .FirstOrDefaultAsync();

            var report = new ReportDTO
            {
                ReportDate = DateOnly.FromDateTime(utcDate),
                TotalEarnings = totalEarnings,
                MostSellingProductName = mostSellingProduct?.ProductName ?? "N/A",
                MostSellingProductQuantity = mostSellingProduct?.TotalQuantity ?? 0
            };

            
            _memoryCache.Set(cacheKey, report, TimeSpan.FromMinutes(10));

            return report;
        }



        public async Task<ReportDTO> GetMonthlyEarningsReport(int month, int year)
        {
            var cacheKey = $"monthly_report_{year}_{month}";

            if (_memoryCache.TryGetValue(cacheKey, out ReportDTO cachedReport))
            {
                return cachedReport;
            }

            var totalEarnings = await _context.Orders
                .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderItems)
                .SumAsync(oi => oi.Quantity * oi.UnitPrice);

            var mostSellingProduct = await _context.OrderItems
                .Where(oi => oi.Order.OrderDate.Month == month && oi.Order.OrderDate.Year == year && oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Product)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .FirstOrDefaultAsync();

            var report = new ReportDTO
            {
                ReportDate = new DateOnly(year, month, 1),
                TotalEarnings = totalEarnings,
                MostSellingProductName = mostSellingProduct?.ProductName ?? "N/A",
                MostSellingProductQuantity = mostSellingProduct?.TotalQuantity ?? 0
            };

            _memoryCache.Set(cacheKey, report, TimeSpan.FromMinutes(10));

            return report;
        }

        public async Task<IEnumerable<ReportDTO>> GetTopSellingProductsReport(int topCount)
        {
            var topSellingProducts = await _context.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Product)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Take(topCount)
                .Select(g => new ReportDTO
                {
                    ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    MostSellingProductName = g.Key.Name,
                    MostSellingProductQuantity = g.Sum(oi => oi.Quantity),
                    TotalEarnings = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToListAsync();

            return topSellingProducts;
        }
    }
}
