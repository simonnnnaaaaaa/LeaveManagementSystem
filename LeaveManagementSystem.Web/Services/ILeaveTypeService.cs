using LeaveManagementSystem.Web.Models.LeaveTypes;

namespace LeaveManagementSystem.Web.Services
{
    public interface ILeaveTypeService
    {
        Task<bool> CheckIfLeaveTypeExists(string name);
        Task<bool> CheckIfLeaveTypeExistsForEdit(LeaveTypeEditVM leaveTypeEdit);
        Task Create(LeaveTypeCreateVM model);
        Task Edit(LeaveTypeEditVM model);
        Task<List<LeaveTypeReadOnlyVM>> GetAllLeaveTypesAsync();
        Task<LeaveTypeReadOnlyVM?> GetLeaveTypeByIdAsync(int id);
        Task<LeaveTypeEditVM> GetLeaveTypeForEditByIdAsync(int id);
        bool LeaveTypeExists(int id);
        Task Remove(int id);
    }
}