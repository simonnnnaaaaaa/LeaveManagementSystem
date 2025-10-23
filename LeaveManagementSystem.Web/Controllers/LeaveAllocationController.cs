using LeaveManagementSystem.Web.Services.LeaveAllocations;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Web.Controllers
{
    [Authorize]
    public class LeaveAllocationController(ILeaveAllocationsService _leaveAllocationsService) : Controller
    {
        public async  Task<IActionResult> Details()
        {
            var employeeVm = await _leaveAllocationsService.GetEmployeeAllocations();
            return View(employeeVm);
        }
    }
}
