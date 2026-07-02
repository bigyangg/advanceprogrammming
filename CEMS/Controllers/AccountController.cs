using CEMS.Infrastructure;
using CEMS.Models;
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
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existing = await unitOfWork.Participants.FindAsync(p => p.Email == model.Email);
            if (existing.Any())
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            }
        }

        if (!ModelState.IsValid)
        {
            return await RedisplayLoginWithSignupAsync(model);
        }

        try
        {
            var participant = new Participant
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone ?? string.Empty
            };
            await unitOfWork.Participants.AddAsync(participant);
            await unitOfWork.SaveChangesAsync();

            HttpContext.Session.SignInAs(participant.PersonId, participant.GetRole(), participant.Name);
            TempData["FlashSuccess"] = $"Welcome, {participant.Name}! Your account has been created.";
            return RedirectToAction("Index", "Events");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("Signup", ex.Message);
            return await RedisplayLoginWithSignupAsync(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.SignOut();
        return RedirectToAction(nameof(Login));
    }

    private async Task<IActionResult> RedisplayLoginWithSignupAsync(SignupViewModel model)
    {
        ViewBag.Participants = await unitOfWork.Participants.GetAllAsync();
        ViewBag.Administrators = await unitOfWork.Administrators.GetAllAsync();
        ViewBag.Signup = model;
        ViewBag.ActiveTab = "participant";
        ViewBag.ShowSignupForm = true;
        return View("Login", new LoginViewModel());
    }
}
