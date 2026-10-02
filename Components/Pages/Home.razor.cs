using Microsoft.AspNetCore.Components.Forms;

namespace CasaDesk.Components.Pages;

public partial class Home
{
    private static readonly string[] Categories = ["Maintenance", "Security", "Garbage", "Common Areas", "Noise"];
    private static readonly string[] Stages = ["Received", "Assigned", "In Progress", "Resolved", "Closed"];
    private static readonly (string Title, string Detail)[] NextSteps =
    [
        ("1. Received", "Admins are notified right away."),
        ("2. Assigned and worked", "You see every status change."),
        ("3. You confirm", "The report closes when you say it is fixed.")
    ];
    private static readonly string[] PipelineFilters = ["All", "Received", "Assigned", "In Progress", "Resolved", "Closed"];

    private readonly List<ReportRecord> Reports =
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
    private readonly List<ActivityItem> Activity =
    [
        new(1042, "Streetlight moved to In Progress", "R. Santos started repair work", "Sep 29", true),
        new(1042, "Streetlight assigned to staff", "Assigned by Jose Dela Cruz", "Sep 28", false),
        new(1038, "Clogged drainage report received", "Your report is awaiting review", "Sep 22", true)
    ];

    private string _page = "dashboard";
    private string _activeFilter = "All";
    private string _category = "Maintenance";
    private string _priority = "Normal";
    private string _location = "Block 4, Main Road";
    private string _newTitle = string.Empty;
    private string _newDescription = string.Empty;
    private string? _photoName;
    private string _messageText = string.Empty;
    private string? _toast;
    private int _selectedReportId = 1042;
    private bool _isAdmin;
    private string _profileName = "Maria Reyes";
    private string _profileEmail = "maria.reyes@example.com";
    private string _profilePhone = "0917 555 0142";
    private string _profileAddress = "Block 4, Lot 12";

