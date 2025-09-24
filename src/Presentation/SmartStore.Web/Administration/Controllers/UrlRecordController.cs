using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Admin.Models.UrlRecord;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Security;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class UrlRecordController : AdminControllerBase
    {
        private readonly IUrlRecordService _urlRecordService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ILanguageService _languageService;

        public UrlRecordController(
            IUrlRecordService urlRecordService,
            AdminAreaSettings adminAreaSettings,
            ILanguageService languageService)
        {
            _urlRecordService = urlRecordService;
            _adminAreaSettings = adminAreaSettings;
            _languageService = languageService;
        }

        private void PrepareUrlRecordModel(UrlRecordModel model, UrlRecord urlRecord, bool forList = false)
        {
            if (!forList)
            {
                var allLanguages = _languageService.GetAllLanguages(true);

                model.AvailableLanguages = allLanguages
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                    .ToList();

                model.AvailableLanguages.Insert(0, new SelectListItem { Text = T("Admin.System.SeNames.Language.Standard"), Value = "0" });
            }

            if (urlRecord != null)
            {
                model.Id = urlRecord.Id;
                model.Slug = urlRecord.Slug;
                model.EntityName = urlRecord.EntityName;
                model.EntityId = urlRecord.EntityId;
                model.IsActive = urlRecord.IsActive;
                model.LanguageId = urlRecord.LanguageId;

                if (urlRecord.EntityName.IsCaseInsensitiveEqual("BlogPost"))
                {
                    model.EntityUrl = Url.Action("Edit", "Blog", new { id = urlRecord.EntityId });
                }
                else if (urlRecord.EntityName.IsCaseInsensitiveEqual("Forum"))
                {
                    model.EntityUrl = Url.Action("EditForum", "Forum", new { id = urlRecord.EntityId });
                }
                else if (urlRecord.EntityName.IsCaseInsensitiveEqual("ForumGroup"))
                {
                    model.EntityUrl = Url.Action("EditForumGroup", "Forum", new { id = urlRecord.EntityId });
                }
                else if (urlRecord.EntityName.IsCaseInsensitiveEqual("NewsItem"))
                {
                    model.EntityUrl = Url.Action("Edit", "News", new { id = urlRecord.EntityId });
                }
                else
                {
                    model.EntityUrl = Url.Action("Edit", urlRecord.EntityName, new { id = urlRecord.EntityId });
                }
            }
        }

        [Permission(Permissions.System.UrlRecord.Read)]
        public ActionResult List(string entityName, int? entityId)
        {
            var model = new UrlRecordListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                EntityName = entityName,
                EntityId = entityId
            };

            var allLanguages = _languageService.GetAllLanguages(true);

            model.AvailableLanguages = allLanguages
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            model.AvailableLanguages.Insert(0, new SelectListItem { Text = T("Admin.System.SeNames.Language.Standard"), Value = "0" });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.System.UrlRecord.Read)]
        public ActionResult List(GridCommand command, UrlRecordListModel model)
        {
            var gridModel = new GridModel<UrlRecordModel>();
            var allLanguages = _languageService.GetAllLanguages(true);
            var defaultLanguageName = T("Admin.System.SeNames.Language.Standard");

            var urlRecords = _urlRecordService.GetAllUrlRecords(command.Page - 1, command.PageSize,
                model.SeName, model.EntityName, model.EntityId, model.LanguageId, model.IsActive);

            var slugsPerEntity = _urlRecordService.CountSlugsPerEntity(urlRecords.Select(x => x.Id).Distinct().ToArray());

            gridModel.Data = urlRecords.Select(x =>
            {
                string languageName;

                if (x.LanguageId == 0)
                {
                    languageName = defaultLanguageName;
                }
                else
                {
                    var language = allLanguages.FirstOrDefault(y => y.Id == x.LanguageId);
                    languageName = (language != null ? language.Name : "".NaIfEmpty());
                }

                var urlRecordModel = new UrlRecordModel();
                PrepareUrlRecordModel(urlRecordModel, x, true);

                urlRecordModel.Language = languageName;
                urlRecordModel.SlugsPerEntity = slugsPerEntity.ContainsKey(x.Id) ? slugsPerEntity[x.Id] : 0;

                return urlRecordModel;
            });

            gridModel.Total = urlRecords.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.System.UrlRecord.Read)]
        public ActionResult Edit(int id)
        {
            var urlRecord = _urlRecordService.GetUrlRecordById(id);
            if (urlRecord == null)
            {
                return RedirectToAction("List");
            }

            var model = new UrlRecordModel();
            PrepareUrlRecordModel(model, urlRecord);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.UrlRecord.Update)]
        public ActionResult Edit(UrlRecordModel model, bool continueEditing)
        {
            var urlRecord = _urlRecordService.GetUrlRecordById(model.Id);
            if (urlRecord == null)
            {
                return RedirectToAction("List");
            }

            if (!urlRecord.IsActive && model.IsActive)
            {
                var urlRecords = _urlRecordService.GetAllUrlRecords(0, int.MaxValue, null, model.EntityName, model.EntityId, model.LanguageId, true);
                if (urlRecords.Count > 0)
                {
                    ModelState.AddModelError("IsActive", T("Admin.System.SeNames.ActiveSlugAlreadyExist"));
                }
            }

            if (ModelState.IsValid)
            {
                urlRecord.Slug = model.Slug;
                urlRecord.EntityName = model.EntityName;
                urlRecord.IsActive = model.IsActive;
                urlRecord.LanguageId = model.LanguageId;

                _urlRecordService.UpdateUrlRecord(urlRecord);

                NotifySuccess(T("Admin.Common.DataEditSuccess"));

                return continueEditing ? RedirectToAction("Edit", new { id = urlRecord.Id }) : RedirectToAction("List");
            }

            PrepareUrlRecordModel(model, null);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.UrlRecord.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var urlRecord = _urlRecordService.GetUrlRecordById(id);
            if (urlRecord == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                _urlRecordService.DeleteUrlRecord(urlRecord);

                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                return RedirectToAction("Edit", new { id = urlRecord.Id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.UrlRecord.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var urlRecords = _urlRecordService.GetUrlRecordsByIds(selectedIds.ToArray());

                foreach (var urlRecord in urlRecords)
                {
                    _urlRecordService.DeleteUrlRecord(urlRecord);
                }
            }

            return Json(new { success = true });
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
        public ActionResult NamesPerEntity(string entityName, int entityId)
        {
            // Permission check not necessary.
            if (entityName.IsEmpty() || entityId == 0)
            {
                return new EmptyResult();
            }

            var count = _urlRecordService.CountSlugsPerEntity(entityName, entityId);

            ViewBag.CountSlugsPerEntity = count;
            ViewBag.UrlRecordListUrl = Url.Action("List", "UrlRecord", new { entityName, entityId });

            return View();
        }
    }
}