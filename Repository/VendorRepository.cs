using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;
public class VendorRepository : IVendorRepository
{
    private readonly AppDbContext _context;
    public VendorRepository(AppDbContext context) => _context = context;

    public async Task<List<Vendor>> GetAllAsync()
        => await _context.Vendors.ToListAsync();

    public async Task<Vendor?> GetByIdAsync(int id)
        => await _context.Vendors.FindAsync(id);

    public async Task AddAsync(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        _context.Vendors.Update(vendor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vendor vendor)
    {
        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Vendors.AnyAsync(v => v.Id == id);
}
