using System.Text.Json;
using Microsoft.JSInterop;

namespace SchedulerWasm.Client;

public class DataService
{
    private readonly IJSRuntime _js;
    private int _nextStudentId = 1;
    private int _nextLessonId = 1;
    private bool _loaded = false;

    public List<Models.Student> Students { get; } = new();
    public List<Models.Lesson> Lessons { get; } = new();

    public DataService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task LoadAsync()
    {
        if (_loaded) return;

        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", "scheduler_data");
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonSerializer.Deserialize<StorageData>(json);
                if (data != null)
                {
                    Students.AddRange(data.Students);
                    Lessons.AddRange(data.Lessons);
                    _nextStudentId = data.NextStudentId;
                    _nextLessonId = data.NextLessonId;
                    _loaded = true;
                    return;
                }
            }
        }
        catch { }

        Seed();
        await SaveAsync();
        _loaded = true;
    }

    private void Seed()
    {
        Students.Add(new Models.Student { Id = _nextStudentId++, Name = "أحمد محمد", Phone = "0501234567", Notes = "طالب ممتاز" });
        Students.Add(new Models.Student { Id = _nextStudentId++, Name = "فاطمة علي", Phone = "0559876543", Notes = "" });
        Students.Add(new Models.Student { Id = _nextStudentId++, Name = "خالد عبدالله", Phone = "0561112233", Notes = "يحتاج مراجعة" });

        Lessons.Add(new Models.Lesson { Id = _nextLessonId++, Subject = "رياضيات", StudentId = 1, Type = Models.LessonType.Weekly, DayOfWeek = DayOfWeek.Sunday, StartTime = "09:00", DurationMinutes = 60 });
        Lessons.Add(new Models.Lesson { Id = _nextLessonId++, Subject = "فيزياء", StudentId = 1, Type = Models.LessonType.Weekly, DayOfWeek = DayOfWeek.Tuesday, StartTime = "09:00", DurationMinutes = 60 });
        Lessons.Add(new Models.Lesson { Id = _nextLessonId++, Subject = "عربي", StudentId = 2, Type = Models.LessonType.Weekly, DayOfWeek = DayOfWeek.Sunday, StartTime = "10:00", DurationMinutes = 45 });
        Lessons.Add(new Models.Lesson { Id = _nextLessonId++, Subject = "إنجليزي", StudentId = 3, Type = Models.LessonType.Weekly, DayOfWeek = DayOfWeek.Monday, StartTime = "09:00", DurationMinutes = 60 });
        Lessons.Add(new Models.Lesson { Id = _nextLessonId++, Subject = "كيمياء", StudentId = 2, Type = Models.LessonType.Weekly, DayOfWeek = DayOfWeek.Sunday, StartTime = "09:30", DurationMinutes = 60 });
    }

    public async Task SaveAsync()
    {
        var data = new StorageData { Students = Students, Lessons = Lessons, NextStudentId = _nextStudentId, NextLessonId = _nextLessonId };
        var json = JsonSerializer.Serialize(data);
        await _js.InvokeVoidAsync("localStorage.setItem", "scheduler_data", json);
    }

    public async Task AddStudent(Models.Student s) { s.Id = _nextStudentId++; Students.Add(s); await SaveAsync(); }
    public async Task UpdateStudent(Models.Student s) { var old = Students.FirstOrDefault(x => x.Id == s.Id); if (old != null) { old.Name = s.Name; old.Phone = s.Phone; old.Notes = s.Notes; } await SaveAsync(); }
    public async Task DeleteStudent(int id) { Students.RemoveAll(s => s.Id == id); Lessons.RemoveAll(l => l.StudentId == id); await SaveAsync(); }
    public Models.Student? GetStudent(int id) => Students.FirstOrDefault(s => s.Id == id);

    public async Task AddLesson(Models.Lesson l) { l.Id = _nextLessonId++; Lessons.Add(l); await SaveAsync(); }
    public async Task UpdateLesson(Models.Lesson l) { var old = Lessons.FirstOrDefault(x => x.Id == l.Id); if (old != null) { old.Subject = l.Subject; old.StudentId = l.StudentId; old.Type = l.Type; old.DayOfWeek = l.DayOfWeek; old.SpecificDate = l.SpecificDate; old.StartTime = l.StartTime; old.DurationMinutes = l.DurationMinutes; old.Notes = l.Notes; } await SaveAsync(); }
    public async Task DeleteLesson(int id) { Lessons.RemoveAll(l => l.Id == id); await SaveAsync(); }
    public Models.Lesson? GetLesson(int id) => Lessons.FirstOrDefault(l => l.Id == id);

    public List<Models.Conflict> DetectConflicts()
    {
        var conflicts = new List<Models.Conflict>();
        var recurring = Lessons.Where(l => l.Type == Models.LessonType.Weekly).ToList();
        for (int i = 0; i < recurring.Count; i++)
        {
            for (int j = i + 1; j < recurring.Count; j++)
            {
                var a = recurring[i]; var b = recurring[j];
                if (a.DayOfWeek != b.DayOfWeek) continue;
                var startA = TimeSpan.Parse(a.StartTime); var endA = startA.Add(TimeSpan.FromMinutes(a.DurationMinutes));
                var startB = TimeSpan.Parse(b.StartTime); var endB = startB.Add(TimeSpan.FromMinutes(b.DurationMinutes));
                if (startA < endB && startB < endA)
                {
                    var overlapStart = startA > startB ? startA : startB;
                    var overlapEnd = endA < endB ? endA : endB;
                    var minutes = (int)(overlapEnd - overlapStart).TotalMinutes;
                    if (minutes > 0) conflicts.Add(new Models.Conflict { A = a, B = b, OverlapMinutes = minutes });
                }
            }
        }
        return conflicts;
    }

    public string GetStudentName(int id) => Students.FirstOrDefault(s => s.Id == id)?.Name ?? "غير معروف";

    private class StorageData
    {
        public List<Models.Student> Students { get; set; } = new();
        public List<Models.Lesson> Lessons { get; set; } = new();
        public int NextStudentId { get; set; }
        public int NextLessonId { get; set; }
    }
}
