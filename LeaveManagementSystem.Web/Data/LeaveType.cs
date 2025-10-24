namespace LeaveManagementSystem.Web.Data
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfDays { get; set; }

        public List<LeaveAllocation>? LeaveAllocations { get; set; }
    }
}

