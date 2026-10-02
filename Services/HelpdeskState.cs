namespace CasaDesk.Services;

public sealed class HelpdeskState
{
    private readonly AccountStore _accountStore;
    public static readonly string[] Categories = ["Maintenance", "Security", "Garbage", "Common Areas", "Noise"];
    public static readonly string[] Stages = ["Received", "Assigned", "In Progress", "Resolved", "Closed"];
    public static readonly (string Title, string Detail)[] NextSteps =
    [
        ("1. Received", "Admins are notified right away."),
        ("2. Assigned and worked", "You see every status change."),
        ("3. You confirm", "The report closes when you say it is fixed.")
    ];

    private readonly List<ReportRecord> _reports =
    [
        new()
        {
            Id = 1042, Title = "Broken streetlight", Category = "Maintenance", Location = "Block 4, Main Road", Priority = "Normal", Submitted = "Sep 27", Status = "In Progress", AssignedTo = "R. Santos",
            Summary = "Repair work is underway. Expected completion within 2 days.",
            Updates = [new(1, "Received", "You submitted this report", "Sep 27, 7:45 PM"), new(2, "Assigned", "Assigned to R. Santos by Jose Dela Cruz", "Sep 28, 2:30 PM"), new(3, "In Progress", "R. Santos started repair work", "Sep 29, 9:10 AM")]
        },
        new()
        {
            Id = 1038, Title = "Clogged drainage", Category = "Maintenance", Location = "Phase 2", Priority = "Normal", Submitted = "Sep 22", Status = "Received", AssignedTo = "Unassigned",
            Summary = "Waiting for an admin to assign staff.",
            Updates = [new(1, "Received", "You submitted this report", "Sep 22, 4:20 PM")]
        }
    ];

    private readonly List<ActivityItem> _activity =
    [
        new(1042, "Streetlight moved to In Progress", "R. Santos started repair work", "Sep 29", true),
        new(1042, "Streetlight assigned to staff", "Assigned by Jose Dela Cruz", "Sep 28", false),
        new(1038, "Clogged drainage report received", "Your report is awaiting review", "Sep 22", true)
    ];

    public event Action? StateChanged;
    public IReadOnlyList<ReportRecord> Reports => _reports;
    public IReadOnlyList<ActivityItem> Activity => _activity;
    public bool IsAdmin { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public string ProfileName { get; set; } = "Maria Reyes";
    public string ProfileEmail { get; set; } = "maria.reyes@example.com";
    public string ProfilePhone { get; set; } = "0917 555 0142";
    public string ProfileAddress { get; set; } = "Block 4, Lot 12";
    public string? Toast { get; private set; }
    public string Initials => string.Join("", ProfileName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => part[0])).ToUpperInvariant();

    public HelpdeskState(AccountStore accountStore) => _accountStore = accountStore;

    public static string StageHint(string stage) => stage switch { "Received" => "Resident submits", "Assigned" => "Admin assigns", "In Progress" => "Staff works", "Resolved" => "Admin resolves", _ => "Resident confirms" };
    public ReportRecord? FindReport(int id) => _reports.FirstOrDefault(report => report.Id == id);

    public bool Register(string name, string email, string password, string address)
    {
        if (!_accountStore.TryRegister(name, email, password, out var account))
        {
            Notify("An account with this email already exists.");
            return false;
        }

        ProfileName = account!.Name;
        ProfileEmail = account.Email;
        ProfileAddress = string.IsNullOrWhiteSpace(address) ? "Add your block and lot" : address.Trim();
        IsAuthenticated = true;
        IsAdmin = false;
        Notify("Your account is ready.");
        return true;
    }

    public bool Login(string email, string password)
    {
        if (!_accountStore.TryAuthenticate(email, password, out var account))
        {
            Notify("Email or password is incorrect.");
            return false;
        }

        ProfileName = account!.Name;
        ProfileEmail = account.Email;
        IsAuthenticated = true;
        IsAdmin = false;
        Notify($"Welcome back, {ProfileName.Split(' ')[0]}.");
        return true;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        IsAdmin = false;
        Notify("You have signed out.");
    }

