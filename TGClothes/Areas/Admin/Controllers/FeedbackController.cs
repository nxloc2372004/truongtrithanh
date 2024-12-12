using Data.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TGClothes.Areas.Admin.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // GET: Admin/Feedback
        public ActionResult Index(int page = 1, int pageSize = 5)
        {
            var feedbacks = _feedbackService.GetAll()
                                            .OrderByDescending(x => x.CreatedDate)
                                            .ToList();

            int totalRecords = feedbacks.Count();
            var pagedFeedbacks = feedbacks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedFeedbacks);
        }



        [HttpDelete]
        public ActionResult Delete(long id)
        {
            _feedbackService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}