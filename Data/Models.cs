using Supabase;
using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TaskManagment.Data
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("role")]
        public string Role { get; set; } = "customer";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("tasks")]
    public class TaskModel : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Открытый конструктор без параметров
        public TaskModel() { }
    }
}
