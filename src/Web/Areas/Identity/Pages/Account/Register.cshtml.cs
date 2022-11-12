// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AyBorg.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("Login");
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("Login");
    }
}
}
