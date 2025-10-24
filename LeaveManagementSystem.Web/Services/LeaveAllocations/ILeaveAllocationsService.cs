using LeaveManagementSystem.Web.Models.LeaveAllocations;

namespace LeaveManagementSystem.Web.Services.LeaveAllocations
{
    public interface ILeaveAllocationsService
    {
        Task AllocateLeave(string employeeId);

        Task<EmployeeAllocationVM> GetEmployeeAllocations(string? userId);
        Task<List<EmployeeListVM>> GetEmployees();
    }



}
