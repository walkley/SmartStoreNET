using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Polls;
using SmartStore.Services.Customers;
using SmartStore.Services.Polls;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Polls;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.Web.Controllers
{
    public partial class PollController : PublicControllerBase
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IPollService _pollService;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Constructors

        public PollController(
            IWorkContext workContext,
            IPollService pollService,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._pollService = pollService;
            this._webHelper = webHelper;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected PollModel PreparePollModel(Poll poll, bool setAlreadyVotedProperty)
        {
            var model = new PollModel
            {
                Id = poll.Id,
                AlreadyVoted = setAlreadyVotedProperty && _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id),
                Name = poll.Name
            };

            var answers = poll.PollAnswers.OrderBy(x => x.DisplayOrder);

            foreach (var answer in answers)
            {
                model.TotalVotes += answer.NumberOfVotes;
            }

            foreach (var pa in answers)
            {
                model.Answers.Add(new PollAnswerModel
                {
                    Id = pa.Id,
                    Name = pa.Name,
                    NumberOfVotes = pa.NumberOfVotes,
                    PercentOfTotalVotes = model.TotalVotes > 0 ? ((Convert.ToDouble(pa.NumberOfVotes) / Convert.ToDouble(model.TotalVotes)) * Convert.ToDouble(100)) : 0,
                });
            }

            return model;
        }

        #endregion

        #region Methods

        /* Added by CTA: This attribute is not available anymore. An alternative is using ViewComponents:
Sample:

public class SampleViewComponent : ViewComponent
    {
        private readonly InjectedService _injectedService;

        public SampleViewComponent (InjectedService injectedService)
        {
            _injectedService = injectedService;
        }


       public IViewComponentResult Invoke(int parameter)
        {
            var object = _injectedService.SampleFunction(parameter);
        // No name is specified, returns the view SampleView (same name as component)
            return View(object);
        }
    }

Then use this to call the view component from any view:

    @await Component.InvokeAsync("SampleView", new { parameter = ""})

https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components?view=aspnetcore-3.1 */
[ChildActionOnly]
        public ActionResult PollBlock(string systemKeyword)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return Content("");

            var cacheKey = string.Format(ModelCacheEventConsumer.POLL_BY_SYSTEMNAME_MODEL_KEY, systemKeyword, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var poll = _pollService.GetPollBySystemKeyword(systemKeyword, _workContext.WorkingLanguage.Id);

                if (poll == null)
                    return new PollModel { Id = 0 };	//we do not cache nulls. that's why let's return an empty record (ID = 0)

                return PreparePollModel(poll, false);
            });

            if (cachedModel == null || cachedModel.Id == 0)
                return Content("");

            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = (PollModel)cachedModel.Clone();
            model.AlreadyVoted = _pollService.AlreadyVoted(model.Id, _workContext.CurrentCustomer.Id);
            model.SystemKeyword = systemKeyword;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Vote(int pollAnswerId)
        {
            var pollAnswer = _pollService.GetPollAnswerById(pollAnswerId);

            if (pollAnswer == null)
            {
                return Json(new { error = T("Polls.AnswerNotFound", pollAnswerId).Text });
            }

            var poll = pollAnswer.Poll;

            if (!poll.Published)
            {
                return Json(new { error = T("Polls.NotAvailable").Text });
            }

            if (_workContext.CurrentCustomer.IsGuest() && !poll.AllowGuestsToVote)
            {
                return Json(new { error = T("Polls.OnlyRegisteredUsersVote").Text });
            }

            bool alreadyVoted = _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id);
            if (!alreadyVoted)
            {
                //vote
                pollAnswer.PollVotingRecords.Add(new PollVotingRecord
                {
                    PollAnswerId = pollAnswer.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    IsApproved = true
                });

                //update totals
                pollAnswer.NumberOfVotes = pollAnswer.PollVotingRecords.Count;

                _pollService.UpdatePoll(poll);
            }

            return Json(new
            {
                html = this.RenderPartialViewToString("_Poll", PreparePollModel(poll, true)),
            });
        }

        /* Added by CTA: This attribute is not available anymore. An alternative is using ViewComponents:
Sample:

public class SampleViewComponent : ViewComponent
    {
        private readonly InjectedService _injectedService;

        public SampleViewComponent (InjectedService injectedService)
        {
            _injectedService = injectedService;
        }


       public IViewComponentResult Invoke(int parameter)
        {
            var object = _injectedService.SampleFunction(parameter);
        // No name is specified, returns the view SampleView (same name as component)
            return View(object);
        }
    }

Then use this to call the view component from any view:

    @await Component.InvokeAsync("SampleView", new { parameter = ""})

https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components?view=aspnetcore-3.1 */
[ChildActionOnly]
        public ActionResult HomePagePolls()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_POLLS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                return _pollService.GetPolls(_workContext.WorkingLanguage.Id, true, 0, int.MaxValue)
                    .Select(x => PreparePollModel(x, false))
                    .ToList();
            });

            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = new List<PollModel>();

            foreach (var p in cachedModel)
            {
                var pollModel = (PollModel)p.Clone();
                pollModel.AlreadyVoted = _pollService.AlreadyVoted(pollModel.Id, _workContext.CurrentCustomer.Id);
                pollModel.SystemKeyword = "Homepage";
                model.Add(pollModel);
            }

            if (model.Count == 0)
                return Content("");

            return PartialView(model);
        }

        #endregion
    }
}