    public void SetAdmin(bool isAdmin)
    {
        IsAdmin = isAdmin;
        StateChanged?.Invoke();
    }

    public ReportRecord AddReport(string title, string category, string location, string priority, string description, string? photoName)
    {
        var id = _reports.Max(report => report.Id) + 1;
        var report = new ReportRecord
        {
            Id = id, Title = title.Trim(), Category = category, Location = location.Trim(), Priority = priority,
            Description = description.Trim(), PhotoName = photoName, Submitted = DateTime.Now.ToString("MMM d"),
            Status = "Received", AssignedTo = "Unassigned", Summary = "Your report was received and is waiting for review.",
            Updates = [new(1, "Received", "You submitted this report", DateTime.Now.ToString("MMM d, h:mm tt"))]
        };
        _reports.Insert(0, report);
        _activity.Insert(0, new(id, $"{report.Title} report received", "Your report is waiting for review", "Just now", true));
        StateChanged?.Invoke();
        return report;
    }

    public void AssignStaff(ReportRecord report, string staff)
    {
        report.AssignedTo = staff;
        if (staff != "Unassigned" && report.Status == "Received")
        {
            ChangeStatus(report, "Assigned", $"Assigned to {staff}");
        }
        else
        {
            Notify($"Assignment updated: {staff}.");
        }
    }

    public bool AdvanceStatus(ReportRecord report)
    {
        var index = Array.IndexOf(Stages, report.Status);
        if (index < 0 || index >= Stages.Length - 2 || (report.Status == "Received" && report.AssignedTo == "Unassigned"))
        {
            return false;
        }

        var next = Stages[index + 1];
        ChangeStatus(report, next, next == "In Progress" ? $"{report.AssignedTo} started work" : $"Report moved to {next}");
        return true;
    }

    public void ConfirmFixed(ReportRecord report) => ChangeStatus(report, "Closed", "Resident confirmed the repair is complete");
    public void ReopenReport(ReportRecord report) => ChangeStatus(report, "In Progress", "Resident reopened the report");

    public bool AddMessage(ReportRecord report, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        report.Messages.Add(new("Maria Reyes", text.Trim(), DateTime.Now.ToString("MMM d, h:mm tt")));
        Notify("Message sent to the admin.");
        return true;
    }

    public void Notify(string message)
    {
        Toast = message;
        StateChanged?.Invoke();
    }

    public void ClearToast()
    {
        Toast = null;
        StateChanged?.Invoke();
    }

    private void ChangeStatus(ReportRecord report, string status, string detail)
    {
        report.Status = status;
        report.Summary = status switch
        {
            "Received" => "Your report was received and is waiting for review.",
            "Assigned" => $"Assigned to {report.AssignedTo}. Staff will follow up soon.",
            "In Progress" => "Repair work is underway. Expected completion within 2 days.",
            "Resolved" => "The work is marked resolved. Please confirm if the repair is complete.",
            _ => "This report is closed. Thank you for confirming the repair."
        };
        var sequence = report.Updates.Count == 0 ? 1 : report.Updates.Max(update => update.Sequence) + 1;
        var date = DateTime.Now.ToString("MMM d, h:mm tt");
        report.Updates.Add(new(sequence, status, detail, date));
        _activity.Insert(0, new(report.Id, $"{report.Title} moved to {status}", detail, "Just now", status is "In Progress" or "Resolved"));
        Notify($"Report updated to {status}.");
    }

    public sealed class ReportRecord
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string Submitted { get; set; } = string.Empty;
        public string Status { get; set; } = "Received";
        public string AssignedTo { get; set; } = "Unassigned";
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PhotoName { get; set; }
        public List<ReportUpdate> Updates { get; set; } = [];
        public List<ReportMessage> Messages { get; set; } = [];
    }

    public sealed record ReportUpdate(int Sequence, string Title, string Detail, string Date);
    public sealed record ReportMessage(string Sender, string Text, string Date);
    public sealed record ActivityItem(int ReportId, string Title, string Detail, string Date, bool IsGold);
}