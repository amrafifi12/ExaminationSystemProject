﻿using ExaminationSystemProject.Models;
using ExaminationSystemProject.Repository;
using ExaminationSystemProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Reposatories;
using System.Security.Claims;

namespace ExaminationSystemProject.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        IInstructorReposatory InstructorReposatory;
        private readonly IStudentRepository studentRepository;

        public AccountController(UserManager<ApplicationUser> _userManager,SignInManager<ApplicationUser> signInManager,IInstructorReposatory _instructorReposatory, IStudentRepository _studentRepository)
        {
            userManager = _userManager;
            this.signInManager = signInManager;
            InstructorReposatory = _instructorReposatory;
            studentRepository = _studentRepository;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserViewModel UserVM)
        {

            if (ModelState.IsValid==true)

            {




                //string id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;


                //Check
                ApplicationUser userModel=  await  userManager.FindByNameAsync(UserVM.userName);
                if (userModel != null)
                {
                   bool found= await userManager.CheckPasswordAsync(userModel, UserVM.Password);
                    if(found)
                    {
                        List<Claim> claims = new List<Claim>();
                        claims.Add(new Claim("UserId", userModel.UserId.ToString()));


                        //createCookie
                        await signInManager.SignInWithClaimsAsync(userModel, isPersistent: false, claims);
                        string name = User.Identity.Name;


                        return RedirectToAction("Index", "Course");
                    }
                }
                ModelState.AddModelError("", "User Name And Password invalid");

            }
            return View(UserVM);
        }





        [HttpGet]
        public IActionResult Registrasion()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Registrasion(RegisterUserViewModel newUserVM)
        {
            if (ModelState.IsValid)
            {
                //CreateAccount
                ApplicationUser userModel = new ApplicationUser();
                userModel.UserName = newUserVM.UserName;
                userModel.PasswordHash = newUserVM.Password;
                userModel.Address = newUserVM.Address;
                userModel.Type = newUserVM.Type;
                if (userModel.Type == "Instructor")
                {
                    Instructor instructor = new Instructor();
                    instructor.Name = newUserVM.UserName;
                    instructor.Address = newUserVM.Address;
                    int instID = InstructorReposatory.InsertWithId(instructor);
                    
                    userModel.UserId=instID;
                }


               else if (userModel.Type == "Student")
                {
                    Student student  = new Student();
                    student.Name = newUserVM.UserName;
                    student.Address = newUserVM.Address;
                    int stID =studentRepository.InsertWithId(student);

                    userModel.UserId = stID;
                }



                IdentityResult result = await userManager.CreateAsync(userModel,newUserVM.Password);

                if (result.Succeeded==true)
                {

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim("UserId", userModel.UserId.ToString()));



                    //createCookie
                    await signInManager.SignInWithClaimsAsync (userModel, isPersistent: false,claims);
                    string name = User.Identity.Name;

                    return RedirectToAction("Test");
                }

                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }

                }


            }

            return View(newUserVM);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public  IActionResult Test()
        {
            var result = User.FindFirst("UserId").Value;

            return Content(result);
        }



    }
}