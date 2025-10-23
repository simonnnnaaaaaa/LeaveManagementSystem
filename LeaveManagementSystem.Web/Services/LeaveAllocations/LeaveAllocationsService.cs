
using AutoMapper;
using LeaveManagementSystem.Web.Models.LeaveAllocations;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace LeaveManagementSystem.Web.Services.LeaveAllocations
{
    public class LeaveAllocationsService
        (ApplicationDbContext _context, 
        IHttpContextAccessor _httpContextAccessor, 
        UserManager<ApplicationUser> _userManager,
        IMapper _mapper) : ILeaveAllocationsService
    {
        public async Task AllocateLeave(string employeeId)
        {
            //get all leave types

            var leaveTypes = await _context.LeaveTypes.ToListAsync();

            //get the current period based on the year

            var currentDate = DateTime.Now;
            var period = await _context.Periods.SingleAsync(q => q.EndDate.Year == currentDate.Year);
            var monthsRemaining = period.EndDate.Month - currentDate.Month;



            //foreach leave type, create an allocation entry

            foreach (var leaveType in leaveTypes)
            {
                var accuralRate = decimal.Divide(leaveType.NumberOfDays, 12);

                var leaveAllocation = new LeaveAllocation
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    PeriodId = period.Id,
                    Days = (int)Math.Ceiling(accuralRate * monthsRemaining)
                };

                _context.Add(leaveAllocation);

            }

            await _context.SaveChangesAsync();

        }

        public async Task<List<LeaveAllocation>> GetAllocations()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            var currentDate = DateTime.Now;

            var LeaveAllocations = await _context.LeaveAllocations
                .Include(q => q.LeaveType) //simuleaza un join
                .Include(q => q.Period)
                .Where(q => q.EmployeeId == user.Id && q.Period.EndDate.Year == currentDate.Year)
                .ToListAsync();

            return LeaveAllocations;

        }

        public async Task<EmployeeAllocationVM> GetEmployeeAllocations()
        {
            var allocations = await GetAllocations();
            var allocationVmList = _mapper.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(allocations);

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            var employeeVm = new EmployeeAllocationVM
            {
                DateOfBirth = (DateOnly)user.DateOfBirth,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = user.Id,
                LeaveAllocations = allocationVmList,

                PeriodName = allocations.FirstOrDefault()?.Period?.Name ?? "—"
            };

            return employeeVm;

        }

    }

    

} 