    private string ProfileName { get => _profileName; set => _profileName = value; }
    private string Initials => string.Join("", _profileName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => part[0])).ToUpperInvariant();
    private string FieldClass => "mt-1.5 w-full rounded-md border border-[#dce2de] bg-white px-3 py-2.5 text-sm font-normal outline-none placeholder:text-[#9aa7aa] focus:border-[#216f7d] focus:ring-2 focus:ring-[#216f7d]/15";
    private ReportRecord SelectedReport => Reports.FirstOrDefault(report => report.Id == _selectedReportId) ?? Reports[0];
    private IEnumerable<ReportRecord> FilteredReports => _activeFilter == "All" ? Reports : Reports.Where(report => report.Status == _activeFilter);
    private string PageLabel => _page switch { "dashboard" => "Resident Dashboard", "new-report" => "Report an Issue", "track" => "Track Report", "profile" => "Profile Settings", "admin" => "Admin Report Pipeline", _ => "Admin Report Detail" };
    private string PageTitleText => _page switch { "dashboard" => $"Good morning, {_profileName.Split(' ')[0]}", "new-report" => "Report an Issue", "track" or "admin-detail" => SelectedReport.Title, "profile" => "Account Settings", "admin" => "Report Pipeline", _ => "Report Detail" };
    private string PageSubtitle => _page switch { "dashboard" => "Follow each report as it moves through the pipeline.", "new-report" => "Your report enters the pipeline at the Received stage.", "track" => $"Report #CD-{SelectedReport.Id}. Submitted {SelectedReport.Submitted}.", "profile" => "Your details and household information.", "admin" => "Move each report forward. Residents see every change.", "admin-detail" => $"Report #CD-{SelectedReport.Id}. Submitted {SelectedReport.Submitted} by {_profileName}.", _ => string.Empty };
    private string BackButtonText => _page switch { "new-report" => "Back", "track" => "Back to My Reports", "profile" => "Back", "admin-detail" => "Back to Pipeline", _ => "Back" };

    private void Navigate(string page) => _page = page;
    private void NavigateDashboard() => Navigate("dashboard");
    private void NavigateProfile() => Navigate("profile");
    private void NavigateAdmin() => Navigate("admin");
    private void NavigateAdminDetail() => Navigate("admin-detail");
    private void StartNewReport() => _page = "new-report";
    private void GoBack() => _page = _page switch { "new-report" or "track" or "profile" => "dashboard", "admin-detail" => "admin", _ => "dashboard" };

    private void ToggleRole()
    {
        _isAdmin = !_isAdmin;
        _page = _isAdmin ? "admin" : "dashboard";
    }

    private void SignOut() => _toast = "Demo session signed out. Authentication is not connected yet.";

    private void OpenReport(int id, bool admin = false)
    {
        _selectedReportId = id;
        _page = admin || _isAdmin ? "admin-detail" : "track";
    }

    private string NavClass(string page) => $"shrink-0 rounded-md px-3 py-2.5 text-left text-xs font-medium transition md:w-full {(_page == page ? "bg-[#dfae2f] text-[#203137]" : "text-white/90 hover:bg-white/10 hover:text-white")}";
    private string CategoryClass(string category) => _category == category ? "border-[#216f7d] bg-[#216f7d] text-white" : "border-[#dce2de] bg-white text-[#34464c] hover:border-[#216f7d]";
    private string FilterClass(string filter) => _activeFilter == filter ? "border-[#216f7d] bg-[#216f7d] text-white" : "border-[#dce2de] text-[#596c72] hover:border-[#216f7d]";
    private int FilterCount(string filter) => filter == "All" ? Reports.Count : Reports.Count(report => report.Status == filter);

    private string StatusBadgeClass(string status) => status switch
    {
        "Received" or "Assigned" => "bg-[#e4f1f2] text-[#216f7d]",
        "In Progress" => "bg-[#fbf1d4] text-[#8d6b13]",
        "Resolved" => "bg-[#e7efe4] text-[#4f754f]",
        _ => "bg-[#edf0eb] text-[#5f6e72]"
    };

    private string StepBarClass(ReportRecord report, string stage)
    {
        var stageIndex = Array.IndexOf(Stages, stage);
        var reportIndex = Array.IndexOf(Stages, report.Status);
        return stageIndex < reportIndex ? "bg-[#216f7d]" : stageIndex == reportIndex ? "bg-[#dfae2f]" : "bg-[#e3e7e1]";
    }

    private string StepTextClass(ReportRecord report, string stage)
    {
        var stageIndex = Array.IndexOf(Stages, stage);
        var reportIndex = Array.IndexOf(Stages, report.Status);
        return stageIndex == reportIndex ? "text-[#9a7415]" : stageIndex < reportIndex ? "text-[#203137]" : "text-[#63747a]";
    }

    private string StageHint(string stage) => stage switch { "Received" => "Resident submits", "Assigned" => "Admin assigns", "In Progress" => "Staff works", "Resolved" => "Admin resolves", _ => "Resident confirms" };

    private void HandlePhotoSelected(InputFileChangeEventArgs args)
    {
        if (args.File.Size > 5 * 1024 * 1024)
        {
            _photoName = null;
            _toast = "Choose an image under 5 MB.";
            return;
        }
        _photoName = args.File.Name;
    }

    private void SubmitReport()
    {
        if (string.IsNullOrWhiteSpace(_newTitle) || string.IsNullOrWhiteSpace(_newDescription) || string.IsNullOrWhiteSpace(_location))
        {
            _toast = "Complete the title, location, and description before submitting.";
            return;
        }

        var id = Reports.Max(report => report.Id) + 1;
        var report = new ReportRecord
        {
            Id = id,
            Title = _newTitle.Trim(),
            Category = _category,
            Location = _location.Trim(),
            Priority = _priority,
            Submitted = DateTime.Now.ToString("MMM d"),
            Status = "Received",
            AssignedTo = "Unassigned",
            Summary = "Your report was received and is waiting for review.",
            Updates = [new(1, "Received", "You submitted this report", DateTime.Now.ToString("MMM d, h:mm tt"))]
        };
        Reports.Insert(0, report);
        Activity.Insert(0, new(id, $"{report.Title} report received", "Your report is waiting for review", "Just now", true));
        _selectedReportId = id;
        _newTitle = string.Empty;
        _newDescription = string.Empty;
        _photoName = null;
        _page = "track";
        _toast = $"Report #CD-{id} submitted.";
    }

    private void SaveProfile() => _toast = "Your profile details have been updated for this session.";

    private void SaveAssignee()
    {
        if (SelectedReport.AssignedTo != "Unassigned" && SelectedReport.Status == "Received")
        {
            ChangeStatus(SelectedReport, "Assigned", $"Assigned to {SelectedReport.AssignedTo}");
        }
        else
        {
            _toast = $"Assignment updated: {SelectedReport.AssignedTo}.";
        }
    }

    private void AdvanceStatus()
    {
        var index = Array.IndexOf(Stages, SelectedReport.Status);
        if (index < 0 || index >= Stages.Length - 2)
        {
            _toast = "This report is ready for resident confirmation.";
            return;
        }
        if (SelectedReport.Status == "Received" && SelectedReport.AssignedTo == "Unassigned")
        {
            _toast = "Assign a staff member before advancing this report.";
            return;
        }
        var next = Stages[index + 1];
        ChangeStatus(SelectedReport, next, next == "In Progress" ? $"{SelectedReport.AssignedTo} started work" : $"Report moved to {next}");
    }

    private void ConfirmFixed() => ChangeStatus(SelectedReport, "Closed", "Resident confirmed the repair is complete");
    private void ReopenReport() => ChangeStatus(SelectedReport, "In Progress", "Resident reopened the report");

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
        Activity.Insert(0, new(report.Id, $"{report.Title} moved to {status}", detail, "Just now", status is "In Progress" or "Resolved"));
        _toast = $"Report updated to {status}.";
    }

    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_messageText))
        {
            _toast = "Write a message before sending.";
            return;
        }
        SelectedReport.Messages.Add(new("Maria Reyes", _messageText.Trim(), DateTime.Now.ToString("MMM d, h:mm tt")));
        _messageText = string.Empty;
        _toast = "Message sent to the admin.";
    }

    private sealed class ReportRecord
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
        public List<ReportUpdate> Updates { get; set; } = [];
        public List<ReportMessage> Messages { get; set; } = [];
    }

    private sealed record ReportUpdate(int Sequence, string Title, string Detail, string Date);
    private sealed record ReportMessage(string Sender, string Text, string Date);
    private sealed record ActivityItem(int ReportId, string Title, string Detail, string Date, bool IsGold);
}