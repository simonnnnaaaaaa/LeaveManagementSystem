using AutoMapper;
using LeaveManagementSystem.Web.Data;
using LeaveManagementSystem.Web.Models.LeaveTypes;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Web.Services.LeaveTypes
{
    public class LeaveTypeService(ApplicationDbContext context, IMapper mapper) : ILeaveTypeService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;


        public async Task<List<LeaveTypeReadOnlyVM>> GetAllLeaveTypesAsync()
        {
            var data = await _context.LeaveTypes.ToListAsync();
            var viewData = _mapper.Map<List<LeaveTypeReadOnlyVM>>(data);
            return viewData;
        }

        public async Task<LeaveTypeReadOnlyVM?> GetLeaveTypeByIdAsync(int id)
        {
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (leaveType == null)
            {
                return null;
            }

            var viewData = _mapper.Map<LeaveTypeReadOnlyVM>(leaveType);

            return viewData;
        }

        public async Task<LeaveTypeEditVM> GetLeaveTypeForEditByIdAsync(int id)
        {
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveType == null)
            {
                return null;
            }
            var viewData = _mapper.Map<LeaveTypeEditVM>(leaveType);
            return viewData;
        }

        public async Task Remove(int id)
        {
            var data = await _context.LeaveTypes.FirstOrDefaultAsync(q => q.Id == id);
            if (data != null)
            {
                _context.Remove(data);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Edit(LeaveTypeEditVM model)
        {
            var leaveType = _mapper.Map<LeaveType>(model);
            _context.Update(leaveType);
            await _context.SaveChangesAsync();
        }

        public async Task Create(LeaveTypeCreateVM model)
        {
            var leaveType = _mapper.Map<LeaveType>(model);

            _context.Add(leaveType);
            await _context.SaveChangesAsync();
        }


        public bool LeaveTypeExists(int id)
        {
            return _context.LeaveTypes.Any(e => e.Id == id);
        }

        public async Task<bool> CheckIfLeaveTypeExists(string name)
        {
            var lowerCaseName = name.ToLower();
            return await _context.LeaveTypes.AnyAsync(q => q.Name.ToLower().Equals(lowerCaseName));
        }

        public async Task<bool> CheckIfLeaveTypeExistsForEdit(LeaveTypeEditVM leaveTypeEdit)
        {
            var lowerCaseName = leaveTypeEdit.Name.ToLower();
            return await _context.LeaveTypes.AnyAsync(q => q.Name.ToLower().Equals(lowerCaseName)
                    && q.Id != leaveTypeEdit.Id);
        }

        public async Task<bool> DaysExceedMaximum(int leaveTypeId, int days)
        {
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(q => q.Id == leaveTypeId);

            return leaveType.NumberOfDays < days;
        }
    }
}
