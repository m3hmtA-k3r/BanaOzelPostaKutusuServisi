using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(AppDbContext _context, UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {

                TotalUsers = await _context.Users.CountAsync(),

                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),

                TotalMessages = await _context.UserMessages.CountAsync(m => !m.IsDraft),

                TodayMessages = await _context.UserMessages
                    .CountAsync(m => !m.IsDraft && m.SendDate >= DateTime.Today),

                UnreadMessages = await _context.UserMessages
                    .CountAsync(m => !m.IsDraft && !m.IsRead),

                PendingReports = await _context.MessageReports.CountAsync(r => !r.IsResolved),

                TrashedMessages = await _context.UserMessages
                    .CountAsync(m => !m.IsDraft && (m.IsDeletedBySender || m.IsDeletedByReceiver)),

                TopSenders = await _context.Users
                    .Select(u => new TopItemViewModel
                    {
                        Name = u.FirstName + " " + u.LastName,
                        Count = u.SentMessages.Count(m => !m.IsDraft)
                    })
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync(),

                TopCategories = await _context.Categories
                    .Select(c => new TopItemViewModel
                    {
                        Name = c.Name,
                        Count = c.Messages.Count(m => !m.IsDraft)
                    })
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync()

            };

            return View(model);
        }


        public async Task<IActionResult> Users(string? search)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();

                query = query.Where(u => u.FirstName.Contains(s)
                                      || u.LastName.Contains(s)
                                      || u.Email.Contains(s)
                                      || u.UserName.Contains(s));
            }

            var users = await query
                .OrderBy(u => u.FirstName)
                .Select(u => new AdminUserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    SentCount = u.SentMessages.Count(m => !m.IsDraft)
                })
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsAdmin = adminUserIds.Contains(user.Id);
            }

            ViewBag.Search = search;

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (currentUser.Id == id)
            {
                TempData["AdminError"] = "Kendi hesabınızı pasif yapamazsınız.";
                return RedirectToAction("Users");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is not null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = user.IsActive
                    ? $"{user.FirstName} {user.LastName} aktifleştirildi."
                    : $"{user.FirstName} {user.LastName} pasif yapıldı.";
            }

            return RedirectToAction("Users");
        }


        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(int id)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (currentUser.Id == id)
            {
                TempData["AdminError"] = "Kendi yönetici yetkinizi kaldıramazsınız.";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is not null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    TempData["AdminSuccess"] = $"{user.FirstName} {user.LastName} artık yönetici değil.";
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["AdminSuccess"] = $"{user.FirstName} {user.LastName} yönetici yapıldı.";
                }
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Reports(bool showResolved = false)
        {
            var query = _context.MessageReports.AsQueryable();

            if (!showResolved)
            {
                query = query.Where(r => !r.IsResolved);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminReportListItemViewModel
                {
                    Id = r.Id,
                    MessageId = r.MessageId,
                    MessageSubject = r.Message.Subject,
                    MessageBody = r.Message.Body,
                    SenderName = r.Message.Sender.FirstName + " " + r.Message.Sender.LastName,
                    ReceiverName = r.Message.Receiver != null
                        ? r.Message.Receiver.FirstName + " " + r.Message.Receiver.LastName
                        : "-",
                    ReporterName = r.ReportedByUser.FirstName + " " + r.ReportedByUser.LastName,
                    Reason = r.Reason,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt,
                    IsResolved = r.IsResolved
                })
                .ToListAsync();

            ViewBag.ShowResolved = showResolved;
            ViewBag.PendingCount = await _context.MessageReports.CountAsync(r => !r.IsResolved);

            return View(reports);
        }


        [HttpPost]
        public async Task<IActionResult> ResolveReport(int id, bool showResolved = false)
        {
            var report = await _context.MessageReports.FirstOrDefaultAsync(r => r.Id == id);

            if (report is not null)
            {
                report.IsResolved = !report.IsResolved;
                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = report.IsResolved
                    ? "Şikayet çözüldü olarak işaretlendi."
                    : "Şikayet yeniden açıldı.";
            }

            return RedirectToAction("Reports", new { showResolved });
        }





    }
}
