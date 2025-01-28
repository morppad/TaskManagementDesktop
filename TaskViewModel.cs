namespace TaskManagment.Data
{
    public class TaskViewModel
    {
        public string Title { get; set; }
        public string Priority { get; set; }
        public string UserName { get; set; } // Имя пользователя (из таблицы users)
        public string DueDate { get; set; }  // Дата завершения задачи
        public string Status { get; set; }
    }
}
