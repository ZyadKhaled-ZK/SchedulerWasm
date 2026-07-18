namespace SchedulerWasm.Client.Models;

public enum LessonType { Weekly, OneTime }

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class Lesson
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public int StudentId { get; set; }
    public LessonType Type { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public DateTime? SpecificDate { get; set; }
    public string StartTime { get; set; } = "08:00";
    public int DurationMinutes { get; set; } = 60;
    public string Notes { get; set; } = "";
}

public class Conflict
{
    public Lesson A { get; set; } = null!;
    public Lesson B { get; set; } = null!;
    public int OverlapMinutes { get; set; }
}
