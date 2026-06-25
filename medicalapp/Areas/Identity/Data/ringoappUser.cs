using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using medicalapp.Models;

namespace medicalapp.Areas.Identity.Data;

public class medicalappUser : IdentityUser
{
    /// <summary>
    /// User role. One of "Patient", "Doctor", or "Admin".
    /// Stored redundantly here for query speed, separate from the Identity AspNetRoles table.
    /// </summary>
    [PersonalData]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Patient? Patient { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
