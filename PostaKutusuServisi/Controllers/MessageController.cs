using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.DTOs.UserMessageDto;
using PostaKutusuServisi.Entities;
using System.Net;
using System.Threading.Tasks;

namespace PostaKutusuServisi.Controllers
{
    [Authorize]
    public class MessageController(UserManager<AppUser> _userManager,
                                   AppDbContext _context) : Controller
    {

        public async Task<IActionResult> Index(MessageFilterDto filter)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.EmailConfirmed = user.EmailConfirmed;


            var query = _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .Where(x => x.ReceiverId == user.Id && !x.IsDraft && !x.IsDeletedByReceiver);

            var messages = await PaginateAsync(ApplyFilters(query, filter), filter);

            return View(messages);
        }


        public async Task<IActionResult> SendMail(string? mail, string? subject)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            await LoadCategoriesAsync(user.Id);

            var dto = new SendMailDto();

            if (!string.IsNullOrEmpty(mail))
            {
                dto.ReceiverMail = mail;
            }

            if (!string.IsNullOrEmpty(subject))
            {
                dto.Subject = subject.StartsWith("Re:") ? subject : "Re: " + subject;
            }

            return View(dto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto, string operation)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);

            // kategori gerçekten bu kullanıcıya_mı_ait?
            int? categoryId = null;

            if (sendMailDto.CategoryId.HasValue)
            {
                var ownsCategory = await _context.Categories
                    .AnyAsync(c => c.Id == sendMailDto.CategoryId && c.UserId == sender.Id);

                if (ownsCategory)
                {
                    categoryId = sendMailDto.CategoryId;
                }
            }

            UserMessage? draft = null;

            if (sendMailDto.DraftId.HasValue)
            {
                draft = await _context.UserMessages
                    .FirstOrDefaultAsync(x => x.Id == sendMailDto.DraftId
                                           && x.SenderId == sender.Id
                                           && x.IsDraft);
            }

            //  taslak_Olarak_Kaydet 
            if (operation == "draft")
            {
                int? receiverId = null;

                if (!string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
                {
                    var possibleReceiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
                    if (possibleReceiver is not null)
                    {
                        receiverId = possibleReceiver.Id;
                    }
                }

                if (draft is not null)
                {
                    draft.Subject = sendMailDto.Subject ?? string.Empty;
                    draft.Body = sendMailDto.Body ?? string.Empty;
                    draft.ReceiverId = receiverId;
                    draft.CategoryId = categoryId;
                    draft.SendDate = DateTime.Now;
                }
                else
                {
                    _context.UserMessages.Add(new UserMessage
                    {
                        SendDate = DateTime.Now,
                        SenderId = sender.Id,
                        ReceiverId = receiverId,
                        Subject = sendMailDto.Subject ?? string.Empty,
                        Body = sendMailDto.Body ?? string.Empty,
                        CategoryId = categoryId,
                        IsDraft = true
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Drafts");
            }

            //  GÖNDER 
            if (string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                ModelState.AddModelError(string.Empty, "Alıcı e-posta adresi gerekli");
                await LoadCategoriesAsync(sender.Id);
                return View(sendMailDto);
            }

            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);

            if (receiver is null)
            {
                ModelState.AddModelError(string.Empty, "Girdiğiniz mail ile sistemde kayıtlı kullanıcı bulunamadı");
                await LoadCategoriesAsync(sender.Id);
                return View(sendMailDto);
            }

            if (draft is not null)
            {
                draft.ReceiverId = receiver.Id;
                draft.Subject = sendMailDto.Subject;
                draft.Body = sendMailDto.Body;
                draft.CategoryId = categoryId;
                draft.SendDate = DateTime.Now;
                draft.IsDraft = false;
            }
            else
            {
                _context.UserMessages.Add(new UserMessage
                {
                    SendDate = DateTime.Now,
                    SenderId = sender.Id,
                    ReceiverId = receiver.Id,
                    Subject = sendMailDto.Subject,
                    Body = sendMailDto.Body,
                    CategoryId = categoryId,
                    IsDraft = false
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }




        public async Task<IActionResult> MailDetail(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

             var message = await _context.UserMessages
                 .Include(x => x.Sender)
                 .Include(x => x.Receiver)
                 .Include(x => x.Category)
                 .FirstOrDefaultAsync(x => x.Id == id && !x.IsDraft &&
                             (x.ReceiverId == user.Id || x.SenderId == user.Id));



            if (message is null)
            {
                return RedirectToAction("Index");
            }

            if (message.ReceiverId == user.Id && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.CurrentUserId = user.Id;

            return View(message);
        }


        public async Task<IActionResult> Important(MessageFilterDto filter)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var query = _context.UserMessages
                 .Include(x => x.Sender)
                 .Include(x => x.Category)
                 .Where(x => x.ReceiverId == user.Id && x.IsImportant && !x.IsDraft && !x.IsDeletedByReceiver);

            var messages = await PaginateAsync(ApplyFilters(query, filter), filter);

            return View(messages);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleImportant(int id, string? returnAction)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == user.Id);

            if (message is not null)
            {
                message.IsImportant = !message.IsImportant;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(returnAction ?? "Index");
        }


        public async Task<IActionResult> Sent(MessageFilterDto filter)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var query = _context.UserMessages
                .Include(x => x.Receiver)
                .Include(x => x.Category)
                .Where(x => x.SenderId == user.Id && !x.IsDraft && !x.IsDeletedBySender);

            var messages = await PaginateAsync(ApplyFilters(query, filter), filter);

            return View(messages);
        }



        public async Task<IActionResult> Forward(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDraft &&
                                         (x.ReceiverId == user.Id || x.SenderId == user.Id));

            if (message is null)
            {
                return RedirectToAction("Index");
            }

            await LoadCategoriesAsync(user.Id);

            var dto = new SendMailDto
            {
                Subject = message.Subject.StartsWith("Fwd:") ? message.Subject : "Fwd: " + message.Subject,
                Body = "\n\n--- İletilen mesaj ---\n"
                     + $"Kimden: {message.Sender.FirstName} {message.Sender.LastName} <{message.Sender.Email}>\n"
                     + $"Tarih: {message.SendDate:dd.MM.yyyy HH:mm}\n\n"
                     + message.Body
            };

            return View("SendMail", dto);
        }


        public async Task<IActionResult> Drafts()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var drafts = await _context.UserMessages
                .Include(x => x.Receiver)
                .Where(x => x.SenderId == user.Id && x.IsDraft)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(drafts);
        }

        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var draft = await _context.UserMessages
                .Include(x => x.Receiver)
                .FirstOrDefaultAsync(x => x.Id == id && x.SenderId == user.Id && x.IsDraft);

            if (draft is null)
            {
                return RedirectToAction("Drafts");
            }

            await LoadCategoriesAsync(user.Id);

            var dto = new SendMailDto
            {
                DraftId = draft.Id,
                ReceiverMail = draft.Receiver?.Email,
                Subject = draft.Subject,
                Body = draft.Body,
                CategoryId = draft.CategoryId
            };

            return View("SendMail", dto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var draft = await _context.UserMessages
                .FirstOrDefaultAsync(x => x.Id == id && x.SenderId == user.Id && x.IsDraft);

            if (draft is not null)
            {
                _context.UserMessages.Remove(draft);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Drafts");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string? returnAction)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x => x.Id == id &&
                                         (x.ReceiverId == user.Id || x.SenderId == user.Id));

            if (message is not null)
            {
                if (message.ReceiverId == user.Id)
                {
                    message.IsDeletedByReceiver = true;
                }

                if (message.SenderId == user.Id)
                {
                    message.IsDeletedBySender = true;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(returnAction ?? "Index");
        }



        public async Task<IActionResult> Trash(MessageFilterDto filter)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var query = _context.UserMessages
                 .Include(x => x.Sender)
                 .Include(x => x.Receiver)
                 .Include(x => x.Category)
                 .Where(x => !x.IsDraft &&
                            ((x.ReceiverId == user.Id && x.IsDeletedByReceiver) ||
                            (x.SenderId == user.Id && x.IsDeletedBySender)));

            var messages = await PaginateAsync(ApplyFilters(query, filter), filter);

            ViewBag.CurrentUserId = user.Id;

            return View(messages);
        }



        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x => x.Id == id &&
                                         (x.ReceiverId == user.Id || x.SenderId == user.Id));

            if (message is not null)
            {
                if (message.ReceiverId == user.Id)
                {
                    message.IsDeletedByReceiver = false;
                }

                if (message.SenderId == user.Id)
                {
                    message.IsDeletedBySender = false;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Trash");
        }

        // IQueryable, henüz çalıştırılmamış bir sorgu tarifidir. // Filtreleme işlemlerini uygulayan için bunu buldum. 
        private IQueryable<UserMessage> ApplyFilters(IQueryable<UserMessage> query, MessageFilterDto filter) 
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x => x.Subject.Contains(search)
                                      || x.Body.Contains(search)
                                      || x.Sender.FirstName.Contains(search)
                                      || x.Sender.LastName.Contains(search)
                                      || (x.Receiver != null &&
                                            (x.Receiver.FirstName.Contains(search) ||
                                             x.Receiver.LastName.Contains(search))));
            }

            if (filter.ReadStatus == "read")
            {
                query = query.Where(x => x.IsRead);
            }
            else if (filter.ReadStatus == "unread")
            {
                query = query.Where(x => !x.IsRead);
            }

            if (filter.OnlyImportant)
            {
                query = query.Where(x => x.IsImportant);
            }
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.SendDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => x.SendDate < filter.EndDate.Value.AddDays(1));
            }

            query = filter.Sort == "oldest"
                ? query.OrderBy(x => x.SendDate)
                : query.OrderByDescending(x => x.SendDate);

            return query;
        }


        private async Task LoadCategoriesAsync(int userId)
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Report(int messageId, string reason, string? description)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x => x.Id == messageId
                                       && x.ReceiverId == user.Id
                                       && !x.IsDraft);

            if (message is null)
            {
                return RedirectToAction("Index");
            }

            var alreadyReported = await _context.MessageReports
                .AnyAsync(r => r.MessageId == messageId && r.ReportedByUserId == user.Id);

            if (alreadyReported)
            {
                TempData["ReportInfo"] = "Bu mesajı daha önce şikayet ettiniz.";
                return RedirectToAction("MailDetail", new { id = messageId });
            }

            _context.MessageReports.Add(new MessageReport
            {
                MessageId = messageId,
                ReportedByUserId = user.Id,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Other" : reason,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                CreatedAt = DateTime.Now,
                IsResolved = false
            });

            await _context.SaveChangesAsync();

            TempData["ReportSuccess"] = "Şikayetiniz yöneticilere iletildi.";

            return RedirectToAction("MailDetail", new { id = messageId });
        }


        private async Task<List<UserMessage>> PaginateAsync(IQueryable<UserMessage> query, MessageFilterDto filter)
        {
            const int pageSize = 15;

            var totalCount = await query.CountAsync();
            var currentPage = filter.Page < 1 ? 1 : filter.Page;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            var list = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            filter.Page = currentPage;

            ViewBag.Filter = filter;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return list;
        }


    }
}
