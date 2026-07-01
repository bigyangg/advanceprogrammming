using CEMS.Infrastructure;
using CEMS.Repositories;
using CEMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers;

public class AccountController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Login()
    {
        ViewBag.Participants = await unitOfWork.Participants.GetAllAsync();
        ViewBag.Administrators = await unitOfWork.Administrators.GetAllAsync();
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Participants = await unitOfWork.Participants.GetAllAsync();
            ViewBag.Administrators = await unitOfWork.Administrators.GetAllAsync();
            return View(model);
        }

        var participant = await unitOfWork.Participants.GetByIdAsync(model.PersonId);
        if (participant is not null)
        {
            HttpContext.Session.SignInAs(participant.PersonId, participant.GetRole(), participant.Name);
            return RedirectToAction("Index", "Events");
        }

        var admin = await unitOfWork.Administrators.GetByIdAsync(model.PersonId);
        if (admin is not null)
        {
            HttpContext.Session.SignInAs(admin.PersonId, admin.GetRole(), admin.Name);
            return RedirectToAction("AdminIndex", "Events");
        }

        ModelState.AddModelError(string.Empty, "Invalid Person ID.");
        ViewBag.Participants = await unitOfWork.Participants.GetAllAsync();
        ViewBag.Administrators = await unitOfWork.Administrators.GetAllAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.SignOut();
        return RedirectToAction(nameof(Login));
    }
}
