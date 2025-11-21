// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proyecto.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public void OnGet()
        {
            // Intentionally empty.
            // This page only displays static information (Access Denied) and requires no
            // server-side processing on GET requests. Keeping this method empty makes
            // it explicit that no action is taken here. If later server-side logic
            // is required (e.g., logging, redirecting), implement it here.
        }
    }
}
